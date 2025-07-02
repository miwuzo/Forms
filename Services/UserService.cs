using FormsApp.Data;
using System.Collections.Generic;
using System.Linq;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;

    public UserService(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<ApplicationUser> GetAllUsers()
    {
        return _db.Users.ToList();
    }

    public ApplicationUser? GetUserById(string id)
    {
        return _db.Users.FirstOrDefault(u => u.Id == id);
    }

    public void SaveUser(ApplicationUser user)
    {
        if (string.IsNullOrEmpty(user.Id))
            _db.Users.Add(user);
        _db.SaveChanges();
    }

    public void DeleteUser(string id)
    {
        var user = _db.Users.FirstOrDefault(u => u.Id == id);
        if (user != null)
        {
            _db.Users.Remove(user);
            _db.SaveChanges();
        }
    }
} 