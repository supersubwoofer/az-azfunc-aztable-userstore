namespace UserStore.Models
{
    public class User
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }

        public User(string email, string userName)
        {
            Id = email;
            UserName = userName;
            IsActive = true;
        }
        public User(string email, string userName, bool isActive)
        {
            Id = email;
            UserName = userName;
            IsActive = isActive;
        }
    }

    public class UserCreateModel
    {
        public string Email { get; set; }
        public string UserName { get; set; }
    }

    public class UserUpdateModel
    {
        public bool IsActive { get; set; }
    }
}
