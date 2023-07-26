using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using BulkyBookWeb.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ROLE_ADMIN)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;


        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> objCategoryList = _unitOfWork.Category.GetAll();
            return View(objCategoryList);
        }

        //Get
        public IActionResult Create()
        {
            return View();
        }


        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {

                //Display CustomError Message 
                //ModelState.AddModelError("CustomError", "The DisplayOrder cannot exactly match the Name");

                //Display Error Message for Name 
                ModelState.AddModelError("Name", "The DisplayOrder cannot exactly match the Name");
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();

                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }

        //Get
        public IActionResult Edit(int? id)
        {
            //return notfound if incorrect id
            if (id == null || id == 0)
            {
                return NotFound();
            }

            //var categoryFromDb = _db.Categories.Find(id);
            var categoryFromDbFirst = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
            //var categoryFromDbSingle = _db.Categories.SingleOrDefault(c => c.Id == id);

            //return notfound if category doesnot exist
            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }

            //return the category if found
            return View(categoryFromDbFirst);
        }


        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {

                //Display CustomError Message 
                //ModelState.AddModelError("CustomError", "The DisplayOrder cannot exactly match the Name");

                //Display Error Message for Name 
                ModelState.AddModelError("Name", "The DisplayOrder cannot exactly match the Name");
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();

                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }

            return View(obj);
        }


        //Get
        public IActionResult Delete(int? id)
        {
            //return notfound if incorrect id
            if (id == null || id == 0)
            {
                return NotFound();
            }

            //var categoryFromDb = _db.Categories.Find(id);
            var categoryFromDbFirst = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
            //var categoryFromDbSingle = _db.Categories.SingleOrDefault(c => c.Id == id);

            //return notfound if category doesnot exist
            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }

            //return the category if found
            return View(categoryFromDbFirst);
        }

        //POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            //var obj = _db.Categories.Find(id);
            var categoryFromDbFirst = _unitOfWork.Category.GetFirstOrDefault(c => c.Id == id);
            if (categoryFromDbFirst == null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Remove(categoryFromDbFirst);
            _unitOfWork.Save();

            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");

        }
    }
}
