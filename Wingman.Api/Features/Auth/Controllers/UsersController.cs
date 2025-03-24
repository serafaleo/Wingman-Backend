using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wingman.Api.Core.Controllers;
using Wingman.Api.Core.Helpers.ExtensionMethods;
using Wingman.Api.Features.Auth.DTOs;
using Wingman.Api.Features.Auth.Services.Interfaces;

namespace Wingman.Api.Features.Auth.Controllers;

public class UsersController(IUsersService service) : BaseController
{
    private readonly IUsersService _service = service;

    [AllowAnonymous]
    [HttpPost("[action]")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequestDto signUpDto)
    {
        return (await _service.SignUp(signUpDto)).ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("[action]")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        return (await _service.Login(loginDto)).ToActionResult();
    }

    [AllowAnonymous]
    [HttpPost("[action]")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto refreshDto)
    {
        return (await _service.Refresh(refreshDto)).ToActionResult();
    }

    [HttpPost("[action]")]
    public async Task<IActionResult> Logout()
    {
        return (await _service.Logout(HttpContext.GetUserId())).ToActionResult();
    }
}
