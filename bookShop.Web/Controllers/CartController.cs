using bookShop.Business.Abstract;
using bookShop.Web.Extensions;
using bookShop.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace bookShop.Web.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IBookService _bookService;

        public CartController(IBookService bookService)
        {
            _bookService = bookService;
        }
        public IActionResult Index()
        {
            var cartCollection = getCollectionFromSession();
            return View(cartCollection);
        }
        public async Task<IActionResult> Add(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (await _bookService.IsExistsAsync(id))
                {
                    CartCollection cartCollection = getCollectionFromSession();
                    var book = await _bookService.GetEntityByIdAsyncWithoutInclude(id);
                    cartCollection.Add(new CartItem { Book = book, Quantity = 1 });
                    saveToSession(cartCollection);
                    return Json(new JsonReturnObject { text = $"{book.Name}Sepete eklendi", success = true });
                }
                return Json(new JsonReturnObject { text = "Ürün ekleme başarısız", success = false });

            }
            return NotFound();
        }

        private void saveToSession(CartCollection cartCollection)
        {
            HttpContext.Session.SetJson("sepetim", cartCollection);
        }

        private CartCollection getCollectionFromSession()
        {

            CartCollection collection = HttpContext.Session.GetJson<CartCollection>("sepetim") ?? new CartCollection();
            return collection;
        }
    }
}
