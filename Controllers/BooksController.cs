using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using BookMateHub.Data;
using BookMateHub.Models;
using System.Threading.Tasks;

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
        book.Status = updatedBook.Status;

        await _context.SaveChangesAsync();
        return Ok("Kitap başarıyla güncellendi.");
    }

public class UpdateBookStatusRequest
{
    public string Status { get; set; }
}

// Kitap durumunu güncelleme (Active, Passive veya Deleted yapma)
[HttpPut("{id}/status")]
[Authorize]
public async Task<IActionResult> UpdateBookStatus(int id, [FromBody] UpdateBookStatusRequest request)
{
    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

    var book = await _context.Books.FindAsync(id);
    if (book == null || book.UserId != userId)
    {
        return NotFound("Kitap bulunamadı veya yetkiniz yok.");
    }

    if (request.Status == "Active" || request.Status == "Passive" || request.Status == "Deleted")
    {
        book.Status = request.Status;
        await _context.SaveChangesAsync();
        return NoContent(); // Başarılı güncelleme
    }

    return BadRequest("Geçersiz durum değeri.");
}





}
