using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;

namespace Online_Auction.Controllers
{
    [Authorize(Roles = Roles.Role_Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<Register> _userManager;

        public AdminController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment, UserManager<Register> userManager)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }
        public IActionResult Dashboard()
        {
            var currentDate = DateTime.Now.Date;

            var product_list = _dbContext.Products
                .Where(x => x.ProductStatus == "active" && x.AuctionEndDate >= currentDate)
                .OrderBy(x => x.AuctionEndDate)
                .ToList();

            ViewBag.TotalProducts = _dbContext.Products.Where(x => x.ProductStatus == "active").Count();
            ViewBag.TotalUsers = _dbContext.Register.Where(u => _dbContext.UserRoles.Where(ur => ur.UserId == u.Id).Any(ur => _dbContext.Roles.Where(r => r.Id == ur.RoleId).Any(r => r.Name == "User"))).Count();
            var totalCurrBidPrice = _dbContext.Products
                                    .Where(x => x.AuctionEndDate.Date == DateTime.Today && x.ProductStatus == "sold")
                                    .Sum(x => x.CurrBidPrice);
            ViewBag.TotalCurrBidPrice = totalCurrBidPrice;
            var soldItemCount = _dbContext.Products
                                .Where(x => x.AuctionEndDate.Date == DateTime.Today && x.ProductStatus == "sold")
                                .Count();
            ViewBag.SoldItemCountToday = soldItemCount;
            var getUserid = _userManager.GetUserId(User);
            var getUserName = _dbContext.Register.Where(x=>x.Id == getUserid).Select(y=>y.FullName).FirstOrDefault();
            ViewBag.UserName = getUserName;

            var getNonActiveProducts = _dbContext.Products.Where(x => x.ProductStatus == "non-active").Count();
            ViewBag.NonActiveProducts = getNonActiveProducts;
            return View(product_list);
        }
        public IActionResult Users(string seacrh)
        {
            IQueryable<Register> query = _dbContext.Register
                             .Where(u => u.Status == "active" &&
                    _dbContext.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Any(ur => _dbContext.Roles
                            .Where(r => r.Id == ur.RoleId)
                            .Any(r => r.Name == "User")));

            if (!string.IsNullOrWhiteSpace(seacrh))
            {
                query = query.Where(u => u.Email.Contains(seacrh));
            }

            var users_list = query.ToList();

            var UserListViewWithProductCount = new List<UsersListViewModel>();

            foreach(var list in users_list)
            {
                int product_count = _dbContext.Products.Where(x=>x.ProductStatus == "active").Count(p=>p.UserId == list.Id);

                var UsersWithCount = new UsersListViewModel
                {
                    Users = list,
                    UserProductCount = product_count
                };

                UserListViewWithProductCount.Add(UsersWithCount);

            }

            return View(UserListViewWithProductCount);
        }
        public IActionResult Category()
        {
            var category_List = _dbContext.Category.ToList();
            var categoryViewModelList = new List<CategoryWithProductCount>();

            foreach (var category in category_List)
            {
                int productCount = _dbContext.Products.Where(x=>x.ProductStatus == "active").Count(p => p.CategoryId == category.CatId);

                var categoryViewModel = new CategoryWithProductCount
                {
                    Category = category,
                    ProductCount = productCount
                };

                categoryViewModelList.Add(categoryViewModel);
            }
            return View(categoryViewModelList);
        }
        [HttpPost]
        public IActionResult add_category()
        {
            string catName = Request.Form["CatName"];
            string catDes = Request.Form["CatDes"];
            var catImg = Request.Form.Files[0];

            string filename = "";
            if(catImg != null)
            {
                string uploadfolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                filename = Guid.NewGuid().ToString() + " " + catImg.FileName;
                string filepath = Path.Combine(uploadfolder, filename);
                string extension = Path.GetExtension(catImg.FileName);
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png" || extension.ToLower() == ".webp")
                {
                    catImg.CopyTo(new FileStream(filepath, FileMode.Create));
                    if (catImg.Length <= 104857600)
                    {
                        Category cat = new Category
                        {
                            CatName = catName,
                            CatDescription = catDes,
                            CatImg = filename
                        };

                        _dbContext.Category.Add(cat);
                        _dbContext.SaveChanges();
                        TempData["temp"] = "Category Added Successfully";
                        return RedirectToAction("Category", "Admin");
                    }
                }
            }
            else
            {
                TempData["temp"] = "Image is not found";
                return RedirectToAction("Category", "Admin");
            }

            return RedirectToAction("Category", "Admin");
        }

