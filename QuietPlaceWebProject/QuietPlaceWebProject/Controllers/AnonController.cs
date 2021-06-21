using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuietPlaceWebProject.Models;
using System.Net;

namespace QuietPlaceWebProject.Controllers
{
    public class AnonController : Controller
    {
        private readonly UserContext _dbUser;

        public AnonController(UserContext dbUser) => _dbUser = dbUser;

        [HttpGet]
        public async Task<IActionResult> Create(int threadId)
        {
            var host = Dns.GetHostName();
            var ip = (await Dns.GetHostEntryAsync(host)).AddressList[0];
            var ipAddress = ip.ToString();

            var user = new User()
            {
                IpAddress = ipAddress,
                IsBanned = false,
                PasscodeId = -1
            };

            _dbUser.Users.Add(user);
            await _dbUser.SaveChangesAsync();

            TempData["PosterId"] = user.Id;

            return RedirectToAction("Create", "Post", new { threadId });
        }
    }
}