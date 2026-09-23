using System.Text.Json;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Interfaces;
using ClothingErp.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace ClothingErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormsController : ControllerBase
{
    private readonly IRepository<FormDefinition> _forms;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public FormsController(IRepository<FormDefinition> forms)
    {
        _forms = forms;
    }

    /// <summary>All available forms/screens (for building the sidebar dynamically, if desired).</summary>
    [HttpGet]
    public async Task<ActionResult<List<FormDefinitionDto>>> GetAll()
    {
        var forms = await _forms.GetAllAsync();
        return Ok(forms.Select(ToDto).ToList());
    }

    [HttpGet("{formId:int}")]
    public async Task<ActionResult<FormDefinitionDto>> GetById(int formId)
    {
        var form = await _forms.FirstOrDefaultAsync(f => f.Id == formId);
        return form is null ? NotFound() : Ok(ToDto(form));
    }

    private static FormDefinitionDto ToDto(FormDefinition f) => new()
    {
        FormId = f.Id,
        Title = f.Title,
        Breadcrumb = f.Breadcrumb,
        Columns = JsonSerializer.Deserialize<List<ColumnDefDto>>(f.ColumnsJson, JsonOptions) ?? new()
    };
}