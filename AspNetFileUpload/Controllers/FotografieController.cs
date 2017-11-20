using System.IO;
using System.Threading.Tasks;
using AspNetFileUpload.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetFileUpload.Controllers
{
    [Route("api/fotografie")]
    public class FotografieController: Controller
    {
        private readonly DatabaseContext _context;

        public FotografieController(DatabaseContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var fotografie = await _context.Fotografie.ToListAsync(); 
            return Ok(fotografie);
        }

        [HttpGet("{id}")]
        public async Task<FileStreamResult> Get(int id)
        {
            var fotografia = await _context.Fotografie.FirstOrDefaultAsync(x => x.Id == id);

            // TODO: aggiungere gestione errore
            
            var stream = new MemoryStream(fotografia.Content);
            
            return new FileStreamResult(stream, fotografia.ContentType);
        }

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            if (!file.ContentType.ToLower().StartsWith("image/"))
                return BadRequest();
            
            var stream = new MemoryStream();
            file.OpenReadStream().CopyTo(stream);

            var fotografia = new Fotografia
            {
                Content = stream.ToArray(),
                FileName = file.Name,
                ContentType = file.ContentType
            };

            await _context.Fotografie.AddAsync(fotografia);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}