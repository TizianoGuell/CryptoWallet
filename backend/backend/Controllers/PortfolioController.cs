using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using backend.Data;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly CryptoWalletContext _context;
        private readonly HttpClient _httpClient;
        private const string Exchange = "satoshitango";

        public PortfolioController(CryptoWalletContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        // GET api/portfolio?userId=1
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int userId)
        {
            if (userId <= 0)
                return BadRequest("userId inválido.");

            // Traer balances positivos junto con su info de cripto
            var balances = await _context.WalletBalances
                .Include(w => w.Cryptocurrency)
                .Where(w => w.UserId == userId && w.Balance > 0)
                .ToListAsync();

            var holdings = new List<object>();
            decimal total = 0m;

            foreach (var wb in balances)
            {
                // Mapear a código de CriptoYa
                var apiCode = wb.Cryptocurrency.Code.ToLower() switch
                {
                    "bitcoin" => "btc",
                    "ethereum" => "eth",
                    "usdc" => "usdc",
                    "litecoin" => "ltc",
                    "xrp" => "xrp",
                    _ => wb.Cryptocurrency.Code.ToLower()
                };

                var url = $"https://criptoya.com/api/{Exchange}/{apiCode}/ars";
                var resp = await _httpClient.GetAsync(url);
                if (!resp.IsSuccessStatusCode)
                    return StatusCode(502, $"Error al obtener precio de {wb.Cryptocurrency.Code}.");

                var json = await resp.Content.ReadAsStringAsync();
                JsonDocument doc;
                try
                {
                    doc = JsonDocument.Parse(json);
                }
                catch (JsonException)
                {
                    return StatusCode(502, $"Error parsing JSON de {wb.Cryptocurrency.Code}.");
                }

                if (!doc.RootElement.TryGetProperty("ask", out var askElem))
                    return StatusCode(502, $"No se encontró 'ask' para {wb.Cryptocurrency.Code}.");

                var pricePerUnit = askElem.GetDecimal();
                var value = pricePerUnit * wb.Balance;
                total += value;

                holdings.Add(new
                {
                    crypto_code = wb.Cryptocurrency.Code,
                    crypto_name = wb.Cryptocurrency.Name,
                    crypto_amount = wb.Balance,
                    pricePerUnit,
                    money = value
                });
            }

            return Ok(new
            {
                holdings,
                total
            });
        }
    }
}
