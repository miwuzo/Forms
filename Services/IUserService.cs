using FormsApp.Data;
using System.Collections.Generic;

public interface IUserService
{
    List<ApplicationUser> GetAllUsers();
    ApplicationUser? GetUserById(string id);
    void SaveUser(ApplicationUser user);
    void DeleteUser(string id);
} 