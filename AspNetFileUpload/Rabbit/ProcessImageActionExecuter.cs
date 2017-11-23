using System;
using System.IO;
using System.Linq;
using AspNetFileUpload.Helpers;
using AspNetFileUpload.Models;
using SixLabors.ImageSharp;

namespace AspNetFileUpload.Rabbit
{
    public static class ProcessImageActionExecuter
    {
        public static bool Execute(DatabaseContext dbContext, ProcessImageAction action)
        {
            var fotografia = dbContext.Fotografie.FirstOrDefault(x => x.Chiave == action.Chiave && x.Dimensione == Dimensione.Originale);

            Console.WriteLine($"elaborazione immagini: {action.Chiave}");

            if (fotografia != null)
            {
                var stream = new MemoryStream(fotografia.Content);
                var image = Image.Load(stream);

                var newSize = ImageHelpers.GetNewSize(800, image.Width, image.Height);
                
                image.Mutate(x => x.Resize(newSize.Item1, newSize.Item2));

                var streamMedio = new MemoryStream();
                image.SaveAsJpeg(streamMedio);

                var media = new Fotografia
                {
                    Chiave = fotografia.Chiave,
                    Content = streamMedio.ToArray(),
                    FileName = fotografia.FileName,
                    ContentType = "image/jpeg",
                    Dimensione = Dimensione.Media
                };
                dbContext.Fotografie.Add(media);

                Console.WriteLine($"aggiunta media: {newSize.Item1} - {newSize.Item2}");

                newSize = ImageHelpers.GetNewSize(200, image.Width, image.Height);
                
                image.Mutate(x => x.Resize(newSize.Item1, newSize.Item2));

                var streamPiccolo = new MemoryStream();
                image.SaveAsJpeg(streamPiccolo);

                var piccola = new Fotografia
                {
                    Chiave = fotografia.Chiave,
                    Content = streamPiccolo.ToArray(),
                    FileName = fotografia.FileName,
                    ContentType = "image/jpeg",
                    Dimensione = Dimensione.Piccola
                };
                dbContext.Fotografie.Add(piccola);

                Console.WriteLine($"aggiunta piccola: {newSize.Item1} - {newSize.Item2}");

                dbContext.SaveChanges();
            }
            else
            {
                Console.WriteLine("fotografia non trovata");
            }

            return true;
        }
    }
}