using System.ComponentModel.DataAnnotations;

namespace CLOTHS_ERP.API.Options
{
    public class MenuOptions
    {
        public const string SectionName = "Menu";

        /// <summary>Name of the entry under "ConnectionStrings".</summary>
        [Required]
        public string ConnectionName { get; set; } = string.Empty;

        /// <summary>Stored procedure that returns the menu (nodes + forms).</summary>
        [Required]
        public string StoredProcedure { get; set; } = string.Empty;

        /// <summary>Route pattern for a form. Placeholders: {site}, {formId}.</summary>
        [Required]
        public string RouteTemplate { get; set; } = string.Empty;

        /// <summary>Claim type in the user's token that carries the PC_ID (profile id).</summary>
        [Required]
        public string PcIdClaimType { get; set; } = string.Empty;
    }
}