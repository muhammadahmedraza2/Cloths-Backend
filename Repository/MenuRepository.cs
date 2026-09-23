using System.Data;
using ClothingErp.Api.Data;
using CLOTHS_ERP.API.Dtos;
using CLOTHS_ERP.API.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CLOTHS_ERP.API.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly AppDbContext _context;

    public MenuRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MenuNodeDto>> GetMenuAsync(
        int pcId,
        CancellationToken cancellationToken = default)
    {
        var result = new List<MenuNodeDto>();

        var connection =
            _context.Database.GetDbConnection();

        await connection.OpenAsync(
            cancellationToken);

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            "sp_Menu_GetAll";

        command.CommandType =
            CommandType.StoredProcedure;

        var parameter =
            command.CreateParameter();

        parameter.ParameterName =
            "@PC_ID";

        parameter.DbType =
            System.Data.DbType.Int32;

        parameter.Value =
            pcId;

        command.Parameters.Add(parameter);


        await using var reader =
            await command.ExecuteReaderAsync(
                cancellationToken);


        // =====================================================
        // RESULT SET 1
        // PC_ID | NODE_ID | DESP | ICON
        // =====================================================

        var nodesById =
            new Dictionary<int, MenuNodeDto>();

        var nodeIdOrdinal =
            reader.GetOrdinal("NODE_ID");

        var despOrdinal =
            reader.GetOrdinal("DESP");

        var iconOrdinal =
            reader.GetOrdinal("ICON");


        while (await reader.ReadAsync(
            cancellationToken))
        {
            var nodeId =
                Convert.ToInt32(
                    reader[nodeIdOrdinal]);

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

            result.Add(node);
        }


        // =====================================================
        // RESULT SET 2
        // FORM_TITLE | SITE | FORM_ID | NODE_ID
        // =====================================================

        if (await reader.NextResultAsync(
            cancellationToken))
        {
            var titleOrdinal =
                reader.GetOrdinal("FORM_TITLE");

            var siteOrdinal =
                reader.GetOrdinal("SITE");

            var formIdOrdinal =
                reader.GetOrdinal("FORM_ID");

            var formNodeOrdinal =
                reader.GetOrdinal("NODE_ID");


            while (await reader.ReadAsync(
                cancellationToken))
            {
                var parentNodeId =
                    Convert.ToInt32(
                        reader[formNodeOrdinal]);


                if (!nodesById.TryGetValue(
                    parentNodeId,
                    out var parent))
                {
                    continue;
                }


                var formId =
                    Convert.ToInt32(
                        reader[formIdOrdinal]);


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
                            $"/app/{site}/{formId}"
                    });
            }
        }


        return result;
    }
}