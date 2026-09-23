namespace CLOTHS_ERP.API.Options;

public class MenuOptions
{
    public const string SectionName = "Menu";

    public string ConnectionName { get; set; } = "DefaultConnection";

    public string StoredProcedure { get; set; } = "sp_Menu_GetAll";

    public string RouteTemplate { get; set; } = "/app/{site}/{formId}";

    public string PcIdClaimType { get; set; } = "PcId";
}