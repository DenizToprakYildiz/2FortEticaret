using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using YazılımProjesi.Models;

namespace YazılımProjesi.Services
{
    public interface ICartService
    {
        IEnumerable<CartItem> GetCartItems();
        void AddToCart(int itemId, int quantity);
        void RemoveFromCart(int itemId);
        void ClearCart();
        decimal GetCartTotal();
        bool PurchaseItems();
        int GetCartItemCount();
    }

    public class CartService : ICartService
    {
        private readonly IItemService _itemService;
        private readonly IPurchaseHistoryService _purchaseHistoryService;
        private readonly IBalanceService _balanceService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string CART_SESSION_KEY = "CartItems";

        public CartService(
            IItemService itemService,
            IPurchaseHistoryService purchaseHistoryService,
            IBalanceService balanceService,
            IHttpContextAccessor httpContextAccessor)
        {
            _itemService = itemService;
            _purchaseHistoryService = purchaseHistoryService;
            _balanceService = balanceService;
            _httpContextAccessor = httpContextAccessor;
        }

        private List<CartItem> GetCartFromSession()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return new List<CartItem>();

                var cartJson = session.GetString(CART_SESSION_KEY);
                if (string.IsNullOrEmpty(cartJson))
                    return new List<CartItem>();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<List<CartItem>>(cartJson, options) ?? new List<CartItem>();
            }
            catch
            {
                return new List<CartItem>();
            }
        }

        private void SaveCartToSession(List<CartItem> cart)
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return;

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var cartJson = JsonSerializer.Serialize(cart, options);
                session.SetString(CART_SESSION_KEY, cartJson);
            }
            catch
            {
                // Log error if needed
            }
        }

        public IEnumerable<CartItem> GetCartItems()
        {
            return GetCartFromSession();
        }

        public int GetCartItemCount()
        {
            var cart = GetCartFromSession();
            return cart.Sum(item => item.Quantity);
        }

        public void AddToCart(int itemId, int quantity)
        {
            if (quantity <= 0)
                return;

            var item = _itemService.GetItemById(itemId);
            if (item == null || !item.IsAvailable)
                return;

            var cart = GetCartFromSession();
            var cartItem = cart.FirstOrDefault(c => c.Item.Id == itemId);
            
            if (item.StockCount < quantity)
            {
                return;
            }

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem 
                { 
                    Item = item, 
                    Quantity = quantity 
                });
            }

            SaveCartToSession(cart);
        }

        public void RemoveFromCart(int itemId)
        {
            var cart = GetCartFromSession();
            var cartItem = cart.FirstOrDefault(c => c.Item.Id == itemId);
            if (cartItem != null)
            {
                var item = _itemService.GetItemById(itemId);
                if (item != null)
                {
                    _itemService.SetStock(itemId, item.StockCount + cartItem.Quantity);
                }
                cart.Remove(cartItem);
                SaveCartToSession(cart);
            }
        }

        public void ClearCart()
        {
            var cart = GetCartFromSession();
            foreach (var cartItem in cart)
            {
                var item = _itemService.GetItemById(cartItem.Item.Id);
                if (item != null)
                {
                    _itemService.SetStock(cartItem.Item.Id, item.StockCount + cartItem.Quantity);
                }
            }
            _httpContextAccessor.HttpContext.Session.Remove(CART_SESSION_KEY);
        }

        public decimal GetCartTotal()
        {
            var cart = GetCartFromSession();
            return cart.Sum(item => item.Item.Price * item.Quantity);
        }

        public bool PurchaseItems()
        {
            var cart = GetCartFromSession();
            if (!cart.Any())
                return false;

            var cartTotal = cart.Sum(item => item.Item.Price * item.Quantity);
            var currentBalance = _balanceService.GetBalance();

            if (currentBalance < cartTotal)
                return false;

            var purchases = new List<object>();
            foreach (var cartItem in cart)
            {
                var purchase = new PurchaseHistory
                {
                    ItemId = cartItem.Item.Id,
                    ItemName = cartItem.Item.Name,
                    Price = cartItem.Item.Price,
                    Quantity = cartItem.Quantity,
                    TotalPrice = cartItem.Item.Price * cartItem.Quantity,
                    PurchaseDate = DateTime.Now
                };
                _purchaseHistoryService.AddPurchase(purchase);

                // Local storage için satın alma bilgilerini hazırla
                purchases.Add(new
                {
                    itemName = purchase.ItemName,
                    price = purchase.Price,
                    quantity = purchase.Quantity,
                    totalPrice = purchase.TotalPrice,
                    purchaseDate = purchase.PurchaseDate
                });
            }

            // Local storage'a kaydet
            var existingHistory = _httpContextAccessor.HttpContext?.Session.GetString("PurchaseHistory");
            var allPurchases = new List<object>();
            
            if (!string.IsNullOrEmpty(existingHistory))
            {
                allPurchases = JsonSerializer.Deserialize<List<object>>(existingHistory) ?? new List<object>();
            }
            
            allPurchases.AddRange(purchases);
            _httpContextAccessor.HttpContext?.Session.SetString("PurchaseHistory", JsonSerializer.Serialize(allPurchases));

            _balanceService.UpdateBalance(currentBalance - cartTotal);
            ClearCart();
            return true;
        }
    }
} 