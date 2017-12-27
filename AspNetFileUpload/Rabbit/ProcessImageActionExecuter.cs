using System;
using System.IO;
using System.Linq;
using AspNetFileUpload.Enums;
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
            var fotografia =
                dbContext.Fotografie.FirstOrDefault(x =>
                    x.Chiave == action.Chiave && x.Dimensione == Dimensione.Originale);

            Console.WriteLine($"elaborazione immagini: {action.Chiave}");

            if (fotografia != null)
            {
                var storageType = string.IsNullOrEmpty(fotografia.Path) ? StorageType.Database : StorageType.File;
                var basePath = "";
                Image<Rgba32> image;
                if (!string.IsNullOrEmpty(fotografia.Path) && File.Exists(fotografia.Path))
                {
                    basePath = Path.GetDirectoryName(fotografia.Path);
                    using (var fileStream = new FileStream(fotografia.Path, FileMode.Open))
                    {
                        image = Image.Load(fileStream);
                    }
                }
                else
                {
                    image = Image.Load(new MemoryStream(fotografia.Content));
                }

                var media = image.Clone(x => x.Resize(new ResizeOptions()
                {
                    Size = new Size(800, 800),
                    Mode = ResizeMode.Pad
                }));

                var newFoto = new Fotografia
                {
                    Chiave = fotografia.Chiave,
                    FileName = fotografia.FileName,
                    ContentType = "image/png",
                    Dimensione = Dimensione.Media
                };

                if (storageType == StorageType.File)
                {
                    var filePath = Path.Combine(basePath, $"{fotografia.Chiave}_{Dimensione.Media}.png");
                    if (File.Exists(filePath)) File.Delete(filePath);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        media.SaveAsPng(fileStream);
                    }

                    newFoto.Path = filePath;
                }
                else
                {
                    using (var destStream = new MemoryStream())
                    {
                        media.SaveAsPng(destStream);
                        newFoto.Content = destStream.ToArray();
                    }
                }

                dbContext.Fotografie.Add(newFoto);

                Console.WriteLine("aggiunta media");

                /*
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
                */
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