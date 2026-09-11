namespace KnjiznicaAPI.Models
{
    public class Zanrovi
    {
        public int Id { get; set; }
        public string imeZanra { get; set; } = string.Empty;
        public ICollection<Knjige> Knjige { get; set; } = new List<Knjige>(); //jedan zanr moze imati vise knjiga 
    }
}
