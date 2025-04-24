namespace backend.Models;

public class User
{
    int id { get; set; }
    string username { get; set; }
    string password { get; set; }
    string email { get; set; }
}

public class UserDTO
{
    int id { get; set; }
    public string username { get; set; }
    public string email { get; set; }
}
