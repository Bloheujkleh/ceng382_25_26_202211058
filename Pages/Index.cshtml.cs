using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using RazorPagesProject.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace RazorPagesProject.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private static List<ClassInformationModel> _classes;
        private const int PageSize = 10;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            if (_classes == null)
            {
                _classes = new List<ClassInformationModel>();
                GenerateSampleData();
            }
        }

        [BindProperty]
        public ClassInformationModel ClassInfo { get; set; }

        public List<ClassInformationModel> Classes { get; set; }
        public int? EditId { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public List<string> SelectedColumns { get; set; }

        public async Task<IActionResult> OnGetAsync(int? pageNumber, string searchTerm, int? editId)
        {
            _logger.LogInformation($"OnGet called with pageNumber={pageNumber}, searchTerm={searchTerm}, editId={editId}");

            // Set current page (default to 1 if not specified)
            CurrentPage = pageNumber ?? 1;
            SearchTerm = searchTerm;

            // If edit mode, populate the form with the class info
            if (editId.HasValue)
            {
                var classToEdit = _classes.FirstOrDefault(c => c.Id == editId.Value);
                if (classToEdit != null)
                {
                    ClassInfo = new ClassInformationModel
                    {
                        Id = classToEdit.Id,
                        ClassName = classToEdit.ClassName,
                        StudentCount = classToEdit.StudentCount,
                        Description = classToEdit.Description
                    };
                }
            }

            // Filter data based on search term
            var filteredData = string.IsNullOrWhiteSpace(SearchTerm)
                ? _classes
                : _classes.Where(c => c.ClassName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                    c.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));

            _logger.LogInformation($"Filtered data count: {filteredData.Count()}");

            // Calculate pagination
            TotalItems = filteredData.Count();
            TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);

            // Ensure current page is within valid range
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            _logger.LogInformation($"Page: {CurrentPage}, TotalPages: {TotalPages}, Skip: {(CurrentPage - 1) * PageSize}, Take: {PageSize}");

            // Get the current page of data
            Classes = filteredData
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            _logger.LogInformation($"Returned items count: {Classes.Count}");
            if (Classes.Any())
            {
                _logger.LogInformation($"First item ID: {Classes.First().Id}");
                _logger.LogInformation($"Last item ID: {Classes.Last().Id}");
            }

            return Page();
        }

        public IActionResult OnPostAdd()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (ClassInfo.Id == 0)
            {
                ClassInfo.Id = _classes.Count > 0 ? _classes.Max(c => c.Id) + 1 : 1;
                _classes.Add(ClassInfo);
            }
            else
            {
                var existingClass = _classes.FirstOrDefault(c => c.Id == ClassInfo.Id);
                if (existingClass != null)
                {
                    existingClass.ClassName = ClassInfo.ClassName;
                    existingClass.StudentCount = ClassInfo.StudentCount;
                    existingClass.Description = ClassInfo.Description;
                }
            }

            return RedirectToPage("./Index", new { searchTerm = SearchTerm, page = CurrentPage });
        }

        public IActionResult OnPostDelete(int id)
        {
            var classToDelete = _classes.FirstOrDefault(c => c.Id == id);
            if (classToDelete != null)
            {
                _classes.Remove(classToDelete);
            }

            return RedirectToPage("./Index", new { searchTerm = SearchTerm, page = CurrentPage });
        }

        public IActionResult OnPostExportToJson(bool exportFiltered, List<string> selectedColumns)
{
    try
    {
        // Adım 1: Veriyi filtrele (isteğe göre)
        var dataToExport = exportFiltered ?
            FilterData(_classes, SearchTerm) :
            _classes;

        // ✅ Adım 2: Sadece o anki sayfanın verisini al
        dataToExport = dataToExport
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        // Adım 3: Kolon filtreleme varsa uygulansın
        if (selectedColumns != null && selectedColumns.Any())
        {
            dataToExport = FilterColumns(dataToExport, selectedColumns);
        }

        // Adım 4: JSON oluştur
        var jsonString = JsonSerializer.Serialize(dataToExport, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var fileName = $"class_data_page{CurrentPage}_{DateTime.Now:yyyyMMddHHmmss}.json";
        return File(
            System.Text.Encoding.UTF8.GetBytes(jsonString),
            "application/json",
            fileName
        );
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = $"Error exporting data: {ex.Message}";
        return RedirectToPage("./Index", new { searchTerm = SearchTerm, page = CurrentPage });
    }
}


        public async Task<IActionResult> OnPostImportFromJson(IFormFile jsonFile)
        {
            try
            {
                if (jsonFile == null || jsonFile.Length == 0)
                {
                    TempData["ErrorMessage"] = "Please select a JSON file to import.";
                    return RedirectToPage("./Index", new { searchTerm = SearchTerm, page = CurrentPage });
                }

                using (var stream = new MemoryStream())
                {
                    await jsonFile.CopyToAsync(stream);
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        var jsonString = await reader.ReadToEndAsync();
                        var importedData = JsonSerializer.Deserialize<List<ClassInformationModel>>(jsonString);
                        
                        if (importedData != null && importedData.Any())
                        {
                            _classes = importedData;
                            TempData["SuccessMessage"] = $"Successfully imported {importedData.Count} classes.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "No valid data found in the JSON file.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error importing data: {ex.Message}";
            }

            return RedirectToPage("./Index", new { searchTerm = SearchTerm, page = CurrentPage });
        }

        private List<ClassInformationModel> FilterData(List<ClassInformationModel> data, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return data;

            return data.Where(c =>
                c.ClassName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.Id)
                .ToList();
        }

        private List<ClassInformationModel> FilterColumns(List<ClassInformationModel> data, List<string> selectedColumns)
        {
            return data.Select(item => new ClassInformationModel
            {
                Id = selectedColumns.Contains("Id") ? item.Id : 0,
                ClassName = selectedColumns.Contains("ClassName") ? item.ClassName : null,
                StudentCount = selectedColumns.Contains("StudentCount") ? item.StudentCount : 0,
                Description = selectedColumns.Contains("Description") ? item.Description : null
            }).ToList();
        }

        private void GenerateSampleData()
        {
            if (_classes.Any()) return;

            var subjects = new[] { "Math", "Science", "History", "English", "Art", "Music", "Physical Education" };
            var levels = new[] { "Beginner", "Intermediate", "Advanced" };
            var descriptions = new[] {
                "Introduction to basic concepts",
                "Comprehensive study of the subject",
                "Advanced topics and applications",
                "Practical hands-on learning",
                "Theoretical foundations"
            };

            var random = new Random();
            for (int i = 1; i <= 100; i++)
            {
                _classes.Add(new ClassInformationModel
                {
                    Id = i,
                    ClassName = $"{subjects[random.Next(subjects.Length)]} - {levels[random.Next(levels.Length)]}",
                    StudentCount = random.Next(10, 31),
                    Description = descriptions[random.Next(descriptions.Length)]
                });
            }

            _logger.LogInformation($"Generated {_classes.Count} sample records");
        }
    }
}
