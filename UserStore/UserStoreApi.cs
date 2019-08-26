using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using UserStore.Models;

namespace UserStore.AzureTableStorage
{
    public static class UserStoreApi
    {
        private const string Route = "user";
        private const string TableName = "users";

        [FunctionName("CreateUser")]
        public static async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = Route)]HttpRequest req,
            [Table(TableName, Connection = "AzureWebJobsStorage")] CloudTable userTable,
            ILogger log)
        {
            log.LogInformation("Creating a new user");

            await userTable.CreateIfNotExistsAsync();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserCreateModel input = JsonConvert.DeserializeObject<UserCreateModel>(requestBody);

            var user = new User(
                input.Email,
                input.UserName
            );

            TableOperation insertOperation = TableOperation.Insert(user.ToTableEntity());

            await userTable.ExecuteAsync(insertOperation);
            return new OkObjectResult(user);
        }

        [FunctionName("GetUsers")]
        public static async Task<IActionResult> GetUsers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route)]HttpRequest req,
            [Table(TableName, Connection = "AzureWebJobsStorage")] CloudTable userTable,
            ILogger log)
        {
            log.LogInformation("Getting users");
            var query = new TableQuery<UserTableEntity>();
            var segment = await userTable.ExecuteQuerySegmentedAsync(query, null);
            return new OkObjectResult(segment.Select(Mappings.ToUser));
        }

        [FunctionName("GetUserById")]
        public static IActionResult GetUserById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Route + "/{id}")]HttpRequest req,
            [Table(TableName, "signup", "{id}",  Connection = "AzureWebJobsStorage")] UserTableEntity userTableEntity,
            ILogger log, string id)
        {
            log.LogInformation("Getting user by id");
            if (userTableEntity == null)
            {
                log.LogInformation($"User {id} not found");
                return new NotFoundResult();
            }
            return new OkObjectResult(userTableEntity.ToUser());
        }

        [FunctionName("UpdateUser")]
        public static async Task<IActionResult> UpdateUser(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = Route + "/{id}")]HttpRequest req,
            [Table(TableName, Connection = "AzureWebJobsStorage")] CloudTable userTable,
            ILogger log, string id)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updated = JsonConvert.DeserializeObject<UserUpdateModel>(requestBody);
            var findOperation = TableOperation.Retrieve<UserTableEntity>("signup", id);
            var findResult = await userTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new NotFoundResult();
            }
            var existingRow = (UserTableEntity)findResult.Result;

            existingRow.IsActive = updated.IsActive;
            var replaceOperation = TableOperation.Replace(existingRow);
            await userTable.ExecuteAsync(replaceOperation);

            return new OkObjectResult(existingRow.ToUser());
        }

    }
}
