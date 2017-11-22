using AspNetFileUpload.Models;

namespace AspNetFileUpload.Rabbit
{
    public static class ActionExecuter
    {
        public static bool Execute(DatabaseContext dbContext, IMessageQueueBaseAction action)
        {
            var ts = new TypeSwitch();
            ts.Case((ProcessImageAction x) => ProcessImageActionExecuter.Execute(dbContext, x));

            return ts.Switch(action);   
        }
    }
}