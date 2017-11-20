namespace AspNetFileUpload.Models
{
    public class Fotografia
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
    }
}