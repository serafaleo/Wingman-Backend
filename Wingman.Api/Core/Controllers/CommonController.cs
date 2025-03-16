using Microsoft.AspNetCore.Mvc;
using Wingman.Api.Core.Helpers.ExtensionMethods;
using Wingman.Api.Core.Models;
using Wingman.Api.Core.Services.Interfaces;

namespace Wingman.Api.Core.Controllers;

public abstract class CommonController<T>(ICommonService<T> service) : BaseController where T : CommonModel
{
    private readonly ICommonService<T> _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAllAsync([FromQuery] int page, [FromQuery] int pageSize)
    {
        return (await _service.GetAllAsync(page, pageSize, HttpContext.GetUserId())).ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
    {
        return (await _service.GetByIdAsync(id, HttpContext.GetUserId())).ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] T model)
    {
        return (await _service.CreateAsync(model, HttpContext.GetUserId())).ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] T model)
    {
        return (await _service.UpdateAsync(id, model, HttpContext.GetUserId())).ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteByIdAsync([FromRoute] Guid id)
    {
        return (await _service.DeleteByIdAsync(id, HttpContext.GetUserId())).ToActionResult();
    }
}
