using System.Text.Json;
using ClothingErp.Api.Dtos;
using ClothingErp.Api.Interfaces;
using ClothingErp.Api.Models;

namespace ClothingErp.Api.Services;

public class MasterDataService : IMasterDataService
{
    private readonly IRepository<MasterRecord> _records;

    public MasterDataService(IRepository<MasterRecord> records)
    {
        _records = records;
    }

    public async Task<List<MasterRecordDto>> GetAllAsync(int formId, string? status, string? search)
    {
        var list = await _records.GetAllAsync(r => r.FormId == formId);

        if (string.Equals(status, "authorized", StringComparison.OrdinalIgnoreCase))
            list = list.Where(r => r.Status == "Authorized").ToList();
        else if (string.Equals(status, "unauthorized", StringComparison.OrdinalIgnoreCase))
            list = list.Where(r => r.Status == "UnAuthorize").ToList();

        var dtos = list.OrderByDescending(r => r.CreatedAt).Select(ToDto).ToList();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            dtos = dtos.Where(d => d.Fields.Values.Any(v => (v?.ToString() ?? "").ToLowerInvariant().Contains(term))).ToList();
        }

        return dtos;
    }

    public async Task<MasterRecordDto?> GetByIdAsync(int formId, Guid id)
    {
        var record = await _records.FirstOrDefaultAsync(r => r.FormId == formId && r.Id == id);
        return record is null ? null : ToDto(record);
    }

    public async Task<MasterRecordDto> CreateAsync(int formId, MasterRecordUpsertDto dto, string? createdBy)
    {
        var record = new MasterRecord
        {
            FormId = formId,
            Status = "UnAuthorize", // new records always start unauthorized (maker-checker)
            Closed = dto.Closed ?? "N",
            DataJson = JsonSerializer.Serialize(dto.Fields),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        await _records.AddAsync(record);
        await _records.SaveChangesAsync();
        return ToDto(record);
    }

    public async Task<MasterRecordDto?> UpdateAsync(int formId, Guid id, MasterRecordUpsertDto dto)
    {
        var record = await _records.FirstOrDefaultAsync(r => r.FormId == formId && r.Id == id);
        if (record is null) return null;

        record.DataJson = JsonSerializer.Serialize(dto.Fields);
        if (!string.IsNullOrEmpty(dto.Closed)) record.Closed = dto.Closed;
        if (!string.IsNullOrEmpty(dto.Status)) record.Status = dto.Status;
        record.UpdatedAt = DateTime.UtcNow;

        _records.Update(record);
        await _records.SaveChangesAsync();
        return ToDto(record);
    }

    /// <summary>Maker-checker approve action: flips an UnAuthorize record to Authorized.</summary>
    public async Task<MasterRecordDto?> AuthorizeAsync(int formId, Guid id)
    {
        var record = await _records.FirstOrDefaultAsync(r => r.FormId == formId && r.Id == id);
        if (record is null) return null;

        record.Status = "Authorized";
        record.UpdatedAt = DateTime.UtcNow;

        _records.Update(record);
        await _records.SaveChangesAsync();
        return ToDto(record);
    }

    public async Task<bool> DeleteAsync(int formId, Guid id)
    {
        var record = await _records.FirstOrDefaultAsync(r => r.FormId == formId && r.Id == id);
        if (record is null) return false;

        _records.Remove(record);
        await _records.SaveChangesAsync();
        return true;
    }

    private static MasterRecordDto ToDto(MasterRecord r)
    {
        var fields = JsonSerializer.Deserialize<Dictionary<string, object?>>(r.DataJson) ?? new();
        return new MasterRecordDto
        {
            Id = r.Id.ToString(),
            Status = r.Status,
            Closed = r.Closed,
            Fields = fields,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}