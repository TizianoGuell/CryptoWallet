using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using backend.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Linq;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly CryptoWalletContext _context;
        private readonly HttpClient _httpClient;
        private const string Exchange = "satoshitango";

        public TransactionsController(CryptoWalletContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        // GET api/transactions?userId=1
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int userId)
        {
            if (userId <= 0)
                return BadRequest("userId inválido.");

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Datetime)
                .Select(t => new {
                    id = t.Id,
                    action = t.Action,
                    crypto_code = t.Cryptocurrency.Code,
                    crypto_amount = t.CryptoAmount,
                    money = t.Money,
                    datetime = t.Datetime
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // GET api/transactions/price?cryptoCode=usdc
        [HttpGet("price")]
        public async Task<IActionResult> GetPrice([FromQuery] string cryptoCode)
        {
            if (string.IsNullOrWhiteSpace(cryptoCode))
                return BadRequest("cryptoCode válido es obligatorio.");

            // Mapear nombres largos a códigos de API de CriptoYa
            var apiCode = cryptoCode.ToLower() switch
            {
                "bitcoin" => "btc",
                "ethereum" => "eth",
                "usdc" => "usdc",
                "litecoin" => "ltc",
                "xrp" => "xrp",
                _ => cryptoCode.ToLower()
            };

            var url = $"https://criptoya.com/api/{Exchange}/{apiCode}/ars";
            var resp = await _httpClient.GetAsync(url);
            var txt = await resp.Content.ReadAsStringAsync();

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(txt);
            }
            catch (JsonException)
            {
                var snippet = txt.Length > 200 ? txt.Substring(0, 200) + "..." : txt;
                return StatusCode(502, $"Error parsing JSON desde CriptoYa: {snippet}");
            }

            if (!doc.RootElement.TryGetProperty("ask", out var askElem))
                return StatusCode(502, "No se encontró el campo 'ask' en la respuesta JSON.");

            var pricePerUnit = askElem.GetDecimal();
            return Ok(new { pricePerUnit });
        }

        // POST api/transactions
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransactionRequest model)
        {
            // 1) Validaciones básicas
            if (model.UserId <= 0 ||
                string.IsNullOrWhiteSpace(model.CryptoCode) ||
                string.IsNullOrWhiteSpace(model.Action) ||
                model.CryptoAmount <= 0)
            {
                return BadRequest("Faltan datos obligatorios o son inválidos.");
            }

            var action = model.Action.ToLower();
            if (action != "purchase" && action != "sale")
                return BadRequest("La acción debe ser 'purchase' o 'sale'.");

            // 2) Verificar cripto existe
            var crypto = await _context.Cryptocurrencies
                .FirstOrDefaultAsync(c => c.Code.ToLower() == model.CryptoCode.ToLower());
            if (crypto == null)
                return NotFound("Criptomoneda no válida.");

            // 3) Si es venta, validar saldo suficiente
            var balance = await _context.WalletBalances
                .FirstOrDefaultAsync(wb => wb.UserId == model.UserId && wb.CryptoId == crypto.Id);
            if (action == "sale")
            {
                if (balance == null || balance.Balance < model.CryptoAmount)
                    return BadRequest("Saldo insuficiente para realizar la venta.");
            }

            // 4) Obtener precio unitario y calcular total
            var apiCodeMap = model.CryptoCode.ToLower() switch
            {
                "bitcoin" => "btc",
                "ethereum" => "eth",
                "usdc" => "usdc",
                "litecoin" => "ltc",
                "xrp" => "xrp",
                _ => model.CryptoCode.ToLower()
            };
            var priceUrl = $"https://criptoya.com/api/{Exchange}/{apiCodeMap}/ars";
            var priceResp = await _httpClient.GetAsync(priceUrl);
            var priceTxt = await priceResp.Content.ReadAsStringAsync();

            JsonDocument priceDoc;
            try
            {
                priceDoc = JsonDocument.Parse(priceTxt);
            }
            catch (JsonException)
            {
                var snippet = priceTxt.Length > 200 ? priceTxt.Substring(0, 200) + "..." : priceTxt;
                return StatusCode(502, $"Error parsing JSON desde CriptoYa en Create: {snippet}");
            }

            if (!priceDoc.RootElement.TryGetProperty("ask", out var askElem))
                return StatusCode(502, "No se encontró 'ask' en la respuesta JSON.");

            var pricePerUnit = askElem.GetDecimal();
            var totalAsk = pricePerUnit * model.CryptoAmount;

            var transaction = new Transaction
            {
                UserId = model.UserId,
                CryptoId = crypto.Id,
                Action = action,
                CryptoAmount = model.CryptoAmount,
                Money = totalAsk,
                Datetime = model.Datetime
            };
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            if (balance != null)
            {
                balance.Balance = action == "purchase"
                    ? balance.Balance + model.CryptoAmount
                    : balance.Balance - model.CryptoAmount;
                balance.LastUpdated = DateTime.Now;
            }
            else if (action == "purchase")
            {
                _context.WalletBalances.Add(new WalletBalance
                {
                    UserId = model.UserId,
                    CryptoId = crypto.Id,
                    Balance = model.CryptoAmount,
                    LastUpdated = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Transacción registrada correctamente.",
                transaction
            });
        }

        // GET api/transactions/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var t = await _context.Transactions
                .Where(x => x.Id == id)
                .Select(x => new {
                    id = x.Id,
                    crypto_code = x.Cryptocurrency.Code,
                    crypto_amount = x.CryptoAmount,
                    money = x.Money,
                    action = x.Action,
                    datetime = x.Datetime
                })
                .FirstOrDefaultAsync();

            if (t == null)
                return NotFound($"Transacción {id} no encontrada.");

            return Ok(t);
        }

        // PATCH api/transactions/{id}
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] JsonElement changes)
        {
            var tx = await _context.Transactions.FindAsync(id);
            if (tx == null)
                return NotFound($"Transacción {id} no encontrada.");

            // Aplicar sólo los campos que vengan en el JSON
            if (changes.TryGetProperty("crypto_amount", out var ca))
                tx.CryptoAmount = ca.GetDecimal();
            if (changes.TryGetProperty("money", out var m))
                tx.Money = m.GetDecimal();
            if (changes.TryGetProperty("action", out var a))
                tx.Action = a.GetString()!;
            if (changes.TryGetProperty("datetime", out var dt))
                tx.Datetime = dt.GetDateTime();

            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Transacción actualizada.",
                transaction = new
                {
                    id = tx.Id,
                    crypto_code = (await _context.Cryptocurrencies.FindAsync(tx.CryptoId))!.Code,
                    crypto_amount = tx.CryptoAmount,
                    money = tx.Money,
                    action = tx.Action,
                    datetime = tx.Datetime
                }
            });
        }

        // DELETE api/transactions/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tx = await _context.Transactions.FindAsync(id);
            if (tx == null)
                return NotFound($"Transacción {id} no encontrada.");

            _context.Transactions.Remove(tx);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
