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
        return CreateResponse(await _service.SignUp(signUpDto));
    }

    [AllowAnonymous]
    [HttpPost("[action]")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        return CreateResponse(await _service.Login(loginDto));
    }

    [AllowAnonymous]
    [HttpPost("[action]")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto refreshDto)
    {
        return CreateResponse(await _service.Refresh(refreshDto));
    }

    [HttpPut("[action]")]
    public async Task<IActionResult> Logout()
    {
        return CreateResponse(await _service.Logout(HttpContext.GetUserId(), HttpContext.GetUserEmail()));
    }
}
