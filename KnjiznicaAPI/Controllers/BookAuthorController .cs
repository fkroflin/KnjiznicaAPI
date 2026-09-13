using KnjiznicaAPI.Data;
using KnjiznicaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnjiznicaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookAuthorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookAuthorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AutorKnjige>>> GetBookAuthors()
        {
            var autori = await _context.AutoriKnjiga.ToListAsync();
            return Ok(autori);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AutorKnjige>> GetBookAuthor(int id)
        {
            var autor = await _context.AutoriKnjiga.FindAsync(id);

            if (autor == null)
            {
                return NotFound($"Autor s ID-em {id} nije pronađen.");
            }

            return Ok(autor);
        }

        [HttpPost]
        public async Task<ActionResult<AutorKnjige>> CreateAuthor(string imeAutora, int godinaRodenja)
        {
            if (string.IsNullOrWhiteSpace(imeAutora))
            {
                return BadRequest("Ime autora ne smije biti prazno.");
            }

            // Case-insensitive check to prevent adding existing authors with the same name
            var autorPostoji = await _context.AutoriKnjiga
                .AnyAsync(a => a.imeAutora.ToLower() == imeAutora.Trim().ToLower());

            if (autorPostoji)
            {
                return BadRequest($"Autor s imenom '{imeAutora}' već postoji u bazi.");
            }

            var noviAutor = new AutorKnjige
            {
                imeAutora = imeAutora.Trim(),
                godinaRodenja = godinaRodenja
            };

            _context.AutoriKnjiga.Add(noviAutor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBookAuthor), new { id = noviAutor.Id }, noviAutor);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookAuthor(int id)
        {
            var autor = await _context.AutoriKnjiga.FindAsync(id);

            if (autor == null)
            {
                return NotFound($"Autor s ID-em {id} ne postoji u bazi.");
            }

            _context.AutoriKnjiga.Remove(autor);
            await _context.SaveChangesAsync();

            return Ok($"Autor s ID-em {id} je uspješno obrisan.");
        }
    }
}