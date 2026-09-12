using KnjiznicaAPI.Data;
using KnjiznicaAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Zanrovi>>> GetZanrovi()
        {
            var zanrovi = await _context.Zanrovi
                .Include(z => z.Knjige)
                .ToListAsync();
            return Ok(zanrovi);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Zanrovi>> GetZanr(int id)
        {
            var zanr = await _context.Zanrovi
                .Include(z => z.Knjige)
                .FirstOrDefaultAsync(z => z.Id == id);

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