using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuietPlaceWebProject.Models;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuietPlaceWebProject.Helpers;
using QuietPlaceWebProject.Interfaces;

namespace QuietPlaceWebProject.Controllers
{
    [Authorize(Roles = "admin, moderator")]
    public class AnonController : Controller
    {
        private readonly UserContext _dbUser;

        public AnonController(UserContext dbUser) => _dbUser = dbUser;

        [HttpGet]
        public IActionResult Anons()
        {
            var users = _dbUser.Users.ToList();

            ViewBag.Roles = _dbUser.Roles.ToDictionary(role 
                => role.Id, role => role.Name);

            return View(users);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Create(int threadId)
        {
            var passcode = TempData["Passcode"] as string ?? "NULL";
            var ipAddress = await GetUserIpAddress();

            var user = new User
            {
                IpAddress = ipAddress,
                Passcode = "NULL",
                RoleId = 4
            };

            _dbUser.Users.Add(user);
            await _dbUser.SaveChangesAsync();

            return string.Compare(passcode, "NULL") == 0 
                ? RedirectToAction("Create", "Post", new { threadId }) 
                : RedirectToAction(nameof(SetPasscode), new { passcode });
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int? userId)
        {
            if (userId is null)
                return NotFound();

            _dbUser.Users.Remove(await _dbUser.Users.FindAsync(userId));
            await _dbUser.SaveChangesAsync();
            
            return RedirectToAction(nameof(Anons));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> SetPasscode(string passcode)
        {
            var ipAddress = await GetUserIpAddress();
            User user;

            try
            {
                user = await _dbUser.Users.Where(localUser
                    => string.Compare(passcode, localUser.Passcode) == 0).FirstAsync();
            }
            catch
            {
                TempData["Passcode"] = passcode;
                
                return RedirectToAction(nameof(Create));
            }
            
            
            if (string.IsNullOrEmpty(passcode) || string.IsNullOrWhiteSpace(passcode))
                return RedirectToAction("Boards", "Board");

            if (string.Compare(ipAddress, user.IpAddress) != 0)
                user.IpAddress = ipAddress;

            await Authenticate(user);
            
            return RedirectToAction("Boards", "Board");
        }

        [HttpGet]
        public async Task<IActionResult> GeneratePasscode(int? userId, int? roleId)
        {
            if (userId is null || roleId is null)
                return NotFound();

            const int lengthPasscode = 12;
            var symbols = TextHelper.Symbols;
            var passcodeWord = new char[lengthPasscode];
            var random = new Random();

            for (var i = 0; i < lengthPasscode; ++i)
                passcodeWord[i] = symbols[random.Next(0, symbols.Length - 1)];

            var user = await _dbUser.Users.FindAsync((int) userId);
            user.Passcode = new string(passcodeWord);
            _dbUser.Entry(user).State = EntityState.Modified;
            await _dbUser.SaveChangesAsync();

            return RedirectToAction(nameof(Anons));
        }
        
        [HttpGet]
        public async Task<IActionResult> Ban(int? userId, bool isBan)
        {
            if (userId is null)
                return NotFound();

            var user = await _dbUser.Users.FindAsync(userId);
            user.RoleId = 3;

            try
            {
                _dbUser.Entry(user).State = EntityState.Modified;
                await _dbUser.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _dbUser.Users.AnyAsync(localUser => localUser.Id == user.Id))
                    return NotFound();
                
                throw;
            }

            return RedirectToAction(nameof(Anons));
        }

        public static async Task<string> GetUserIpAddress()
        {
            var host = Dns.GetHostName();
            var ip = (await Dns.GetHostEntryAsync(host)).AddressList[0];
            var ipAddress = ip.ToString();

            return ipAddress;
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