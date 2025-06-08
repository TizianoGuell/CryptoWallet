using Microsoft.AspNetCore.Mvc;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletBalancesController : ControllerBase
    {
        private readonly CryptoWalletContext _context;

        public WalletBalancesController(CryptoWalletContext context)
        {
            _context = context;
        }

        // GET api/walletbalances?userId=4
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int userId)
        {
            if (userId <= 0)
                return BadRequest("userId inválido.");

            var balances = await _context.WalletBalances
                .Where(wb => wb.UserId == userId)
                .Select(wb => new {
                    cryptoCode = wb.Cryptocurrency.Code,
                    wb.Balance
                })
                .ToListAsync();

            return Ok(balances);
        }
    }
}
