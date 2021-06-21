using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuietPlaceWebProject.Models;

namespace QuietPlaceWebProject.Controllers
{
    public class BoardController : Controller
    {
        private readonly BoardContext _dbBoard;
        private readonly UserContext _dbUser;
        
        public BoardController(BoardContext dbBoard, UserContext dbUser)
        {
            _dbBoard = dbBoard;
            _dbUser = dbUser;
            
            InitialDatabase();
        }

        [HttpGet]
        public IActionResult Boards()
        {
            if (TempData is not null)
            {
                ViewBag.NotifyIsEnabled = TempData["NotifyIsEnabled"] as bool? ?? false;
                ViewBag.TextNotification = (string) TempData["TextNotification"] ?? string.Empty;
            }
            
            var boards = _dbBoard.Boards.ToList();

            return View(boards);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_dbUser.Roles, "Id", "Name");
            
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Id, Name, DomainName, MaxCountOfThreads, IsHidden, AccessRoleId")]
            Board board)
        {
            ViewBag.Roles = new SelectList(_dbUser.Roles, "Id", "Name");
            
            if (!ModelState.IsValid) 
                return View(board);
            
            await _dbBoard.Boards.AddAsync(board);
            await _dbBoard.SaveChangesAsync();
            EnableNotification("Доска создана успешно");
            
            return RedirectToAction(nameof(Boards));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? boardId)
        {
            if (boardId is null)
                return NotFound();

            var board = await _dbBoard.Boards.FindAsync(boardId);
            ViewBag.Roles = new SelectList(_dbUser.Roles, "Id", "Name");

            return View(board);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([Bind("Id, Name, DomainName, MaxCountOfThreads, IsHidden, AccessRoleId")]
            Board board)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(_dbUser.Roles, "Id", "Name");
                
                return View(board);
            }

            try
            {
                // _dbBoard.Boards.Update(board);
                _dbBoard.Entry(board).State = EntityState.Modified;
                await _dbBoard.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _dbBoard.Boards.AnyAsync(localBoard => localBoard.Id == board.Id))
                    return NotFound();
                
                throw;
            }

            EnableNotification("Доска изменена успешно");
            
            return RedirectToAction(nameof(Boards));
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int? boardId)
        {
            if (_dbBoard.Boards.Count() == 1)
                return RedirectToAction(nameof(Boards));
            
            if (boardId is null)
                return NotFound();
            
            var board = await _dbBoard.Boards.FindAsync(boardId);
            
            return View(board);
        }
        
        [HttpPost]
        public async Task<IActionResult> Remove(int boardId)
        {
            var threadList = _dbBoard.Threads.Where(thread => thread.BoardId == boardId).ToList();

            foreach (var thread in threadList)
            {
                var postList = _dbBoard.Posts.Where(post => post.ThreadId == thread.Id).ToList();

                foreach (var post in postList)
                    _dbBoard.Posts.Remove(post);

                _dbBoard.Threads.Remove(thread);
            }
            
            _dbBoard.Boards.Remove(await _dbBoard.Boards.FindAsync(boardId));
            await _dbBoard.SaveChangesAsync();
            EnableNotification("Доска удалена успешно");
            
            return RedirectToAction(nameof(Boards));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
        
        private void InitialDatabase()
        {
            if (_dbBoard.Boards.Any()) 
                return;
            
            var defaultBoard = new Board
            {
                Id = 0,
                Name = "Разработка Имиджборды",
                DomainName = "/dib/",
                MaxCountOfThreads = 20,
                IsHidden = false,
                AccessRoleId = 0
            };

            var defaultPasscode = new Passcode
            {
                Id = 0,
                Code = "adm1n",
                RoleId = 0
            };

            var defaultRole = new Role
            {
                Id = 0,
                Name = "Admin"
            };

            _dbBoard.Boards.Add(defaultBoard);
            _dbUser.Passcodes.Add(defaultPasscode);
            _dbUser.Roles.Add(defaultRole);

            _dbBoard.SaveChanges();
            _dbUser.SaveChanges();
        }

        private void EnableNotification(string text)
        {
            TempData["NotifyIsEnabled"] = true;
            TempData["TextNotification"] = text;
        }
    }
}