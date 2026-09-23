using System.Data;
using CLOTHS_ERP.API.Dtos;
using CLOTHS_ERP.API.Interfaces;
using CLOTHS_ERP.API.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CLOTHS_ERP.API.Services;

public class MenuService : IMenuService
{
    private const string PcIdParameter = "@PC_ID";

    private const string ColNodeId = "NODE_ID";
    private const string ColDesp = "DESP";
    private const string ColIcon = "ICON";

    private const string ColFormTitle = "FORM_TITLE";
    private const string ColSite = "SITE";
    private const string ColFormId = "FORM_ID";

    private readonly string _connectionString;
    private readonly MenuOptions _options;

    public MenuService(
        IConfiguration configuration,
        IOptions<MenuOptions> options)
    {
        _options = options.Value;

        _connectionString =
            configuration.GetConnectionString(
                _options.ConnectionName)
            ?? throw new InvalidOperationException(
                $"Connection string '{_options.ConnectionName}' not found.");
    }

    public async Task<List<MenuNodeDto>> GetMenuAsync(
        int pcId,
        CancellationToken cancellationToken = default)
    {
        var menu = new List<MenuNodeDto>();

        var nodesById =
            new Dictionary<int, MenuNodeDto>();

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(
                _options.StoredProcedure,
                connection)
            {
                CommandType = CommandType.StoredProcedure
            };

        command.Parameters.Add(
            PcIdParameter,
            SqlDbType.Int).Value = pcId;

        await connection.OpenAsync(
            cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        // =========================================================
        // RESULT SET 1
        // PC_ID | NODE_ID | DESP | ICON
        // =========================================================

        var nodeIdOrdinal =
            reader.GetOrdinal(ColNodeId);

        var despOrdinal =
            reader.GetOrdinal(ColDesp);

        var iconOrdinal =
            reader.GetOrdinal(ColIcon);

        while (await reader.ReadAsync(
            cancellationToken))
        {
            var nodeId =
                reader.GetInt32(nodeIdOrdinal);

            var node =
                new MenuNodeDto
                {
                    Id = nodeId,

                    Label =
                        reader.IsDBNull(despOrdinal)
                            ? string.Empty
                            : reader.GetString(despOrdinal),

                    Icon =
                        reader.IsDBNull(iconOrdinal)
                            ? null
                            : reader.GetString(iconOrdinal)
                };

            nodesById[nodeId] = node;

            menu.Add(node);
        }

        // =========================================================
        // RESULT SET 2
        // FORM_TITLE | SITE | FORM_ID | NODE_ID
        // =========================================================

        if (await reader.NextResultAsync(
            cancellationToken))
        {
            var titleOrdinal =
                reader.GetOrdinal(ColFormTitle);

            var siteOrdinal =
                reader.GetOrdinal(ColSite);

            var formIdOrdinal =
                reader.GetOrdinal(ColFormId);

            var formNodeOrdinal =
                reader.GetOrdinal(ColNodeId);

            while (await reader.ReadAsync(
                cancellationToken))
            {
                var parentNodeId =
                    reader.GetInt32(formNodeOrdinal);

                if (!nodesById.TryGetValue(
                    parentNodeId,
                    out var parent))
                {
                    continue;
                }

                var formId =
                    reader.GetInt32(formIdOrdinal);

                var title =
                    reader.IsDBNull(titleOrdinal)
                        ? string.Empty
                        : reader.GetString(titleOrdinal);

                var site =
                    reader.IsDBNull(siteOrdinal)
                        ? string.Empty
                        : reader.GetString(siteOrdinal);

                parent.Children.Add(
                    new MenuNodeDto
                    {
                        Id = formId,

                        FormId = formId,

                        Label = title,

                        Route =
                            BuildRoute(
                                site,
                                formId)
                    });
            }
        }

        return menu;
    }

    private string BuildRoute(
        string site,
        int formId)
    {
        return _options.RouteTemplate
            .Replace(
                "{site}",
                site,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{formId}",
                formId.ToString(),
                StringComparison.OrdinalIgnoreCase);
    }
}