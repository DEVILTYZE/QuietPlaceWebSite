using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
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
            
            // var posterId = TempData["PosterId"] as int? ?? -1;

            if (answerId != -1)
                TempData["TextPost"] = ">>" + answerId + "\r\n" + TempData["TextPost"];

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
            {
                ViewBag.ThreadId = post.ThreadId;
                ViewBag.PosterId = post.PosterId;
                
                return View(post);
            }

            post.Text = ReworkTextPost(post.Text);

            _dbBoard.Posts.Add(post);
            await _dbBoard.SaveChangesAsync();

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

            return RedirectToAction(nameof(Posts), new { post.ThreadId });
        }

        [HttpGet]
        public IActionResult ToAnswer(int? threadId, int? postId)
        {
            TempData["AnswerId"] = postId;

            return RedirectToAction(nameof(Create), new { threadId });
        }

        private static string ReworkTextPost(string text)
        {
            const string aOrangeStart = "<a style=\"color: orange\">";
            const string aGreenStart = "<a style\"color: green\">";
            const string aEnd = "</a>";
            var matches = GetAnswerIdsStrings(text);
            var greenStrings = GetGreenTextStrings(text);

            var matchArray = matches as Match[] ?? matches.ToArray();
            text = "\"" + matchArray.Aggregate(text, (current, match) 
                => current.Replace(match.Value, "\"" + aOrangeStart.Insert(
                                                         3, $"href=\"#post{match.Value.Substring(2, match.Value.Length - 2)}\" ") 
                                                     + match.Value + aEnd + "\"")) + "\"";

            var matchArrayGreen = greenStrings as Match[] ?? greenStrings.ToArray();
            text = "\"" + matchArrayGreen.Aggregate(text, (current, match) 
                => current.Replace(match.Value, "\"" + aGreenStart + match.Value + aEnd + "\"")) + "\"";

            var withOutQuotationMarks = GetTextWithoutQuotationMarks(text);

            if (withOutQuotationMarks.Any())
                text = text.Substring(1, text.Length - 2);
            
            return text;
            
            // return matchArray.Select(match 
            //     => int.Parse(match.Value.Substring(2, match.Value.Length - 2))).ToList();
        }

        private static IEnumerable<Match> GetAnswerIdsStrings(string text)
        {
            var regex = new Regex(@">>\d+");
            var matches = regex.Matches(text);

            return matches.ToList();
        }

        private static IEnumerable<Match> GetGreenTextStrings(string text)
        {
            var regex = new Regex(">^.+$", RegexOptions.Multiline);
            var matches = regex.Matches(text);

            return matches.ToList();
        }

        private static IEnumerable<Match> GetTextWithoutQuotationMarks(string text)
        {
            var regexFirst = new Regex("^\"+");
            var regexSecond = new Regex("\"+$");
            var matches = regexFirst.Matches(text);
            
            matches = (MatchCollection) matches.Concat(regexSecond.Matches(text));

            return matches.ToList();
        }
    }
}