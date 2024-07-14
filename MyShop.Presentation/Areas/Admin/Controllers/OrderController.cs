using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.BusinessLogic.Models;
using MyShop.BusinessLogic.Repositories;
using MyShop.BusinessLogic.ViewModels;
using MyShop.DataAccess.Implementation;
using MyShop.Presentation.Utilities;
using Stripe;

namespace MyShop.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize (Roles =SD.AdminRole)]
    public class OrderController : Controller
    {
        private readonly IUniteOfWork _uniteOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUniteOfWork uniteOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _uniteOfWork = uniteOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetData()
        {
            IEnumerable<OrderHeader> orderHeaders;
            orderHeaders = _uniteOfWork.OrderHeader.GetAll(Includword: "ApplicationUser");

            //foreach (var order in orderHeader)
            //{
            //    Console.WriteLine($"Order ID: {order.Id}, PhoneNumber: {order.Phone}");
            //}

            return Json(new { data = orderHeaders });
        }


        public IActionResult Details(int orderid)
        {
            OrderVM orderVM = new OrderVM()
            {
                OrderHeader = _uniteOfWork.OrderHeader.GetFirstOrDefault(u=> u.Id == orderid, Includword: "ApplicationUser"),
                OrderDetails = _uniteOfWork.OrderDetail.GetAll(x=> x.OrderHeaderId == orderid, Includword:"Product")
            };

            return View(orderVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            var orderfromdb = _uniteOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderfromdb.Name = OrderVM.OrderHeader.Name;
            orderfromdb.Phone = OrderVM.OrderHeader.Phone;
            orderfromdb.Address = OrderVM.OrderHeader.Address;
            orderfromdb.City = OrderVM.OrderHeader.City;

            if (OrderVM.OrderHeader.Carrier != null)
            {
                orderfromdb.Carrier = OrderVM.OrderHeader.Carrier;
            }

            if (OrderVM.OrderHeader.TrackingNumber != null)
            {
                orderfromdb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            _uniteOfWork.OrderHeader.Update(orderfromdb);
            _uniteOfWork.Complete();
            TempData["Update"] = "Item has Updated Successfully";
            return RedirectToAction("Details", "Order", new { orderid = orderfromdb.Id });
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult StartProccess()
        {
            _uniteOfWork.OrderHeader.UpdateOrderStatus(OrderVM.OrderHeader.Id, SD.Proccessing, null);
            _uniteOfWork.Complete();

            TempData["Update"] = "Order Status Has Updated Successfully";
            return RedirectToAction("Details", "Order", new { orderid = OrderVM.OrderHeader.Id });
        }


        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult StartShipping()
        {
            var orderFromDb = _uniteOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            orderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            orderFromDb.OrderStatus = SD.Shipped;
            orderFromDb.ShippingDate = DateTime.Now;


            _uniteOfWork.OrderHeader.Update(orderFromDb);
            _uniteOfWork.Complete();

            TempData["Update"] = "Order Status Has Shipped Successfully";
            return RedirectToAction("Details", "Order", new { orderid = OrderVM.OrderHeader.Id });
        }



        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult CancelOrder()
        {
            var orderFromDb = _uniteOfWork.OrderHeader.GetFirstOrDefault(u => u.Id == OrderVM.OrderHeader.Id);
            if(orderFromDb.PaymentStatus == SD.Approve)
            {
                var option = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderFromDb.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(option);

                _uniteOfWork.OrderHeader.UpdateOrderStatus(orderFromDb.Id, SD.Cancelled, SD.Refund);
            }

            else
                _uniteOfWork.OrderHeader.UpdateOrderStatus(orderFromDb.Id, SD.Cancelled, SD.Cancelled);

            _uniteOfWork.Complete();

            TempData["Update"] = "Order Has Canceled Successfully";
            return RedirectToAction("Details", "Order", new { orderid = OrderVM.OrderHeader.Id });
        }
    }
}
