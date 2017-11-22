namespace AspNetFileUpload.Rabbit
{
    public interface IMessageQueueAccessLayer
    {
        bool SendAction(IMessageQueueBaseAction action);
    }
}