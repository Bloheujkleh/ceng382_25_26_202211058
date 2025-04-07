using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Week5RazorApp.Models; // Models klasörünüze uygun namespace

namespace Week5RazorApp.Pages
{
    public class IndexModel : PageModel
    {
        // Classes koleksiyonunun tanımlanması
        private static List<ClassInformationModel> Classes = new List<ClassInformationModel>();

        [BindProperty]
        public ClassInformationModel ClassInfo { get; set; } = new ClassInformationModel();

        // Sayfa yüklenirken çağrılacak method
        public void OnGet()
        {
            ViewData["Classes"] = Classes; // ViewData'ya Classes koleksiyonunu aktarıyoruz
        }

        // OnPostAdd methodu ile formdan gelen veriyi ekliyoruz
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
        // OnPostDelete methodu ile item siliniyor
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
