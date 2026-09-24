namespace ClothingErp.Api.Models;

public class MasterRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int FormId { get; set; }
    public FormDefinition? Form { get; set; }

    /// <summary>
    /// JSON object holding the dynamic columns for this form, e.g.
    /// {"code":"03123","name":"RAZA STATIONARY SETUP","instrumentId":"MK17"}
    /// </summary>
    public string DataJson { get; set; } = "{}";

    /// <summary>"Authorized" or "UnAuthorize" - maker-checker workflow status.</summary>
    public string Status { get; set; } = "UnAuthorize";

    /// <summary>"Y" or "N"</summary>
    public string Closed { get; set; } = "N";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}