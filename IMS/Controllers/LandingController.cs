using Microsoft.AspNetCore.Mvc;

public class LandingController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
