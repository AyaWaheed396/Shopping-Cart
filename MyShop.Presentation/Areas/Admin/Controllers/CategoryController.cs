using Microsoft.AspNetCore.Mvc;
using MyShop.BusinessLogic.Models;
using MyShop.BusinessLogic.Repositories;
using MyShop.DataAccess.Data;


namespace MyShop.Presentation.Areas.Admin.Controllers
{
    [Area ("Admin")]
    public class CategoryController : Controller
    {
        private IUniteOfWork _uniteOfWork;
        public CategoryController(IUniteOfWork uniteOfWork)
        {
            _uniteOfWork = uniteOfWork;
        }

        public IActionResult Index()
        {
            var categories = _uniteOfWork.Category.GetAll();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _uniteOfWork.Category.Add(category);
                _uniteOfWork.Complete();
                TempData["Create"] = "Data Has Created Successfully";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null | id == 0)
                NotFound();

            var categoryInDb = _uniteOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            return View(categoryInDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _uniteOfWork.Category.Update(category);
                _uniteOfWork.Complete();
                TempData["Update"] = "Data Has Updated Successfully";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null | id == 0)
                NotFound();

            var categoryInDb = _uniteOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            return View(categoryInDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int? id)
        {
            var categoryInDb = _uniteOfWork.Category.GetFirstOrDefault(x => x.Id == id);
            if (categoryInDb == null)
                NotFound();

            _uniteOfWork.Category.Remove(categoryInDb);
            _uniteOfWork.Complete();
            TempData["Delete"] = "Data Has Deleted Successfully";
            return RedirectToAction("Index");
        }

    }
}
