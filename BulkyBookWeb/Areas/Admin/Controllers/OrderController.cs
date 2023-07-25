using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		[BindProperty]
		public OrderViewModel OrderViewModel { get; set; }

		public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Details(int orderId)
		{
			OrderViewModel = new OrderViewModel()
			{
				OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "ApplicationUser"),
				OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "Product")
			};
			return View(OrderViewModel);
		}

		[ActionName("Details")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Details_Pay_Now()
		{
			OrderViewModel.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderViewModel.OrderHeader.Id, includeProperties: "ApplicationUser");
			OrderViewModel.OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == OrderViewModel.OrderHeader.Id , includeProperties: "Product");

			//Stripe Settings
			var domain = "https://localhost:44387/";
			var options = new SessionCreateOptions
			{
				PaymentMethodTypes = new List<string> { "card" },
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
				SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderViewModel.OrderHeader.Id}",
				CancelUrl = domain + $"admin/order/details?orderId={OrderViewModel.OrderHeader.Id}"
			};

			foreach (var item in OrderViewModel.OrderDetail)
			{
				var sessionLineItem = new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						UnitAmount = (long)(item.Price * 100), //20.00 -> 2000
						Currency = "usd",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = item.Product.Title
						}

					},
					// Provide the exact Price ID (for example, pr_1234) of the product you want to sell
					//Price = item.Price.ToString(),
					Quantity = 1,
				};

				options.LineItems.Add(sessionLineItem);

			}

			var service = new SessionService();
			Session session = service.Create(options);

			_unitOfWork.OrderHeader.UpdateStripePaymentId(OrderViewModel.OrderHeader.Id, session.Id, session.PaymentIntentId);
			_unitOfWork.Save();

			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);
		}

		public IActionResult PaymentConfirmation(int orderHeaderId)
		{
			OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderId);

			if (orderHeader.PaymentStatus == SD.PAYMENT_STATUS_DELAYED_PAYMENT)
			{
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);
				// check the stripe status
				if (session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, orderHeader.SessionId!, session.PaymentIntentId);
					_unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus!, SD.PAYMENT_STATUS_APPROVED);

					_unitOfWork.Save();

				}
			}

			return View(orderHeaderId);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
		public IActionResult UpdateOrderDetail()
		{
			var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderViewModel.OrderHeader.Id, tracked: false);
			orderHeaderFromDb.Name = OrderViewModel.OrderHeader.Name;
			orderHeaderFromDb.PhoneNumber = OrderViewModel.OrderHeader.PhoneNumber;
			orderHeaderFromDb.StreetAddress = OrderViewModel.OrderHeader.StreetAddress;
			orderHeaderFromDb.City = OrderViewModel.OrderHeader.City;
			orderHeaderFromDb.State = OrderViewModel.OrderHeader.State;
			orderHeaderFromDb.PostalCode = OrderViewModel.OrderHeader.PostalCode;
			if (OrderViewModel.OrderHeader.Carrier != null)
			{
				orderHeaderFromDb.Carrier = OrderViewModel.OrderHeader.Carrier;
			}
			if (OrderViewModel.OrderHeader.TrackingNumber != null)
			{
				orderHeaderFromDb.TrackingNumber = OrderViewModel.OrderHeader.TrackingNumber;
			}

			_unitOfWork.OrderHeader.Update(orderHeaderFromDb);
			_unitOfWork.Save();
			TempData["Success"] = "Order Details Updated Successfully";

			return RedirectToAction("Details", "Order", new { orderId = orderHeaderFromDb.Id });
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
		public IActionResult StartProcessing()
		{
			_unitOfWork.OrderHeader.UpdateStatus(OrderViewModel.OrderHeader.Id, SD.STATUS_IN_PROCESS);
			_unitOfWork.Save();
			TempData["Success"] = "Order Status Updated Successfully";

			return RedirectToAction("Details", "Order", new { orderId = OrderViewModel.OrderHeader.Id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
		public IActionResult ShipOrder()
		{
			var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u=>u.Id== OrderViewModel.OrderHeader.Id, tracked:false);
			orderHeader.TrackingNumber = OrderViewModel.OrderHeader.TrackingNumber;
			orderHeader.Carrier = OrderViewModel.OrderHeader.Carrier;
			orderHeader.OrderStatus = SD.STATUS_SHIPPED;
			orderHeader.ShippingDate = DateTime.Now;
			if(orderHeader.PaymentStatus== SD.PAYMENT_STATUS_DELAYED_PAYMENT)
			{
				orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
			}

			_unitOfWork.OrderHeader.Update(orderHeader);
			_unitOfWork.Save();
			TempData["Success"] = "Order Shipped Successfully";
			return RedirectToAction("Details", "Order", new { orderId = OrderViewModel.OrderHeader.Id });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = SD.ROLE_ADMIN + "," + SD.ROLE_EMPLOYEE)]
		public IActionResult CancelOrder()
		{
			var orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderViewModel.OrderHeader.Id, tracked: false);
			if(orderHeader.PaymentStatus == SD.PAYMENT_STATUS_APPROVED)
			{
				var options = new RefundCreateOptions()
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderHeader.PaymentIntentId
				};
				var service = new RefundService();
				Refund refund = service.Create(options);
				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.STATUS_CANCELLD, SD.STATUS_REFUNDED);
			}
			else
			{
				_unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.STATUS_CANCELLD, SD.STATUS_CANCELLD);
			}

			_unitOfWork.Save();
			TempData["Success"] = "Order Cancelled Successfully";
			return RedirectToAction("Details", "Order", new { orderId = OrderViewModel.OrderHeader.Id });
		}


		#region API CALLS
		[HttpGet]
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> orderHeaders;

			if (User.IsInRole(SD.ROLE_ADMIN) || User.IsInRole(SD.ROLE_EMPLOYEE))
			{
				orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

			}
			else
			{
				var claimsIdentity = User.Identity as ClaimsIdentity;
				var claim = claimsIdentity!.FindFirst(ClaimTypes.NameIdentifier);
				orderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == claim!.Value, includeProperties: "ApplicationUser");
			}

			orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

			switch (status)
			{
				case "pending":
					orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PAYMENT_STATUS_DELAYED_PAYMENT);
					break;
				case "inprocess":
					orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.STATUS_IN_PROCESS);
					break;

				case "completed":
					orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.STATUS_SHIPPED);
					break;
				case "approved":
					orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.STATUS_APPROVED);
					break;
				default:
					break;
			}
			return Json(new { data = orderHeaders });
		}

		#endregion
	}
}
