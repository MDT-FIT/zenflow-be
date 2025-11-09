using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zenflow.Enumirators;
using Zenflow.Filters;
using Zenflow.Helpers;
using Zenflow.Models;
using Zenflow.Services;

namespace Zenflow.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BankController(BankService banksService, AnalyticService analyticService)
        : ControllerBase
    {
        private readonly BankService _banksService = banksService;
        private readonly AnalyticService _analyticService = analyticService;

        [HttpGet("bank-configs/{userId}")]
        public async Task<IActionResult> ListBankConfigs([FromRoute] string userId)
        {
            try
            {
                string clientToken = HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] ?? "";

                if (HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] == null)
                {
                    (string clientNewToken, double expires) = await _banksService.GetTinkClientToken();
                    clientToken = clientNewToken;
                    HttpContext.Response.Cookies.Append(EnvConfig.TinkClientJwt, clientNewToken, CookieConfig.Default(expires));
                }


                string link = await _banksService.GetTinkLink(userId, clientToken);

                List<BankConfig> allUserConfigs = _banksService.ListBankConfigs(userId);


                BankConfig? otherBankConfig = allUserConfigs.FirstOrDefault(config => config.Name == BankName.OTHER);

                if (otherBankConfig != null)
                {
                    otherBankConfig.ApiLink = link;
                }

                return Ok(allUserConfigs);
            }
            catch (ExceptionTypes.NotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Untrackable exception occured while attempt of getting user's {userId} list of BankConfigs:\n{ex}"
                );
                return BadRequest("Something went wrong");
            }
        }

        [HttpPost("transactions")]
        public async Task<ActionResult> ListTransactions([FromBody] TransactionFilter filter)
        {
            if (filter == null || filter.UserId is null)
                return BadRequest();

            string clientToken = HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] ?? "";
            string token = HttpContext.Request.Cookies[EnvConfig.TinkJwt] ?? "";

            if (HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] == null)
            {
                (string clientNewToken, double expires) = await _banksService.GetTinkClientToken();
                clientToken = clientNewToken;
                HttpContext.Response.Cookies.Append(EnvConfig.TinkClientJwt, clientNewToken, CookieConfig.Default(expires));
            }



            if (HttpContext.Request.Cookies[EnvConfig.TinkJwt] == null)
            {
                (string newToken, double expires) = await _banksService.GetTinkUserAccessToken(filter.UserId, clientToken);
                token = newToken;
                HttpContext.Response.Cookies.Append(EnvConfig.TinkJwt, newToken, CookieConfig.Default(expires));
            }



            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is invalid or expired");
            }

            try
            {
                List<DTO.TinkTransaction> transactions = await _banksService
                    .ListTransactionsAsync(filter, token)
                    .ConfigureAwait(false);
                return Ok(transactions);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("stats/expenses")]
        public async Task<ActionResult> GetExpensesStats([FromBody] StatsFilter filter)
        {
            return await StatsEndPointBase(_analyticService.GetExpensesAsync, filter, "expenses")
                .ConfigureAwait(false);
        }

        [HttpPost("stats/income")]
        public async Task<ActionResult> GetIncomeStats([FromBody] StatsFilter filter)
        {
            return await StatsEndPointBase(_analyticService.GetIncomeAsync, filter, "income")
                .ConfigureAwait(false);
        }

        private async Task<ActionResult> StatsEndPointBase(
            Func<StatsFilter, string, Task<Stats>> getOperation,
            StatsFilter filter,
            string typeName
        )
        {
            if (filter == null || filter.UserId is null)
                return BadRequest();

            string clientToken = HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] ?? "";
            string token = HttpContext.Request.Cookies[EnvConfig.TinkJwt] ?? "";

            if (HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] == null)
            {
                (string clientNewToken, double expires) = await _banksService.GetTinkClientToken();
                clientToken = clientNewToken;
                HttpContext.Response.Cookies.Append(EnvConfig.TinkClientJwt, clientNewToken, CookieConfig.Default(expires));
            }

            if (HttpContext.Request.Cookies[EnvConfig.TinkJwt] == null)
            {
                (string newToken, double expires) = await _banksService.GetTinkUserAccessToken(filter.UserId, clientToken);
                token = newToken;
                HttpContext.Response.Cookies.Append(EnvConfig.TinkJwt, newToken, CookieConfig.Default(expires));
            }



            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is invalid or expired");
            }

            try
            {
                Stats stats = await getOperation(filter, token).ConfigureAwait(false);
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
                return StatusCode(
                    502,
                    new { message = $"Tink API request failed: {httpEx.Message}" }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to get {typeName}: {ex.Message}" });
            }
        }

        [HttpPost("stats/top-card")]
        public async Task<ActionResult> GetTopCard([FromBody] StatsFilter filter)
        {
            if (filter == null || filter.UserId is null)
                return BadRequest();

            string clientToken = HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] ?? "";
            string token = HttpContext.Request.Cookies[EnvConfig.TinkJwt] ?? "";

            if (HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] == null)
            {
                (string clientNewToken, double expires) = await _banksService.GetTinkClientToken();
                clientToken = clientNewToken;
                HttpContext.Response.Cookies.Append(EnvConfig.TinkClientJwt, clientNewToken, CookieConfig.Default(expires));
            }



            if (HttpContext.Request.Cookies[EnvConfig.TinkJwt] == null)
            {
                (string newToken, double expires) = await _banksService.GetTinkUserAccessToken(filter.UserId, clientToken);
                token = newToken;
                HttpContext.Response.Cookies.Append(EnvConfig.TinkJwt, newToken, CookieConfig.Default(expires));
            }



            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is invalid or expired");
            }

            try
            {
                DTO.TinkCardResponse card = await _analyticService
                    .GetMostUsedCardAsync(filter, token)
                    .ConfigureAwait(false);
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
                return StatusCode(
                    502,
                    new { message = $"Tink API request failed: {httpEx.Message}" }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to get expenses: {ex.Message}" });
            }
        }

        [HttpGet("balances")]
        public async Task<IActionResult> GetBalances(
            [FromQuery] List<string> accountIds,
            [FromQuery] string userId
        )
        {
            if (accountIds.Equals(null) || accountIds.Count == 0)
                return BadRequest(new { message = "At least one accountId is required." });

            if (userId == null)
                return BadRequest(new { message = "User is required" });

            string clientToken = HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] ?? "";
            string token = HttpContext.Request.Cookies[EnvConfig.TinkJwt] ?? "";

            if (HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] == null)
            {
                (string clientNewToken, double expires) = await _banksService.GetTinkClientToken();
                clientToken = clientNewToken;
                HttpContext.Response.Cookies.Append(EnvConfig.TinkClientJwt, clientNewToken, CookieConfig.Default(expires));
            }

            if (HttpContext.Request.Cookies[EnvConfig.TinkJwt] == null)
            {
                (string newToken, double expires) = await _banksService.GetTinkUserAccessToken(userId, clientToken);
                token = newToken;
                HttpContext.Response.Cookies.Append(EnvConfig.TinkJwt, newToken, CookieConfig.Default(expires));
            }

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
                List<Balance> balances = await _banksService
                    .GetBalancesAsync(accountIds, token, userId)
                    .ConfigureAwait(false);
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
                return StatusCode(
                    502,
                    new { message = $"Tink API request failed: {httpEx.Message}" }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to get balances: {ex.Message}" });
            }
        }

        [HttpGet("connect/other-bank/get-link")]
        public async Task<IActionResult> GetLinkOtherBank([FromQuery] string userId)
        {
            try
            {
                // Client credentials token (for app-level auth)

                string clientToken = HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] ?? "";

                if (HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] == null)
                {
                    (string clientNewToken, double expires) = await _banksService.GetTinkClientToken();
                    clientToken = clientNewToken;
                    HttpContext.Response.Cookies.Append(EnvConfig.TinkClientJwt, clientNewToken, CookieConfig.Default(expires));
                }


                string link = await _banksService.GetTinkLink(userId, clientToken);

                return Ok(new
                {
                    link
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { error = "Внутрішня помилка сервера", details = ex.Message }
                );
            }
        }

        [HttpPost("connect/other-bank")]
        public async Task<IActionResult> ConnectOtherBank([FromQuery] string userId)
        {
            try
            {
                string clientToken = HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] ?? "";
                string token = HttpContext.Request.Cookies[EnvConfig.TinkJwt] ?? "";

                if (HttpContext.Request.Cookies[EnvConfig.TinkClientJwt] == null)
                {
                    (string clientNewToken, double expires) = await _banksService.GetTinkClientToken();
                    clientToken = clientNewToken;
                    HttpContext.Response.Cookies.Append(EnvConfig.TinkClientJwt, clientNewToken, CookieConfig.Default(expires));
                }



                if (HttpContext.Request.Cookies[EnvConfig.TinkJwt] == null)
                {
                    (string newToken, double expires) = await _banksService.GetTinkUserAccessToken(userId, clientToken);
                    token = newToken;
                    HttpContext.Response.Cookies.Append(EnvConfig.TinkJwt, newToken, CookieConfig.Default(expires));
                }



                await _banksService.ConnectOtherBankAsync(userId, token).ConfigureAwait(false);

                return Ok(new { message = "Акаунти користувача успішно підключені" });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(
                    502,
                    new { error = "Помилка при з'єднанні з банком", details = ex.Message }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { error = "Внутрішня помилка сервера", details = ex.Message }
                );
            }
        }

        // GET: api/Banks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BankConfig>>> GetBanks()
        {
            List<BankConfig> banks = await _banksService.GetBanksAsync().ConfigureAwait(false);
            return Ok(banks);
        }

        // GET: api/Banks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BankConfig>> GetBank(string id)
        {
            BankConfig bank = await _banksService.GetBankByIdAsync(id).ConfigureAwait(false);
            if (bank == null)
                return NotFound();
            return Ok(bank);
        }

        // PUT: api/Banks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBank(string id, BankConfig bank)
        {
            await _banksService.UpdateBankAsync(id, bank).ConfigureAwait(false);
            return NoContent();
        }

        // POST: api/Banks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BankConfig>> PostBank(BankConfig bank)
        {
            BankConfig createdBank = await _banksService.AddBankAsync(bank).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetBank), new { id = createdBank.Id }, createdBank);
        }

        // DELETE: api/Banks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBank(string id)
        {
            bool success = await _banksService.DeleteBankAsync(id).ConfigureAwait(false);
            if (!success)
                return NotFound();
            return NoContent();
        }
    }
}
