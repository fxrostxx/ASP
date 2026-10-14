using Microsoft.AspNetCore.Mvc;
namespace Academy.Controllers
{
	public class HomeController : Controller
	{
		// HOME: Index
		public IActionResult Index()
		{
			return View();
		}
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View();
		}
	}
}
