using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetFileUpload.Enums;
using AspNetFileUpload.Helpers;
using AspNetFileUpload.Models;
using AspNetFileUpload.Rabbit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AspNetFileUpload.Controllers
{
    [Route("api/fotografie")]
    public class FotografieController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IMessageQueueAccessLayer _rabbit;
        private readonly IConfiguration _config;

        public FotografieController(
            DatabaseContext context,
            IMessageQueueAccessLayer rabbit,
            IConfiguration config)
        {
            _context = context;
            _rabbit = rabbit;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var fotografie = await _context.Fotografie.Select(x => new {x.Chiave, x.Dimensione, x.FileName}).ToListAsync();

            return Ok(fotografie);
        }

        [HttpGet("{chiave}/{dimensione}")]
        public async Task<IActionResult> Get(int chiave, Dimensione dimensione)
        {
            var fotografia = await _context.Fotografie.FirstOrDefaultAsync(x => x.Chiave == chiave && x.Dimensione == dimensione);

            if (fotografia == null)
                return NotFound();

            switch (Enum.Parse<StorageType>(_config["Files:Storage"].Default("database"), true))
            {
                case StorageType.Database:
                    var stream = new MemoryStream(fotografia.Content);

                    return new FileStreamResult(stream, fotografia.ContentType);
                case StorageType.File:
                    if (!System.IO.File.Exists(fotografia.Path))
                        return NotFound();
                    
                    var memory = new MemoryStream();
                    using (var fileStream = new FileStream(fotografia.Path, FileMode.Open))
                    {
                        await fileStream.CopyToAsync(memory);
                    }
                    memory.Position = 0;
                    return File(memory, fotografia.ContentType, fotografia.FileName);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [HttpPost("{chiave}")]
        public async Task<IActionResult> Post(int chiave, [FromForm] IFormFile file)
        {            
            if (chiave == 0)
                return BadRequest();
            
            if (!file.ContentType.ToLower().StartsWith("image/"))
                return BadRequest();

            _context.Database.ExecuteSqlCommand($"DELETE FROM Fotografie WHERE Chiave = {chiave}");
            
            var fotografia = new Fotografia
            {
                Chiave = chiave,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Dimensione = Dimensione.Originale
            };
            
            switch (Enum.Parse<StorageType>(_config["Files:Storage"].Default("database"), true))
            {
                case StorageType.Database:

                    var stream = new MemoryStream();
                    file.OpenReadStream().CopyTo(stream);

                    fotografia.Content = stream.ToArray();

                    break;
                case StorageType.File:
                    var storagePath = _config["Files:Path"].Default("");
                    var path = Path.Combine(storagePath, "Fotografie");
                    Directory.CreateDirectory(path);

                    var filePath = Path.Combine(path, $"{chiave}_{Dimensione.Originale}{Path.GetExtension(file.FileName)}");
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    fotografia.Path = filePath;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await _context.Fotografie.AddAsync(fotografia);

            await _context.SaveChangesAsync();

            _rabbit.SendAction(new ProcessImageAction {Chiave = chiave});

            return Ok();
        }
    }
}