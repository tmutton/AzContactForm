using FluentValidation;
using System.Collections;

namespace AzContactForm.FunctionApp
{
    public class ConfigValidation: AbstractValidator<IDictionary>
    {
        public ConfigValidation()
        {
            RuleFor(x => x).Custom((config, context) => {
                if (!config.Contains("EventTopic")) context.AddFailure("EventTopic", "EventTopic cannot be empty");

                if (!config.Contains("EventTopicUrl")) context.AddFailure("EventTopicUrl", "EventTopicUrl cannot be empty");

                if (!config.Contains("EventTopicSasKey")) context.AddFailure("EventTopicSasKey", "EventTopicSasKey cannot be empty");
            });
        }
    }
}