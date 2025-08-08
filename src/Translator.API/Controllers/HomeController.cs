using Microsoft.AspNetCore.Mvc;

namespace Translator.API.Controllers;

public class HomeController : Controller
{
    
    public IActionResult Index()
    {
        return View();
    }
}