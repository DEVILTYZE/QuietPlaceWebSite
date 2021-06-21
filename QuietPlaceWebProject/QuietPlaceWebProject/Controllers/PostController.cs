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
        public async Task<IActionResult> Posts(int threadId)
        {
            var posts = _dbBoard.Posts.Where(post => post.ThreadId == threadId).ToList();
            var thread = await _dbBoard.Threads.FindAsync(threadId);
            ViewBag.ThreadId = threadId;
            ViewBag.Title = thread.Name;
            
            return View(posts);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int threadId)
        {
            var ipAddressOfUser = await AnonController.GetUserIpAddress();
            var posterList = _dbUser.Users.Where(localUser => localUser.IpAddress == ipAddressOfUser).ToList();

            var posterId = posterList.Count == 1 ? posterList.First().Id : -1;
            
            // var posterId = TempData["PosterId"] as int? ?? -1;

            if (posterId == -1)
                return RedirectToAction("Create", "Anon", new { threadId });

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
        public async Task<IActionResult> Create([Bind("Id, Text, DateOfCreation, PosterId, PostOfTovarishchId, IsOriginalPoster, ThreadId")]
            Post post)
        {
            post.DateOfCreation = DateTime.Now;
            post.IsOriginalPoster = IsOriginalPoster(post.PosterId, post.ThreadId);
            
            if (!ModelState.IsValid)
                return View(post);

            _dbBoard.Posts.Add(post);
            await _dbBoard.SaveChangesAsync();

            return RedirectToAction(nameof(Posts), new { threadId = post.ThreadId });
        }

        private bool IsOriginalPoster(int posterId, int threadId)
            => _dbBoard.Threads.Any(thread => thread.PosterId == posterId && thread.Id == threadId);
    }
}