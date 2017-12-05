using System;

namespace AzContactForm
{
    public interface IMessage
    {
        string Name { get; set; }

        string Email { get; set; }

        string MessageBody { get; set; }

        DateTimeOffset Created { get; set; }
    }
}
