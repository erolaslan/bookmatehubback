public class Note
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime Date { get; set; }
    public int BookId { get; set; }
    public Book Book { get; set; }
}
