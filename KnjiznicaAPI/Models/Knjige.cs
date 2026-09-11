namespace KnjiznicaAPI.Models
{
    public class Knjige
    {
        public int Id { get; set; }
        public string nazivKnjige { get; set; } = string.Empty;
        public int AutorKnjigeId { get; set; }
        public AutorKnjige? AutorKnjige { get; set; } //relacija 1:N s autorom knjige - jedna knjiga ima jednog autora, jedan autor moze imati vise knjiga
        public ICollection<Zanrovi> Zanrovi { get; set; } = new List<Zanrovi>(); //relacija N:N s zanrovima - jedna knjiga moze imati vise zanrova, jedan zanr moze imati vise knjiga
        public DateTime datumUnosa { get; set; }
    }
}
