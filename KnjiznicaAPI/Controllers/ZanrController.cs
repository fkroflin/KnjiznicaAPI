using KnjiznicaAPI.Data;
using KnjiznicaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KnjiznicaAPI.DTOs;

namespace KnjiznicaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZanrController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ZanrController(AppDbContext context)
        {
            _context = context;
        }

        // Maps genres to ZanrDto to prevent cycle serialization errors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ZanrDto>>> GetZanrovi()
        {
            var zanrovi = await _context.Zanrovi
                .Select(z => new ZanrDto
                {
                    Id = z.Id,
                    ImeZanra = z.imeZanra,
                    // Selects only the title strings of connected books
                    Knjige = z.Knjige.Select(k => k.nazivKnjige).ToList()
                })
                .ToListAsync();

            return Ok(zanrovi);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Zanrovi>> GetZanr(int id)
        {
            // Retrieves a single genre with connected book titles mapped to ZanrDto
            var zanr = await _context.Zanrovi.
                Where(z => z.Id == id)
                .Select(z => new ZanrDto
                {
                    Id = z.Id,
                    ImeZanra = z.imeZanra,
                    Knjige = z.Knjige.Select(k => k.nazivKnjige).ToList()
                })
                .FirstOrDefaultAsync();

            if (zanr == null)
            {
                return NotFound($"Žanr s ID-em {id} nije pronađen.");
            }

            return Ok(zanr);
        }

        [HttpPost]
        public async Task<ActionResult<Zanrovi>> CreateZanr(string imeZanra)
        {
            if (string.IsNullOrWhiteSpace(imeZanra))
            {
                return BadRequest("Naziv žanra ne smije biti prazan.");
            }

            var noviZanr = new Zanrovi
            {
                imeZanra = imeZanra
            };

            _context.Zanrovi.Add(noviZanr);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetZanr), new { id = noviZanr.Id }, noviZanr);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateZanr(int id, string imeZanra)
        {
            var zanr = await _context.Zanrovi.FindAsync(id);

            if (zanr == null)
            {
                return NotFound($"Žanr s ID-em {id} ne postoji u bazi.");
            }

            if (string.IsNullOrWhiteSpace(imeZanra))
            {
                return BadRequest("Naziv žanra ne smije biti prazan.");
            }

            zanr.imeZanra = imeZanra;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteZanr(int id)
        {
            var zanr = await _context.Zanrovi.FindAsync(id);

            if (zanr == null)
            {
                return NotFound($"Žanr s ID-em {id} ne postoji u bazi.");
            }

            _context.Zanrovi.Remove(zanr);
            await _context.SaveChangesAsync();

            return Ok($"Žanr s ID-em {id} je uspješno obrisan.");
        }
    }
}