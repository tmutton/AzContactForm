namespace AzContactForm.FunctionApp
{
    //https://docs.microsoft.com/en-us/azure/event-grid/event-schema

    public class Event<T>
    {
        public string Topic { get; set; }

        public string Subject { get; set; }

        public string EventType { get; set; }

        public string EventTime { get; set; }

        public string Id { get; set; }

        public T Data { get; set; }
    }
}
