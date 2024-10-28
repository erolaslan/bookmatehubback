public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int UserId { get; set; } // User sınıfı yerine UserId
    public string Status { get; set; } = "Active"; // Varsayılan olarak "Active"
}
