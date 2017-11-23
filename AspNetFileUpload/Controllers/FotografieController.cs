using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetFileUpload.Models;
using AspNetFileUpload.Rabbit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetFileUpload.Controllers
{
    [Route("api/fotografie")]
    public class FotografieController : Controller
    {
        private readonly DatabaseContext _context;
        private readonly IMessageQueueAccessLayer _rabbit;

        public FotografieController(
            DatabaseContext context,
            IMessageQueueAccessLayer rabbit)
        {
            _context = context;
            _rabbit = rabbit;
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

            var stream = new MemoryStream(fotografia.Content);

            return new FileStreamResult(stream, fotografia.ContentType);
        }

        [HttpPost("{chiave}")]
        public async Task<IActionResult> Post(int chiave, [FromForm] IFormFile file)
        {
            _context.Database.ExecuteSqlCommand($"DELETE FROM Fotografie WHERE Chiave = {chiave}");
            
            if (chiave == 0)
                return BadRequest();
            
            if (!file.ContentType.ToLower().StartsWith("image/"))
                return BadRequest();

            var stream = new MemoryStream();
            file.OpenReadStream().CopyTo(stream);

            var fotografia = new Fotografia
            {
                Chiave = chiave,
                Content = stream.ToArray(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                Dimensione = Dimensione.Originale
            };

            await _context.Fotografie.AddAsync(fotografia);

            await _context.SaveChangesAsync();

            _rabbit.SendAction(new ProcessImageAction {Chiave = fotografia.Id});

            return Ok();
        }
    }
}