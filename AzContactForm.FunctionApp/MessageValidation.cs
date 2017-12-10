using FluentValidation;

namespace AzContactForm.FunctionApp
{
    public class MessageValidation : AbstractValidator<Message>
    {
        public const int MaximumNameLength = 50;

        public const int MaximumEmailLength = 254;

        public const int MaximumMessageLength = 1000;

        public MessageValidation()
        {
            // name
            RuleFor(message => message.Name).NotEmpty().WithMessage("Name cannot be empty");
            RuleFor(message => message.Name).MaximumLength(MaximumNameLength).WithMessage(string.Format("Name length cannot exceed {0} characters", MaximumNameLength));

            // email
            RuleFor(message => message.Email).EmailAddress().When(x => x.Email.Length > 0).WithMessage("Please enter a valid email address");
            RuleFor(message => message.Email).MaximumLength(MaximumEmailLength).WithMessage("Email addresses cannot exceed {0} characters");

            // message
            RuleFor(message => message.MessageBody).NotEmpty().WithMessage("Message cannot be empty");
            RuleFor(message => message.MessageBody).MaximumLength(MaximumMessageLength).WithMessage(string.Format("Message length cannot exceed {0} characters", MaximumMessageLength));
        }
    }
}
