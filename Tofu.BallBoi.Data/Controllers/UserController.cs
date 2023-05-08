using Microsoft.AspNetCore.Mvc;
using Tofu.BallBoi.Abstractions.DataTransferObjects;
using Tofu.BallBoi.Abstractions.Interfaces;
using Tofu.BallBoi.Core.Models;

namespace Tofu.BallBoi.Data.Controllers
{
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("/Users")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("/Users/{id}")]
        public async Task<IActionResult> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("/Users")]
        public async Task<IActionResult> AddUser([FromBody] UserDTO user)
        {
            await _userRepository.AddUserAsync(user);
            return CreatedAtAction(nameof(GetUserByIdAsync), new { id = user.Id }, user);
        }

        [HttpPut("/Users")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDTO user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            await _userRepository.UpdateUserAsync(user);
            return Ok(user);
        }

        [HttpDelete("/Users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userRepository.DeleteUserAsync(id);
            return Ok(result);
        }

    }
}
