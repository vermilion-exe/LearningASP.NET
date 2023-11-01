using GStoreWeb.DataAccess.Repository.IRepository;
using GStoreWeb.Models.ViewModels;
using GStoreWeb.Models;
using GStoreWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace GStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class CompanyController : Controller
    {
        private IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            Company company = new Company();
            if (id != 0 && id != null)
            {
                company = _unitOfWork.CompanyUnit.Get(c => c.Id == id);
            }
            return View(company);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                {
                    _unitOfWork.CompanyUnit.Add(company);
                }
                else
                {
                    _unitOfWork.CompanyUnit.Update(company);
                }
                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(company);
            }
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companies = _unitOfWork.CompanyUnit.GetAll().ToList();

            return Json(new { data = companies });
        }

        public IActionResult Delete(int? id)
        {
            Company company = _unitOfWork.CompanyUnit.Get(p => p.Id == id);
            if (company == null)
            {
                return Json(new { success = false, message = "Company not found" });
            }
            else
            {
                _unitOfWork.CompanyUnit.Remove(company);
                _unitOfWork.Save();
            }

            return Json(new { success = true, message = "Company successfully deleted" });
        }
        #endregion
    }
}
