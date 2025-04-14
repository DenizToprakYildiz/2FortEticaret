using System;
using System.ComponentModel.DataAnnotations;

namespace YazılımProjesi.Models
{
    public enum ItemType
    {
        Australium,
        Hat,
        Weapon,
        Strangifier,
    }

    public class TF2Item
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [Display(Name = "Ürün Adı")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Ürün açıklaması zorunludur.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Ürün fiyatı zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok miktarı zorunludur.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0'dan küçük olamaz.")]
        [Display(Name = "Stok")]
        public int StockCount { get; set; }

        [Required(ErrorMessage = "Ürün tipi zorunludur.")]
        [Display(Name = "Ürün Tipi")]
        public ItemType Type { get; set; }

        [Display(Name = "Resim URL")]
        public string ImageUrl { get; set; }

        [Display(Name = "Kalite")]
        public string Quality { get; set; }

        [Display(Name = "Listelenme Tarihi")]
        public DateTime ListedDate { get; set; }

        [Display(Name = "Mevcut")]
        public bool IsAvailable { get; set; }
    }
} 