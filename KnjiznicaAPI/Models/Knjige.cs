namespace KnjiznicaAPI.Models
{
    public class Knjige
    {
        public int Id { get; set; }
        public string nazivKnjige { get; set; } = string.Empty;
        public int AutorKnjigeId { get; set; }
        public AutorKnjige? AutorKnjige { get; set; } //1:N relation with book author - one book has one author, one author can have multiple books
        public ICollection<Zanrovi> Zanrovi { get; set; } = new List<Zanrovi>(); //N:N relation with genres - one book can have multiple genres, one genre can have multiple books
        public DateTime datumUnosa { get; set; }
    }
}
