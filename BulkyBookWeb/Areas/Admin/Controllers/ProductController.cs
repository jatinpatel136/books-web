using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.ROLE_ADMIN)]
	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _hostEnvironment;

		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_hostEnvironment = hostEnvironment;
		}

		public IActionResult Index()
		{
			
			return View();
		}

		
		//Get
		public IActionResult Upsert(int? id)
		{
			ProductViewModel productViewModel = new ProductViewModel()
			{
				Product = new Product(),
				CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
				{
					Text = i.Name,
					Value = i.Id.ToString()
				}),
				CoverTypeList = _unitOfWork.CoverType.GetAll().Select(i => new SelectListItem
				{
					Text = i.Name,
					Value = i.Id.ToString()
				})
			};


			

			if(id ==null || id == 0)
			{
				//create Product
				//ViewBag.CategoryList = categoryList;
				//ViewData["CoverTypeList"] = coverTypeList;
				return View(productViewModel);
			}
			else
			{
				//update product
				productViewModel.Product = _unitOfWork.Product.GetFirstOrDefault(p => p.Id == id);
				return View(productViewModel);
			}
		}

		//POST
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Upsert(ProductViewModel obj, IFormFile? file)
		{
			if (ModelState.IsValid)
			{
				string wwwRootPath = _hostEnvironment.WebRootPath;
				if (file != null)
				{
					string fileName = Guid.NewGuid().ToString();
					var uploads = Path.Combine(wwwRootPath, @"images\products");
					var extension = Path.GetExtension(file.FileName);

					//Delete if image already exists in the directory
					if (obj.Product.ImageUrl != null)
					{
						var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));

						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}

					using(var fileStreams = new FileStream(Path.Combine(uploads, fileName+extension), FileMode.Create))
					{
						file.CopyTo(fileStreams);
					}

					obj.Product.ImageUrl = @"\images\products\" + fileName + extension;
				}

				if(obj.Product.Id == 0)
				{
					//This is creating new product
					_unitOfWork.Product.Add(obj.Product);
				}
				else
				{
					//This is updating the existing product
					_unitOfWork.Product.Update(obj.Product);
				}

				_unitOfWork.Save();

				TempData["success"] = "Product added successfully";
				return RedirectToAction("Index");
			}

			return View(obj);
		}


		#region API CALLS

		[HttpGet]
		public IActionResult GetAll()
		{
			IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
			return Json(new { data = productList });
		}

		
		//POST
		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var productFromDbFirst = _unitOfWork.Product.GetFirstOrDefault(c => c.Id == id);

			if (productFromDbFirst == null)
			{
				return Json(new {success= false, message= "Error while deleting"});
			}

			var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, productFromDbFirst.ImageUrl.TrimStart('\\'));

			if (System.IO.File.Exists(oldImagePath))
			{
				System.IO.File.Delete(oldImagePath);
			}

			_unitOfWork.Product.Remove(productFromDbFirst);
			_unitOfWork.Save();


			return Json(new { success = true, message = "delete successful" });
		}

		#endregion
	}


}
