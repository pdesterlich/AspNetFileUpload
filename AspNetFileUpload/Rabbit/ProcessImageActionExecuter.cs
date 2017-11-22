using System;
using AspNetFileUpload.Models;

namespace AspNetFileUpload.Rabbit
{
    public static class ProcessImageActionExecuter
    {
        public static bool Execute(DatabaseContext dbContext, ProcessImageAction action)
        {
            // TODO: elaborazione immagini
            Console.WriteLine("elaborazione immagini");
            return true;
        }
    }
}