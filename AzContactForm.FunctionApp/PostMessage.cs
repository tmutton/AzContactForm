using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzContactForm
{
    public static class PostMessage
    {
        //public const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=ztepb7oskerymstorage;AccountKey=TpIjQT0finPgswlPEFaxixvfRShszopN3SlAx3eNJ+TVgIMd066rIllvIKimYIPECieRfVi08lzOWnscJQ5Uyg==;EndpointSuffix=core.windows.net";
        public const string TableName = "messages";
        
        [FunctionName("Post")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "message")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var ConnectionString = System.Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);

            ConnectionString = "DefaultEndpointsProtocol=https;AccountName=oxtavr4gardv6storage;AccountKey=8kzqqe64h9f3chsJrooPeerpdiEhqR8nLGSomkxOhle6lfNiCp1Ztczu+IM4X+kKzJImDJWWixtnTGhKxQoPjg==";

            // Create account, client and table
            var account = CloudStorageAccount.Parse(ConnectionString);
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference(TableName);
            await table.CreateIfNotExistsAsync();

            // parse query parameters
            log.Info("Parsing query parameters");

            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;

            string email = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "email", true) == 0)
                .Value;

            string message = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "message", true) == 0)
                .Value;

            // parse request body parameters
            log.Info("Parsing request body parameters");

            dynamic data = await req.Content.ReadAsAsync<object>();

            name = name ?? data.name;
            email = email ?? data.email;
            message = message ?? data.message;

            var msg = new Message(name, email, message);

            // todo: add validation for parameters
            table.Execute(TableOperation.Insert(msg));

            // return a 200 response message
            return req.CreateResponse(HttpStatusCode.OK, "Thank you for your message");
        }
    }
}
