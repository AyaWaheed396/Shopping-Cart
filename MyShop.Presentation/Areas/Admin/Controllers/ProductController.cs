using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyShop.BusinessLogic.Models;
using MyShop.BusinessLogic.Repositories;
using MyShop.BusinessLogic.ViewModels;


namespace MyShop.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUniteOfWork _uniteOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUniteOfWork uniteOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _uniteOfWork = uniteOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public IActionResult GetData()
        {
            var products = _uniteOfWork.Product.GetAll(Includword: "Category");
            return Json(new { data = products });
        }


        [HttpGet]
        public IActionResult Create()
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _uniteOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                })
            };

            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductVM ProductVM, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                string RootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var Upload = Path.Combine(RootPath, @"Images\Products");
                    var ext = Path.GetExtension(file.FileName);

                    using (var fileStream = new FileStream(Path.Combine(Upload, fileName + ext), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    ProductVM.Product.Img = @"Images\Products\" + fileName + ext;
                }

                _uniteOfWork.Product.Add(ProductVM.Product);
                _uniteOfWork.Complete();
                TempData["Create"] = "Data Has Created Successfully";
                return RedirectToAction("Index");
            }

            return View(ProductVM.Product);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null | id == 0)
                NotFound();

            //var ProductInDb = _uniteOfWork.Product.GetFirstOrDefault(x => x.Id == id);

            ProductVM productVM = new ProductVM()
            {
                Product = _uniteOfWork.Product.GetFirstOrDefault(x => x.Id == id),
                CategoryList = _uniteOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                })
            };

            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductVM ProductVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string RootPath = _webHostEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var Upload = Path.Combine(RootPath, @"Images\Products");
                    var ext = Path.GetExtension(file.FileName);

                    if (ProductVM.Product.Img != null)
                    {
                        var oldImg = Path.Combine(RootPath, ProductVM.Product.Img.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImg))
                            System.IO.File.Delete(oldImg);
                    }

                    using (var fileStream = new FileStream(Path.Combine(Upload, fileName + ext), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    ProductVM.Product.Img = @"Images\Products\" + fileName + ext;
                    /*@"Images\Products\" + fileName + ext;*/
                }



                _uniteOfWork.Product.Update(ProductVM.Product);
                _uniteOfWork.Complete();
                TempData["Update"] = "Data Has Updated Successfully";
                return RedirectToAction("Index");
            }

            return View(ProductVM.Product);
        }





        [HttpDelete]
        public IActionResult DeleteProduct(int? id)
        {
            var ProductInDb = _uniteOfWork.Product.GetFirstOrDefault(x => x.Id == id);
            if (ProductInDb == null)
                return Json(new { success = false, message = "Error while deleting" });

            _uniteOfWork.Product.Remove(ProductInDb);
            var oldImg = Path.Combine(_webHostEnvironment.WebRootPath, ProductInDb.Img.TrimStart('\\'));

            if (System.IO.File.Exists(oldImg))
                System.IO.File.Delete(oldImg);

            _uniteOfWork.Complete();


            return Json(new { success = true, message = "file has been deleted" });


        }

    }
}
