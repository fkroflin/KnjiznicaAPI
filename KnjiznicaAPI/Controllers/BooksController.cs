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
        public async Task<ActionResult<KnjigaDto>> CreateBook(string nazivKnjige, string imeAutora, [FromQuery] List<string> zanrovi)
        {
            if (string.IsNullOrWhiteSpace(nazivKnjige))
            {
                return BadRequest("Naziv knjige ne smije biti prazan.");
            }

            if (string.IsNullOrWhiteSpace(imeAutora))
            {
                return BadRequest("Ime autora ne smije biti prazno.");
            }

            var cleanBookTitle = nazivKnjige.Trim();
            var cleanAuthorName = imeAutora.Trim();

            var author = await _context.AutoriKnjiga
                .FirstOrDefaultAsync(a => a.imeAutora.ToLower() == imeAutora.Trim().ToLower());

            if (author == null)
            {
                author = new AutorKnjige { imeAutora = cleanAuthorName };
                _context.AutoriKnjiga.Add(author);
            }

            // 2. Find or create Genres
            var bookGenres = new List<Zanrovi>();
            if (zanrovi != null && zanrovi.Any())
            {
                foreach (var zanrName in zanrovi)
                {
                    if (string.IsNullOrWhiteSpace(zanrName)) continue;

                    var cleanZanrName = zanrName.Trim();
                    var genre = await _context.Zanrovi
                        .FirstOrDefaultAsync(z => z.imeZanra.ToLower() == cleanZanrName.ToLower());

                    if (genre == null)
                    {
                        genre = new Zanrovi { imeZanra = cleanZanrName };
                        _context.Zanrovi.Add(genre);
                    }

                    bookGenres.Add(genre);
                }
            }

            // 3. Create Book
            var newBook = new Knjige
            {
                nazivKnjige = cleanBookTitle,
                AutorKnjige = author,
                Zanrovi = bookGenres
            };

            _context.Knjige.Add(newBook);
            await _context.SaveChangesAsync();

            var resultDto = new KnjigaDto
            {
                Id = newBook.Id,
                NazivKnjige = newBook.nazivKnjige,
                ImeAutora = author.imeAutora,
                Zanrovi = bookGenres.Select(z => z.imeZanra).ToList()
            };

            return CreatedAtAction(nameof(GetBook), new { id = newBook.Id }, resultDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, string? nazivKnjige, string? imeAutora, [FromQuery] List<string>? zanrovi)
        {
            // Find existing book in DB, INCLUDING its connected genres list
            var knjiga = await _context.Knjige
                .Include(k => k.Zanrovi)
                .Include(k => k.AutorKnjige)
                .FirstOrDefaultAsync(k => k.Id == id);

            if (knjiga == null)
            {
                return NotFound($"Knjiga s ID-em {id} ne postoji u bazi.");
            }

            if (!string.IsNullOrWhiteSpace(nazivKnjige))
            {
                knjiga.nazivKnjige = nazivKnjige.Trim();
            }

            if (!string.IsNullOrWhiteSpace(imeAutora))
            {
                var cleanAutorName = imeAutora.Trim();
                var autor = await _context.AutoriKnjiga
                    .FirstOrDefaultAsync(a => a.imeAutora.ToLower() == cleanAutorName.ToLower());

                if (autor == null)
                {
                    autor = new AutorKnjige { imeAutora = cleanAutorName };
                    _context.AutoriKnjiga.Add(autor);
                }

                knjiga.AutorKnjige = autor;
            }


            if (zanrovi != null && zanrovi.Any())
            {
                foreach (var zanrName in zanrovi)
                {
                    if (string.IsNullOrWhiteSpace(zanrName)) continue;

                    var cleanZanrName = zanrName.Trim();

                    // Find the genre in the database or create it if it doesn't exist
                    var genre = await _context.Zanrovi
                        .FirstOrDefaultAsync(z => z.imeZanra.ToLower() == cleanZanrName.ToLower());

                    if (genre == null)
                    {
                        genre = new Zanrovi { imeZanra = cleanZanrName };
                        _context.Zanrovi.Add(genre);
                    }

                    // Add a genre to the book only if it doesn't already have one
                    if (!knjiga.Zanrovi.Any(z => z.imeZanra.ToLower() == cleanZanrName.ToLower()))
                    {
                        knjiga.Zanrovi.Add(genre);
                    }
                }
            }

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