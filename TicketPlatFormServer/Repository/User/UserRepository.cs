
using Microsoft.EntityFrameworkCore;
using TicketPlatFormServer.DBModel;
using TicketPlatFormServer.Repository;

public class UserRepository :IUserRepository
{
    private readonly TicketContext _db;

    public UserRepository(TicketContext db)
    {
        // Context 의존성 주입
        _db = db;

    }
    
    public async Task<User?> GetByEmail(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Equals(email) && x.IsDeleted == false)!;
        return user;
    }

    public async Task<User> Sign(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}