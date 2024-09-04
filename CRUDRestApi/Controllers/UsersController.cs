using CRUDRestApi.DataBase.Interfaces;
using CRUDRestApi.DataBase.Models;
using CRUDRestApi.Models;
using CRUDRestApi.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRUDRestApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        public UserController(ILogger<UserController> logger,IUserService userService)
        {
            _userService = userService;
            _logger = logger;
            _logger.LogDebug(1, "injected into UserController");
        }

        // Insert model
        [HttpPost("UserModel")]
        public async Task<ActionResult<UserModel>> InsertUser([FromBody]UserModel model)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);
            if (model == null)
            {
                _logger.LogWarning("InsertUser called with null user");
                return BadRequest("User data cannot be null");
            }
            try
            {
                var result = await _userService.InsertNewUser(model);
                if (!result)
                {
                    return Conflict("User with provided email or username already exists.");
                }

                return Ok(model);
            } 
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred while inserting a new user.");
                return StatusCode(500, "An error occurred while creating the user.");
            }
        }

        // Get User By Id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"ID must not be zero or negative: {id}");
                return BadRequest("Invalid ID");
            }

            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {id} not found");
                    return NotFound();
                }
                _logger.LogInformation($"Fetching user with ID {id}");
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving the user with ID {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }
        // Delete user
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUserById(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning($"id must not be zero or negative");
                return BadRequest("invalid id");
            }
            try
            {
                var result = await _userService.DeleteUser(id);
                if (!result)
                {
                    _logger.LogWarning($"User with ID {id} not found");
                    return NotFound();
                }
                _logger.LogInformation($"User with ID {id} was successfully deleted.");
                return Ok();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"An error occurred while deleting user with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost("change")]
        public async Task<IActionResult> ChangeValue([FromQuery] string value, [FromQuery] string column, [FromQuery] int id)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(column))
            {
                _logger.LogWarning("Invalid value or column");
                return BadRequest("Invalid value or column");
            }

            if (id <= 0)
            {
                _logger.LogWarning($"Invalid ID provided: {id}");
                return BadRequest("Invalid ID");
            }

            try
            {
                var result = await _userService.UpdateUserField(id, column, value);

                if (!result)
                {
                    _logger.LogWarning($"User with ID {id} not found or invalid column");
                    return NotFound("User not found or invalid column");
                }

                _logger.LogInformation($"User with ID {id} was updated. {column} changed to {value}");
                return Ok("User updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating user with ID {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }

        }

        /*[HttpGet("Id:int")]
        public async Task<IActionResult> GetHistoryChange(int id)
        {
            return Ok();
        }*/
    }
}
