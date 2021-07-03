using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuietPlaceWebProject.Models;

namespace QuietPlaceWebProject.Controllers
{
    [Authorize(Roles = "admin")]
    public class RoleController : Controller
    {
        private readonly UserContext _dbUser;

        public RoleController(UserContext dbUser) => _dbUser = dbUser;
        
        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            var roles = await _dbUser.Roles.ToListAsync();
            
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            var role = new Role
            {
                Name = roleName
            };
            
            if (roleName.Length is >= 3 and <= 30)
                return RedirectToAction(nameof(Roles));

            await _dbUser.Roles.AddAsync(role);
            await _dbUser.SaveChangesAsync();

            return RedirectToAction(nameof(Roles));
        }

        [HttpGet]
        public async Task<IActionResult> Remove(int? roleId)
        {
            if (roleId is null)
                return NotFound();

            var role = await _dbUser.Roles.FindAsync(roleId);

            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int roleId)
        {
            try
            {
                var role = await _dbUser.Roles.FindAsync(roleId);
                _dbUser.Roles.Remove(role);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _dbUser.Roles.AnyAsync(localRole => localRole.Id == roleId))
                    return NotFound();
                
                throw;
            }

            return RedirectToAction(nameof(Roles));
        }
    }
}