        public IActionResult Delete_Category(int id)
        {
            var get_cat_by_id = _dbContext.Category.Find(id);

            var get_product_Count = _dbContext.Products.Where(x=>x.CategoryId == id).Count();

            if(get_product_Count > 0)
            {
                TempData["message"] = "This Category Contain Items. You can't Delete it";
                return RedirectToAction("Category", "Admin");
            }

            _dbContext.Category.Remove(get_cat_by_id);
            _dbContext.SaveChanges();
            return RedirectToAction("Category", "Admin");
        }

        public IActionResult Products()
        {
            var non_activeProducts = _dbContext.Products.Where(x => x.ProductStatus == "non-active").Include(y => y.category).ToList();
            return View(non_activeProducts);  
        }
        public IActionResult ProductApprove(int id)
        {
            var get_product = _dbContext.Products.Find(id);

            if (get_product != null)
            {
                get_product.ProductStatus = "active";

                _dbContext.Products.Update(get_product);
                _dbContext.SaveChanges();
            }
            return RedirectToAction("Products", "Admin");
        }

        public IActionResult ProductReject(int id)
        {
            var get_product = _dbContext.Products.Find(id);

            if (get_product != null)
            {
                get_product.ProductStatus = "reject";

                _dbContext.Products.Update(get_product);
                _dbContext.SaveChanges();
            }
            return RedirectToAction("Products", "Admin");
        }

        public IActionResult RejectedItemList()
        {
            var get_rejected_items = _dbContext.Products.Where(x => x.ProductStatus == "reject").Include(y=>y.category).ToList();
            return View(get_rejected_items);
        }

        public IActionResult Reports()
        {
            var getReports = _dbContext.Reports.Where(x=>x.status == "active").Include(y=>y.fromuser).Include(z=>z.touser).ToList();
            return View(getReports);
        }

        public async Task<IActionResult> MarkFraud(string id)
        {
            var get_user = await _userManager.FindByIdAsync(id);
            if (get_user != null)
            {
                get_user.Status = "fraud";
                var result = await _userManager.UpdateAsync(get_user);
                if (result.Succeeded)
                {
                    var getProductWithUser = _dbContext.Products.Where(x => x.UserId == get_user.Id).ToList();

                    if (getProductWithUser != null)
                    {
                        foreach(var product in getProductWithUser)
                        {
                            product.ProductStatus = "fraud";
                        }
                    }

                    _dbContext.SaveChanges();

                    TempData["SuccessMessage"] = "User Marked As Fraud";
                    return RedirectToAction("Users", "Admin");
                }
                return RedirectToAction("Users", "Admin");
            }
            return RedirectToAction("Users","Admin");
        }



        public IActionResult FraudUsers()
        {
            var getfraudUser = _dbContext.Register.Where(x => x.Status == "fraud").ToList();
            return View(getfraudUser);
        }



        public async Task<IActionResult> MarkASActive(string id)
        {
            var getUser = await _userManager.FindByIdAsync(id);

            if (getUser != null)
            {
                getUser.Status = "active";
                var result = await _userManager.UpdateAsync(getUser);
                if (result.Succeeded)
                {
                    var get_user_product = _dbContext.Products.Where(x=>x.UserId == getUser.Id).ToList();
                    foreach(var product in get_user_product)
                    {
                        product.ProductStatus = "active";
                    }
                    _dbContext.SaveChanges();
                    return RedirectToAction("FraudUsers", "Admin");
                }
            }
            return RedirectToAction("FraudUsers", "Admin");
        }



