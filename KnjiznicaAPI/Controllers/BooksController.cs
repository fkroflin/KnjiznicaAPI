using KnjiznicaAPI.Data;
using KnjiznicaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnjiznicaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Knjige>>> GetBooks()
        {
            var knjige = await _context.Knjige
                .Include(k => k.AutorKnjige)
                .Include(k => k.Zanrovi)
                .ToListAsync();

            return Ok(knjige);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Knjige>> GetBook(int id)
        {
            var knjiga = await _context.Knjige
                .Include(k => k.AutorKnjige)
                .Include(k => k.Zanrovi)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (knjiga == null)
            {
                return NotFound($"Knjiga s ID-em {id} nije pronađena.");
            }

            return Ok(knjiga);
        }

        [HttpPost]
        public async Task<ActionResult<Knjige>> CreateBook(string nazivKnjige, int autorKnjigeId, [FromQuery] List<int> zanrIds)
        {
            if (string.IsNullOrWhiteSpace(nazivKnjige))
            {
                return BadRequest("Naziv knjige ne smije biti prazan.");
            }

            var autorPostoji = await _context.AutoriKnjiga.AnyAsync(a => a.Id == autorKnjigeId);
            if (!autorPostoji)
            {
                return BadRequest($"Autor s ID-em {autorKnjigeId} ne postoji u bazi.");
            }

            var odabraniZanrovi = await _context.Zanrovi
                .Where(z => zanrIds.Contains(z.Id))
                .ToListAsync();

            if (odabraniZanrovi.Count != zanrIds.Distinct().Count())
            {
                return BadRequest("Jedan ili više poslanih žanr ID-eva ne postoje u bazi.");
            }

            var novaKnjiga = new Knjige
            {
                nazivKnjige = nazivKnjige,
                AutorKnjigeId = autorKnjigeId,
                datumUnosa = DateTime.UtcNow,
                Zanrovi = odabraniZanrovi
            };

            _context.Knjige.Add(novaKnjiga);
            await _context.SaveChangesAsync();

            // Učitavamo potpunu knjigu s navigacijskim svojstvima za odgovor
            var kreiranaKnjiga = await _context.Knjige
                .Include(k => k.AutorKnjige)
                .Include(k => k.Zanrovi)
                .FirstOrDefaultAsync(k => k.Id == novaKnjiga.Id);

            return CreatedAtAction(nameof(GetBook), new { id = novaKnjiga.Id }, kreiranaKnjiga);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, string nazivKnjige, int autorKnjigeId, [FromQuery] List<int> zanrIds)
        {
            var knjiga = await _context.Knjige
                .Include(k => k.Zanrovi)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (knjiga == null)
            {
                return NotFound($"Knjiga s ID-em {id} ne postoji u bazi.");
            }

            if (string.IsNullOrWhiteSpace(nazivKnjige))
            {
                return BadRequest("Naziv knjige ne smije biti prazan.");
            }

            var autorPostoji = await _context.AutoriKnjiga.AnyAsync(a => a.Id == autorKnjigeId);
            if (!autorPostoji)
            {
                return BadRequest($"Autor s ID-em {autorKnjigeId} ne postoji u bazi.");
            }

            var odabraniZanrovi = await _context.Zanrovi
                .Where(z => zanrIds.Contains(z.Id))
                .ToListAsync();

            if (odabraniZanrovi.Count != zanrIds.Distinct().Count())
            {
                return BadRequest("Jedan ili više poslanih žanr ID-eva ne postoje u bazi.");
            }

            knjiga.nazivKnjige = nazivKnjige;
            knjiga.AutorKnjigeId = autorKnjigeId;
            knjiga.Zanrovi = odabraniZanrovi; 

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var knjiga = await _context.Knjige.FindAsync(id);

            if (knjiga == null)
            {
                return NotFound($"Knjiga s ID-em {id} ne postoji u bazi.");
            }

            _context.Knjige.Remove(knjiga);
            await _context.SaveChangesAsync();

            return Ok($"Knjiga s ID-em {id} je uspješno obrisana.");
        }
    }
}