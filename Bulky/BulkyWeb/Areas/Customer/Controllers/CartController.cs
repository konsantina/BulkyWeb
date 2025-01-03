﻿using BulkyBook.DataAccess.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Kiota.Abstractions;
using Stripe;
using Stripe.Checkout;
using System.Runtime.ConstrainedExecution;
using System.Security.Claims;


namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }  
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
    
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader= new ()
            };

          //  ShoppingCartVM.OrderTotal = 0; 

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (decimal)(cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode= ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (decimal)(cart.Price * cart.Count);

            }
            return View(ShoppingCartVM);
        }


        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");

            if (ShoppingCartVM.OrderHeader == null)
            {
                ShoppingCartVM.OrderHeader = new OrderHeader();
            }

            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;

            // Γέμισε τις τιμές του OrderHeader με τα δεδομένα του χρήστη
            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = applicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = applicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = applicationUser.City;
            ShoppingCartVM.OrderHeader.State = applicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = applicationUser.PostalCode;

            // Υπολογισμός συνολικού κόστους
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (decimal)(cart.Price * cart.Count);
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {


                StripeConfiguration.ApiKey = "sk_test_51QFfv2Cdqx3PsZpmYFxxU8dR0OXDTEYMswuL5hcaAbUTeonp9OPL37g5J8A46vLvnA6o9ODrx7CS3BawD57mOO9700enBg8HEz";
                var domain = "https://localhost:7159/";
                var options = new SessionCreateOptions
                {
                   SuccessUrl = domain+ $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                   CancelUrl = domain+"customer/cart/index",
                   LineItems = new List<SessionLineItemOptions>(),
                   Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList) {
                    var sessionLineItem = new SessionLineItemOptions {
                        PriceData = new SessionLineItemPriceDataOptions { 
                          UnitAmount = (long)(item.Price*100), //$20.50 => 2050
                          Currency="usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title            
                          }
                        },
                        Quantity = item.Count
                      };
                       options.LineItems.Add(sessionLineItem);  
                }

                var service = new SessionService();
              Session session = service.Create(options);
                _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
                _unitOfWork.Save();
                _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);




                // Regular customer
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.PaymentStatusPending;
            }
            else
            {
                // Company user
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            // Προσθήκη OrderDetails
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }

            _unitOfWork.Save();

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id) {

            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayPayment) {
                //this is an order by customer
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.Status.ToLower() == "paid") {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId); 
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            
            return View(id);
        }
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId) {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);         
                _unitOfWork.ShoppingCart.Remove(cartFromDb);    
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart) {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;

                }
                else {
                    return shoppingCart.Product.Price100;
                
                }
            
            }
            
        }


    }
}

