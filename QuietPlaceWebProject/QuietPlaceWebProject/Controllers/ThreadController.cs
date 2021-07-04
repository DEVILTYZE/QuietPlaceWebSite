using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuietPlaceWebProject.Interfaces;
using QuietPlaceWebProject.Models;

namespace QuietPlaceWebProject.Controllers
{
    public class ThreadController : Controller
    {
        private readonly BoardContext _dbBoard;
        private readonly UserContext _dbUser;

        public ThreadController(BoardContext dbBoard, UserContext dbUser)
        {
            _dbBoard = dbBoard;
            _dbUser = dbUser;
        }
        
        [HttpGet]
        public async Task<IActionResult> Threads(int? boardId)
        {
            if (boardId is null)
                return RedirectToAction("NotFoundPage", "Anon");
            
            // if (TempData is not null)
            // {
            //     ViewBag.NotifyIsEnabled = TempData["NotifyIsEnabled"] as bool? ?? false;
            //     ViewBag.NotifyCode = TempData["NotifyCode"] as int? ?? 404;
            // }
            
            var threads = await _dbBoard.Threads.Where(thread => thread.BoardId == boardId).ToListAsync();
            var board = await _dbBoard.Boards.FindAsync(boardId);
            ViewBag.Posts = await _dbBoard.Posts.ToListAsync();
            ViewBag.FullName = board.DomainName + " — " + board.Name;
            ViewBag.Name = board.Name;
            ViewBag.BoardId = boardId;
            
            if (threads.Count == 0) 
                ViewBag.Message = "Доска пустая. Будьте первым, кто добавит тред!";
            
            return View(threads);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? boardId)
        {
            if (boardId is null)
                return RedirectToAction("NotFoundPage", "Anon");

            if (TempData["IsBanned"] is null)
                return RedirectToAction("IsBanned", "Anon", new {boardId});

            var maxCountOfThreads = (await _dbBoard.Boards.FindAsync(boardId)).MaxCountOfThreads;

            if (await _dbBoard.Threads.CountAsync() > maxCountOfThreads)
                return RedirectToAction("Boards", "Board");

            if (!await IsHighRole())
                SetCaptcha();

            ViewBag.BoardId = boardId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Name, HasBumpLimit, BoardId, PosterId")]
            Thread thread, string textPost, string captchaWord)
        {
            thread.HasBumpLimit = true;
            thread.Name = thread.Name.Trim();

            captchaWord ??= "NULL";
            var captchaWordValid = TempData["CaptchaWord"] as string ?? "NULL";
            
            if (!ModelState.IsValid || textPost.Length is 0 or > 5000 || string.CompareOrdinal(
                captchaWord.ToUpper(), captchaWordValid.ToUpper()) != 0)
            {
                if (!await IsHighRole())
                    SetCaptcha();
                
                ViewBag.BoardId = thread.BoardId;
                ViewBag.TextPost = textPost;
                
                return View(thread);
            }
            
            TempData["TextPost"] = textPost.Trim();
            TempData["IsOP"] = true;

            await _dbBoard.Threads.AddAsync(thread);
            await _dbBoard.SaveChangesAsync();
            
            return RedirectToAction("Create", "Post", new { threadId = thread.Id });
        }

        [Authorize(Roles = "admin, moderator")]
        [HttpGet]
        public async Task<IActionResult> Remove(int? threadId)
        {
            if (threadId is null)
                return RedirectToAction("NotFoundPage", "Anon");

            var thread = await _dbBoard.Threads.FindAsync(threadId);
            ViewBag.TextPost = await _dbBoard.Posts.Where(localPost => localPost.ThreadId == threadId).FirstAsync();

            return View(thread);
        }
        
        [Authorize(Roles = "admin, moderator")]
        [HttpPost]
        public async Task<IActionResult> Remove(int threadId)
        {
            Thread thread;
            
            try
            {
                var posts = _dbBoard.Posts.Where(post => post.ThreadId == threadId).ToList();
                _dbBoard.Posts.RemoveRange(posts);

                thread = await _dbBoard.Threads.FindAsync(threadId);
                _dbBoard.Threads.Remove(thread);
                await _dbBoard.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _dbBoard.Threads.AnyAsync(localThread => localThread.Id == threadId))
                    return RedirectToAction("NotFoundPage", "Anon");
                
                throw;
            }

            return RedirectToAction(nameof(Threads), new { thread.BoardId });
        }

        private async Task<bool> IsHighRole()
        {
            var ipAddress = await AnonController.GetUserIpAddress();
            var user = await _dbUser.Users.Where(localUser 
                => string.Compare(localUser.IpAddress, ipAddress) == 0).FirstAsync();

            return user.RoleId <= 3;
        }
        
        // Captcha
        private void SetCaptcha()
        {
            var captcha = GetRandomCaptcha();
            ViewBag.CaptchaImage = captcha.ImageUrl;
            TempData["CaptchaWord"] = captcha.Word;
        }
        
        private ICaptcha GetRandomCaptcha()
        {
            var captchas = _dbBoard.Captchas.ToList();
            var random = new Random();

            return captchas[random.Next(0, captchas.Count - 1)];
        }
    }
}