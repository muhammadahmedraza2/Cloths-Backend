using System.Data;
using CLOTHS_ERP.API.Dtos;
using CLOTHS_ERP.API.Interfaces;
using CLOTHS_ERP.API.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace CLOTHS_ERP.API.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly string _connectionString;
    private readonly MenuOptions _options;

    public MenuRepository(
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
        var result = new List<MenuNodeDto>();

        var nodesById = new Dictionary<int, MenuNodeDto>();

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
            "@PC_ID",
            SqlDbType.Int).Value = pcId;

        await connection.OpenAsync(cancellationToken);

        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);

        // =====================================================
        // RESULT SET 1
        // PC_ID | NODE_ID | DESP | ICON
        // =====================================================

        var nodeIdOrdinal =
            reader.GetOrdinal("NODE_ID");

        var despOrdinal =
            reader.GetOrdinal("DESP");

        var iconOrdinal =
            reader.GetOrdinal("ICON");

        while (await reader.ReadAsync(cancellationToken))
        {
            var nodeId =
                Convert.ToInt32(
                    reader[nodeIdOrdinal]);

            var node = new MenuNodeDto
            {
                Id = nodeId,

                Label =
                    reader.IsDBNull(despOrdinal)
                        ? string.Empty
                        : Convert.ToString(
                            reader[despOrdinal]) ?? string.Empty,

                Icon =
                    reader.IsDBNull(iconOrdinal)
                        ? null
                        : Convert.ToString(
                            reader[iconOrdinal])
            };

            nodesById[nodeId] = node;

            result.Add(node);
        }

        // =====================================================
        // RESULT SET 2
        // FORM_TITLE | SITE | FORM_ID | NODE_ID
        // =====================================================

        if (await reader.NextResultAsync(cancellationToken))
        {
            var titleOrdinal =
                reader.GetOrdinal("FORM_TITLE");

            var siteOrdinal =
                reader.GetOrdinal("SITE");

            var formIdOrdinal =
                reader.GetOrdinal("FORM_ID");

            var formNodeOrdinal =
                reader.GetOrdinal("NODE_ID");

            while (await reader.ReadAsync(cancellationToken))
            {
                var parentNodeId =
                    Convert.ToInt32(
                        reader[formNodeOrdinal]);

                if (!nodesById.TryGetValue(
                    parentNodeId,
                    out var parentNode))
                {
                    continue;
                }

                var formId =
                    Convert.ToInt32(
                        reader[formIdOrdinal]);

                var title =
                    reader.IsDBNull(titleOrdinal)
                        ? string.Empty
                        : Convert.ToString(
                            reader[titleOrdinal]) ?? string.Empty;

                var site =
                    reader.IsDBNull(siteOrdinal)
                        ? string.Empty
                        : Convert.ToString(
                            reader[siteOrdinal]) ?? string.Empty;

                parentNode.Children.Add(
                    new MenuNodeDto
                    {
                        Id = formId,
                        FormId = formId,
                        Label = title,
                        Route = BuildRoute(
                            site,
                            formId)
                    });
            }
        }

        return result;
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