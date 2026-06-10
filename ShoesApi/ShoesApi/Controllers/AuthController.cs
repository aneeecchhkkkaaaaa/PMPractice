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
<<<<<<< HEAD
        private readonly is25_practice_shoesContext _context;
=======
        private readonly Is25AndreevShoesContext _context;
        private object? dto;
>>>>>>> 4d5bc66886331d9f10f31ef3b829f2900897090f

        public AuthController(is25_practice_shoesContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<User>> Autorization(
            [FromBody] LoginDto dto)

        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == dto.Login);

            if (user == null || user.Password != dto.Password)
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
