using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TodoApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserServices userServices;

    public UserController(UserServices _userService)
    {
        userServices = _userService;

    }

    [HttpGet]
    public async Task<ActionResult<List<User>>> GetAllUSers()
    {
        return await userServices.GetAllUsers();
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUSer(String id)
    {
        return await userServices.GetUSer(id);

    }

    // [HttpPost]
    // public async Task<IActionResult> CreateUser([FromBody] User user)
    // {
    //     var createdUser = await userServices.Create(user);

    //     if (createdUser == null)
    //     {
    //         return Conflict("User with this email already exists.");
    //     }

    //     // Generate a token for the created user
    //     var token = userServices.GenerateToken(user.Email);

    //     // Return both the token and the created user
    //     return Ok(new { Token = token, User = createdUser });
    // }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] User user)
    {
        var createdUser = await userServices.Create(user);

        return Ok(new { Message = "User registered successfully. Please check your email for verification." });
    }





    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
    {
        var result = await userServices.VerifyEmailAsync(email, token);

        if (result)
        {
            return Ok("Email verification successful");
        }
        else
        {
            return BadRequest("Invalid email or token");
        }
    }




    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var email = request.UserEmail;
        var currentPassword = request.CurrentPassword;
        var newPassword = request.NewPassword;

        var isPasswordChanged = await userServices.ChangePassword(email, currentPassword, newPassword);

        if (isPasswordChanged)
        {
            return Ok(new { Message = "Password changed successfully." });
        }
        else
        {
            return BadRequest("Failed to change password. Please check your current password.");
        }
    }




    // [HttpPost("LoginUser")]
    // public ActionResult Login([FromBody] LoginRequest user)
    // {
    //     var isAuthenticated = userServices.Authenticate(user.Email, user.Password);

    //     if (isAuthenticated != null)
    //     {
    //         bool isTokenValid = userServices.VerifyToken(user.Token, user.Email);

    //         if (isTokenValid)
    //         {
    //             return Ok(new { Message = "Authentication successful", User = user });
    //         }
    //         else
    //         {
    //             return BadRequest("Invalid token.");
    //         }
    //     }
    //     else
    //     {
    //         return Unauthorized("Invalid email or password.");
    //     }
    // }


}