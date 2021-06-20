using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Posts()
        {
            return null;
        }

        [HttpGet]
        public IActionResult Create(int threadId, int senderId)
        {
            ViewBag.TextPost = TempData["TextPost"];
            if (_dbUser.Users.Any(user => user.Id != senderId))
                return RedirectToAction("Create", "Anon", new {threadId});

            ViewBag.ThreadId = threadId;
            ViewBag.SenderId = senderId;
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Text, DateOfCreation, SenderId, PostOfTovarishchId, IsOriginalPoster, ThreadId")]
            Post post)
        {
            if (!ModelState.IsValid)
                return View(post);
            
            post.DateOfCreation = DateTime.Now;
            post.IsOriginalPoster = IsOriginalPoster(post.SenderId, post.ThreadId);

            _dbBoard.Posts.Add(post);
            await _dbBoard.SaveChangesAsync();

            return RedirectToAction(nameof(Posts));
        }

        private bool IsOriginalPoster(int senderId, int threadId)
            => _dbBoard.Threads.Any(thread => thread.PosterId == senderId && thread.Id == threadId);
    }
}