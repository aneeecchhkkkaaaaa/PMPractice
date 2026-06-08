using Microsoft.AspNetCore.Mvc;
using ShoesApi.Models;
using ShoesApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace ShoesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly Is25AndreevShoesContext _context;

        public AuthController(Is25AndreevShoesContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<User>> Autorization(
            [FromQuery] string login,
            [FromQuery] string password
            )
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);

            if (user == null || user.Password != password)
            {
                return NotFound();
            }
            else
            {
                var response = new User
                {
                    UserId = user.UserId,
                    RoleId = user.RoleId,
                    LastName = user.LastName,
                    FirstName = user.FirstName,
                    Patronymic = user.Patronymic,
                    Login = user.Login,
                    Password = user.Password,
                    Orders = user.Orders,
                    Role = user.Role
                };
                return Ok(response);
            }
        }
    }
}
