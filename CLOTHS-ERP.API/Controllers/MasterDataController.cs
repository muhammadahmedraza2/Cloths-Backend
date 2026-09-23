using ClothingErp.Api.Dtos;
using ClothingErp.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/forms/{formId:int}/records")]
[Authorize]
public class MasterDataController : ControllerBase
{
    private readonly IMasterDataService _service;

    public MasterDataController(IMasterDataService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /api/forms/1047/records?status=authorized&amp;search=raza
    /// status: "authorized" | "unauthorized" | omitted (both)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<MasterRecordDto>>> GetAll(int formId, [FromQuery] string? status, [FromQuery] string? search)
    {
        return Ok(await _service.GetAllAsync(formId, status, search));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MasterRecordDto>> GetById(int formId, Guid id)
    {
        var record = await _service.GetByIdAsync(formId, id);
        return record is null ? NotFound() : Ok(record);
    }

    [HttpPost]
    public async Task<ActionResult<MasterRecordDto>> Create(int formId, MasterRecordUpsertDto dto)
    {
        var record = await _service.CreateAsync(formId, dto, User.Identity?.Name);
        return CreatedAtAction(nameof(GetById), new { formId, id = record.Id }, record);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MasterRecordDto>> Update(int formId, Guid id, MasterRecordUpsertDto dto)
    {
        var record = await _service.UpdateAsync(formId, id, dto);
        return record is null ? NotFound() : Ok(record);
    }

    /// <summary>Maker-checker approve action: flips an UnAuthorize record to Authorized.</summary>
    [HttpPatch("{id:guid}/authorize")]
    public async Task<ActionResult<MasterRecordDto>> Authorize(int formId, Guid id)
    {
        var record = await _service.AuthorizeAsync(formId, id);
        return record is null ? NotFound() : Ok(record);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(int formId, Guid id)
    {
        var ok = await _service.DeleteAsync(formId, id);
        return ok ? NoContent() : NotFound();
    }
}