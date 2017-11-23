namespace AspNetFileUpload.Rabbit
{
    public class ProcessImageAction: IMessageQueueBaseAction
    {
        public string Action { get; } = "ProcessImageAction";
        public int Chiave { get; set; }
    }
}