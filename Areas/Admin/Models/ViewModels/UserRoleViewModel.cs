namespace e_store.Areas.Admin.Models.ViewModels;

public class UserRoleViewModel
{
    public string UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public IList<string> Role { get; set; }
    public IList<string>? AllRoles { get; set; }
    
}