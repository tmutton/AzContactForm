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
        public static string EventTopic;

        public static string EventTopicUrl;

        public static string EventTopicSasKey;

        [FunctionName("Post")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "message")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // validate the required environment variables
            var envVariables = Environment.GetEnvironmentVariables();

            ValidationResult envVarValidationResult = new ConfigValidation().Validate(envVariables);

            var envVarValidationResponse = new ValidationResponse("Validation failure", envVarValidationResult.Errors.ToDictionary(x => x.PropertyName, x => x.ErrorMessage));

            // return a 400 with the validation errors
            if (!envVarValidationResult.IsValid) return req.CreateResponse(HttpStatusCode.BadRequest, envVarValidationResponse);
            
            // read variables from app settings
            EventTopic = Environment.GetEnvironmentVariable("EventTopic", EnvironmentVariableTarget.Process);

            EventTopicUrl = Environment.GetEnvironmentVariable("EventTopicUrl", EnvironmentVariableTarget.Process);

            EventTopicSasKey = Environment.GetEnvironmentVariable("EventTopicSasKey", EnvironmentVariableTarget.Process);

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
            var messageRequest = new Message(name, email, message);

            // validation
            ValidationResult results = new MessageValidation().Validate(messageRequest);

            // create a custom validation response using key value pairs
            var validationResponse = new ValidationResponse("Validation failure", results.Errors.ToDictionary(x => x.PropertyName, x => x.ErrorMessage));

            // return a 400 with the validation errors
            if (!results.IsValid) return req.CreateResponse(HttpStatusCode.BadRequest, validationResponse);

            // send event to event grid
            var events = new List<Event<Message>>();

            var eventMessage = new Event<Message>
            {
                Id = Guid.NewGuid().ToString(),
                Topic = EventTopic,
                Subject = "/azContactForm/",
                EventType = "AzContactForm.NewMessage",
                EventTime = DateTimeOffset.Now.ToString("o"),
                Data = messageRequest
            };
            events.Add(eventMessage);

            HttpClient httpClient = new HttpClient
            {
                BaseAddress = new Uri(EventTopicUrl)
            };

            var json = JsonConvert.SerializeObject(events);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "");
            request.Headers.Add("aeg-sas-key", EventTopicSasKey);
            request.Content = new StringContent(json,
                                                Encoding.UTF8,
                                                "application/json");
            
            await httpClient.SendAsync(request)
                  .ContinueWith(responseTask =>
                  {
                      Console.WriteLine("Response: {0}", responseTask.Result);

                      log.Info("Sent Event to Event Grid");
                  });
            
            // return a 200 response message
            return req.CreateResponse(HttpStatusCode.OK, "Your message has been sent. Thank you.");
        }
    }
}
