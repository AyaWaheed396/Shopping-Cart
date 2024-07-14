using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.BusinessLogic.Repositories;
using MyShop.Presentation.Utilities;

namespace MyShop.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.AdminRole)]
    public class DashboardController : Controller
    {
        private IUniteOfWork _uniteOfWork;
        public DashboardController(IUniteOfWork uniteOfWork)
        {
            _uniteOfWork = uniteOfWork;
        }

        public IActionResult Index()
        {
            ViewBag.Orders = _uniteOfWork.OrderHeader.GetAll().Count();
            ViewBag.ApprovedOrders = _uniteOfWork.OrderHeader.GetAll(x => x.OrderStatus == SD.Approve).Count();
            ViewBag.Users = _uniteOfWork.ApplicationUser.GetAll().Count();
            ViewBag.Products = _uniteOfWork.Product.GetAll().Count();


            return View();
        }
    }
}