        public IActionResult GetAllInfo(string id)
        {
            var get_User_data = _dbContext.Register.Find(id);
            var get_Products_related = _dbContext.Products.Where(x=>x.UserId == get_User_data.Id).ToList();
            var get_bid_history = _dbContext.Bids.Where(x => x.UserId == get_User_data.Id).Include(y=>y.Products).ToList();
            var get_report = _dbContext.Reports.Where(x => x.ToUserId == get_User_data.Id).Include(y=>y.fromuser).ToList();
            var get_Bought_Item = _dbContext.Products.Where(x => x.SoldToUserId == get_User_data.Id).ToList();

            GetAllInfoModel data = new GetAllInfoModel
            {
                User = get_User_data,
                Items = get_Products_related,
                Bids = get_bid_history,
                reports = get_report,
                ItemBought = get_Bought_Item
            };

            return View(data);
        }


        public IActionResult ReportDone(int id)
        {
            var get_report = _dbContext.Reports.Find(id);
            get_report.status = "done";
            _dbContext.Update(get_report); ;
            _dbContext.SaveChanges();
            return RedirectToAction("Reports", "Admin");
        }

        public IActionResult Edit_Category(int Id)
        {
            var get_category = _dbContext.Category.Find(Id);
            return View(get_category);
        }
        public IActionResult UpdateCat(int id, Category category)
        {
            var get_categoryforUpdate = _dbContext.Category.Find(id);
            if(get_categoryforUpdate != null)
            {
                get_categoryforUpdate.CatName = category.CatName;
                get_categoryforUpdate.CatDescription = category.CatDescription;

                if (category.CatPicture != null)
                {
                    string filename = "";
                    string uploadfolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    filename = Guid.NewGuid().ToString() + " " + category.CatPicture.FileName;
                    string filepath = Path.Combine(uploadfolder, filename);
                    string extension = Path.GetExtension(category.CatPicture.FileName);
                    if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png" || extension.ToLower() == ".webp")
                    {
                        category.CatPicture.CopyTo(new FileStream(filepath, FileMode.Create));
                        get_categoryforUpdate.CatImg = filename;
                    }
                    else
                    {
                        TempData["Message"] = "Extension Not Allowed";
                        return RedirectToAction("Edit_Category", "Admin", new { id = id });
                    }
                }

                _dbContext.SaveChanges();
                return RedirectToAction("Category", "Admin");
            }
            return RedirectToAction("Category", "Admin");
        }


        public IActionResult Merge_Category(int ID)
        {
            
            var Get_Cat = _dbContext.Category.Find(ID);

            if(Get_Cat != null)
            {
                var Cat_List = _dbContext.Category.Where(x=>x.CatId != ID).ToList();

                MergeCatModel data = new MergeCatModel
                {
                    MCategory = Get_Cat,
                    CategoriesList = Cat_List
                };

                return View(data);
            }

            return RedirectToAction("Category", "Admin");
        }

        [HttpPost]
        public IActionResult MergeCat(int cat1, int cat2)
        {
            try
            {
                using (var transaction = _dbContext.Database.BeginTransaction())
                {

                    var productsToUpdate = _dbContext.Products.Where(x => x.CategoryId == cat1).ToList();
                    foreach (var product in productsToUpdate)
                    {
                        product.CategoryId = cat2;
                    }

                    var categoryToRemove = _dbContext.Category.Find(cat1);
                    if (categoryToRemove != null)
                    {
                        _dbContext.Category.Remove(categoryToRemove);
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        transaction.Rollback();
                        return NotFound("Category not found");
                    }

                    transaction.Commit();
                }

                return RedirectToAction("Category", "Admin");
            }
            catch (Exception ex)
            {
                TempData["message"] = "Please Select Category to Merge";
                return RedirectToAction("Merge_Category", "Admin", new {id = cat1});
            }
        }






    }
}
