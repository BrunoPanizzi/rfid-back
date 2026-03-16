using Backend.Database;
using Microsoft.EntityFrameworkCore;
namespace Backend.Features.Users;

public interface IUserService
{
    Task<UserDto> GetUserAsync(int id);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task<UserDto> UpdateUserAsync(int id, CreateUserDto dto);
}

public class UserService(AppDbContext db) : IUserService
{
    private readonly AppDbContext _db = db;

    public async Task<UserDto> GetUserAsync(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id)
            ?? throw new KeyNotFoundException($"User with id {id} not found");

        return MapToDto(user);
    }

    public Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        return Task.FromResult(_db.Users.Select(MapToDto).AsEnumerable());
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        var user = await _db.Users.AddAsync(new User { Name = dto.Name, Email = dto.Email });

        await _db.SaveChangesAsync();

        return MapToDto(user.Entity);
    }

    public async Task<UserDto> UpdateUserAsync(int id, CreateUserDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == id)
            ?? throw new KeyNotFoundException($"User with id {id} not found");

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return MapToDto(user);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.UserId,
        Name = user.Name,
        Email = user.Email,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
    };
}
