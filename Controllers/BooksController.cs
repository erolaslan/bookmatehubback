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
        // Kullanıcı ID'sini token'dan alın
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
        // Kullanıcı ID'sini token'dan alın
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        // Geçersiz status değeri kontrolü
        if (status != "Active" && status != "Passive" && status != "Deleted")
        {
            return BadRequest("Geçersiz durum değeri.");
        }

        var books = _context.Books
            .Where(b => b.UserId == userId && b.Status == status) // Belirtilen duruma göre filtreleme
            .OrderBy(b => b.Id)
            .ToList();
        return Ok(books);
    }

    // Kitap güncelleme
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateBook(int id, Book updatedBook)
    {
        // Kullanıcı ID'sini token'dan alın
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

    // Kitap silme (Durumu "Deleted" olarak güncelle)
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteBook(int id)
    {
        // Kullanıcı ID'sini token'dan alın
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var book = await _context.Books.FindAsync(id);
        if (book == null || book.UserId != userId)
        {
            return NotFound("Kitap bulunamadı veya yetkiniz yok.");
        }

        book.Status = "Deleted"; // Kitabın durumunu "Deleted" olarak güncelle
        await _context.SaveChangesAsync();
        return Ok("Kitap başarıyla silindi.");
    }

    // Kitap durumunu güncelleme (Active, Passive veya Deleted yapma)
    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateBookStatus(int id, [FromBody] string status)
    {
        // Kullanıcı ID'sini token'dan alın
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var book = await _context.Books.FindAsync(id);
        if (book == null || book.UserId != userId)
        {
            return NotFound("Kitap bulunamadı veya yetkiniz yok.");
        }

        if (status == "Active" || status == "Passive" || status == "Deleted")
        {
            book.Status = status;
            await _context.SaveChangesAsync();
            return Ok("Kitap durumu başarıyla güncellendi.");
        }

        return BadRequest("Geçersiz durum değeri.");
    }
}
