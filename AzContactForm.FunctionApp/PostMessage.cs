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
using System.Collections.Generic;
using System.Text;

namespace AzContactForm
{
    public static class PostMessage
    {
        const string StorageTableName = "messages";

        [FunctionName("Post")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "message")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // attempt to get the storage connection string from the application settings
            string StorageConnectionString = Environment.GetEnvironmentVariable("StorageConnection", EnvironmentVariableTarget.Process);

            // if we were unable to get the storage connection string
            if (string.IsNullOrEmpty(StorageConnectionString))
            {
                log.Error("No connection string for the storage could be found");

                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            // create account, client and reference table
            var storageAccount = CloudStorageAccount.Parse(StorageConnectionString);

            var storageTableClient = storageAccount.CreateCloudTableClient();

            var storageTable = storageTableClient.GetTableReference(StorageTableName);

            // attempt to create the table if it doesn't exist
            var tableExists = storageTable.Exists();

            if (!tableExists)
            {
                log.Info("Table '{0}' does not exist. Creating table..", StorageTableName);

                // the table doesn't exist - create it
                storageTable.Create();
            }

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

            // validation
            var validation = new List<string>();

            if (string.IsNullOrEmpty(name)) validation.Add("No name was provided"); // name

            if (string.IsNullOrEmpty(name)) validation.Add("No message was provided"); // message

            // when there are validation errors, return a 400 response
            if (validation.Count > 0) return req.CreateResponse(HttpStatusCode.BadRequest, string.Join(Environment.NewLine, validation));

            // construct message object
            var msg = new Message(name, email, message);

            // insert message object into the storage table
            storageTable.Execute(TableOperation.Insert(msg));

            log.Info("Inserted message into table '{0}'");

            // return a 200 response message
            return req.CreateResponse(HttpStatusCode.OK, "Your message has been sent. Thank you.");
        }
    }
}
