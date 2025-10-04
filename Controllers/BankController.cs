using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using Microsoft.AspNetCore.Mvc;


namespace FintechStatsPlatform.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BankController(BankService banksService) : ControllerBase
	{
		private readonly BankService _banksService = banksService;

		[HttpGet("bank-configs/{userId}")]
		public IActionResult ListBankConfigs([FromRoute] string userId)
		{
			List<BankConfig> allUserConfigs = _banksService.ListBankConfigs(userId);

			return Ok(allUserConfigs);
		}

		[HttpPost("connect/other-bank/{code}")]
		public async Task<IActionResult> ConnectOtherBank(string userId, [FromRoute] string code)
		{
			try
			{
				userId = User.FindFirst("sub")?.Value ?? "";

				var token = _banksService.GetTinkAccessToken(code, scope: "accounts:read");

				HttpContext.Response.Cookies.Append("other_bank_token",token,new CookieOptions {
					HttpOnly = true,
					Secure = true,
					SameSite = SameSiteMode.Strict,
					Expires = DateTimeOffset.UtcNow.AddHours(1)
				});

				await _banksService.ConnectOtherBankAsync(userId, token);

				return Ok(new { message = "Акаунти користувача успішно підключені" });
			}
			catch (HttpRequestException ex)
			{
				// Помилка при зверненні до Tink API
				return StatusCode(502, new { error = "Помилка при з'єднанні з банком", details = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { error = "Внутрішня помилка сервера", details = ex.Message });
			}
		}


		// GET: api/Banks
		[HttpGet]
		public async Task<ActionResult<IEnumerable<BankConfig>>> GetBanks()
		{
			var banks = await _banksService.GetBanksAsync();
			return Ok(banks);
		}

		// GET: api/Banks/5
		[HttpGet("{id}")]
		public async Task<ActionResult<BankConfig>> GetBank(string id)
		{
			var bank = await _banksService.GetBankByIdAsync(id);
			if (bank == null) return NotFound();
			return Ok(bank);
		}

		// PUT: api/Banks/5
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[HttpPut("{id}")]
		public async Task<IActionResult> PutBank(string id, BankConfig bank)
		{
			var success = await _banksService.UpdateBankAsync(id, bank);
			if (!success) return NotFound();
			return NoContent();
		}

		// POST: api/Banks
		// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
		[HttpPost]
		public async Task<ActionResult<BankConfig>> PostBank(BankConfig bank)
		{
			var createdBank = await _banksService.AddBankAsync(bank);
			return CreatedAtAction(nameof(GetBank), new { id = createdBank.Id }, createdBank);
		}

		// DELETE: api/Banks/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteBank(string id)
		{
			var success = await _banksService.DeleteBankAsync(id);
			if (!success) return NotFound();
			return NoContent();
		}
	}
}
