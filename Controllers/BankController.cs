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

        [HttpGet("test-tink")]
        public IActionResult TestTink()
        {
            var tokenJson =  _bankService.GetTinkAccessToken();
            return Ok(tokenJson); // повертаємо JSON як рядок
        }
    }
}
