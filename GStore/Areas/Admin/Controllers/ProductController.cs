using GStoreWeb.DataAccess.Repository.IRepository;
using GStoreWeb.Models;
using GStoreWeb.Models.ViewModels;
using GStoreWeb.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace GStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class ProductController : Controller
    {
        private IUnitOfWork _unitOfWork;
        private IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {

            
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> categories = _unitOfWork.CategoryUnit.GetAll().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
            ProductVM productVM = new()
            {
                Product = new Product(),
                CategoryList = categories
            };
            if(id != 0 && id != null)
            {
                productVM.Product = _unitOfWork.ProductUnit.Get(u => u.Id == id);
            }
            return View(productVM);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(wwwRootPath, @"images\product");

                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using(FileStream stream = new FileStream(Path.Combine(filePath, fileName), mode:FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if(productVM.Product.Id == 0)
                {
                    _unitOfWork.ProductUnit.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.ProductUnit.Update(productVM.Product);
                }
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                IEnumerable<SelectListItem> categories = _unitOfWork.CategoryUnit.GetAll().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
                productVM.CategoryList = categories;
                return View(productVM);
            }
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = _unitOfWork.ProductUnit.GetAll(includeProperties: "Category").ToList();

            return Json(new { data = products });
        }

        public IActionResult Delete(int? id)
        {
            Product product = _unitOfWork.ProductUnit.Get(p=>p.Id==id);
            if(product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }
            else
            {
                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
                _unitOfWork.ProductUnit.Remove(product);
                _unitOfWork.Save();
            }

            return Json(new { success = true, message = "Product successfully deleted" });
        }
        #endregion
    }
}
