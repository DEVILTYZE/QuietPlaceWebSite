using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuietPlaceWebProject.Helpers;
using QuietPlaceWebProject.Interfaces;
using QuietPlaceWebProject.Models;

namespace QuietPlaceWebProject.Controllers
{
    [Authorize(Roles = "admin, moderator")]
    public class AnonController : Controller
    {
        private readonly UserContext _dbUser;

        public AnonController(UserContext dbUser) => _dbUser = dbUser;

        [HttpGet]
        public IActionResult Anons(int? roleId)
        {
            var users = roleId is null 
                ? _dbUser.Users.ToList()
                : _dbUser.Users.Where(localUser => localUser.RoleId == roleId).ToList();

            ViewBag.PasscodeMessage = TempData["PasscodeMessage"];
            ViewBag.Roles = new SelectList(_dbUser.Roles, "Id", "Name");

            if (users.Count == 0)
                ViewBag.Message = "Анонов с такой ролью нет";

            return View(users);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Create(int? threadId)
        {
            var passcode = TempData["Passcode"] as string ?? "NULL";
            var ipAddress = await GetUserIpAddress();

            if (await _dbUser.Users.AnyAsync(localUser => string.Compare(localUser.IpAddress, ipAddress) == 0))
            {
                var currentUser = await _dbUser.Users.FirstAsync(localUser
                    => string.Compare(localUser.IpAddress, ipAddress) == 0);
                    
                if (string.Compare(passcode, "NULL") != 0 
                    && string.Compare(currentUser.Passcode, passcode) == 0)
                    return RedirectToAction(nameof(SetPasscode), new {passcode});
                
                return threadId is not null 
                    ? RedirectToAction("Create", "Post", new {threadId}) 
                    : RedirectToAction("Boards", "Board");
            }
            
            var user = new User
            {
                IpAddress = ipAddress,
                Passcode = passcode,
                RoleId = 4
            };

            _dbUser.Users.Add(user);
            await _dbUser.SaveChangesAsync();

            if (string.Compare(passcode, "NULL") != 0)
                return RedirectToAction(nameof(SetPasscode), new {passcode});
                
            return threadId is not null 
                ? RedirectToAction("Create", "Post", new {threadId}) 
                : RedirectToAction("Boards", "Board");
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int? userId)
        {
            if (userId is null)
                return RedirectToAction(nameof(NotFoundPage));

            _dbUser.Users.Remove(await _dbUser.Users.FindAsync(userId));
            await _dbUser.SaveChangesAsync();
            
            return RedirectToAction(nameof(Anons));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> SetPasscode(string passcode)
        {
            var ipAddress = await GetUserIpAddress();
            User user, ipUser;

            try
            {
                ipUser = await _dbUser.Users.Where(localUser 
                    => string.Compare(localUser.IpAddress, ipAddress) == 0).FirstAsync();

                user = await _dbUser.Users.Where(localUser
                    => string.Compare(passcode, localUser.Passcode) == 0).FirstAsync();
                
                if (ipUser.Id == 1 && user.Id != 1)
                    return RedirectToAction("Boards", "Board");
            }
            catch
            {
                TempData["Passcode"] = passcode;
                
                return RedirectToAction(nameof(Create));
            }
            
            
            if (string.IsNullOrEmpty(passcode) || string.IsNullOrWhiteSpace(passcode))
                return RedirectToAction("Boards", "Board");

            if (string.Compare(ipAddress, user.IpAddress) != 0)
            {
                _dbUser.Users.Remove(ipUser);
                user.IpAddress = ipAddress;
                _dbUser.Entry(user).State = EntityState.Modified;
                await _dbUser.SaveChangesAsync();
            }

            await Authenticate(user);
            
            return RedirectToAction("Boards", "Board");
        }

        [HttpGet]
        public async Task<IActionResult> GeneratePasscode(int? userId, int? roleId)
        {
            if (userId is null && roleId is null)
                return RedirectToAction(nameof(Anons));
            
            const int lengthPasscode = 6;
            var symbols = TextHelper.Symbols;
            var passcodeWord = new char[lengthPasscode];
            var random = new Random();
            var passcodes = _dbUser.Users.ToList().Select(localUser => localUser.Passcode).ToList();

            do
            {
                for (var i = 0; i < lengthPasscode; ++i)
                    passcodeWord[i] = symbols[random.Next(0, symbols.Length - 1)];
            } 
            while (passcodes.Contains(new string(passcodeWord)));

            User user;

            if (userId is not null)
            {
                user = await _dbUser.Users.FindAsync((int) userId);
                user.Passcode = new string(passcodeWord);
                _dbUser.Entry(user).State = EntityState.Modified;
            }
            else
            {
                user = new User
                {
                    IpAddress = "Passcode", 
                    RoleId = (int) roleId, 
                    Passcode = new string(passcodeWord)
                };

                TempData["PasscodeMessage"] = "Сгенерирован пасскод " + user.Passcode + " для роли " 
                                          + (await _dbUser.Roles.FindAsync(user.RoleId)).Name;
                
                await _dbUser.AddAsync(user);
            }
            
            await _dbUser.SaveChangesAsync();

            return RedirectToAction(nameof(Anons));
        }
        
        [HttpGet]
        public async Task<IActionResult> Ban(int? userId, bool? action)
        {
            if (userId is null)
                return RedirectToAction(nameof(NotFoundPage));

            var user = await _dbUser.Users.FindAsync(userId);
            
            if (action is not null && action.Value)
                user.RoleId = 3;
            else
            {
                _dbUser.Users.Remove(user);
                await _dbUser.SaveChangesAsync();
                return RedirectToAction(nameof(Anons));
            }

            try
            {
                _dbUser.Entry(user).State = EntityState.Modified;
                await _dbUser.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _dbUser.Users.AnyAsync(localUser => localUser.Id == user.Id))
                    return RedirectToAction(nameof(NotFoundPage));
                
                throw;
            }

            return RedirectToAction(nameof(Anons));
        }
        
        [AllowAnonymous]
        [HttpGet]
        public IActionResult NotFoundPage() => View();

        public static async Task<string> GetUserIpAddress()
        {
            var host = Dns.GetHostName();
            var ip = (await Dns.GetHostEntryAsync(host)).AddressList[0];
            var ipAddress = ip.ToString();

            return ipAddress;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> IsBanned(int? boardId, int? threadId)
        {
            var ipAddress = await GetUserIpAddress();
            var user = await _dbUser.Users.Where(localUser => localUser.IpAddress == ipAddress).FirstAsync();

            if (user.RoleId == 5) 
                return RedirectToAction(nameof(NotFoundPage));
            
            TempData["IsBanned"] = false;
                
            return boardId is not null
                ? RedirectToAction("Create", "Thread", new {boardId})
                : RedirectToAction("Create", "Post", new {threadId});

        }

        private async Task Authenticate(IUser user)
        {
            var role = await _dbUser.Roles.FindAsync(user.RoleId);
            var claims = new List<Claim>
            {
                new(ClaimsIdentity.DefaultNameClaimType, user.Passcode),
                new(ClaimsIdentity.DefaultRoleClaimType, role.Name)
            };

            var id = new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
    }
}