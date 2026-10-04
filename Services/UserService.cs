using ClothingErp.Api.Data;
using ClothingErp.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace ClothingErp.Api.Services;

public class UserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<UserResponseDto>> GetUsersAsync() =>
        _db.Users
            .AsNoTracking()
            .Select(x => new UserResponseDto
            {
                Id = x.Id,
                Username = x.UserName ?? "",
                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                FullName = x.FullName,
                Role = x.Role
            })
            .ToListAsync();

    public async Task SetUserActiveAsync(Guid id, bool active)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);

        if (user is null)
            throw new KeyNotFoundException("User not found.");

        user.IsActive = active;
        await _db.SaveChangesAsync();
    }
}