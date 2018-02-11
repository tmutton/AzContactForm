using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Linq;
using System.Threading.Tasks;
using AzContactForm.FunctionApp;
using FluentValidation.Results;

namespace AzContactForm
{
    public static class PostMessage
    {
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
            log.Info("Sent Event to Event Grid");

            // return a 200 response message
            return req.CreateResponse(HttpStatusCode.OK, "Your message has been sent. Thank you.");
        }
    }
}
