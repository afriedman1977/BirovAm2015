using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirovAm.data
{
    public class ProductsRepository
    {
        public void AddCategory(Category c)
        {
            using (var ctx = new BirovAmContext())
            {
                ctx.Categories.Add(c);
                ctx.SaveChanges();
            }
        }

        public void EditCategory(Category c)
        {
            using (var ctx = new BirovAmContext())
            {
                ctx.Entry(c).State = EntityState.Modified;
                ctx.SaveChanges();
            }
        }

        public void DeleteCategory(int cId)
        {
            using (var ctx = new BirovAmContext())
            {
                Category c = ctx.Categories.Where(x => x.CategoryID == cId).FirstOrDefault();
                ctx.Entry(c).State = EntityState.Deleted;
                ctx.SaveChanges();
            }
        }

        public void AddSize(Size s, List<Category> c)
        {
            using (var ctx = new BirovAmContext())
            {
                if (ctx.Sizes.Any(x => x.Size1 == s.Size1))
                {
                    return;
                }
                foreach (Category ca in c)
                {
                    ctx.Categories.Attach(ca);
                }
                s.Categories = c;
                ctx.Sizes.Add(s);
                ctx.SaveChanges();
            }
        }

        public void EditSize(Size s)
        {
            using (var ctx = new BirovAmContext())
            {
                List<Category> ctxCategories = new List<Category>();
                foreach (Category cato in s.Categories)
                {
                    Category ctxCategory = ctx.Categories.Where(z => z.CategoryID == cato.CategoryID).FirstOrDefault();
                    ctxCategories.Add(ctxCategory);
                }
                Size size = ctx.Sizes.Include(x => x.Categories).Where(x => x.SizeID == s.SizeID).FirstOrDefault();
                size.Size1 = s.Size1;
                size.SizeCode = s.SizeCode;
                size.Categories = ctxCategories;
                ctx.SaveChanges();
            }
        }

        public void DeleteSize(int sId)
        {
            using (var ctx = new BirovAmContext())
            {
                Size s = ctx.Sizes.Where(si => si.SizeID == sId).FirstOrDefault();
                ctx.Entry(s).State = EntityState.Deleted;
                ctx.SaveChanges();
            }
        }

        public void AddProduct(Product p)
        {
            using (var ctx = new BirovAmContext())
            {
                ctx.Products.Add(p);
                ctx.SaveChanges();
            }
        }

        public void AddProductSizes(List<ProductsSize> ps)
        {
            using (var ctx = new BirovAmContext())
            {
                foreach (ProductsSize s in ps)
                {
                    ProductsSize prs = ctx.ProductsSizes.Where(i => i.ProductID == s.ProductID && i.SizeID == s.SizeID).FirstOrDefault();
                    if (prs == null)
                    {
                        if (s.Included)
                        {
                            ctx.ProductsSizes.Add(s);
                        }
                    }
                    else
                    {
                        if (s.Included)
                        {
                            if (prs.Stock != s.Stock)
                            {
                                prs.Stock = s.Stock;
                            }
                        }
                        else
                        {
                            ctx.Entry(prs).State = EntityState.Deleted;
                        }
                    }
                    ctx.SaveChanges();
                }

            }
        }

        public void EditProduct(Product p)
        {
            using (var ctx = new BirovAmContext())
            {
                ctx.Entry(p).State = EntityState.Modified;
                ctx.SaveChanges();
            }
        }

        public void DeleteProduct(int pId)
        {
            using (var ctx = new BirovAmContext())
            {
                List<ProductsSize> ps = ctx.ProductsSizes.Where(x => x.ProductID == pId).ToList();
                foreach (ProductsSize pr in ps)
                {
                    ctx.Entry(pr).State = EntityState.Deleted;
                }
                Product p = ctx.Products.Where(x => x.ProductID == pId).FirstOrDefault();
                ctx.Entry(p).State = EntityState.Deleted;
                ctx.SaveChanges();
            }
        }

        public IEnumerable<Category> AllCategories()
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Categories.ToList();
            }
        }

        public Category CategoryById(int id)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Categories.Where(c => c.CategoryID == id).FirstOrDefault();
            }
        }

        public IEnumerable<Size> AllSizes()
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Sizes.Include(s => s.Categories).OrderBy(s => s.SizeCode).ToList();
            }
        }

        public Size SizeById(int id)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Sizes.Include(s => s.Categories).Where(s => s.SizeID == id).FirstOrDefault();
            }
        }

        public IEnumerable<Product> AllProducts()
        {
            using (var ctx = new BirovAmContext())
            {
                ctx.Configuration.LazyLoadingEnabled = false;
                return ctx.Products.Include(p => p.ProductsSizes.Select(x => x.Size)).OrderBy(p => p.ProductCode).ToList();
            }
        }

        public Product ProductByID(int pId)
        {
            using (var ctx = new BirovAmContext())
            {
                return ctx.Products.Where(p => p.ProductID == pId).Include(p => p.ProductsSizes.Select(x => x.Size)).Include(p => p.Category).FirstOrDefault();
            }
        }
    }
}
