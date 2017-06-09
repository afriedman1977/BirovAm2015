using BirovAm.data;
using BirovAm2015.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BirovAm2015.Controllers
{
    public class ProductsController : Controller
    {
        // GET: Products
        public ActionResult Index()
        {
            var pim = new ProductsIndexModel();
            var repo = new ProductsRepository();
            pim.Categories = repo.AllCategories().ToList();
            pim.Products = repo.AllProducts().ToList();
            return View(pim);
        }

        public ActionResult Categories()
        {
            var repo = new ProductsRepository();
            return View(repo.AllCategories());
        }

        [HttpPost]
        public ActionResult AddCategory(Category c)
        {
            var repo = new ProductsRepository();
            repo.AddCategory(c);
            return Redirect("/Products/Categories");
        }

        public ActionResult GetCategoryById(int id)
        {
            var repo = new ProductsRepository();
            Category c = repo.CategoryById(id);
            var categoryJson = new
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                CategoryDescription = c.CategoryDescription
            };
            return Json(categoryJson, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditCategory(Category c)
        {
            var repo = new ProductsRepository();
            repo.EditCategory(c);
            return Redirect("/Products/Categories");
        }

        [HttpPost]
        public ActionResult DeleteCategory(int id)
        {
            var repo = new ProductsRepository();
            repo.DeleteCategory(id);
            return Redirect("/Products/Categories");
        }

        public ActionResult Sizes()
        {
            SizesViewModel svm = new SizesViewModel();
            var repo = new ProductsRepository();
            svm.Sizes = repo.AllSizes().ToList();
            svm.Categories = repo.AllCategories().ToList();
            return View(svm);
        }

        [HttpPost]
        public ActionResult AddSize(Size s, List<Category> categories)
        {
            var repo = new ProductsRepository();
            repo.AddSize(s, categories.Where(x => x.Included == true).ToList());
            return Redirect("/Products/Sizes");
        }

        public ActionResult GetSizeById(int id)
        {
            var repo = new ProductsRepository();
            Size size = repo.SizeById(id);
            var sizeJson = new
            {
                SizeID = size.SizeID,
                SizeCode = size.SizeCode,
                Size1 = size.Size1,
                Categories = cats(size.Categories.ToList())
            };
            return Json(sizeJson, JsonRequestBehavior.AllowGet);
        }

        private List<Category> cats(List<Category> cs)
        {
            List<Category> categories = new List<Category>();
            foreach (Category c in cs)
            {
                categories.Add(new Category
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    Included = true
                });
            }
            return categories;
        }

        [HttpPost]
        public ActionResult EditSize(Size s, List<Category> categories)
        {
            var repo = new ProductsRepository();
            s.Categories = categories.Where(x => x.Included == true).ToList();
            repo.EditSize(s);
            return Redirect("/Products/Sizes");
        }

        [HttpPost]
        public ActionResult DeleteSize(int sId)
        {
            var repo = new ProductsRepository();
            repo.DeleteSize(sId);
            return Redirect("/Products/Sizes");
        }

        [HttpPost]
        public ActionResult AddProduct(Product p, HttpPostedFileBase SoundFile)
        {
            if (SoundFile != null)
            {
                //string fileName = Guid.NewGuid() + Path.GetExtension(SoundFile.FileName);
                string fileName = SoundFile.FileName;
                SoundFile.SaveAs(Server.MapPath("~/SoundFiles/" + fileName));
                p.SoundFilePath = fileName;
            }
            var repo = new ProductsRepository();
            repo.AddProduct(p);
            return Redirect("/Products/EnterSizes?pId=" + p.ProductID);
        }

        public ActionResult EnterSizes(int pId)
        {
            var repo = new ProductsRepository();
            EnterSizesViewModel esvm = new EnterSizesViewModel();
            esvm.Product = repo.ProductByID(pId);
            esvm.Sizes = repo.AllSizes().Where(s => s.Categories.Any(c => c.CategoryID == esvm.Product.CategoryID)).ToList();
            return View(esvm);
        }

        [HttpPost]
        public ActionResult SubmitSizes(List<ProductsSize> ps)
        {
            var repo = new ProductsRepository();
            repo.AddProductSizes(ps);
            return Redirect("/Products/Index");
        }

        public ActionResult GetProductById(int id)
        {
            var repo = new ProductsRepository();
            Product p = repo.ProductByID(id);
            var jsonProduct = new Product
            {
                ProductID = p.ProductID,
                ProductCode = p.ProductCode,
                StyleNumber = p.StyleNumber,
                Brand = p.Brand,
                Description = p.Description,
                Price = p.Price,
                CategoryID = p.CategoryID,
                SoundFilePath = p.SoundFilePath
            };
            return Json(jsonProduct, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditProduct(Product p, HttpPostedFileBase SoundFile)
        {
            string fileName = "";
            if (SoundFile != null)
            {
                //string fileName = Guid.NewGuid() + Path.GetExtension(SoundFile.FileName);
                fileName = SoundFile.FileName;
                SoundFile.SaveAs(Server.MapPath("~/SoundFiles/" + fileName));
            }
            p.SoundFilePath = SoundFile != null ? fileName : p.SoundFilePath;
            var repo = new ProductsRepository();
            repo.EditProduct(p);
            return Redirect("/Products/EnterSizes?pId=" + p.ProductID);
        }

        public ActionResult GetDetails(int id)
        {
            var repo = new ProductsRepository();
            return View(repo.ProductByID(id));
        }

        [HttpPost]
        public ActionResult DeleteProduct(int id)
        {
            var repo = new ProductsRepository();
            repo.DeleteProduct(id);
            return Redirect("/Products/Index");
        }
    }
}