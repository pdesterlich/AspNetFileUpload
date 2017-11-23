using System;
using System.IO;
using System.Linq;
using AspNetFileUpload.Helpers;
using AspNetFileUpload.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

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

                using (var destStream = new MemoryStream())
                {
                    image.Clone(x => x.Resize(new ResizeOptions()
                    {
                        Size = new Size(800, 800),
                        Mode = ResizeMode.Pad
                    })).SaveAsPng(destStream); 

                    var newFoto = new Fotografia
                    {
                        Chiave = fotografia.Chiave,
                        Content = destStream.ToArray(),
                        FileName = fotografia.FileName,
                        ContentType = "image/png",
                        Dimensione = Dimensione.Media
                    };
                    dbContext.Fotografie.Add(newFoto);

                    Console.WriteLine("aggiunta media");
                }
                
                using (var destStream = new MemoryStream())
                {
                    image.Clone(x => x.Resize(new ResizeOptions()
                    {
                        Size = new Size(200, 200),
                        Mode = ResizeMode.Pad
                    })).SaveAsPng(destStream); 

                    var newFoto = new Fotografia
                    {
                        Chiave = fotografia.Chiave,
                        Content = destStream.ToArray(),
                        FileName = fotografia.FileName,
                        ContentType = "image/png",
                        Dimensione = Dimensione.Piccola
                    };
                    dbContext.Fotografie.Add(newFoto);

                    Console.WriteLine("aggiunta piccola");
                }

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