namespace Week5RazorApp.Models
{
    public class ClassInformationTable
    {      public int Id { get; set; }  // ✅ Bunu ekle!
        public string ClassName { get; set; } = string.Empty; // Varsayılan değer eklendi
        public int StudentCount { get; set; }
        public string Description { get; set; } = string.Empty; // Varsayılan değer eklendi
    }
}