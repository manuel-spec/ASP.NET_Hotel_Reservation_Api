using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    private readonly UserServices loginservices;

    public LoginController(UserServices _loginservices)
    {
        loginservices = _loginservices;
    }

    [AllowAnonymous]
    [Route("authenticate")]
    [HttpPost]
    public ActionResult Login([FromBody] User user)
    {

        var token = loginservices.Authenticate(user.Email, user.Password);

        if (token == null)
        {
            return Unauthorized();
        }

        return Ok(new { token, user });
    }
}