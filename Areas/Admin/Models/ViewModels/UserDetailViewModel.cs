using e_store.Models;

namespace e_store.Areas.Admin.Models.ViewModels;

public class UserDetailViewModel
{
    public string UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public IList<string> Roles { get; set; }
    public List<Order> Orders { get; set; }
    
}