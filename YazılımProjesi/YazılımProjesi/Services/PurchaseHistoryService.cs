using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using YazılımProjesi.Models;

namespace YazılımProjesi.Services
{
    public interface IPurchaseHistoryService
    {
        void AddPurchase(PurchaseHistory purchase);
        List<PurchaseHistory> GetAllPurchases();
    }

    public class PurchaseHistoryService : IPurchaseHistoryService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string PURCHASE_HISTORY_SESSION_KEY = "PurchaseHistory";

        public PurchaseHistoryService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private List<PurchaseHistory> GetPurchasesFromSession()
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return new List<PurchaseHistory>();

                var purchasesJson = session.GetString(PURCHASE_HISTORY_SESSION_KEY);
                if (string.IsNullOrEmpty(purchasesJson))
                    return new List<PurchaseHistory>();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<List<PurchaseHistory>>(purchasesJson, options) ?? new List<PurchaseHistory>();
            }
            catch
            {
                return new List<PurchaseHistory>();
            }
        }

        private void SavePurchasesToSession(List<PurchaseHistory> purchases)
        {
            try
            {
                var session = _httpContextAccessor.HttpContext?.Session;
                if (session == null) return;

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var purchasesJson = JsonSerializer.Serialize(purchases, options);
                session.SetString(PURCHASE_HISTORY_SESSION_KEY, purchasesJson);
            }
            catch
            {
                // Log error if needed
            }
        }

        public void AddPurchase(PurchaseHistory purchase)
        {
            var purchases = GetPurchasesFromSession();
            purchase.Id = purchases.Count + 1;
            purchase.PurchaseDate = System.DateTime.Now;
            purchases.Add(purchase);
            SavePurchasesToSession(purchases);
        }

        public List<PurchaseHistory> GetAllPurchases()
        {
            var purchases = GetPurchasesFromSession();
            return purchases.OrderByDescending(p => p.PurchaseDate).ToList();
        }
    }
} 