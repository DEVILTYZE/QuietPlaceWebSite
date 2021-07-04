using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using QuietPlaceWebProject.Helpers;
using QuietPlaceWebProject.Interfaces;
using QuietPlaceWebProject.Models;

namespace QuietPlaceWebProject.Controllers
{
    public class PostController : Controller
    {
        private readonly BoardContext _dbBoard;
        private readonly UserContext _dbUser;
        private readonly IHostEnvironment _environment;
        
        public PostController(BoardContext dbBoard, UserContext dbUser, IHostEnvironment environment)
        {
            _dbBoard = dbBoard;
            _dbUser = dbUser;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Posts(int? threadId)
        {
            if (threadId is null)
                return RedirectToAction("NotFoundPage", "Anon");
            
            if (TempData is not null)
            {
                ViewBag.NotifyIsEnabled = TempData["NotifyIsEnabled"] as bool? ?? false;
                ViewBag.NotifyCode = TempData["NotifyCode"] as int? ?? 404;
            }
            
            var posts = await _dbBoard.Posts.Where(post => post.ThreadId == threadId).ToListAsync();
            var thread = await _dbBoard.Threads.FindAsync(threadId);
            ViewBag.ThreadId = threadId;
            ViewBag.Title = thread.Name;
            
            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? threadId)
        {
            if (threadId is null)
                return RedirectToAction("NotFoundPage", "Anon");

            if (TempData["IsBanned"] is null)
                return RedirectToAction("IsBanned", "Anon", new {threadId});
            
            var thread = await _dbBoard.Threads.FindAsync(threadId);
            
            if (thread.HasBumpLimit &&
                (await _dbBoard.Posts.Where(localPost => localPost.ThreadId == threadId).CountAsync()) > 500)
            {
                _dbBoard.Threads.Remove(thread);
                await _dbBoard.SaveChangesAsync();

                return RedirectToAction("Threads", "Thread", new {thread.BoardId});
            }

            var post = new Post();
            var ipAddressOfUser = await AnonController.GetUserIpAddress();
            var posters = _dbUser.Users.Where(localUser => localUser.IpAddress == ipAddressOfUser).ToList();
            var posterId = posters.Count == 1 ? posters.First().Id : -1;
            var answerId = TempData["AnswerId"] as string;

            if (posterId == -1)
                return RedirectToAction("Create", "Anon", new { threadId });
            
            if (answerId is not null)
                post.Text = ">>" + answerId + "\r\n";

            var isOriginalPoster = TempData["IsOP"] as bool? ?? false;

            if (isOriginalPoster)
            {
                TempData["CaptchaWord"] = null;
                
                try
                {
                    thread.PosterId = posterId;
                    _dbBoard.Entry(thread).State = EntityState.Modified;
                    await _dbBoard.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _dbBoard.Threads.AnyAsync(localThread => localThread.Id == thread.Id))
                        return RedirectToAction("NotFoundPage", "Anon");
                
                    throw;
                }
            }
            else if (!await IsHighRole())
                await SetCaptcha();
            
            ViewBag.TextPost = TempData["TextPost"];
            ViewBag.ThreadId = threadId;
            ViewBag.PosterId = posterId;
            
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [Bind("Id, Text, DateOfCreation, PosterId, PostOfTovarishchId, IsOriginalPoster, ThreadId")]
            Post post, string captchaWord)
        {
            post.DateOfCreation = DateTime.Now;
            post.IsOriginalPoster = IsOriginalPoster(post.PosterId, post.ThreadId);

            var textPost = post.Text;
            post.Text = TextHelper.RemoveTags(post.Text);

            var captchaWordValid = TempData["CaptchaWord"] as string ?? "NULL";
            captchaWord ??= "NULL";
            
            if (!ModelState.IsValid || post.Text.Length == 0 ||
                string.CompareOrdinal(captchaWord.ToUpper(), captchaWordValid.ToUpper()) != 0)
            {
                if (!await IsHighRole())
                    await SetCaptcha();

                post.Text = textPost;
                ViewBag.ThreadId = post.ThreadId;
                ViewBag.PosterId = post.PosterId;
                
                return View(post);
            }
            
            post.Text = textPost;

            _dbBoard.Posts.Add(post);
            await _dbBoard.SaveChangesAsync();

            return RedirectToAction(nameof(Posts), new { threadId = post.ThreadId });
        }

