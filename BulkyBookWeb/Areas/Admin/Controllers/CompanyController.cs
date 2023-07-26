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
	public class CompanyController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		public CompanyController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			
			return View();
		}

		
		//Get
		public IActionResult Upsert(int? id)
		{
			Company company = new Company();			

			if(id ==null || id == 0)
			{
				//create Company
				return View(company);
			}
			else
			{
				//update company
				company = _unitOfWork.Company.GetFirstOrDefault(p => p.Id == id);
				return View(company);
			}
		}

		//POST
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Upsert(Company obj)
		{
			if (ModelState.IsValid)
			{
				if(obj.Id == 0)
				{
					_unitOfWork.Company.Add(obj);
					TempData["success"] = "Company created successfully";
				}
				else
				{
					_unitOfWork.Company.Update(obj);
					TempData["success"] = "Company updated successfully";
				}

				_unitOfWork.Save();
				return RedirectToAction("Index");
			}

			return View(obj);
		}


		#region API CALLS

		[HttpGet]
		public IActionResult GetAll()
		{
			IEnumerable<Company> companyList = _unitOfWork.Company.GetAll();
			return Json(new { data = companyList});
		}

		
		//POST
		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var companyFromDbFirst = _unitOfWork.Company .GetFirstOrDefault(c => c.Id == id);

			if (companyFromDbFirst == null)
			{
				return Json(new {success= false, message= "Error while deleting"});
			}


			_unitOfWork.Company.Remove(companyFromDbFirst);
			_unitOfWork.Save();


			return Json(new { success = true, message = "Delete successful" });
		}

		#endregion
	}


}
