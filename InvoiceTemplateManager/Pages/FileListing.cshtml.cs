using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace InvoiceTemplateManager.Pages
{
    public class FileListingModel : PageModel
    {
        private readonly ILogger<FileListingModel> _logger;
        private string _filesPath;

        public FileListingModel(ILogger<FileListingModel> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _filesPath = env.ContentRootPath + "/Files/";
        }

        public string[] FileList { get; private set; }

        public void OnGet()
        {
            string[] filesAbsolutePath = Directory.GetFiles(_filesPath);
            int count = filesAbsolutePath.Length;
            string[] fileNames = new string[count];
            int i = 0;
            foreach (string fileName in filesAbsolutePath)
            {
                fileNames[i++] = Path.GetFileName(fileName);
            }
            FileList = fileNames;
        }
    }
}
