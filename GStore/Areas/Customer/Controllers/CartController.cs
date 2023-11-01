using GStoreWeb.DataAccess.Repository.IRepository;
using GStoreWeb.Models;
using GStoreWeb.Models.ViewModels;
using GStoreWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace GStore.Areas.Customer.Controllers
{
    [Area("Customer")]
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
            string userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartUnit.GetAll(includeProperties:"Product").Where(i => i.ApplicationUserId == userId).ToList(),
                OrderHeader = new()
            };
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Product.Price);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary() {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            string userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartUnit.GetAll(includeProperties: "Product").Where(i => i.ApplicationUserId == userId).ToList(),
                OrderHeader = new()
            };
            ShoppingCartVM.OrderHeader.User = _unitOfWork.ApplicationUserUnit.Get(u=>u.Id == userId);
            ApplicationUser user = ShoppingCartVM.OrderHeader.User;
            ShoppingCartVM.OrderHeader.Name = user.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = user.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = user.StreetAddress;
            ShoppingCartVM.OrderHeader.City = user.City;
            ShoppingCartVM.OrderHeader.PostalCode = user.PostalCode;


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Product.Price);
            }
            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			string userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCartUnit.GetAll(includeProperties: "Product").Where(i => i.ApplicationUserId == userId).ToList();

            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;   
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            ApplicationUser applicationUser = _unitOfWork.ApplicationUserUnit.Get(u=>u.Id == userId);

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Count * cart.Product.Price);
			}
            if (applicationUser.CompanyId == null)
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }
            _unitOfWork.OrderHeaderUnit.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail details = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Product.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetailUnit.Add(details);
            }
            _unitOfWork.Save();

            if (applicationUser.CompanyId == null)
            {
                string domain = "https://localhost:7155/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };
                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(cart.Product.Price * 100),
                            Currency = "azn",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = cart.Product.Name
                            }
                        },
                        Quantity = cart.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeaderUnit.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            var orderHeader = _unitOfWork.OrderHeaderUnit.Get(o=>o.Id == id);
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderUnit.UpdateStripePaymentID(orderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderUnit.UpdateStatus(orderHeader.Id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCartUnit.GetAll().Where(x=>x.ApplicationUserId==orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCartUnit.RemoveRange(shoppingCarts);
            _unitOfWork.Save();

            return View(id);
        }

        public IActionResult Plus(int? cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCartUnit.Get(c=>c.Id == cartId);
            cart.Count += 1;
            _unitOfWork.ShoppingCartUnit.Update(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int? cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCartUnit.Get(c => c.Id == cartId);
            if (cart.Count > 1)
            {
                cart.Count -= 1;
                _unitOfWork.ShoppingCartUnit.Update(cart);
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.ShoppingCartUnit.Remove(cart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCartUnit.GetAll().Where(x => x.ApplicationUserId == cart.ApplicationUserId).Count());
            }
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int? cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCartUnit.Get(c => c.Id == cartId);
            _unitOfWork.ShoppingCartUnit.Remove(cart);
            _unitOfWork.Save();
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCartUnit.GetAll().Where(x => x.ApplicationUserId == cart.ApplicationUserId).Count());
            return RedirectToAction(nameof(Index));
        }
    }
}
