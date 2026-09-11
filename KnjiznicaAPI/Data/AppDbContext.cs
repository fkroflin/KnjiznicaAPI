using KnjiznicaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KnjiznicaAPI.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<AutorKnjige> AutoriKnjiga { get; set; }
        public DbSet<Zanrovi> Zanrovi { get; set; }
        public DbSet<Knjige> Knjige { get; set; }
    }
}
