using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lifelog.Userstore
{
    public static class CustomUserStore
    {
        private const string Route = "user";
        private const string TableName = "users";

        [FunctionName("CreateUser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)]HttpRequest req,
            [Table(TableName, Connection = "AzureWebJobsStorage")] CloudTable userTable,
            ILogger log)
        {
            log.LogInformation("Creating a new user");

            await userTable.CreateIfNotExistsAsync();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<UserCreateModel>(requestBody);

            var user = new User() {
                Firstname = input.Firstname,
                Lastname = input.Lastname,
                Email = input.Email
            };

            TableOperation insertOperation = TableOperation.Insert(user.ToTableEntity());

            await userTable.ExecuteAsync(insertOperation);
            return new OkObjectResult(user);
        }

    }

    // Models
    public class User
    {
        public string Email { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }

    public class UserCreateModel
    {
        public string Email { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
    }

    // Entity
    public class UserTableEntity : TableEntity
    {
        public UserTableEntity(string skey, string srow)
        {
            this.PartitionKey = skey;
            this.RowKey = srow;
        }

        public UserTableEntity() { }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    // Model & Entity Mappings
    public static class Mappings
    {
        public static UserTableEntity ToTableEntity(this User user)
        {
            return new UserTableEntity()
            {
                PartitionKey = "signup",
                RowKey = user.Email,
                FirstName = user.Firstname,
                LastName = user.Lastname,
                Email = user.Email,
            };
        }

        public static User ToUser(this UserTableEntity userEntity)
        {
            return new User()
            {
                Email = userEntity.Email,
                Firstname = userEntity.FirstName,
                Lastname = userEntity.LastName,
            };
        }

    }
}
