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
        public async Task<ActionResult<AutorKnjige>> CreateBookAuthor(string authorName, int yearOfBirth)
        {
            if (string.IsNullOrWhiteSpace(authorName))
            {
                return BadRequest("Ime autora ne smije biti prazno.");
            }

            var noviAutor = new AutorKnjige
            {
                imeAutora = authorName,
                godinaRodenja = yearOfBirth
            };

            _context.AutoriKnjiga.Add(noviAutor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBookAuthor), new { id = noviAutor.Id }, noviAutor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBookAuthor(int id, string authorName, int yearOfBirth)
        {
            var autor = await _context.AutoriKnjiga.FindAsync(id);

            if (autor == null)
            {
                return NotFound($"Autor s ID-em {id} ne postoji u bazi.");
            }

            if (string.IsNullOrWhiteSpace(authorName))
            {
                return BadRequest("Ime autora ne smije biti prazno.");
            }

            autor.imeAutora = authorName;
            autor.godinaRodenja = yearOfBirth;

            await _context.SaveChangesAsync();

            return NoContent(); 
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