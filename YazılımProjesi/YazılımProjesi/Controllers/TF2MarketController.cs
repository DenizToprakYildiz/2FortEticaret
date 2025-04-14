using Microsoft.AspNetCore.Mvc;
using YazılımProjesi.Models;
using YazılımProjesi.Services;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace YazılımProjesi.Controllers
{
    public class TF2MarketController : Controller
    {
        private readonly IItemService _itemService;
        private readonly ICartService _cartService;
        private readonly IPurchaseHistoryService _purchaseHistoryService;
        private readonly IBalanceService _balanceService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TF2MarketController(
            IItemService itemService, 
            ICartService cartService, 
            IPurchaseHistoryService purchaseHistoryService,
            IBalanceService balanceService,
            IHttpContextAccessor httpContextAccessor)
        {
            _itemService = itemService;
            _cartService = cartService;
            _purchaseHistoryService = purchaseHistoryService;
            _balanceService = balanceService;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            var items = _itemService.GetAllItems();
            return View(items);
        }

        [HttpPost]
        public IActionResult AddToCart(int itemId, int quantity)
        {
            var item = _itemService.GetItemById(itemId);
            if (item == null || !item.IsAvailable || item.StockCount < quantity)
            {
                return Json(new { success = false, message = "Yetersiz stok!" });
            }

            // Stok kontrolü ve güncelleme
            if (_itemService.UpdateStock(itemId, quantity))
            {
                _cartService.AddToCart(itemId, quantity);
                return Json(new { 
                    success = true, 
                    message = "Ürün sepete eklendi!",
                    newStock = item.StockCount - quantity
                });
            }

            return Json(new { success = false, message = "Stok güncellenirken bir hata oluştu!" });
        }

        public IActionResult Cart()
        {
            var cartItems = _cartService.GetCartItems();
            ViewBag.CartTotal = _cartService.GetCartTotal();
            return View(cartItems);
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int itemId)
        {
            _cartService.RemoveFromCart(itemId);
            TempData["SuccessMessage"] = "Ürün sepetten kaldırıldı!";
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        public IActionResult Purchase()
        {
            try
            {
                var cartTotal = _cartService.GetCartTotal();
                var currentBalance = _balanceService.GetBalance();

                if (cartTotal > currentBalance)
                {
                    TempData["ErrorMessage"] = "Yetersiz bakiye! Lütfen bakiyenizi kontrol edin.";
                    return RedirectToAction(nameof(Cart));
                }

                if (_cartService.PurchaseItems())
                {
                    _balanceService.UpdateBalance(currentBalance - cartTotal);
                    TempData["SuccessMessage"] = "Satın alma işlemi başarıyla tamamlandı!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Satın alma işlemi başarısız oldu. Lütfen sepetinizin boş olmadığından ve stok durumunun yeterli olduğundan emin olun.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Satın alma işlemi sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
            }
            return RedirectToAction(nameof(Cart));
        }

        public IActionResult PurchaseHistory()
        {
            var purchases = _purchaseHistoryService.GetAllPurchases();
            return View(purchases);
        }

        [HttpPost]
        public IActionResult AddBalance(decimal amount)
        {
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz miktar!";
                return RedirectToAction(nameof(Index));
            }

            if (_balanceService.AddBalance(amount))
            {
                TempData["SuccessMessage"] = $"${amount:N2} başarıyla bakiyenize eklendi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Bakiye yükleme işlemi başarısız oldu!";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult AddStock(string itemName, decimal price, int quantity, string imageUrl)
        {
            if (_httpContextAccessor.HttpContext?.Session.GetString("IsAdminLoggedIn") != "true")
            {
                TempData["ErrorMessage"] = "Bu işlem için admin yetkisi gereklidir!";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(itemName) || price <= 0 || quantity <= 0 || string.IsNullOrEmpty(imageUrl))
            {
                TempData["ErrorMessage"] = "Geçersiz ürün bilgileri!";
                return RedirectToAction("Index", "Admin");
            }

            var newItem = new TF2Item
            {
                Name = itemName,
                Description = itemName, // Using name as description for now
                Price = price,
                StockCount = quantity,
                ImageUrl = imageUrl,
                Type = ItemType.Weapon, // Default type
                Quality = "Unique", // Default quality
                IsAvailable = true
            };

            _itemService.AddItem(newItem);
            TempData["SuccessMessage"] = "Ürün başarıyla eklendi!";
            return RedirectToAction("Index", "Admin");
        }

        [HttpPost]
        public IActionResult UpdateStock(int itemId, int newStock)
        {
            if (_httpContextAccessor.HttpContext?.Session.GetString("IsAdminLoggedIn") != "true")
            {
                TempData["ErrorMessage"] = "Bu işlem için admin yetkisi gereklidir!";
                return RedirectToAction("Index");
            }

            if (newStock < 0)
            {
                TempData["ErrorMessage"] = "Stok miktarı 0'dan küçük olamaz!";
                return RedirectToAction("Index", "Admin");
            }

            _itemService.SetStock(itemId, newStock);
            TempData["SuccessMessage"] = "Stok başarıyla güncellendi!";
            return RedirectToAction("Index", "Admin");
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            var count = _cartService.GetCartItemCount();
            return Json(count);
        }
    }
} 