using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;

namespace InvoiceTemplateManager.Pages
{
    public class RemoveFilesModel : PageModel
    {
        private string _filesPath;

        public RemoveFilesModel(IWebHostEnvironment env)
        {
            _filesPath = env.ContentRootPath + "/Files/";
        }

        public IFileInfo RemoveFile { get; private set; }

        public IActionResult OnGet()
        {
            foreach (string fileName in Directory.GetFiles(_filesPath))
            {
                System.IO.File.Delete(fileName);
            }
                
            return RedirectToPage("/FileListing");
        }
    }
}
