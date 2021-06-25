﻿using System;
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
        public async Task<IActionResult> Posts(int? threadId)
        {
            if (threadId is null)
                return NotFound();
            
            if (TempData is not null)
                ViewBag.NotifyIsEnabled = TempData["NotifyIsEnabled"] as bool? ?? false;
            
            var posts = _dbBoard.Posts.Where(post => post.ThreadId == threadId).ToList();
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
        public async Task<IActionResult> Create(
            [Bind("Id, Text, DateOfCreation, PosterId, PostOfTovarishchId, IsOriginalPoster, ThreadId")]
            Post post, string textPost)
        {
            post.DateOfCreation = DateTime.Now;
            post.IsOriginalPoster = IsOriginalPoster(post.PosterId, post.ThreadId);
            
            if (!string.IsNullOrEmpty(textPost) || !string.IsNullOrWhiteSpace(textPost))
                post.Text = textPost.Trim();
            
            if (!ModelState.IsValid && post.Text.Length > 0)
            {
                ViewBag.ThreadId = post.ThreadId;
                ViewBag.PosterId = post.PosterId;
                
                return View(post);
            }

            _dbBoard.Posts.Add(post);
            await _dbBoard.SaveChangesAsync();
            TempData["NotifyIsEnabled"] = true;

            return RedirectToAction(nameof(Posts), new { threadId = post.ThreadId });
        }

        private bool IsOriginalPoster(int posterId, int threadId)
            => _dbBoard.Threads.Any(thread => thread.PosterId == posterId && thread.Id == threadId);

        [HttpGet]
        public async Task<IActionResult> Remove(int? postId)
        {
            if (postId is null)
                return NotFound();

            var post = await _dbBoard.Posts.FindAsync(postId);

            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int postId)
        {
            var post = await _dbBoard.Posts.FindAsync(postId);
            _dbBoard.Posts.Remove(post);
            await _dbBoard.SaveChangesAsync();
            TempData["NotifyIsEnabled"] = true;

            return RedirectToAction(nameof(Posts), new { post.ThreadId });
        }

        [HttpGet]
        public IActionResult ToAnswer(int? threadId, int? postId)
        {
            TempData["AnswerId"] = postId;

            return RedirectToAction(nameof(Create), new { threadId });
        }
    }
}