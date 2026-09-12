namespace KnjiznicaAPI.DTOs
{
    public class ZanrDto
    {
        public int Id { get; set; }
        public string ImeZanra { get; set; } = string.Empty;
        public List<string> Knjige { get; set; } = new List<string>();
    }
}