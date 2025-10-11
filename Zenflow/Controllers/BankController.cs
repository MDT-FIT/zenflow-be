using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using FintechStatsPlatform.Exceptions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using FintechStatsPlatform.Helpers;


namespace FintechStatsPlatform.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BankController(BankService banksService, AnalyticService analyticService) : ControllerBase
	{
		private readonly BankService _banksService = banksService;
        private readonly AnalyticService _analyticService = analyticService;

        [HttpGet("bank-configs/{userId}")]
		public IActionResult ListBankConfigs([FromRoute] string userId)
		{
            try
            {
                List<BankConfig> allUserConfigs = _banksService.ListBankConfigs(userId);
                return Ok(allUserConfigs);
            }
            catch (ExceptionTypes.NotFoundException ex) 
            { 
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Untrackable exception occured while attempt of getting user's {userId} list of BankConfigs:\n{ex.ToString()}");
                return BadRequest("Something went wrong");
            }
			
		}

        [HttpPost("transactions")]
        public async Task<ActionResult> ListTransactions([FromBody] TransactionFilter filter)
        {
            if (filter == null || filter.UserId is null) return BadRequest();

            string? token = HttpContext.Request.Cookies[_tinkJwtTokenKey];

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is invalid or expired");
            }

            try
            {
                var transactions = await _banksService.ListTransactionsAsync(filter, token);
                return Ok(transactions);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("stats/expenses")]
        public async Task<ActionResult> GetExpensesStats([FromBody] StatsFilter filter)
        {
            return await StatsEndPointBase(_analyticService.GetExpensesAsync, filter, "expenses");
        }

        [HttpPost("stats/income")]
        public async Task<ActionResult> GetIncomeStats([FromBody] StatsFilter filter)
        {
            return await StatsEndPointBase(_analyticService.GetIncomeAsync, filter, "income");
        }

        private async Task<ActionResult> StatsEndPointBase(Func<StatsFilter, string, Task<Stats>> getOperation, StatsFilter filter, string typeName)
        {
            if (filter == null || filter.UserId is null) return BadRequest();

            string? token = HttpContext.Request.Cookies[_tinkJwtTokenKey];

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is invalid or expired");
            }

            try
            {
                var stats = await getOperation(filter, token);
                return Ok(stats);
            }
            catch (ExceptionTypes.JsonParsingException jsonEx)
            {
                return StatusCode(500, jsonEx.Message);
            }
            catch (ExceptionTypes.ExternalApiException apiEx)
            {
                return StatusCode(500, apiEx.Message);
            }
            catch (HttpRequestException httpEx)
            {
                return StatusCode(502, new { message = $"Tink API request failed: {httpEx.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to get {typeName}: {ex.Message}" });
            }
        }

        [HttpPost("stats/top-card")]
        public async Task<ActionResult> GetTopCard([FromBody] StatsFilter filter)
        {
            if (filter == null || filter.UserId is null) return BadRequest();

            string? token = HttpContext.Request.Cookies[_tinkJwtTokenKey];

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is invalid or expired");
            }

            try
            {
                var card = await _analyticService.GetMostUsedCardAsync(filter, token);
                return Ok(card);
            }
            catch (ExceptionTypes.JsonParsingException jsonEx)
            {
                return StatusCode(500, jsonEx.Message);
            }
            catch (ExceptionTypes.ExternalApiException apiEx)
            {
                return StatusCode(500, apiEx.Message);
            }
            catch (HttpRequestException httpEx)
            {
                return StatusCode(502, new { message = $"Tink API request failed: {httpEx.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to get expenses: {ex.Message}" });
            }
        }
        
        [HttpGet("balances")]
        public async Task<IActionResult> GetBalances([FromQuery] List<string> accountIds, [FromQuery] string userId)

        {
            if (accountIds.Equals(null) || !accountIds.Any())
                return BadRequest(new { message = "At least one accountId is required." });

            if (userId == null)
                return BadRequest(new { message = "User is required"});

            string token = HttpContext.Request.Cookies[_tinkJwtTokenKey] ?? "";

            try
            {
                if (string.IsNullOrEmpty(token))
                    return Unauthorized(new { message = "Failed to obtain user access token." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to get access token: {ex.Message}" });
            }

            try
            {
                var balances = await _banksService.GetBalancesAsync(accountIds, token, userId);
                return Ok(balances);
            }
            catch (ExceptionTypes.JsonParsingException jsonEx) 
            {
                return StatusCode(500, jsonEx.Message);
            }
            catch (ExceptionTypes.ExternalApiException apiEx) 
            {
                return StatusCode(500, apiEx.Message);
            }
            catch (HttpRequestException httpEx)
            {
                return StatusCode(502, new { message = $"Tink API request failed: {httpEx.Message}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to get balances: {ex.Message}" });
            }
        }



        [HttpPost("connect/other-bank/{code}")]
        public async Task<IActionResult> ConnectOtherBank(string userId, [FromRoute] string code)
        {
            try
            {
                var token = _banksService.GetTinkAccessToken(code);

				HttpContext.Response.Cookies.Append(EnvConfig.TinkJwt,token, CookieConfig.Default);

                await _banksService.ConnectOtherBankAsync(userId, token).ConfigureAwait(false);

                return Ok(new { message = "Акаунти користувача успішно підключені" });
            }
            catch (HttpRequestException ex)
            {
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
            var banks = await _banksService.GetBanksAsync().ConfigureAwait(false);
            return Ok(banks);
        }

        // GET: api/Banks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BankConfig>> GetBank(string id)
        {
            var bank = await _banksService.GetBankByIdAsync(id).ConfigureAwait(false);
            if (bank == null) return NotFound();
            return Ok(bank);
        }

        // PUT: api/Banks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBank(string id, BankConfig bank)
        {
            var success = await _banksService.UpdateBankAsync(id, bank).ConfigureAwait(false);
            if (!success) return NotFound();
            return NoContent();
        }

        // POST: api/Banks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BankConfig>> PostBank(BankConfig bank)
        {
            var createdBank = await _banksService.AddBankAsync(bank).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetBank), new { id = createdBank.Id }, createdBank);
        }

        // DELETE: api/Banks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBank(string id)
        {
            var success = await _banksService.DeleteBankAsync(id).ConfigureAwait(false);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
