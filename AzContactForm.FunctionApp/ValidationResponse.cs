using System.Collections.Generic;

namespace AzContactForm.FunctionApp
{
    public class ValidationResponse
    {
        public object Result { get; set; }

        public Dictionary<string, string> Errors { get; set; }

        public ValidationResponse(object result, Dictionary<string, string> errors)
        {
            Result = result;
            Errors = errors;
        }
    }
}
