using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using QuietPlaceWebProject.Models;

namespace QuietPlaceWebProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class BoardController : Controller
    {
        private readonly BoardContext _dbBoard;
        private readonly UserContext _dbUser;
        private readonly IHostEnvironment _environment;
        
        public BoardController(BoardContext dbBoard, UserContext dbUser, IHostEnvironment environment)
        {
            _dbBoard = dbBoard;
            _dbUser = dbUser;
            _environment = environment;
            
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

            List<Board> boards;
            var ipAddress = await AnonController.GetUserIpAddress();
            var user = await _dbUser.Users.Where(localUser => localUser.IpAddress == ipAddress).ToListAsync();

            if (user.Count == 1 && user.First().RoleId <= 2)
                boards = await _dbBoard.Boards.ToListAsync();
            else
                boards = await _dbBoard.Boards.Where(localBoard => !localBoard.IsHidden).ToListAsync();

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
            [Bind("Id, Name, DomainName, Description, MaxCountOfThreads, IsHidden, AccessRoleId")] Board board, 
            IFormFile upload)
        {
            ViewBag.Roles = new SelectList(_dbUser.Roles, "Id", "Name");
            board.Name = board.Name.Trim();

            if (upload is not null)
            {
                var fileName = Path.GetFileName(upload.FileName);

                if (IsImage(fileName))
                {
                    board.ImageUrl = fileName;
                    SaveImage(fileName, upload);
                }
            }
            
            if (!ModelState.IsValid || upload is null) 
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
                return RedirectToAction("NotFoundPage", "Anon");

            var board = await _dbBoard.Boards.FindAsync(boardId);
            ViewBag.Roles = new SelectList(_dbUser.Roles, "Id", "Name");

            return View(board);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            [Bind("Id, Name, DomainName, Description, MaxCountOfThreads, IsHidden, AccessRoleId")] Board board,
            IFormFile upload)
        {
            if (upload is not null)
            {
                var fileName = Path.GetFileName(upload.FileName);

                if (IsImage(fileName))
                {
                    board.ImageUrl = fileName;
                    SaveImage(fileName, upload);
                }
            }
            
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
                    return RedirectToAction("NotFoundPage", "Anon");
                
                throw;
            }

            SetNotificationInfo(0);
            
            return RedirectToAction(nameof(Boards));
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int? boardId)
        {
            if (boardId is null)
                return RedirectToAction("NotFoundPage", "Anon");
            
            if (_dbBoard.Boards.Count() == 1)
                return RedirectToAction(nameof(Boards));
            
            var board = await _dbBoard.Boards.FindAsync(boardId);
            
            return View(board);
        }
        
        [HttpPost]
        public async Task<IActionResult> Remove(int boardId)
        {
            var path = _environment.ContentRootPath + @"\wwwroot\files\";
            var path2 = _environment.ContentRootPath + @"\wwwroot\images\";
            
            try
            {
                var threads = _dbBoard.Threads.Where(thread => thread.BoardId == boardId).ToList();

                foreach (var posts in threads.Select(
                    thread => _dbBoard.Posts.Where(post => post.ThreadId == thread.Id).ToList()))
                {
                    foreach (var post in posts.Where(post => post.MediaUrl is not null))
                        System.IO.File.Delete(path + post.MediaUrl);

                    _dbBoard.Posts.RemoveRange(posts);
                }
                
                _dbBoard.Threads.RemoveRange(threads);
                
                foreach (var thread in threads.Where(thread => thread.MediaUrl is not null))
                    System.IO.File.Delete(path + thread.MediaUrl);

                var board = await _dbBoard.Boards.FindAsync(boardId);
                
                if (board.ImageUrl is not null)
                    System.IO.File.Delete(path2 + board.ImageUrl);
                
                _dbBoard.Boards.Remove(board);
                await _dbBoard.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _dbBoard.Boards.AnyAsync(localBoard => localBoard.Id == boardId))
                    return RedirectToAction("NotFoundPage", "Anon");
                
                throw;
            }
            
            SetNotificationInfo(-1);
            
            return RedirectToAction(nameof(Boards));
        }

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

        private static bool IsImage(string name)
        {
            var extension = Path.GetExtension(name);

            return string.CompareOrdinal(extension, ".png") == 0 ||
                   string.CompareOrdinal(extension, ".jpg") == 0 ||
                   string.CompareOrdinal(extension, ".jpeg") == 0 ||
                   string.CompareOrdinal(extension, ".bmp") == 0;

        }

        private void SaveImage(string name, IFormFile upload)
        {
            var path = _environment.ContentRootPath + @"\wwwroot\images\" + name;
            var image = new Bitmap(upload.OpenReadStream());
            using var fs = new FileStream(path, FileMode.Create);
            image.Save(fs, ImageFormat.Png);
        }
        
        private void InitialDatabase()
        {
            if (_dbBoard.Boards.Any()) 
                return;
            
            var firstBoard = new Board
            {
                Name = "Разработка Имиджборды",
                DomainName = "/dib/",
                Description = "Доска для обсуждения разработки имиджборды",
                ImageUrl = "dev_pic.png",
                MaxCountOfThreads = 20,
                IsHidden = false,
                AccessRoleId = 1
            };

            var firstThread = new Thread
            {
                Name = "Проверка текста",
                BoardId = 1,
                HasBumpLimit = true,
                PosterId = 1
            };

            var firstPost = new Post
            {
                Text = "Это первый пост в треде проверки текста.",
                DateOfCreation = DateTime.Now,
                IsOriginalPoster = true,
                PosterId = 1,
                ThreadId = 1,
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
            
            var privilegedRole = new Role
            {
                Name = "privileged"
            };

            var defaultRole = new Role
            {
                Name = "anon"
            };

            var bannedRole = new Role
            {
                Name = "banned"
            };

            _dbBoard.Boards.Add(firstBoard);
            _dbBoard.Threads.Add(firstThread);
            _dbBoard.Posts.Add(firstPost);
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