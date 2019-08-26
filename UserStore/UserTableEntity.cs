using Microsoft.WindowsAzure.Storage.Table;

public class UserTableEntity : TableEntity
{
    public UserTableEntity(string skey, string srow)
    {
        this.PartitionKey = skey;
        this.RowKey = srow;
    }

    public UserTableEntity() { }

    public string UserName { get; set; }
    public bool IsActive { get; set; }
}