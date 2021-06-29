using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Threads(int? boardId)
        {
            if (boardId is null)
                return NotFound();
            
            if (TempData is not null)
            {
                ViewBag.NotifyIsEnabled = TempData["NotifyIsEnabled"] as bool? ?? false;
                ViewBag.NotifyCode = TempData["NotifyCode"] as int? ?? 404;
            }
            
            var threads = _dbBoard.Threads.Where(thread => thread.BoardId == boardId).ToList();
            var board = _dbBoard.Boards.Find(boardId);
            ViewBag.FullName = board.DomainName + " — " + board.Name;
            ViewBag.Name = board.Name;
            ViewBag.BoardId = boardId;
            
            if (threads.Count == 0) 
                ViewBag.Message = "Доска пустая. Будьте первым, кто добавит тред!";
            
            return View(threads);
        }

        [HttpGet]
        public IActionResult Create(int? boardId)
        {
            if (boardId is null)
                return NotFound();
            
            ViewBag.BoardId = boardId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Name, HasBumpLimit, BoardId, PosterId")]
            Thread thread, string textPost)
        {
            thread.HasBumpLimit = true;
            thread.Name = thread.Name.Trim();
            
            if (!ModelState.IsValid)
            {
                ViewBag.BoardId = thread.BoardId;
                ViewBag.TextPost = textPost;
                
                return View(thread);
            }
            
            TempData["TextPost"] = textPost.Trim();
            TempData["IsOP"] = true;

            await _dbBoard.Threads.AddAsync(thread);
            await _dbBoard.SaveChangesAsync();
            SetNotificationInfo(1);
            
            return RedirectToAction("Create", "Post", new { threadId = thread.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int? threadId)
        {
            if (threadId is null)
                return NotFound();

            var thread = await _dbBoard.Threads.FindAsync(threadId);

            return View(thread);
        }
        
        [HttpPost]
        public async Task<IActionResult> Remove(int threadId)
        {
            var posts = _dbBoard.Posts.Where(post => post.ThreadId == threadId).ToList();
            _dbBoard.Posts.RemoveRange(posts);

            var thread = await _dbBoard.Threads.FindAsync(threadId);
            var boardId = thread.BoardId;
            _dbBoard.Threads.Remove(thread);
            await _dbBoard.SaveChangesAsync();
            SetNotificationInfo(-1);

            return RedirectToAction(nameof(Threads), new { boardId });
        }

        private void SetNotificationInfo(int code)
        {
            TempData["NotifyIsEnabled"] = true;
            TempData["NotifyCode"] = code;
        }
    }
}