using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Week5RazorApp.Models;
using System.Linq;

namespace Week5RazorApp.Pages
{
    public class IndexModel : PageModel
    {
        private static List<ClassInformationModel> Classes = new List<ClassInformationModel>();

        [BindProperty]
        public ClassInformationModel ClassInfo { get; set; } = new ClassInformationModel();

        [BindProperty(SupportsGet = true)]
        public string ClassNameFilter { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public int? MinStudentCount { get; set; }

        public PaginatedList<ClassInformationTable> PaginatedClasses { get; set; } =
            new PaginatedList<ClassInformationTable>(new List<ClassInformationTable>(), 0, 1, 10);

        public void OnGet(int? pageIndex)
        {   Console.WriteLine(">>> OnGet triggered after code change");
            if (Classes.Count == 0)
            {
                var random = new Random();
                for (int i = 1; i <= 100; i++)
                {
                    Classes.Add(new ClassInformationModel
                    {
                        Id = i,
                        ClassName = $"Class {i}",
                        StudentCount = random.Next(10, 100),
                        Description = $"Description for Class {i}"
                    });
                }
            }

            var filteredClasses = Classes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(ClassNameFilter))
            {
                filteredClasses = filteredClasses.Where(c => c.ClassName.Contains(ClassNameFilter));
            }

            if (MinStudentCount.HasValue)
            {
                filteredClasses = filteredClasses.Where(c => c.StudentCount >= MinStudentCount.Value);
            }

            var displayClasses = filteredClasses
                .Select(c => new ClassInformationTable
                {
                    ClassName = c!.ClassName, // 💡 uyarı 1 giderildi
                    StudentCount = c.StudentCount,
                    Description = c.Description
                }).ToList();

            int pageSize = 10;

            PaginatedClasses = PaginatedList<ClassInformationTable>.Create(displayClasses, pageIndex ?? 1, pageSize)!; // 💡 uyarı 2 & 3 giderildi
        }

        public IActionResult OnPostAdd()
        {
            if (ModelState.IsValid)
            {
                ClassInfo.Id = Classes.Count > 0 ? Classes.Max(c => c.Id) + 1 : 1;
                Classes.Add(ClassInfo);
                return RedirectToPage();
            }
            return Page();
        }

        public IActionResult OnPostDelete(int id)
        {
            var classToDelete = Classes.FirstOrDefault(c => c.Id == id);
            if (classToDelete != null)
            {
                Classes.Remove(classToDelete);
            }
            return RedirectToPage();
        }
    }
}
