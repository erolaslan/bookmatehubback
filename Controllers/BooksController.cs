using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using BookMateHub.Data;
using BookMateHub.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BookMateHubDbContext _context;

    public BooksController(BookMateHubDbContext context)
    {
        _context = context;
    }

    // Kitap ekleme
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddBook(Book book)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        book.UserId = userId;
        book.Status = "Active"; // Varsayılan olarak "Active" durumu atanır

        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        return Ok("Kitap başarıyla eklendi.");
    }

    // Kullanıcıya ait kitapları listeleme (status parametresi ile kırılım)
    [HttpGet]
    [Authorize]
    public IActionResult GetBooks([FromQuery] string status = "Active")
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        if (status != "Active" && status != "Passive" && status != "Deleted")
        {
            return BadRequest("Geçersiz durum değeri.");
        }

        var books = _context.Books
            .Where(b => b.UserId == userId && b.Status == status)
            .OrderBy(b => b.Id)
            .ToList();
        return Ok(books);
    }

    // Kitap güncelleme
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateBook(int id, Book updatedBook)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var book = await _context.Books.FindAsync(id);
        if (book == null || book.UserId != userId)
        {
            return NotFound("Kitap bulunamadı veya yetkiniz yok.");
        }

        book.Title = updatedBook.Title;
        book.Author = updatedBook.Author;
        book.ReadingDate = updatedBook.ReadingDate;
        book.Status = updatedBook.Status;

        await _context.SaveChangesAsync();
        return Ok("Kitap başarıyla güncellendi.");
    }

    // Kitap silme (statüyü "Deleted" olarak işaretleme)
    [HttpGet("delete")]
    [Authorize]
    public async Task<IActionResult> DeleteBook([FromQuery] int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var book = await _context.Books.FindAsync(id);
        if (book == null || book.UserId != userId)
        {
            return NotFound("Kitap bulunamadı veya yetkiniz yok.");
        }

        book.Status = "Deleted";
        await _context.SaveChangesAsync();
        return NoContent(); // Başarılı güncelleme
    }

    // Kitaba ait notları getirme
    [HttpGet("{id}/notes")]
    [Authorize]
    public async Task<IActionResult> GetNotesForBook(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var notes = await _context.Notes
            .Where(n => n.BookId == id && n.Book.UserId == userId)
            .OrderBy(n => n.Date)
            .ToListAsync();

        return Ok(notes);
    }

    // Kitap puanını güncelleme
    [HttpPut("{id}/rating")]
    [Authorize]
    public async Task<IActionResult> UpdateBookRating(int id, [FromBody] int rating)
    {
        if (rating < 1 || rating > 5)
        {
            return BadRequest("Puan 1 ile 5 arasında olmalıdır.");
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var book = await _context.Books
            .Where(b => b.UserId == userId && b.Id == id)
            .FirstOrDefaultAsync();

        if (book == null)
        {
            return NotFound("Kitap bulunamadı veya yetkiniz yok.");
        }

        book.Rating = rating;
        await _context.SaveChangesAsync();

        return NoContent(); // Başarılı güncelleme
    }

    // Yeni not eklemek için yardımcı sınıf
    public class NoteRequest
    {
        public string Content { get; set; }
    }

    // Kitaba not ekleme
    [HttpPost("{id}/notes")]
    [Authorize]
    public async Task<IActionResult> AddNoteToBook(int id, [FromBody] NoteRequest noteRequest)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var book = await _context.Books
            .Where(b => b.UserId == userId && b.Id == id)
            .FirstOrDefaultAsync();

        if (book == null)
        {
            return NotFound("Kitap bulunamadı veya yetkiniz yok.");
        }

        var note = new Note
        {
            Content = noteRequest.Content,
            Date = DateTime.UtcNow,
            BookId = book.Id
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        return Ok(note);
    }

    // Kitap detayını getirme
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetBookDetails(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var book = await _context.Books
            .Where(b => b.UserId == userId && b.Id == id)
            .Include(b => b.Notes) // Kitapla ilgili notları da dahil etmek için
            .FirstOrDefaultAsync();

        if (book == null)
        {
            return NotFound("Kitap bulunamadı veya yetkiniz yok.");
        }

        return Ok(book);
    }
}
