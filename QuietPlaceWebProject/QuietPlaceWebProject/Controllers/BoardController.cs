using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
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

        private void InitialDatabase()
        {
            if (!_dbBoard.Boards.Any()) 
                return;
            
            var defaultBoard = new Board()
            {
                Id = 0,
                Name = "Разработка Имиджборды",
                DomainName = "dib",
                MaxCountOfThreads = 20,
                IsHidden = false,
                AccessRoleId = 0
            };

            var defaultPasscode = new Passcode()
            {
                Id = 0,
                Code = "adm1n",
                RoleId = 0
            };

            var defaultRole = new Role()
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

        public IActionResult Boards()
        {
            var boards = _dbBoard.Boards.ToList();
            
            return View(boards);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}