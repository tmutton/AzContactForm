using System;

namespace AzContactForm
{
    public class Message: IMessage
    {
        public Message(string name, string email, string messageBody)
        {
            Name = name;
            Email = email;
            MessageBody = messageBody;
            Created = DateTimeOffset.Now;
        }

        public Message(string name, string email, string messageBody, DateTimeOffset created): 
            this(name, email, messageBody)
        {
            Created = created;
        }

        public string Name { get; set; }

        public string Email { get; set; }

        public string MessageBody { get; set; }

        public DateTimeOffset Created { get; set; }
    }
}