        [Authorize(Roles = "admin, moderator")]
        [HttpGet]
        public async Task<IActionResult> Remove(int? postId)
        {
            if (postId is null)
                return RedirectToAction("NotFoundPage", "Anon");

            var post = await _dbBoard.Posts.FindAsync(postId);

            return View(post);
        }

        [Authorize(Roles = "admin, moderator")]
        [HttpPost]
        public async Task<IActionResult> Remove(int postId)
        {
            Post post;
            
            try
            {
                post = await _dbBoard.Posts.FindAsync(postId);
                _dbBoard.Posts.Remove(post);
                await _dbBoard.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _dbBoard.Posts.AnyAsync(localPost => localPost.Id == postId))
                    return RedirectToAction("NotFoundPage", "Anon");
                
                throw;
            }

            return RedirectToAction(nameof(Posts), new { post.ThreadId });
        }

        [HttpGet]
        public IActionResult ToAnswer(int? threadId, int? postId)
        {
            if (threadId is null || postId is null)
                return RedirectToAction("NotFoundPage", "Anon");

            var originalPost = _dbBoard.Posts.First(localPost => localPost.ThreadId == threadId);

            if (originalPost.Id == postId)
                TempData["AnswerId"] = postId + " (OP)";
            else
                TempData["AnswerId"] = postId.ToString();

            return RedirectToAction(nameof(Create), new { threadId });
        }
        
        private async Task<bool> IsHighRole()
        {
            var ipAddress = await AnonController.GetUserIpAddress();
            var user = await _dbUser.Users.Where(localUser 
                    => string.Compare(localUser.IpAddress, ipAddress) == 0).FirstAsync();

            return user.RoleId <= 3;
        }
        
        private bool IsOriginalPoster(int posterId, int threadId)
            => _dbBoard.Threads.Any(thread => thread.PosterId == posterId && thread.Id == threadId);

        // Captcha
        private async Task SetCaptcha()
        {
            var captcha = await GetCaptchaAsync();
            ViewBag.CaptchaImage = captcha.ImageUrl;
            TempData["CaptchaWord"] = captcha.Word;
        }

        private async Task<ICaptcha> GetCaptchaAsync()
        {
            var id = await _dbBoard.Captchas.CountAsync();

            if (id == 100)
            {
                var randomCaptcha = _dbBoard.Captchas.First();
                System.IO.File.Delete(randomCaptcha.ImageUrl);
                _dbBoard.Captchas.Remove(randomCaptcha);
                await _dbBoard.SaveChangesAsync();
            }
            
            const int lenght = 6;
            const int symbolWidth = 100;
            var symbols = TextHelper.Symbols;
            var random = new Random();
            var captchaWord = new char[lenght];

            for (var i = 0; i < lenght; ++i)
                captchaWord[i] = symbols[random.Next(0, symbols.Length - 1)];

            var word = new string(captchaWord);

            var mainBitmap = new Bitmap(symbolWidth * lenght, 100);
            var g = Graphics.FromImage(mainBitmap);
            var index = 0;
            
            foreach (var symbol in captchaWord)
            {
                var baseImagePath = _environment.ContentRootPath + $@"\wwwroot\captcha\base_images\";
                baseImagePath += char.IsDigit(symbol) ? $"number_{symbol}.png" : $"letter_{symbol}.png";
                var symbolBitmap = new Bitmap(baseImagePath);
                
                g.DrawImage(symbolBitmap, index, 0);
                index += symbolWidth;
            }

            await using var fs = new FileStream(
                _environment.ContentRootPath + $@"\wwwroot\captcha\images\captcha_{id}.png", 
                FileMode.Create);
            mainBitmap.Save(fs, ImageFormat.Png);
            
            var captcha = new Captcha
            {
                ImageUrl = $"captcha_{id}.png", 
                Word = word
            };

            await _dbBoard.Captchas.AddAsync(captcha);
            await _dbBoard.SaveChangesAsync();

            var listCaptchas = _dbBoard.Captchas.ToList();

            foreach (var localCaptcha in listCaptchas.Where(localCaptcha => string.CompareOrdinal(localCaptcha.Word, word) == 0))
                return localCaptcha;

            return listCaptchas.First();
        }
    }
}