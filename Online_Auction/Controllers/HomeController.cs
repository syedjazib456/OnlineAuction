using Microsoft.AspNetCore.Mvc;
using Online_Auction.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;


namespace Online_Auction.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<Register> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment, UserManager<Register> userManager)
        {
            _logger = logger;
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public void CheckProductForSale()
        {
            var currentTime = DateTime.Now;
            var get_products = _dbContext.Products
                .Where(p => p.AuctionEndDate <= currentTime && p.ProductStatus == "active")
                .ToList();

            foreach (var product in get_products)
            {
                // Check if the product is already sold
                if (product.ProductStatus != "sold")
                {
                    product.ProductStatus = "sold";
                    var bid = _dbContext.Bids.FirstOrDefault(x => x.BidAmount == product.CurrBidPrice && x.ProductId == product.ProductId);
                    var seller = _dbContext.Register.FirstOrDefault(x => x.Id == product.UserId);

                    if (bid != null)
                    {
                        var buyer = _dbContext.Register.FirstOrDefault(x => x.Id == bid.UserId);

                        if (seller != null && buyer != null)
                        {

                            product.SoldToUserId = buyer.Id;

                            // Add seller notification
                            string sellerNotificationMessage = $"Your item '{product.ProductTitle}' has been sold to {buyer.FullName}. You can contact them.";
                            var sellerNotification = new Notifications
                            {
                                NotificationMessage = sellerNotificationMessage,
                                NotificationTimeStamp = currentTime,
                                NotificationTo = seller.Id,
                                Link = buyer.Id
                            };
                            _dbContext.Notifications.Add(sellerNotification);

                            // Add buyer notification
                            string buyerNotificationMessage = $"Congratulations! You have successfully purchased the item '{product.ProductTitle}'.";
                            var buyerNotification = new Notifications
                            {
                                NotificationMessage = buyerNotificationMessage,
                                NotificationTimeStamp = currentTime,
                                NotificationTo = buyer.Id,
                                Link = seller.Id
                            };
                            _dbContext.Notifications.Add(buyerNotification);
                        }
                        else
                        {
                            // Handle null user (seller or buyer)
                            // Log or handle the error appropriately
                        }
                    }
                    else
                    {

                        string sellerNotificationMessage = $"Your item '{product.ProductTitle}' End Date Has been Finished. No one Bid Yet";
                        var sellerNotification = new Notifications
                        {
                            NotificationMessage = sellerNotificationMessage,
                            NotificationTimeStamp = currentTime,
                            NotificationTo = seller.Id,
                            Link = seller.Id
                        };

                        _dbContext.Notifications.Add(sellerNotification);
                        product.SoldToUserId = "none";
                    }
                }
            }

            _dbContext.SaveChanges();
        }

        public IActionResult GetNotification()
        {
            var getcurrentuser = _userManager.GetUserId(User);
            var get_notification = _dbContext.Notifications.Where(x => x.NotificationTo == getcurrentuser).ToList();
            var json = JsonConvert.SerializeObject(get_notification);
            return Content(json, "application/json");
        }

        public IActionResult DeleteNotification(int notiid)
        {
            var findnoti = _dbContext.Notifications.Find(notiid);
            if (findnoti != null)
            {
                _dbContext.Remove(findnoti);
                _dbContext.SaveChanges();
                return Json("Notification Deleted");
            }

            return Json("Notification not Found");
        }

        public IActionResult Index()
        {
            var category_List = _dbContext.Category.ToList();
            var LatestItemList = _dbContext.Products.Where(z=>z.ProductStatus == "active").OrderByDescending(x => x.TimeStamp).Take(6).ToList();
            var TopBidedItem = _dbContext.Products.Where(z => z.ProductStatus == "active").OrderByDescending(y => y.CurrBidPrice).Take(6).ToList();
            var categoryViewModelList = new List<CategoryWithProductCount>(); // Change to List<CategoryWithProductCount>

            foreach (var category in category_List)
            {
                int productCount = _dbContext.Products
                    .Count(p => p.ProductStatus == "active" && p.CategoryId == category.CatId);

                var categoryViewModel = new CategoryWithProductCount
                {
                    Category = category,
                    ProductCount = productCount
                };

                categoryViewModelList.Add(categoryViewModel);
            }

            var itemsForHomePage = new GetProductsForHomePage
            {
                LatestItems = LatestItemList,
                TopBidedItems = TopBidedItem,
                CategoryWithCount = categoryViewModelList
            };

            return View(itemsForHomePage);
        }


        public async Task<IActionResult> NotificationClick(int id)
        {
            var get_click_notification = await _dbContext.Notifications.FindAsync(id);
            if (get_click_notification != null)
            {
                get_click_notification.NotificationStatus = "read";
            }
            await _dbContext.SaveChangesAsync();
            return Json("Done");
        }



        public IActionResult SellAnItem()
        {
            var SellAnItem = new SellAnItem
            {
                Products = new Products(),
                Category = _dbContext.Category.ToList()
            };
            return View(SellAnItem);
        }

        public async Task<IActionResult> UploadProduct(SellAnItem product)
        {
            if (product.Products != null)
            {
                string filename = "0";
                string filename1 = "0";
                string filename2 = "0";
                if (product.Products.ProductPicture != null)
                {
                    string uploadfolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    filename = Guid.NewGuid().ToString() + " " + product.Products.ProductPicture.FileName;
                    string filepath = Path.Combine(uploadfolder, filename);
                    string extension = Path.GetExtension(product.Products.ProductPicture.FileName);
                    if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png" || extension.ToLower() == ".webp")
                    {
                        if (product.Products.ProductPicture.Length <= 104857600)
                        {
                           product.Products.ProductPicture.CopyTo(new FileStream(filepath, FileMode.Create));
                        }
                        else
                        {
                            TempData["error"] = "File size exceeds the limit (100MB)";
                        }
                    }
                    else
                    {
                        TempData["error"] = "Invalid file format";
                    }
                }

                if (product.Products.ProductPicture1 != null)
                {
                    string uploadfolder1 = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    filename1 = Guid.NewGuid().ToString() + " " + product.Products.ProductPicture1.FileName;
                    string filepath1 = Path.Combine(uploadfolder1, filename1);
                    string extension1 = Path.GetExtension(product.Products.ProductPicture1.FileName);
                    if (extension1.ToLower() == ".jpg" || extension1.ToLower() == ".jpeg" || extension1.ToLower() == ".png" || extension1.ToLower() == ".webp")
                    {
                        if (product.Products.ProductPicture1.Length <= 104857600)
                        {
                            product.Products.ProductPicture1.CopyTo(new FileStream(filepath1, FileMode.Create));
                        }
                        else
                        {
                            TempData["error"] = "File size exceeds the limit (100MB)";
                        }
                    }
                    else
                    {
                        TempData["error"] = "Invalid file format";
                    }
                }

                if (product.Products.ProductPicture2 != null)
                {
                    string uploadfolder2 = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    filename2 = Guid.NewGuid().ToString() + " " + product.Products.ProductPicture2.FileName;
                    string filepath2 = Path.Combine(uploadfolder2, filename2);
                    string extension2 = Path.GetExtension(product.Products.ProductPicture2.FileName);
                    if (extension2.ToLower() == ".jpg" || extension2.ToLower() == ".jpeg" || extension2.ToLower() == ".png" || extension2.ToLower() == ".webp")
                    {
                        if (product.Products.ProductPicture2.Length <= 104857600)
                        {
                            product.Products.ProductPicture2.CopyTo(new FileStream(filepath2, FileMode.Create));
                        }
                        else
                        {
                            TempData["error"] = "File size exceeds the limit (100MB)";
                        }
                    }
                    else
                    {
                        TempData["error"] = "Invalid file format";
                    }
                }

                // Get the current logged-in user
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var uId = user.Id;

                    // Create a new product entity
                    var newProduct = new Products
                    {
                        ProductTitle = product.Products.ProductTitle,
                        ProductDescription = product.Products.ProductDescription,
                        MinBidPrice = product.Products.MinBidPrice,
                        CurrBidPrice = product.Products.MinBidPrice,
                        CategoryId = product.Products.CategoryId,
                        AuctionStartDate = product.Products.AuctionStartDate,
                        AuctionEndDate = product.Products.AuctionEndDate,
                        UserId = uId,
                        ProductImage = filename,
                        ProductImage1 = filename1,
                        ProductImage2 = filename2,
                        TimeStamp = DateTime.Now.TimeOfDay,
                        ProductStatus = "non-active"
                    };


                    _dbContext.Products.Add(newProduct);
                    await _dbContext.SaveChangesAsync();
                    TempData["temp"] = "Your Item is Under review By Our Team";
                    return RedirectToAction("MyItems", "Home");
                }
            }
            else
            {
                TempData["error"] = "No product data provided";
            }
            return RedirectToAction("SellAnItem", "Home");
        }


        public IActionResult AllProducts(int page = 1, int pageSize = 9)
        {
            var getcurruser = _userManager.GetUserId(User);
            var GetAllProducts = _dbContext.Products.Where(x=>x.ProductStatus == "active" && x.UserId != getcurruser).ToList();
            var TotalProducts = GetAllProducts.Count();
            var TotalPage = (int)Math.Ceiling(TotalProducts / (double)pageSize);
            var currentPageProducts = GetAllProducts.Skip((page - 1) * pageSize).Take(pageSize);

            var viewModel = new AllProducts
            {
                products = currentPageProducts,
                PageNumber = page,
                PageSize = pageSize,
                TotalProducts = TotalProducts,
                TotalPages = TotalPage
            };

            return View(viewModel);
        }

        public async Task<IActionResult> MyItems()
        {
            var GetCurrentUser = await _userManager.GetUserAsync(User);
            if(GetCurrentUser != null)
            {
                var userId =  GetCurrentUser.Id;
                var GetItemsForUser = _dbContext.Products.Where(x=>x.UserId == userId).ToList();
                return View(GetItemsForUser);
            }
            return View();
        }

        public IActionResult DeleteItem(int id)
        {
            if(id != null)
            {
                var getitembids = _dbContext.Bids.Where(x => x.ProductId == id).ToList();
                foreach (var bid in getitembids)
                {
                    _dbContext.Remove(bid);
                }
                var getItem = _dbContext.Products.Find(id);
                _dbContext.Products.Remove(getItem);
                _dbContext.SaveChanges();
                TempData["temp"] = "Item Deleted Successfully";
                return RedirectToAction("MyItems", "Home");
            }
            TempData["delete"] = "There is an error deleting the item please try later";
            return RedirectToAction("MyItems", "Home");
        }


        public IActionResult SingleProduct(int id)
        {
            if(id != null)
            {
                var getCurrUser = _userManager.GetUserId(User);
                var FindItem = _dbContext.Products
                    .Where(p => p.ProductId == id)
                    .Include(x => x.category)
                    .Include(y => y.User)
                    .FirstOrDefault();
                if(FindItem != null)
                {
                    var Ratings = _dbContext.Ratings.Where(z => z.UserId == FindItem.UserId).ToList();
                    int ratingcount = Ratings.Count();
                    decimal? averageRating = 0;
                    decimal? averageShipRating = 0;
                    decimal totalRating = 0.0m;
                    decimal totalShipRating = 0.0m;

                    if (ratingcount > 0)
                    {
                        foreach (var rating in Ratings)
                        {
                            totalRating += rating.UserRating;
                            totalShipRating += rating.ShipRating;
                        }

                        averageRating = (totalRating / (ratingcount * 5)) * 100;
                        averageShipRating = (totalShipRating / (ratingcount * 5)) * 100;
                    }

                    var getRating = _dbContext.Ratings.FirstOrDefault(x => x.FromUser == getCurrUser && x.ProductID == id);
                    if(getRating != null)
                    {
                        var getRateCount = getRating.UserRating;
                        ViewBag.Rating = getRateCount;
                    }

                    ViewBag.AverageRating = averageRating;
                    ViewBag.AverageShipRating = averageShipRating;
                    return View(FindItem);
                }
            }
            return RedirectToAction("AllProducts","Home");
        }

        public async Task<IActionResult> PlaceBid(int Pid)
        {
            if (Pid != null)
            {
                var getProduct = _dbContext.Products.Find(Pid);
                if (getProduct != null)
                {
                    var bidAmountString = Request.Form["BidAmount"];
                    decimal bidAmount;
                    if (decimal.TryParse(bidAmountString, out bidAmount))
                    {
                        if (bidAmount > getProduct.MinBidPrice)
                        {
                            var GetCurrentUser = await _userManager.GetUserAsync(User);
                            if (GetCurrentUser != null)
                            {
                                var userId = GetCurrentUser.Id;
                                if (getProduct.UserId != userId)
                                {

                                

                                Bids SaveBid = new Bids
                                {
                                    UserId = userId,
                                    BidAmount = bidAmount,
                                    ProductId = Pid,
                                    TimeStamp = DateTime.Now,
                                };
                                getProduct.CurrBidPrice = bidAmount;
                                _dbContext.Bids.Add(SaveBid);
                                _dbContext.Update(getProduct);
                                await _dbContext.SaveChangesAsync();
                                TempData["ErrorMessage"] = "Bid Placed Successfully";
                                return RedirectToAction("SingleProduct", "Home", new { id = Pid });

                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "You can't place bid on your own Items";
                                    return RedirectToAction("SingleProduct", "Home", new { id = Pid });
                                }
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Please Login First";
                                return RedirectToAction("SingleProduct", "Home", new { id = Pid });
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Please Enter Higher Amount Than Current Bid Amount";
                            return RedirectToAction("SingleProduct", "Home", new { id = Pid });
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "There is an error Parsing bidamount Please Try Later";
                        return RedirectToAction("SingleProduct", "Home", new { id = Pid });
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Product Not Found";
                    return RedirectToAction("SingleProduct", "Home", new { id = Pid });
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Product Not Found";
                return RedirectToAction("SingleProduct", "Home", new { id = Pid });
            }
        }

        public IActionResult EditItem(int id)
        {
            var ItemForEdit = _dbContext.Products.Find(id);
            ViewBag.Category = _dbContext.Category.ToList();
            return View(ItemForEdit);
        }

        public IActionResult EditItemNew(Products Product, int Id)
        {
            var getItem = _dbContext.Products.Find(Id);
            if(getItem != null)
            {
        
            string filename = getItem.ProductImage;
            string filename1 = getItem.ProductImage1;
            string filename2 = getItem.ProductImage2;
            if (Product.ProductPicture != null)
            {
                string uploadfolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                filename = Guid.NewGuid().ToString() + " " + Product.ProductPicture.FileName;
                string filepath = Path.Combine(uploadfolder, filename);
                string extension = Path.GetExtension(Product.ProductPicture.FileName);
                if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png" || extension.ToLower() == ".webp")
                {
                    if (Product.ProductPicture.Length <= 104857600)
                    {
                        Product.ProductPicture.CopyTo(new FileStream(filepath, FileMode.Create));
                    }
                    else
                    {
                        TempData["error"] = "File size exceeds the limit (100MB)";
                    }
                }
                else
                {
                    TempData["error"] = "Invalid file format";
                }
            }

                if (Product.ProductPicture1 != null)
                {
                    string uploadfolder1 = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    filename1 = Guid.NewGuid().ToString() + " " + Product.ProductPicture1.FileName;
                    string filepath1 = Path.Combine(uploadfolder1, filename1);
                    string extension1 = Path.GetExtension(Product.ProductPicture1.FileName);
                    if (extension1.ToLower() == ".jpg" || extension1.ToLower() == ".jpeg" || extension1.ToLower() == ".png" || extension1.ToLower() == ".webp")
                    {
                        if (Product.ProductPicture1.Length <= 104857600)
                        {
                            Product.ProductPicture1.CopyTo(new FileStream(filepath1, FileMode.Create));
                        }
                        else
                        {
                            TempData["error"] = "File size exceeds the limit (100MB)";
                        }
                    }
                    else
                    {
                        TempData["error"] = "Invalid file format";
                    }
                }

                if (Product.ProductPicture2 != null)
                {
                    string uploadfolder2 = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    filename2 = Guid.NewGuid().ToString() + " " + Product.ProductPicture2.FileName;
                    string filepath2 = Path.Combine(uploadfolder2, filename2);
                    string extension2 = Path.GetExtension(Product.ProductPicture2.FileName);
                    if (extension2.ToLower() == ".jpg" || extension2.ToLower() == ".jpeg" || extension2.ToLower() == ".png" || extension2.ToLower() == ".webp")
                    {
                        if (Product.ProductPicture2.Length <= 104857600)
                        {
                            Product.ProductPicture2.CopyTo(new FileStream(filepath2, FileMode.Create));
                        }
                        else
                        {
                            TempData["error"] = "File size exceeds the limit (100MB)";
                        }
                    }
                    else
                    {
                        TempData["error"] = "Invalid file format";
                    }
                }


                getItem.ProductTitle = Product.ProductTitle;
                getItem.ProductDescription = Product.ProductDescription;
                getItem.AuctionEndDate = Product.AuctionEndDate;
                getItem.MinBidPrice = Product.MinBidPrice;
                getItem.CategoryId = Product.CategoryId;
                getItem.ProductImage = filename;
                getItem.ProductImage1 = filename1;
                getItem.ProductImage2 = filename2;

                _dbContext.Products.Update(getItem);
                _dbContext.SaveChanges();
                return RedirectToAction("SingleProduct","Home", new {id = Id});
            }

            TempData["error"] = "Product Not Found In Database";
            return RedirectToAction("SingleProduct", "Home", new { id = Id });
        }

        public IActionResult BidHistory(int ID)
        {
            var getBidHistory = _dbContext.Bids.Where(x=>x.ProductId == ID).Include(y=>y.User).ToList();
            return View(getBidHistory);
        }

        public IActionResult Profile(string ID)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login_Front", "Account");
            }
            var getDataForProfile = _dbContext.Register.FirstOrDefault(x => x.Id == ID);
            var getItems = _dbContext.Products.Where(x => x.UserId == ID).ToList();
            var getBidHistory = _dbContext.Bids.Where(x => x.UserId == ID).Include(y=>y.Products).ToList();
            var getBoughtItem = _dbContext.Products.Where(x => x.SoldToUserId == ID).ToList();

            ProfileModel model = new ProfileModel()
            {
                User = getDataForProfile,
                Products = getItems,
                Bid = getBidHistory,
                ItemsBought = getBoughtItem
            };
            return View(model);
        }

        public IActionResult Report(string uid)
        {
            ViewBag.UID = uid;
            return View();
        }

        [HttpPost]
        public IActionResult SubmitReport(Report data)
        {
            if(data != null)
            {
                var CurrentUserId = _userManager.GetUserId(User);
                Report report = new Report
                {
                    Name = data.Name,
                    ReportMessage = data.ReportMessage,
                    ToUserId = data.ToUserId,
                    FromUserId = CurrentUserId

                };
                _dbContext.Reports.Add(report);
                _dbContext.SaveChanges();
                TempData["ReportErrorMessage"] = "Report Submit Successfully. Our team will contact you soon";
                return RedirectToAction("Profile", "Home", new { id = data.ToUserId });
            }
            else
            {
                TempData["ReportErrorMessage"] = "Form is Empty Please Fill Report Form";
                return RedirectToAction("Profile", "Home", new { id = data.ToUserId });
            }

            TempData["ReportErrorMessage"] = "Report Submit Successfully";
            return RedirectToAction("Profile", "Home", new { id = data.ToUserId });
        }

        public IActionResult EditProfile(string id)
         {
            var getUserData = _dbContext.Register.Find(id);
            return View(getUserData);
        }
        public async Task<IActionResult> EditProfileSubmit(Register data)
        {
            var CurrentUserId = _userManager.GetUserId(User);
            var getData = await _userManager.FindByIdAsync(CurrentUserId);
            if (data != null)
            {
                getData.FullName = data.FullName;
                getData.Address = data.Address;
                getData.number = data.number;
                if(data.ProfileImage != null)
                {
                    string filename = "";
                    string uploadfolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    filename = Guid.NewGuid().ToString() + " " + data.ProfileImage.FileName;
                    string filepath = Path.Combine(uploadfolder, filename);
                    string extension = Path.GetExtension(data.ProfileImage.FileName);
                    if (extension.ToLower() == ".jpg" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".png" || extension.ToLower() == ".webp")
                    {
                        if (data.ProfileImage.Length <= 104857600)
                        {
                            data.ProfileImage.CopyTo(new FileStream(filepath, FileMode.Create));
                            getData.ProfilePicture = filename;
                        }
                        else
                        {
                            TempData["error"] = "File size exceeds the limit (100MB)";
                        }
                    }
                    else
                    {
                        TempData["error"] = "Invalid file format";
                    }
                }

                var result = await _userManager.UpdateAsync(getData);
                TempData["ReportErrorMessage"] = "Profile Edit Successfully";
                return RedirectToAction("Profile", "Home", new { id = CurrentUserId });
            }
            return View();
        }
        [HttpGet]
        public IActionResult SearchItem(string q)
        {
            if (q != null)
            {
                ViewBag.q = q;
                var getlist = _dbContext.Products.Where(e => e.ProductTitle.Contains(q)).ToList();
                return View(getlist);
            }
            else
            {
                // Handle the case when q is null, for example, by returning an empty list
                return View(new List<Products>());
            }
        }

        public IActionResult CatItems(int id, int page = 1, int pageSize = 9)
        {
            var getItemByCat = _dbContext.Products.Where(x=>x.CategoryId == id).ToList();
            var TotalProducts = getItemByCat.Count();
            var TotalPage = (int)Math.Ceiling(TotalProducts / (double)pageSize);
            var currentPageProducts = getItemByCat.Skip((page - 1) * pageSize).Take(pageSize);
            var categoryName = _dbContext.Category
                                 .Where(y => y.CatId == id)
                                 .Select(y => y.CatName)
                                 .FirstOrDefault();
            ViewBag.CatName = categoryName;

            var viewModel = new AllProducts
            {
                products = currentPageProducts,
                PageNumber = page,
                PageSize = pageSize,
                TotalProducts = TotalProducts,
                TotalPages = TotalPage
            };

            return View(viewModel);
        }

        public IActionResult AboutUs()
        {
            return View();
        }

       public IActionResult SubmitRating(string userId, int productId, int rating)
       {
            var getcurruser = _userManager.GetUserId(User);

            Ratings rate = new Ratings
            {
                FromUser = getcurruser,
                ProductID = productId,
                UserId = userId,
                UserRating = rating
            };

            _dbContext.Ratings.Add(rate);
            _dbContext.SaveChanges();

            return Json("Rate Submit Successfully");
       }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
