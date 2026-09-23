using System.Data;
using CLOTHS_ERP.API.Dtos;
using CLOTHS_ERP.API.Interfaces;
using CLOTHS_ERP.API.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CLOTHS_ERP.API.Services
{
    public class MenuService : IMenuService
    {
        // SP contract (parameter + column names). These must match the stored procedure.
        private const string PcIdParameter = "@PC_ID";

        private const string ColNodeId = "NODE_ID";
        private const string ColDesp = "DESP";
        private const string ColIcon = "ICON";
        private const string ColFormTitle = "FORM_TITLE";
        private const string ColSite = "SITE";
        private const string ColFormId = "FORM_ID";

        private readonly string _connectionString;
        private readonly MenuOptions _options;

        public MenuService(IConfiguration configuration, IOptions<MenuOptions> options)
        {
            _options = options.Value;

            _connectionString = configuration.GetConnectionString(_options.ConnectionName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{_options.ConnectionName}' not found.");
        }

        public async Task<List<MenuNodeDto>> GetMenuAsync(int pcId, CancellationToken cancellationToken = default)
        {
            var menu = new List<MenuNodeDto>();
            var nodesById = new Dictionary<int, MenuNodeDto>();

            await using var conn = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(_options.StoredProcedure, conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(PcIdParameter, SqlDbType.Int).Value = pcId;

            await conn.OpenAsync(cancellationToken);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            // ---------- Result set 1: menu nodes ("Table") ----------
            var nodeIdOrd = reader.GetOrdinal(ColNodeId);
            var despOrd = reader.GetOrdinal(ColDesp);
            var iconOrd = reader.GetOrdinal(ColIcon);

            while (await reader.ReadAsync(cancellationToken))
            {
                var node = new MenuNodeDto
                {
                    Label = reader.GetString(despOrd),
                    Icon = reader.IsDBNull(iconOrd) ? null : reader.GetString(iconOrd)
                };

                nodesById[reader.GetInt32(nodeIdOrd)] = node;
                menu.Add(node); // order comes from the SP (SortOrder)
            }

            // ---------- Result set 2: forms under each node ("Table1") ----------
            if (await reader.NextResultAsync(cancellationToken))
            {
                var titleOrd = reader.GetOrdinal(ColFormTitle);
                var siteOrd = reader.GetOrdinal(ColSite);
                var formIdOrd = reader.GetOrdinal(ColFormId);
                var fNodeOrd = reader.GetOrdinal(ColNodeId);

                while (await reader.ReadAsync(cancellationToken))
                {
                    if (!nodesById.TryGetValue(reader.GetInt32(fNodeOrd), out var parent))
                        continue;

                    var formId = reader.GetInt32(formIdOrd);
                    var site = reader.GetString(siteOrd);

                    parent.Children.Add(new MenuNodeDto
                    {
                        Label = reader.GetString(titleOrd),
                        FormId = formId,
                        Route = BuildRoute(site, formId)
                    });
                }
            }

            return menu;
        }

        private string BuildRoute(string site, int formId) =>
            _options.RouteTemplate
                .Replace("{site}", site, StringComparison.OrdinalIgnoreCase)
                .Replace("{formId}", formId.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}