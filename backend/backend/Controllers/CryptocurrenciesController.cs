using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CryptocurrenciesController : ControllerBase
    {
        private readonly CryptoWalletContext _context;

        public CryptocurrenciesController(CryptoWalletContext context)
            => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _context.Cryptocurrencies
                                     .OrderBy(c => c.Name)
                                     .Select(c => new { c.Code, c.Name })
                                     .ToListAsync();
            return Ok(list);
        }
    }
}
