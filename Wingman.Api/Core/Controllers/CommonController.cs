using Microsoft.AspNetCore.Mvc;
using Wingman.Api.Core.Helpers.ExtensionMethods;
using Wingman.Api.Core.Services.Interfaces;

namespace Wingman.Api.Core.Controllers;

public abstract class CommonController<T>(IBaseService<T> service) : BaseController
{
    private readonly IBaseService<T> _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] int page, [FromQuery] int pageSize)
    {
        return CreateResponse(await _service.GetAllAsync(HttpContext.GetUserId()!.Value, page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
    {
        return CreateResponse(await _service.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] T model)
    {
        return CreateResponse(await _service.CreateAsync(model, HttpContext.GetUserId()!.Value));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] T model)
    {
        return CreateResponse(await _service.UpdateAsync(model, HttpContext.GetUserId()!.Value));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteByIdAsync([FromRoute] Guid id)
    {
        return CreateResponse(await _service.DeleteByIdAsync(id));
    }
}
