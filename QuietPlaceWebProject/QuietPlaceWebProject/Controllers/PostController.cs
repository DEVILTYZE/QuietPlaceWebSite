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
    [Authorize(Roles = "admin, moderator, anon")]
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

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Posts(int? threadId)
        {
            if (threadId is null)
                return NotFound();
            
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
                return NotFound();
            
            var ipAddressOfUser = await AnonController.GetUserIpAddress();
            var posters = _dbUser.Users.Where(localUser => localUser.IpAddress == ipAddressOfUser).ToList();
            var posterId = posters.Count == 1 ? posters.First().Id : -1;
            var answerId = TempData["AnswerId"] as int? ?? -1;

            if (posterId == -1)
                return RedirectToAction("Create", "Anon", new { threadId });
            
            if (answerId != -1)
                ViewBag.AnswerPost = ">>" + answerId + "\n";

            var isOriginalPoster = TempData["IsOP"] as bool? ?? false;

            if (isOriginalPoster)
            {
                TempData["CaptchaWord"] = null;
                var thread = await _dbBoard.Threads.FindAsync(threadId);
                
                try
                {
                    thread.PosterId = posterId;
                    _dbBoard.Entry(thread).State = EntityState.Modified;
                    await _dbBoard.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _dbBoard.Threads.AnyAsync(localThread => localThread.Id == thread.Id))
                        return NotFound();
                
                    throw;
                }
            }
            else
                await SetCaptcha();
            
            ViewBag.TextPost = TempData["TextPost"];
            ViewBag.ThreadId = threadId;
            ViewBag.PosterId = posterId;
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [Bind("Id, Text, DateOfCreation, PosterId, PostOfTovarishchId, IsOriginalPoster, ThreadId")]
            Post post, string textPost, string captchaWord)
        {
            post.DateOfCreation = DateTime.Now;
            post.IsOriginalPoster = IsOriginalPoster(post.PosterId, post.ThreadId);

            textPost = GetText(post, textPost);

            var captchaWordValid = TempData["CaptchaWord"] as string ?? "NULL";
            captchaWord ??= "NULL";
            
            if (!ModelState.IsValid || textPost is null || textPost.Length == 0 || post.Text.Length >= 5000 ||
                string.CompareOrdinal(captchaWord.ToUpper(), captchaWordValid.ToUpper()) != 0)
            {
                await SetCaptcha();
                
                ViewBag.ThreadId = post.ThreadId;
                ViewBag.PosterId = post.PosterId;
                
                return View(post);
            }

            _dbBoard.Posts.Add(post);
            await _dbBoard.SaveChangesAsync();
            SetNotificationInfo(1);

            return RedirectToAction(nameof(Posts), new { threadId = post.ThreadId });
        }

        private bool IsOriginalPoster(int posterId, int threadId)
            => _dbBoard.Threads.Any(thread => thread.PosterId == posterId && thread.Id == threadId);

        [Authorize(Roles = "admin, moderator")]
        [HttpGet]
        public async Task<IActionResult> Remove(int? postId)
        {
            if (postId is null)
                return NotFound();

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
                    return NotFound();
                
                throw;
            }
            
            SetNotificationInfo(-1);

            return RedirectToAction(nameof(Posts), new { post.ThreadId });
        }

        [HttpGet]
        public IActionResult ToAnswer(int? threadId, int? postId)
        {
            TempData["AnswerId"] = postId;

            return RedirectToAction(nameof(Create), new { threadId });
        }

        private void SetNotificationInfo(int code)
        {
            TempData["NotifyIsEnabled"] = true;
            TempData["NotifyCode"] = code;
        }

        // Captcha
        private async Task SetCaptcha()
        {
            var captcha = await GetCaptchaAsync();
            ViewBag.CaptchaImage = captcha.ImageUrl;
            TempData["CaptchaWord"] = captcha.Word;
        }

        private static string GetText(IPost post, string textPost)
        {
            if (!string.IsNullOrEmpty(textPost) || !string.IsNullOrWhiteSpace(textPost))
            {
                post.Text = textPost.Trim();
                textPost = TextHelper.RemoveTags(post.Text);
            }
            else if (!string.IsNullOrEmpty(post.Text) || !string.IsNullOrWhiteSpace(post.Text))
                textPost = TextHelper.RemoveTags(post.Text);

            return textPost;
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