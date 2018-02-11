using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Linq;
using System.Threading.Tasks;
using AzContactForm.FunctionApp;
using FluentValidation.Results;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using System.Text;

namespace AzContactForm
{
    public static class PostMessage
    {
        public static string eventTopic = "/SUBSCRIPTIONS/EFCF1B1C-B797-4AD3-B432-43D90045E26D/RESOURCEGROUPS/FIBTO/PROVIDERS/MICROSOFT.EVENTGRID/TOPICS/DELIVERYREQUESTS";

        public static string eventTopicUrl = "https://deliveryrequests.westus2-1.eventgrid.azure.net/api/events";

        public static string eventTopicSasKey = "kv2rlUCX34Qx933/6ijZSiNwnjB1LzovggSeWy1RhFM=";

        [FunctionName("Post")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "message")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

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
            
            // construct message object
            var messageToSave = new Message(name, email, message);

            // validation
            MessageValidation validator = new MessageValidation();
            ValidationResult results = validator.Validate(messageToSave);

            // create a custom validation response using key value pairs
            var validationResponse = new ValidationResponse("Validation failure", results.Errors.ToDictionary(x => x.PropertyName, x => x.ErrorMessage));

            // return a 400 with the validation errors
            if (!results.IsValid) return req.CreateResponse(HttpStatusCode.BadRequest, validationResponse);

            // todo: send event to event grid
            var events = new List<Event<Message>>();

            var event1 = new Event<Message>
            {
                Id = Guid.NewGuid().ToString(),
                Topic = eventTopic,
                Subject = "WindDetails",
                EventTime = DateTimeOffset.Now.ToString("o"),
                EventType = "newRequest",
                Data = messageToSave
            };
            events.Add(event1);

            HttpClient httpClient = new HttpClient
            {
                BaseAddress = new Uri(eventTopicUrl)
            };

            var json = JsonConvert.SerializeObject(events);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "");
            request.Headers.Add("aeg-sas-key", eventTopicSasKey);
            request.Content = new StringContent(json,
                                                Encoding.UTF8,
                                                "application/json");

            httpClient.SendAsync(request)
                  .ContinueWith(responseTask =>
                  {
                      Console.WriteLine("Response: {0}", responseTask.Result);
                  });

            log.Info("Sent Event to Event Grid");

            // return a 200 response message
            return req.CreateResponse(HttpStatusCode.OK, "Your message has been sent. Thank you.");
        }
    }
}
