using BulkyBook.DataAccess.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("admin")]
    public class OrderController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public OrderController(IUnitOfWork unitOfWork) {

            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var orderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            return View(orderList);
        }


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusPending);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusCompleted);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                case "all":
                    break; // Επιστρέφει όλα τα αντικείμενα
                default:
                    break;
            }

            return Json(new { data = objOrderHeaders });
        }
        #endregion
    }
}