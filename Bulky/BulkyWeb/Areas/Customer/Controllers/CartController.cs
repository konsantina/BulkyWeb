using BulkyBook.DataAccess.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }  
        public CartController(IUnitOfWork unitOfWork, ShoppingCartVM shoppingCartVM)
        {
            _unitOfWork = unitOfWork;
            ShoppingCartVM = shoppingCartVM;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
