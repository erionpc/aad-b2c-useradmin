using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.Extensions.Logging;
using B2CUserAdmin.API.Abstractions;
using B2CUserAdmin.Shared.Users;
using B2CUserAdmin.API.Exceptions;

namespace B2CUserAdmin.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService UserService;
        private readonly ILogger<UsersController> Logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            UserService = userService;
            Logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<UserViewModel>>> GetUsers(Guid? objectId = null, string emailSearch = null)
        {
            try
            { 
                if (objectId.HasValue)
                {
                    var user = await UserService.GetByObjectIdAsync(objectId.Value);
                    if (user is null) 
                        return NotFound();

                    return Ok(user);
                }

                if (emailSearch is null)
                {
                    var users = await UserService.GetAllAsync();

                    return Ok(users);
                }
                else
                {
                    var users = await UserService.GetByEmailAsync(emailSearch);

                    return Ok(users);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while getting users");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserViewModel>> NewUser(UserViewModel user)
        {
            try
            {
                return Ok(await UserService.CreateAsync(user));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while creating user");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PutUser(UserViewModel user)
        {
            try
            {
                await UserService.UpdateAsync(user);

                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                Logger.LogError(ex, $"User not found");
                return NotFound();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while updating user");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{objectId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteUser(string objectId)
        {
            try
            {
                await UserService.DeleteAsync(objectId);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                Logger.LogError(ex, $"User not found");
                return NotFound();
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error occurred while deleting user");
                return BadRequest();
            }
        }
    }
}
