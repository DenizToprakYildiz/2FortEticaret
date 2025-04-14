using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using YazılımProjesi.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace YazılımProjesi.Services
{
    public interface IItemService
    {
        List<TF2Item> GetAllItems();
        TF2Item GetItemById(int id);
        bool UpdateStock(int itemId, int quantity);
        void AddItem(TF2Item item);
        void SetStock(int itemId, int newStock);
    }

    public class ItemService : IItemService
    {
        private const string ITEMS_FILE_PATH = "wwwroot/data/items.json";
        private static readonly List<TF2Item> _defaultItems = new List<TF2Item>
        {
            new TF2Item
            {
                Id = 1,
                Name = "Australium Rocket Launcher",
                Description = "A shiny Australium Rocket Launcher",
                Price = 100.00m,
                Type = ItemType.Weapon,
                ImageUrl = "/images/Aussie_Original.png",
                IsAvailable = true,
                Quality = "Unique",
                ListedDate = System.DateTime.Now,
                StockCount = 5
            },
            new TF2Item
            {
                Id = 2,
                Name = "Golden Frying Pan",
                Description = "The legendary Golden Frying Pan",
                Price = 6000.00m,
                Type = ItemType.Weapon,
                ImageUrl = "/images/Pan.jpg",
                IsAvailable = true,
                Quality = "Unique",
                ListedDate = System.DateTime.Now,
                StockCount = 2
            },
            new TF2Item
            {
                Id = 3,
                Name = "Grease oil",
                Description = "The legendary grease oil",
                Price = 320.00m,
                Type = ItemType.Strangifier,
                ImageUrl = "/images/Gresyagi.jpg",
                IsAvailable = true,
                Quality = "Unique",
                ListedDate = System.DateTime.Now,
                StockCount = 10
            },
            new TF2Item
            {
                Id = 4,
                Name = "FishCake",
                Description = "The legendary FishCake",
                Price = 320.00m,
                Type = ItemType.Strangifier,
                ImageUrl = "/images/Fishcake.jpg",
                IsAvailable = true,
                Quality = "Unique",
                ListedDate = System.DateTime.Now,
                StockCount = 45
            }
        };

        public ItemService()
        {
            InitializeItems();
        }

        private void InitializeItems()
        {
            var directory = Path.GetDirectoryName(ITEMS_FILE_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(ITEMS_FILE_PATH))
            {
                SaveItemsToFile(_defaultItems);
            }
        }

        private List<TF2Item> GetItemsFromFile()
        {
            try
            {
                if (!File.Exists(ITEMS_FILE_PATH))
                {
                    SaveItemsToFile(_defaultItems);
                    return _defaultItems;
                }

                var jsonString = File.ReadAllText(ITEMS_FILE_PATH);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                return JsonSerializer.Deserialize<List<TF2Item>>(jsonString, options) ?? _defaultItems;
            }
            catch
            {
                return _defaultItems;
            }
        }

        private void SaveItemsToFile(List<TF2Item> items)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var jsonString = JsonSerializer.Serialize(items, options);
                File.WriteAllText(ITEMS_FILE_PATH, jsonString);
            }
            catch
            {
                // Log error if needed
            }
        }

        public List<TF2Item> GetAllItems()
        {
            return GetItemsFromFile();
        }

        public TF2Item GetItemById(int id)
        {
            var items = GetItemsFromFile();
            return items.FirstOrDefault(i => i.Id == id);
        }

        public bool UpdateStock(int itemId, int quantity)
        {
            var items = GetItemsFromFile();
            var item = items.FirstOrDefault(i => i.Id == itemId);
            if (item == null || item.StockCount < quantity || !item.IsAvailable)
                return false;

            item.StockCount -= quantity;
            if (item.StockCount == 0)
            {
                item.IsAvailable = false; // Stok bittiğinde ürün artık mevcut değil
            }
            SaveItemsToFile(items);
            return true;
        }

        public void AddItem(TF2Item item)
        {
            var items = GetItemsFromFile();
            item.Id = items.Max(i => i.Id) + 1;
            item.ListedDate = System.DateTime.Now;
            item.IsAvailable = true;
            items.Add(item);
            SaveItemsToFile(items);
        }

        public void SetStock(int itemId, int newStock)
        {
            var items = GetItemsFromFile();
            var item = items.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
            {
                item.StockCount = newStock;
                item.IsAvailable = newStock > 0;
                SaveItemsToFile(items);
            }
        }
    }
} 