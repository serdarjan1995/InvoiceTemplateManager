using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace InvoiceTemplateManager.Pages
{
    public class IndexModel : PageModel
    {
        private const string SessionKeyName = "session";
        private const string SessionTemplate = "template";
        
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions = { ".txt", ".csv" };
        private readonly string _filesPath;
        private IWebHostEnvironment _environment;
        public bool _templateUploaded = false;
        public string _templateContent;
        public bool _invoiceFilesSuccess = false;

        public IndexModel(IConfiguration config, IWebHostEnvironment env)
        {
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");
            _environment = env;
            _filesPath = _environment.ContentRootPath + "/Files/";
        }

        public void OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeyName)))
            {
                var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var stringChars = new char[32];
                var random = new Random();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                var finalString = new String(stringChars);

                HttpContext.Session.SetString(SessionKeyName, finalString);
            }
            _templateUploaded = !string.IsNullOrEmpty(HttpContext.Session.GetString(SessionTemplate));
            _templateContent = HttpContext.Session.GetString("template");
        }

        [BindProperty]
        public TemplateFile FileUpload { get; set; }

        public string Message { get; private set; }
        public async Task<IActionResult> OnPostUploadAsync()
        {
            _templateUploaded = !string.IsNullOrEmpty(HttpContext.Session.GetString(SessionTemplate));
            _templateContent = HttpContext.Session.GetString("template");

            if (!ModelState.IsValid)
            {
                Message = "Please correct the form.";

                return Page();
            }

            IFormFile uploadedFile = FileUpload.FormFile;
            if (string.IsNullOrEmpty(uploadedFile.FileName))
            {
                ModelState.AddModelError(uploadedFile.Name,
                            $"Filename empty");

                return Page();
            }

            // Don't trust the file name sent by the client. To display
            // the file name, HTML-encode the value.
            var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                uploadedFile.FileName);
            var ext = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
            {
                ModelState.AddModelError(uploadedFile.FileName,
                            $"File ({trustedFileNameForDisplay}) " +
                            "file type isn't permitted extension.");

                return Page();
            }

            // Check the file length. This check doesn't catch files that only have 
            // a BOM as their content.
            if (FileUpload.FormFile.Length == 0)
            {
                ModelState.AddModelError(uploadedFile.Name,
                    $"File ({trustedFileNameForDisplay}) is empty.");

                return Page();
            }

            if (uploadedFile.Length > _fileSizeLimit)
            {
                var megabyteSizeLimit = _fileSizeLimit / 1048576;
                ModelState.AddModelError(uploadedFile.Name,
                    $"File ({trustedFileNameForDisplay}) exceeds " +
                    $"{megabyteSizeLimit:N1} MB.");

                return Page();
            }

            try
            {
                MemoryStream memoryStream = new MemoryStream();
                await FileUpload.FormFile.CopyToAsync(memoryStream);

                if (memoryStream.Length == 0)
                {
                    ModelState.AddModelError(uploadedFile.Name,
                        $"File ({trustedFileNameForDisplay}) is empty.");
                }
                memoryStream.Position = 0;
                var reader = new StreamReader(memoryStream, Encoding.UTF8);
                if (!_templateUploaded)
                {
                    _templateContent = reader.ReadToEnd();
                    HttpContext.Session.SetString(SessionTemplate, _templateContent);
                }
                else
                {
                    string formattedTemplate = _templateContent;
                    string pattern = @"\{[A-Z]+\}";
                    Match m = Regex.Match(formattedTemplate, pattern);
                    int i = 0;
                    while (m.Success)
                    {
                        formattedTemplate = formattedTemplate.Replace(m.Value, "{" + i + "}");
                        m = Regex.Match(formattedTemplate, pattern);
                        i++;
                    }

                    int line = 1;
                    int successCount = 0;
                    string csvLine = reader.ReadLine();
                    while (!string.IsNullOrEmpty(csvLine))
                    {
                        try
                        {
                            string currentDate = DateTime.Now.Date.ToString("dd.MM.yyyy");
                            string[] splitted = csvLine.Split(';');
                            var filePath = Path.Combine(
                                _filesPath, splitted[0] + ".txt");
                            string outputString = String.Format(formattedTemplate, currentDate, splitted[1], splitted[2],
                                splitted[0], splitted[3]);
                            using (StreamWriter fileStream = new StreamWriter(filePath))
                            {
                                await fileStream.WriteAsync(outputString);
                                successCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Message += "Line " + line + " has errors. ";
                        }
                        finally
                        {
                            csvLine = reader.ReadLine();
                            line++;
                        }
                        
                    }
                    Message += "Generated " + successCount + " files";
                    if (successCount>0)
                    {
                        _invoiceFilesSuccess = true;
                    }

                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ModelState.AddModelError(uploadedFile.Name,
                    "Parsing failed. Please check your file.");
            }

            _templateUploaded = true;
            return Page();
        }

    }


    public class TemplateFile
    {
        [Required]
        [Display(Name = "File")]
        public IFormFile FormFile { get; set; }
    }
}
