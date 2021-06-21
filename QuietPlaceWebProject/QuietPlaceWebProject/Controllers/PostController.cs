using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuietPlaceWebProject.Models;

namespace QuietPlaceWebProject.Controllers
{
    public class PostController : Controller
    {
        private readonly BoardContext _dbBoard;
        private readonly UserContext _dbUser;
        
        public PostController(BoardContext dbBoard, UserContext dbUser)
        {
            _dbBoard = dbBoard;
            _dbUser = dbUser;
        }

        [HttpGet]
        public async Task<IActionResult> Posts(int threadId)
        {
            var posts = _dbBoard.Posts.Where(post => post.ThreadId == threadId).ToList();
            var thread = await _dbBoard.Threads.FindAsync(threadId);
            ViewBag.ThreadId = threadId;
            ViewBag.Title = thread.Name;
            
            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int threadId, bool? isOriginalPoster)
        {
            var posterId = TempData["PosterId"] as int? ?? -1;

            if (posterId == -1)
            {
                TempData["IsOP"] = isOriginalPoster;
                
                return RedirectToAction("Create", "Anon", new { threadId });
            }

            isOriginalPoster = TempData["IsOP"] as bool? ?? false;

            if (isOriginalPoster.Value)
            {
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

            ViewBag.TextPost = TempData["TextPost"];
            ViewBag.ThreadId = threadId;
            ViewBag.PosterId = posterId;
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Text, DateOfCreation, PosterId, PostOfTovarishchId, IsOriginalPoster, ThreadId")]
            Post post)
        {
            if (!ModelState.IsValid)
                return View(post);
            
            post.DateOfCreation = DateTime.Now;
            post.IsOriginalPoster = IsOriginalPoster(post.PosterId, post.ThreadId);

            _dbBoard.Posts.Add(post);
            await _dbBoard.SaveChangesAsync();

            return RedirectToAction(nameof(Posts));
        }

        private bool IsOriginalPoster(int posterId, int threadId)
            => _dbBoard.Threads.Any(thread => thread.PosterId == posterId && thread.Id == threadId);
    }
}