namespace ClothingErp.Api.Models;

/// <summary>
/// One row per "screen" in the ERP (Stationery Setup, Sales Order, Bank Setup, etc.).
/// Id matches the formId values used by the Angular frontend's menu.config.ts /
/// form-registry.service.ts, so the two stay in sync.
/// </summary>
public class FormDefinition
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Breadcrumb { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of { key, label } column definitions, e.g.
    /// [{"key":"code","label":"Stationery Code"},{"key":"name","label":"Stationery Name"}]
    /// </summary>
    public string ColumnsJson { get; set; } = "[]";

    public ICollection<MasterRecord> Records { get; set; } = new List<MasterRecord>();
}