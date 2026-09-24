namespace ClothingErp.Api.Dtos;

public class ColumnDefDto
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class FormDefinitionDto
{
    public int FormId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Breadcrumb { get; set; } = string.Empty;
    public List<ColumnDefDto> Columns { get; set; } = new();
}

/// <summary>
/// Flat, dynamic representation of a record — the dynamic column values from
/// DataJson are merged into "Fields" so the Angular table can bind straight to
/// row[col.key], exactly like the localStorage version did.
/// </summary>
public class MasterRecordDto
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = "UnAuthorize";
    public string Closed { get; set; } = "N";
    public Dictionary<string, object?> Fields { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MasterRecordUpsertDto
{
    public string? Status { get; set; }
    public string? Closed { get; set; }
    public Dictionary<string, object?> Fields { get; set; } = new();
}