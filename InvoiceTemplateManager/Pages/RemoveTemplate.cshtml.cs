using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InvoiceTemplateManager.Pages
{
    public class RemoveTemplateModel : PageModel
    {
        private const string SessionTemplate = "template";

        public IActionResult OnGet()
        {
            HttpContext.Session.Remove(SessionTemplate);
            return RedirectToPage("/Index");
        }
    }
}
