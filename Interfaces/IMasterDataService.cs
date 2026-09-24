using ClothingErp.Api.Dtos;

namespace ClothingErp.Api.Interfaces;

public interface IMasterDataService
{
    Task<List<MasterRecordDto>> GetAllAsync(int formId, string? status, string? search);
    Task<MasterRecordDto?> GetByIdAsync(int formId, Guid id);
    Task<MasterRecordDto> CreateAsync(int formId, MasterRecordUpsertDto dto, string? createdBy);
    Task<MasterRecordDto?> UpdateAsync(int formId, Guid id, MasterRecordUpsertDto dto);
    Task<MasterRecordDto?> AuthorizeAsync(int formId, Guid id);
    Task<bool> DeleteAsync(int formId, Guid id);
}