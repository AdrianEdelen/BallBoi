using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using Tofu.BallBoi.Abstractions;
using Tofu.BallBoi.Core;
namespace Tofu.BallBoi.Data;
internal class UserRepository : IUserRepository
{
    private readonly DatabaseContext _context;
    public UserRepository(DatabaseContext databaseContext)
    {
        _context = databaseContext;
    }
    public async Task<User> AddUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var matchingUser =  await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
        if (matchingUser == null)
        {
            return false;
        }

        _context.Users.Remove(matchingUser);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User>? GetUserByIdAsync(int id)
    {
        var matchingUser = await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
        if (matchingUser == null)
        {
            return null;
        }
        return matchingUser;

    }

    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Attach(user);
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return user;
    }
}