namespace KnjiznicaAPI.DTOs
{
    public class KnjigaDto
    {
        public int Id { get; set; }
        public string NazivKnjige { get; set; } = string.Empty;
        public DateTime DatumUnosa { get; set; }
        public string? ImeAutora { get; set; }
        public List<string> Zanrovi { get; set; } = new List<string>();
    }
}