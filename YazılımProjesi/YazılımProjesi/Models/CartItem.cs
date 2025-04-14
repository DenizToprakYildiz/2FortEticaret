using System;

namespace YazılımProjesi.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public TF2Item Item { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Item.Price * Quantity;
    }
} 