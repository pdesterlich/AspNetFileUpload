using JsonSubTypes;
using Newtonsoft.Json;

namespace AspNetFileUpload.Rabbit
{
    [JsonConverter(typeof(JsonSubtypes), "Action")]
    public interface IMessageQueueBaseAction
    {
        string Action { get; }
    }
}