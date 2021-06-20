using System.Linq;
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
        
        // GET
        public IActionResult Threads(int? boardId)
        {
            if (boardId == null)
                return NotFound();
            
            var threads = _dbBoard.Threads.Where(thread => thread.BoardId == boardId).ToList();
            var board = _dbBoard.Boards.First(localBoard => localBoard.Id == boardId);
            ViewBag.FullName = board.DomainName + " — " + board.Name;
            ViewBag.Name = board.Name;
            ViewBag.BoardId = boardId;
            
            if (threads.Count == 0) 
                ViewBag.Message = "Доска пустая. Будьте первым, кто добавит тред!";
            
            return View(threads);
        }
    }
}