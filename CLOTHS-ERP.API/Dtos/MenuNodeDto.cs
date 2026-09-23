namespace CLOTHS_ERP.API.Dtos
{
    public class MenuNodeDto
    {
        public string Label { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Route { get; set; }
        public int? FormId { get; set; }
        public List<MenuNodeDto> Children { get; set; } = new();
    }
}
