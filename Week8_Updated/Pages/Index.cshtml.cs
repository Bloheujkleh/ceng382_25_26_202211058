using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ceng382_25_26_202211058.Data;
using ceng382_25_26_202211058.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace LabProject.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }

        public List<Class> PagedList { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Filter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalPages { get; set; }
        public const int PageSize = 10;

        public async Task<IActionResult> OnGetAsync()
        {
            // Veritabanında hiç veri yoksa 100 örnek veri ekle
            if (!await _context.Classes.AnyAsync())
            {
                var random = new Random();
                var exampleClasses = Enumerable.Range(1, 100).Select(i => new Class
                {
                    Name = $"Class {i}",
                    PersonCount = 20 + (i % 10),
                    Description = $"Auto-generated class #{i}",
                    IsActive = true
                }).ToList();

                _context.Classes.AddRange(exampleClasses);
                await _context.SaveChangesAsync();
            }

            // Kullanıcı giriş kontrolü
            string? sessionUsername = HttpContext.Session.GetString("username");
            string? sessionToken = HttpContext.Session.GetString("token");
            string? sessionSessionId = HttpContext.Session.GetString("session_id");

            string? cookieUsername = HttpContext.Request.Cookies["username"];
            string? cookieToken = HttpContext.Request.Cookies["token"];
            string? cookieSessionId = HttpContext.Request.Cookies["session_id"];

            if (string.IsNullOrEmpty(sessionUsername) || string.IsNullOrEmpty(sessionToken) || string.IsNullOrEmpty(sessionSessionId) ||
                string.IsNullOrEmpty(cookieUsername) || string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(cookieSessionId) ||
                sessionUsername != cookieUsername || sessionToken != cookieToken || sessionSessionId != cookieSessionId)
            {
                TempData["ErrorMessage"] = "Authentication failed. Please log in.";
                return RedirectToPage("/Login");
            }

            // Veritabanından verileri çek
            var query = _context.Classes.AsQueryable();

            if (!string.IsNullOrEmpty(Filter))
            {
                query = query.Where(x => x.Name.Contains(Filter));
            }

            TotalPages = (int)Math.Ceiling(await query.CountAsync() / (double)PageSize);

            PagedList = await query
                .OrderBy(x => x.Id)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }

        // --- Export Sadece Bu Sayfadaki Verileri JSON olarak indir ---
        // Bu fonksiyonu kaldırıyorum çünkü artık JSON export gerekmiyor ve Utils yok.

        // --- Class Ekleme ---
        public async Task<IActionResult> OnPostAddClassAsync(string NewClassName, int NewStudentCount, string NewDescription)
        {
            var newClass = new Class
            {
                Name = NewClassName,
                PersonCount = NewStudentCount,
                Description = NewDescription,
                IsActive = true // veya formdan alabilirsin
            };

            _context.Classes.Add(newClass);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { PageNumber, Filter });
        }

        // --- Class Silme ---
        public async Task<IActionResult> OnPostDeleteClassAsync(int DeleteId)
        {
            var item = await _context.Classes.FindAsync(DeleteId);
            if (item != null)
            {
                _context.Classes.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { PageNumber, Filter });
        }

        // --- Class Güncelleme ---
        public async Task<IActionResult> OnPostEditClassAsync(int EditId, string EditName, int EditStudentCount, string EditDescription)
        {
            var item = await _context.Classes.FindAsync(EditId);
            if (item != null)
            {
                item.Name = EditName;
                item.PersonCount = EditStudentCount;
                item.Description = EditDescription;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { PageNumber, Filter });
        }
    }
}
