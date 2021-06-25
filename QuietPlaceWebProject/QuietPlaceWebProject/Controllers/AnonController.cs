using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuietPlaceWebProject.Models;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace QuietPlaceWebProject.Controllers
{
    public class AnonController : Controller
    {
        private readonly UserContext _dbUser;

        public AnonController(UserContext dbUser) => _dbUser = dbUser;

        [HttpGet]
        public IActionResult Anons()
        {
            var users = _dbUser.Users.ToList();

            ViewBag.Passcodes = _dbUser.Passcodes.ToList();
            
            ViewBag.Roles = _dbUser.Roles.ToDictionary(role 
                => role.Id, role => role.Name);

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int threadId)
        {
            var ipAddress = await GetUserIpAddress();

            var user = new User
            {
                IpAddress = ipAddress,
                IsBanned = false,
                PasscodeId = -1
            };

            _dbUser.Users.Add(user);
            await _dbUser.SaveChangesAsync();

            return RedirectToAction("Create", "Post", new { threadId });
        }

        public async Task<IActionResult> BanUnBan(int? userId, bool isBan)
        {
            if (userId is null)
                return NotFound();

            var user = await _dbUser.Users.FindAsync(userId);
            user.IsBanned = isBan;

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
    }
}