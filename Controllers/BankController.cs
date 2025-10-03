using FintechStatsPlatform.Models;
using FintechStatsPlatform.Services;
using Microsoft.AspNetCore.Mvc;

namespace FintechStatsPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankController :ControllerBase
    {
        private readonly BankService _bankService;

        public BankController(BankService bankService)
        {
            _bankService = bankService;
        }

        [HttpGet("get-tink-token")]
        public IActionResult getTinkAccesssToken()
        {
            var tokenJson =  _bankService.GetTinkAccessToken();
            return Ok(tokenJson); // повертаємо JSON як рядок
        }

        [HttpGet("bank-configs/{userId}")]
        public IActionResult getConfigs([FromRoute] string userId) 
        {
           
            return Ok(_bankService.listBankConfigs(userId));
        }
    }
}
