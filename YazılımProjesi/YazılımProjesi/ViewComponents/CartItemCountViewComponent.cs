using Microsoft.AspNetCore.Mvc;
using YazılımProjesi.Services;

namespace YazılımProjesi.ViewComponents
{
    public class CartItemCountViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public CartItemCountViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            var count = _cartService.GetCartItemCount();
            return View(count);
        }
    }
} 