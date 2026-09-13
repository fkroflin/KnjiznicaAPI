using KnjiznicaAPI.Data;
using KnjiznicaAPI.DTOs;
using KnjiznicaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnjiznicaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        // Connection to the SQL database via Entity Framework Core
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<KnjigaDto>>> GetBooks()
        {
            var knjige = await _context.Knjige
                .Select(k => new KnjigaDto
                {
                    Id = k.Id,
                    NazivKnjige = k.nazivKnjige,
                    DatumUnosa = k.datumUnosa,
                    ImeAutora = k.AutorKnjige != null ? k.AutorKnjige.imeAutora : "Nepoznat autor",
                    Zanrovi = k.Zanrovi.Select(z => z.imeZanra).ToList()
                })
                .ToListAsync();

            return Ok(knjige);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<KnjigaDto>> GetBook(int id)
        {
            // Search for one book matching the given ID and transform it to KnjigaDto
            var knjiga = await _context.Knjige
                .Where(k => k.Id == id)
                .Select(k => new KnjigaDto
                {
                    Id = k.Id,
                    NazivKnjige = k.nazivKnjige,
                    DatumUnosa = k.datumUnosa,
                    ImeAutora = k.AutorKnjige != null ? k.AutorKnjige.imeAutora : "Nepoznat autor",
                    Zanrovi = k.Zanrovi.Select(z => z.imeZanra).ToList()
                })
                .FirstOrDefaultAsync();

            if (knjiga == null)
            {
                return NotFound($"Knjiga s ID-em {id} nije pronađena.");
            }

            return Ok(knjiga);
        }

        [HttpPost]
        public async Task<ActionResult<KnjigaDto>> CreateBook(string nazivKnjige, string imeAutora, [FromQuery] List<string> imenaZanrova)
        {
            if (string.IsNullOrWhiteSpace(nazivKnjige))
            {
                return BadRequest("Naziv knjige ne smije biti prazan.");
            }

            if (string.IsNullOrWhiteSpace(imeAutora))
            {
                return BadRequest("Ime autora ne smije biti prazno.");
            }

            var autor = await _context.AutoriKnjiga
                .FirstOrDefaultAsync(a => a.imeAutora.ToLower() == imeAutora.Trim().ToLower());

            if (autor == null)
            {
                autor = new AutorKnjige
                {
                    imeAutora = imeAutora.Trim(),
                    godinaRodenja = 0 // Podrazumijevana vrijednost ako autor još ne postoji
                };
                _context.AutoriKnjiga.Add(autor);
                await _context.SaveChangesAsync();
            }

            var odabraniZanrovi = new List<Zanrovi>();
            foreach (var imeZanra in imenaZanrova.Distinct())
            {
                if (string.IsNullOrWhiteSpace(imeZanra)) continue;

                var cistoIme = imeZanra.Trim();

                // Search if this genre already exists in the database
                var zanr = await _context.Zanrovi
                    .FirstOrDefaultAsync(z => z.imeZanra.ToLower() == cistoIme.ToLower());

                if (zanr == null)
                {
                    zanr = new Zanrovi { imeZanra = cistoIme };
                    _context.Zanrovi.Add(zanr);
                    await _context.SaveChangesAsync();
                }

                odabraniZanrovi.Add(zanr);
            }

            var novaKnjiga = new Knjige
            {
                nazivKnjige = nazivKnjige.Trim(),
                AutorKnjigeId = autor.Id,
                datumUnosa = DateTime.UtcNow,
                Zanrovi = odabraniZanrovi
            };

            _context.Knjige.Add(novaKnjiga);
            await _context.SaveChangesAsync();

            var rezultatDto = new KnjigaDto
            {
                Id = novaKnjiga.Id,
                NazivKnjige = novaKnjiga.nazivKnjige,
                DatumUnosa = novaKnjiga.datumUnosa,
                ImeAutora = autor.imeAutora,
                Zanrovi = odabraniZanrovi.Select(z => z.imeZanra).ToList()
            };

            return CreatedAtAction(nameof(GetBook), new { id = novaKnjiga.Id }, rezultatDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, string nazivKnjige, string imeAutora, [FromQuery] List<string> imenaZanrova)
        {
            // Find existing book in DB, INCLUDING its connected genres list
            var knjiga = await _context.Knjige
                .Include(k => k.Zanrovi)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (knjiga == null)
            {
                return NotFound($"Knjiga s ID-em {id} ne postoji u bazi.");
            }

            if (string.IsNullOrWhiteSpace(nazivKnjige) || string.IsNullOrWhiteSpace(imeAutora))
            {
                return BadRequest("Naziv knjige i ime autora ne smiju biti prazni.");
            }

            var autor = await _context.AutoriKnjiga
                .FirstOrDefaultAsync(a => a.imeAutora.ToLower() == imeAutora.Trim().ToLower());

            if (autor == null)
            {
                autor = new AutorKnjige
                {
                    imeAutora = imeAutora.Trim(),
                    godinaRodenja = 0
                };
                _context.AutoriKnjiga.Add(autor);
                await _context.SaveChangesAsync();
            }

            var noviZanrovi = new List<Zanrovi>();
            foreach (var imeZanra in imenaZanrova.Distinct())
            {
                if (string.IsNullOrWhiteSpace(imeZanra)) continue;

                var cistoIme = imeZanra.Trim();
                var zanr = await _context.Zanrovi
                    .FirstOrDefaultAsync(z => z.imeZanra.ToLower() == cistoIme.ToLower());

                if (zanr == null)
                {
                    zanr = new Zanrovi { imeZanra = cistoIme };
                    _context.Zanrovi.Add(zanr);
                    await _context.SaveChangesAsync();
                }

                noviZanrovi.Add(zanr);
            }

            knjiga.nazivKnjige = nazivKnjige.Trim();
            knjiga.AutorKnjigeId = autor.Id;
            knjiga.Zanrovi = noviZanrovi;

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

            // Mark book for deletion and save changes
            _context.Knjige.Remove(knjiga);
            await _context.SaveChangesAsync();

            return Ok($"Knjiga s ID-em {id} je uspješno obrisana.");
        }
    }
}