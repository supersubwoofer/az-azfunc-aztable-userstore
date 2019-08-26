using UserStore.Models;

public static class Mappings
{
    public static UserTableEntity ToTableEntity(this User user)
    {
        return new UserTableEntity()
        {
            PartitionKey = "signup",
            RowKey = user.Id,
            UserName = user.UserName,
            IsActive = user.IsActive
        };
    }

    public static User ToUser(this UserTableEntity userEntity)
    {
        return new User(
            userEntity.RowKey,
            userEntity.UserName,
            userEntity.IsActive
            );
    }
}