using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ceng382_25_26_202211058.Data;
using ceng382_25_26_202211058.Models;

namespace ceng382_25_26_202211058.Pages.Classes
{
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }

        public IList<Class> ClassList { get; set; }

        public async Task OnGetAsync()
        {
            ClassList = await _context.Classes.ToListAsync();
        }
    }
} 