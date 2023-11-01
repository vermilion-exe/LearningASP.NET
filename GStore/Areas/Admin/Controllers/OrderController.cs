using GStoreWeb.DataAccess.Repository.IRepository;
using GStoreWeb.Models;
using GStoreWeb.Models.ViewModels;
using GStoreWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace GStore.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
		{
			return View();
		}
        public IActionResult Details(int id)
        {
            OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeaderUnit.Get(h => h.Id == id, includeProperties: "User"),
                OrderDetails = _unitOfWork.OrderDetailUnit.GetAll(includeProperties: "Product").Where(d => d.OrderHeaderId == id)
            };
            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult UpdateOrderDetails() {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderUnit.Get(o => o.Id == OrderVM.OrderHeader.Id);
            orderHeader.Name = OrderVM.OrderHeader.Name;
            orderHeader.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeader.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeader.City = OrderVM.OrderHeader.City;
            orderHeader.PostalCode = OrderVM.OrderHeader.PostalCode;
            if(!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
                orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
                orderHeader.Carrier = OrderVM.OrderHeader.TrackingNumber;
            _unitOfWork.OrderHeaderUnit.Update(orderHeader);
            _unitOfWork.Save();

            TempData["Success"] = "Order details updated successfully";

            return RedirectToAction(nameof(Details), new {id=orderHeader.Id});
        }
        [HttpPost]
        [Authorize(Roles =SD.RoleAdmin+ "," + SD.RoleEmployee)]
        public IActionResult StartProcessing() {
            _unitOfWork.OrderHeaderUnit.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details updated successfully.";
            return RedirectToAction(nameof(Details), new {id=OrderVM.OrderHeader.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult ShipOrder()
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderUnit.Get(o=>o.Id==OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.ShipmentDate = DateTime.Now;
            if(orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.OrderHeaderUnit.Update(orderHeader);
            _unitOfWork.Save();

            _unitOfWork.OrderHeaderUnit.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusShipped);
            _unitOfWork.Save();
            TempData["Success"] = "Order shipped successfully.";
            return RedirectToAction(nameof(Details), new { id = OrderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult CancelOrder()
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderUnit.Get(o => o.Id == OrderVM.OrderHeader.Id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeaderUnit.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeaderUnit.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            _unitOfWork.Save();
            TempData["Success"] = "Order cancelled successfully.";
            return RedirectToAction(nameof(Details), new { id = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        public IActionResult Details()
        {
                string domain = "https://localhost:7155/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"admin/order/PaymentConfirmation?id={OrderVM.OrderHeader.Id}",
                    CancelUrl = domain + $"admin/order/details?id={OrderVM.OrderHeader.Id}",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };
                foreach (var cart in OrderVM.OrderDetails)
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
                _unitOfWork.OrderHeaderUnit.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int id)
        {
            var orderHeader = _unitOfWork.OrderHeaderUnit.Get(o => o.Id == id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderUnit.UpdateStripePaymentID(orderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderUnit.UpdateStatus(orderHeader.Id, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            return View(id);
        }
        #region API CALLS
        [HttpGet]
		public IActionResult GetAll(string status)
		{
            List<OrderHeader> orderHeaders;

            if(User.IsInRole(SD.RoleAdmin) || User.IsInRole(SD.RoleEmployee))
            {
                orderHeaders = _unitOfWork.OrderHeaderUnit.GetAll(includeProperties: "User").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                string userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderHeaders = _unitOfWork.OrderHeaderUnit.GetAll(includeProperties: "User").Where(o=>o.ApplicationUserId==userId).ToList();    
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.StatusPending).ToList();
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.StatusInProcess).ToList();
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.StatusShipped).ToList();
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(x => x.OrderStatus == SD.StatusApproved).ToList();
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
		}

		#endregion
	}
}
