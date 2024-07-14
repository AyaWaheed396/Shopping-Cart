using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.BusinessLogic.Models;
using MyShop.BusinessLogic.Repositories;
using MyShop.Presentation.Utilities;
using System.Security.Claims;
using X.PagedList;

namespace MyShop.Presentation.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUniteOfWork _uniteOfWork;
       
        public HomeController(IUniteOfWork uniteOfWork)
        {
            _uniteOfWork = uniteOfWork;
            
        }

        public IActionResult Index(int? page)
        {
            var pageNumber = page ?? 1;
            int pageSize = 8;


            var products = _uniteOfWork.Product.GetAll().ToPagedList(pageNumber, pageSize);
            return View(products);
        }

        public IActionResult Details(int ProductId)
        {
            var product = _uniteOfWork.Product.GetFirstOrDefault(x => x.Id == ProductId, Includword: "Category");

            if (product == null)
                return NotFound(); 
            

            ShoppingCart obj = new ShoppingCart()
            {
                ProductId = ProductId,
                Product = product,
                Count = 1
            };

            return View(obj);       
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart Cartobj = _uniteOfWork.ShoppingCart.GetFirstOrDefault(
                u => u.ApplicationUserId == claim.Value && u.ProductId == shoppingCart.ProductId);


            if(Cartobj == null)
            {
                _uniteOfWork.ShoppingCart.Add(shoppingCart);
                _uniteOfWork.Complete();

                HttpContext.Session.SetInt32(SD.SessionKey,
                    _uniteOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value).ToList().Count()
                );

            }

            else
            {
                _uniteOfWork.ShoppingCart.IncreaseCount(Cartobj, shoppingCart.Count);
                _uniteOfWork.Complete();
            }

            return RedirectToAction("Index");
        }
    }
}
