public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public DateTime ReadingDate { get; set; }
    public int UserId { get; set; }
    public string Status { get; set; } = "Active";
    public int? Rating { get; set; } // Kitaba puan vermek için nullable int
    public ICollection<Note> Notes { get; set; } // Kitaba ait notlar
}
