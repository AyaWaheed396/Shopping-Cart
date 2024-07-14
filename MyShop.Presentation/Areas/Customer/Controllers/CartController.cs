using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyShop.BusinessLogic.Models;
using MyShop.BusinessLogic.Repositories;
using MyShop.BusinessLogic.ViewModels;
using MyShop.Presentation.Utilities;
using Stripe.Checkout;
using Stripe.Terminal;
using System.Security.Claims;

namespace MyShop.Presentation.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUniteOfWork _uniteOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public int TotalCarts { get; set; }
        public CartController(IUniteOfWork uniteOfWork)
        {
            _uniteOfWork = uniteOfWork;

        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM  = new ShoppingCartVM()
            {
                CartsList = _uniteOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, Includword:"Product")
            };

            foreach (var item in ShoppingCartVM.CartsList)
            {
                ShoppingCartVM.TotalCarts += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);
        }

        [HttpGet]
		public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                CartsList = _uniteOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, Includword: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _uniteOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);


            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.Phone = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;


			foreach (var item in ShoppingCartVM.CartsList)
            {
                //ShoppingCartVM.TotalCarts += (item.Count * item.Product.Price);
                ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
            }

            return View(ShoppingCartVM);
		}


		[HttpPost]
		[ActionName("Summary")]
		[ValidateAntiForgeryToken]
		public IActionResult PostSummary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			if (claim == null)
			{
				// Handle the case where the user is not authenticated or the claim is not found
				return Unauthorized();
			}

			ShoppingCartVM = new ShoppingCartVM()
			{
				CartsList = _uniteOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, Includword: "Product"),
				OrderHeader = new OrderHeader()
			};

			ShoppingCartVM.OrderHeader.ApplicationUser = _uniteOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);

			if (ShoppingCartVM.OrderHeader.ApplicationUser == null)
			{
				// Handle the case where the user is not found
				return NotFound();
			}

			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City; 
			ShoppingCartVM.OrderHeader.Phone = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;

			foreach (var item in ShoppingCartVM.CartsList)
			{
				ShoppingCartVM.OrderHeader.TotalPrice += (item.Count * item.Product.Price);
			}

			ShoppingCartVM.OrderHeader.OrderStatus = SD.Pending;
			ShoppingCartVM.OrderHeader.PaymentStatus = SD.Pending;
			ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

			_uniteOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_uniteOfWork.Complete();

			foreach (var item in ShoppingCartVM.CartsList)
			{
				OrderDetail orderDetail = new OrderDetail()
				{
					ProductId = item.ProductId,
					//orde = ShoppingCartVM.OrderHeader.Id,
					OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
					Price = item.Product.Price,
					Count = item.Count,
				};

				_uniteOfWork.OrderDetail.Add(orderDetail);
				_uniteOfWork.Complete();
			}

			var domain = "http://localhost:5215/";
			var options = new SessionCreateOptions
			{
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
				SuccessUrl = domain + $"customer/cart/orderconfirmation?id={ShoppingCartVM.OrderHeader.Id}",
				CancelUrl = domain + $"customer/cart/index"
			};

			foreach (var item in ShoppingCartVM.CartsList)
			{
				var sessionLineOption = new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						UnitAmount = (long)(item.Product.Price * 100),
						Currency = "usd",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = item.Product.Name,
						},
					},
					Quantity = item.Count,
				};
				options.LineItems.Add(sessionLineOption);
			}

			var service = new SessionService();
			Session session = service.Create(options);
			ShoppingCartVM.OrderHeader.SessionId = session.Id;

			_uniteOfWork.Complete();

			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);
		}



		public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _uniteOfWork.OrderHeader.GetFirstOrDefault(u=> u.Id == id);

            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if(session.PaymentStatus.ToLower() == "paid")
            {
                _uniteOfWork.OrderHeader.UpdateOrderStatus(id, SD.Approve, SD.Approve);
                //ShoppingCartVM.OrderHeader.PaymentIntentId = session.PaymentIntentId;

                orderHeader.PaymentIntentId = session.PaymentIntentId;
                _uniteOfWork.Complete();
            }

            List<ShoppingCart> shoppingCarts = _uniteOfWork.ShoppingCart.GetAll(u=> u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _uniteOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _uniteOfWork.Complete();
            return View(id);
        }


        public IActionResult Plus(int cartid)
        {
            var shoppingCart = _uniteOfWork.ShoppingCart.GetFirstOrDefault(x=> x.Id == cartid);
            _uniteOfWork.ShoppingCart.IncreaseCount(shoppingCart, 1);
            _uniteOfWork.Complete();

            return RedirectToAction("Index");
        }

		public IActionResult Minus(int cartid)
		{
			var shoppingCart = _uniteOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartid);

            if(shoppingCart.Count <= 1)
            {
                _uniteOfWork.ShoppingCart.Remove(shoppingCart);
				var count = _uniteOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count()-1;
				HttpContext.Session.SetInt32(SD.SessionKey, count);
                //_uniteOfWork.Complete();
                //            return RedirectToAction("Index", "Home");
            }

            else
				_uniteOfWork.ShoppingCart.DecreaseCount(shoppingCart, 1);

			_uniteOfWork.Complete();

			return RedirectToAction("Index");
		}


        public IActionResult Remove(int cartid)
        {
			var shoppingCart = _uniteOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartid);
			_uniteOfWork.ShoppingCart.Remove(shoppingCart);
			_uniteOfWork.Complete();

            var count = _uniteOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == shoppingCart.ApplicationUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.SessionKey, count);

            return RedirectToAction("Index");
		}

	}
}
