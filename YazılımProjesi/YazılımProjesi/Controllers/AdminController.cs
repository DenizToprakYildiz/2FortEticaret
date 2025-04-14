using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using YazılımProjesi.Services;

namespace YazılımProjesi.Controllers
{
    public class AdminController : Controller
    {
        private const string AdminUsername = "admin";
        private const string AdminPassword = "1234";
        private const string AdminSessionKey = "IsAdminLoggedIn";
        private readonly IItemService _itemService;

        public AdminController(IItemService itemService)
        {
            _itemService = itemService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (username == AdminUsername && password == AdminPassword)
            {
                HttpContext.Session.SetString(AdminSessionKey, "true");
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Geçersiz kullanıcı adı veya şifre!";
            return View();
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString(AdminSessionKey) != "true")
            {
                return RedirectToAction("Login");
            }
            var items = _itemService.GetAllItems();
            return View(items);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove(AdminSessionKey);
            return RedirectToAction("Login");
        }
    }
} 