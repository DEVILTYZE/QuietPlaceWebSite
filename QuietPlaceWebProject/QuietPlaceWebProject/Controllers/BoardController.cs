using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuietPlaceWebProject.Models;

namespace QuietPlaceWebProject.Controllers
{
    [Authorize(Roles = "admin")]
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

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Boards()
        {
            if (TempData is not null)
            {
                ViewBag.NotifyIsEnabled = TempData["NotifyIsEnabled"] as bool? ?? false;
                ViewBag.NotifyCode = TempData["NotifyCode"] as int? ?? 404;
            }
            
            var boards = await _dbBoard.Boards.ToListAsync();

            return View(boards);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(_dbUser.Roles, "Id", "Name");
            
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(
            [Bind("Id, Name, DomainName, MaxCountOfThreads, IsHidden, AccessRoleId")] Board board)
        {
            ViewBag.Roles = new SelectList(_dbUser.Roles, "Id", "Name");
            board.Name = board.Name.Trim();
            
            if (!ModelState.IsValid) 
                return View(board);
            
            await _dbBoard.Boards.AddAsync(board);
            await _dbBoard.SaveChangesAsync();
            SetNotificationInfo(1);
            
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
        public async Task<IActionResult> Edit(
            [Bind("Id, Name, DomainName, MaxCountOfThreads, IsHidden, AccessRoleId")] Board board)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(_dbUser.Roles, "Id", "Name");
                
                return View(board);
            }

            try
            {
                _dbBoard.Entry(board).State = EntityState.Modified;
                await _dbBoard.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _dbBoard.Boards.AnyAsync(localBoard => localBoard.Id == board.Id))
                    return NotFound();
                
                throw;
            }

            SetNotificationInfo(0);
            
            return RedirectToAction(nameof(Boards));
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int? boardId)
        {
            if (boardId is null)
                return NotFound();
            
            if (_dbBoard.Boards.Count() == 1)
                return RedirectToAction(nameof(Boards));
            
            var board = await _dbBoard.Boards.FindAsync(boardId);
            
            return View(board);
        }
        
        [HttpPost]
        public async Task<IActionResult> Remove(int boardId)
        {
            try
            {
                var threads = _dbBoard.Threads.Where(thread => thread.BoardId == boardId).ToList();

                foreach (var posts in threads.Select(
                    thread => _dbBoard.Posts.Where(post => post.ThreadId == thread.Id).ToList()))
                    _dbBoard.Posts.RemoveRange(posts);

                _dbBoard.Threads.RemoveRange(threads);
            
                _dbBoard.Boards.Remove(await _dbBoard.Boards.FindAsync(boardId));
                await _dbBoard.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _dbBoard.Boards.AnyAsync(localBoard => localBoard.Id == boardId))
                    return NotFound();
                
                throw;
            }
            
            SetNotificationInfo(-1);
            
            return RedirectToAction(nameof(Boards));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult NotFoundPage() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        private void SetNotificationInfo(int code)
        {
            TempData["NotifyIsEnabled"] = true;
            TempData["NotifyCode"] = code;
        }
        
        private void InitialDatabase()
        {
            if (_dbBoard.Boards.Any()) 
                return;
            
            var defaultBoard = new Board
            {
                Name = "Разработка Имиджборды",
                DomainName = "/dib/",
                MaxCountOfThreads = 20,
                IsHidden = false,
                AccessRoleId = 1
            };

            var defaultThread = new Thread
            {
                Name = "Проверка текста",
                BoardId = 1,
                HasBumpLimit = true,
                PosterId = 1
            };

            var firstAnon = new User
            {
                IpAddress = AnonController.GetUserIpAddress().Result,
                Passcode = "adm1n",
                RoleId = 1
            };
            
            var adminRole = new Role
            {
                Name = "admin"
            };

            var moderatorRole = new Role
            {
                Name = "moderator"
            };

            var bannedRole = new Role
            {
                Name = "banned"
            };

            var defaultRole = new Role
            {
                Name = "anon"
            };
            
            var privilegedRole = new Role
            {
                Name = "privileged"
            };

            _dbBoard.Boards.Add(defaultBoard);
            _dbBoard.Threads.Add(defaultThread);
            _dbUser.Users.Add(firstAnon);
            _dbUser.Roles.Add(adminRole);
            _dbUser.Roles.Add(moderatorRole);
            _dbUser.Roles.Add(bannedRole);
            _dbUser.Roles.Add(defaultRole);
            _dbUser.Roles.Add(privilegedRole);

            _dbBoard.SaveChanges();
            _dbUser.SaveChanges();
        }
    }
}