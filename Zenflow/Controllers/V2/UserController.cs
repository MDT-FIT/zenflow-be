using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using FintechStatsPlatform.Models; // Потрібно для User
using FintechStatsPlatform.Services; // Потрібно для UserService
using Zenflow.DTO;
// Можливо, вам доведеться додати using для DTO
// using FintechStatsPlatform.DTO; 

namespace Zenflow.Controllers.V2
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("2.0")] // Вказуємо V2
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService, FintechContext context)
        {
            _userService = userService;
        }

        // 1. GET /api/v2/users DELETED
        // Change for new API version

        // 2. GET{id} /api/v2/user return cutted DTO without Id, AccountIds, Accounts
        [HttpGet("{id}")]
        public async Task<ActionResult<String>> GetUser(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id).ConfigureAwait(false);
                if (user == null)
                    return NotFound($"User with id {id} not found");
                var createdTime = await _userService.GetUserTime(id, true);
                string userInfo = $"User {id} was created at {createdTime}";

                return Ok(userInfo);
            }
            catch (Exception ex) 
            { 
                return BadRequest(ex.Message);
            }
            
        }

        // 3. New method
        [HttpGet("{id}/activity")]
        public async Task<ActionResult<DateTime>> GetUserActivity(string id)
        {
            try
            {
                var updateTime = await _userService.GetUserTime(id);
                return Ok($" User {id} was active at {updateTime}");
            }
            catch (Exception ex) 
            { 
                return BadRequest(ex.Message);
            }
        }
    }
}