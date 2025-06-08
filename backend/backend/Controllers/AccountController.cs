using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using backend.Models.ViewModels;
using BCrypt.Net;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly CryptoWalletContext _context;

        public AccountController(CryptoWalletContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
                return BadRequest("Todos los campos son obligatorios.");

            if (model.Password != model.ConfirmPassword)
                return BadRequest("Las contraseñas no coinciden.");

            if (_context.Users.Any(u => u.Username == model.Username || u.Email == model.Email))
                return Conflict("Nombre de usuario o email ya existen.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var newUser = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = hashedPassword
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("Usuario registrado correctamente.");
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Usuario y contraseña requeridos.");

            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                return Unauthorized("Credenciales inválidas.");

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email
            });
        }


    }
}
