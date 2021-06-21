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
        public IActionResult Create(int boardId)
        {
            ViewBag.BoardId = boardId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Name, HasBumpLimit, BoardId, PosterId")]
            Thread thread, string textPost)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TextPost = textPost;
                return View(thread);
            }
            
            thread.HasBumpLimit = true;
            TempData["TextPost"] = textPost;

            await _dbBoard.Threads.AddAsync(thread);
            await _dbBoard.SaveChangesAsync();

            return RedirectToAction("Create", "Post", 
                new { threadId = thread.Id, isOriginalPoster = true });
        }
    }
}