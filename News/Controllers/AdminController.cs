using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using News.Models;
using News.ViewModel;
using Newtonsoft.Json;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using static News.Controllers.AdminController;

namespace News.Controllers
{
    [Authorize]
    public class AdminController : Controller

    {
        private readonly IHtmlLocalizer<AdminController> _localizer;

        private readonly RmlubecoMediaContext _db;
        private string[] selectedColorCodes;

        public AdminController(RmlubecoMediaContext db, IHtmlLocalizer<AdminController> localizer)
        {
            _db = db;
            _localizer = localizer;

        }
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login(User user)
        {
            var u = _db.Users.Include(x => x.UserStatus).Where(x => x.UserName == user.UserName && x.UserPassword == user.UserPassword).FirstOrDefault();
            if (u != null)
            {
                var claims = new List<Claim>
                {
                       new Claim(ClaimTypes.NameIdentifier, u.UserName),
                        new Claim(ClaimTypes.Sid, u.UserId.ToString()),
                        new Claim(ClaimTypes.Role, u.UserStatus.StatusName.ToString())
                };
                var useridentity = new ClaimsIdentity(claims, "AimTech");
                ClaimsPrincipal principal = new ClaimsPrincipal(useridentity);
                HttpContext.SignInAsync(principal);
                return RedirectToAction("News", "Admin");
            }
            else
            {
                ViewBag.Error = "Ad və ya şifrə yanlışdır";
                return View();
            }
        }

        public IActionResult LogOut()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Admin", "Login");
        }
        public IActionResult UpdatePassword(string oldpassword, string newpass, string repeatpass)
        {
            int? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = int.Parse(HttpContext.User.Claims.First(x => x.Type == ClaimTypes.Sid).Value);
            }
            if (oldpassword == null || newpass == null || repeatpass == null)
            {
                ViewBag.Error = "Məlumat boş ola bilməz";

            }
            var olduser = _db.Users.SingleOrDefault(x => x.UserId == userId);
            if (olduser != null)
            {
                if (oldpassword == olduser.UserPassword)
                {
                    if (newpass == repeatpass)
                    {
                        olduser.UserPassword = newpass;
                        _db.SaveChanges();
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ViewBag.Error = "Yeni şifrə uyğun gəlmir";
                    }
                }
                else
                {
                    ViewBag.Error = "Köhnə şifrə düzgün deyil";
                }
            }
            else
            {
                ViewBag.Error = "İstifadəçi tapılmadı";
            }


            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AddUser(string userName, string userPassword)
        {

            var moderatorStatus = _db.Users.Include(x => x.UserStatus).FirstOrDefault(status => status.UserStatusId == 2);

            if (moderatorStatus == null)
            {
                return BadRequest("Moderator status not found.");
            }

            var newUser = new User
            {
                UserName = userName,
                UserPassword = userPassword,
                UserStatusId = moderatorStatus.UserStatusId
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();

            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult ContactSubmissions()
        {
            var contactSubmissions = _db.ContactFormSubmissions.ToList();
            return View(contactSubmissions);
        }


        public IActionResult News(int page = 1, int pageSize = 12)
        {
            int totalNewsCount = _db.Xeberlers.Count(); // Get the total count of news
            var paginatedNews = _db.Xeberlers
                .Include(x => x.CategoryForNews)
                .ThenInclude(x => x.CategoryForNewsCategory)
                .OrderByDescending(x => x.NewsDate)
                .Skip((page - 1) * pageSize) // Skip items based on the page number
                .Take(pageSize) // Take items based on the page size
                .Select(n => new NewsViewModel
                {
                    XeberlerModel = n,
                    CategoryForNewsModel = n.CategoryForNews.ToList(),
                })
                .ToList();

            int totalPages = (int)Math.Ceiling((double)totalNewsCount / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HidePrevious = page == 1;
            ViewBag.HideNext = page >= totalPages;

            return View(paginatedNews);
        }


        [HttpGet]
        public IActionResult AddNews()
        {
            ViewBag.TitleColorNames = _db.TitleColors.ToList();
            ViewBag.Category = _db.Categories.ToList();
            ViewBag.Advantages = new SelectList(_db.Advantages.ToList(), "AdvantageId", "AdvantageName");

            return View();
        }


        public class NewsForHangfire
        {
            public Xeberler Xeber { get; set; }
            public string SekilPath { get; set; }
            public string[] SekillerPaths { get; set; }
            public int[] Kateqoriyalar { get; set; }
            public int SelectedColorId { get; set; }
        }
        public static DateTime TrimMilliseconds(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, 0);
        }

        public IActionResult Secmek(Xeberler xeber, IFormFile sekil, IFormFile[] sekiller, int[] kateqoriyalar, int SelectedColorId)
        {
            // Baku, Azerbaycan saat dilimi için bir TimeZoneInfo nesnesi oluşturun
            TimeZoneInfo bakuTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Azerbaijan Standard Time");

            if (xeber.XeberlerFutureDate.HasValue)
            {
                DateTime xeberFutureDateBaku = TimeZoneInfo.ConvertTimeToUtc(xeber.XeberlerFutureDate.Value, bakuTimeZone);

                // Şu andaki UTC tarihini alın
                DateTime nowUtc = DateTime.UtcNow;

                if (AdminController.TrimMilliseconds(xeberFutureDateBaku) == AdminController.TrimMilliseconds(nowUtc))
                {
                    var sekilPath = sekil != null ? SekilUpload(sekil) : null;
                    var sekillerPaths = sekiller?.Select(SekilUpload).ToArray();
                    var newsForHangfire = new NewsForHangfire
                    {
                        Xeber = xeber,
                        SekilPath = sekilPath,
                        SekillerPaths = sekillerPaths,
                        Kateqoriyalar = kateqoriyalar,
                        SelectedColorId = SelectedColorId
                    };
                    AddNews(newsForHangfire);
                    return RedirectToAction("News");
                }
                else
                {
                    var sekilPath = sekil != null ? SekilUpload(sekil) : null;
                    var sekillerPaths = sekiller?.Select(SekilUpload).ToArray();

                    var newsForHangfire = new NewsForHangfire
                    {
                        Xeber = xeber,
                        SekilPath = sekilPath,
                        SekillerPaths = sekillerPaths,
                        Kateqoriyalar = kateqoriyalar,
                        SelectedColorId = SelectedColorId
                    };

                    BackgroundJob.Schedule(() => AddNews(newsForHangfire), xeberFutureDateBaku);
                    return RedirectToAction("News");
                }
            }
            else
            {
                // xeber.XeberlerFutureDate null ise ilgili işlemleri yapabilirsiniz
                // Örneğin, bir hata mesajı gösterebilir veya başka bir işlem yapabilirsiniz
                // Bu kısıma uygun bir işlem ekleyebilirsiniz.

                return RedirectToAction("News"); // veya başka bir sayfaya yönlendirme yapabilirsiniz
            }
        }



        public void AddNewsForHangFire(Xeberler xeber, IFormFile sekil, IFormFile[] sekiller, int[] kateqoriyalar)
        {
            var sekilUploadResult = sekil != null ? SekilUpload(sekil) : default;
            var sekillerUploadResults = sekiller?.Select(SekilUpload).ToArray();

            var newsForHangfire = new NewsForHangfire
            {
                Xeber = xeber,
                SekilPath = sekilUploadResult,
                SekillerPaths = sekillerUploadResults?.ToArray(),
                Kateqoriyalar = kateqoriyalar,
                //SelectedColorId = SelectedColorId
            };

            AddNews(newsForHangfire);
        }

        public void AddNews(NewsForHangfire newsForHangfire)
        {
            var xeber = newsForHangfire.Xeber;
            if (!string.IsNullOrEmpty(xeber.NewsTitleAz) && !string.IsNullOrEmpty(xeber.NewsContentAz))
            {
                if (!string.IsNullOrEmpty(xeber.NewsTitleEn) && !string.IsNullOrEmpty(xeber.NewsContentAz))
                {
                    xeber.NewsLangSupport = "inter";
                }
                else
                {
                    xeber.NewsLangSupport = "az";
                }
            }
            else
            {
                xeber.NewsLangSupport = "en";
            }
            _db.Xeberlers.Add(xeber);
            _db.SaveChanges();
            xeber.NewsView = 0;
            xeber.NewsDate = xeber.XeberlerFutureDate;

            if (!string.IsNullOrEmpty(newsForHangfire.SekilPath))
            {
                xeber.NewsPhoto = newsForHangfire.SekilPath;
            }

            if (newsForHangfire.SelectedColorId != 0)
            {
                var selectedColor = _db.TitleColors.FirstOrDefault(tc => tc.TitleColorId == newsForHangfire.SelectedColorId);
                if (selectedColor != null)
                {
                    xeber.NewsColor = selectedColor.TitleColorCode;
                }
            }



            if (newsForHangfire.SekillerPaths != null)
            {
                foreach (var item in newsForHangfire.SekillerPaths)
                {
                    Photo photo = new Photo();
                    photo.PhotoUrl = item;
                    photo.PhotoNewsId = xeber.NewsId;
                    _db.Photos.Add(photo);
                }
            }

            if (newsForHangfire.Kateqoriyalar != null)
            {
                foreach (var item in newsForHangfire.Kateqoriyalar)
                {
                    CategoryForNews categoryForNews = new CategoryForNews();
                    categoryForNews.CategoryForNewsCategoryId = item;
                    categoryForNews.CategoryForNewsNews = xeber;
                    _db.CategoryForNews.Add(categoryForNews);
                }
            }
            _db.SaveChanges();
        }

        public IActionResult EditNews(int id)
        {
            ViewBag.Advantages = new SelectList(_db.Advantages.ToList(), "AdvantageId", "AdvantageName");
            ViewBag.Photos = _db.Photos.Where(x => x.PhotoNewsId == id).ToList();
            ViewBag.Category = _db.Categories.ToList();
            ViewBag.TitleColorNames = _db.TitleColors.ToList();
            return View(_db.Xeberlers.Include(x => x.CategoryForNews).SingleOrDefault(x => x.NewsId == id));
        }

        [HttpPost]
        public IActionResult EditNews(int id, Xeberler xeberler, int[] kateqoriyalar, IFormFile[] sekiller, IFormFile sekil, string[] fileName, int SelectedColorId)
        {
            var oldnews = _db.Xeberlers.Include(x => x.Photos).SingleOrDefault(x => x.NewsId == id);

            if (sekil != null)
            {
                oldnews.NewsPhoto = SekilUpload(sekil);
            }

            var newImageFileNames = sekiller.Select(file => file.FileName).ToList();

            var existingFiles = oldnews.Photos.Select(p => p.PhotoUrl).ToList();

            var removedImages = existingFiles.Except(newImageFileNames).ToList();
            var imagesToRemove = oldnews.Photos.Where(p => removedImages.Contains(p.PhotoUrl)).ToList();
            _db.Photos.RemoveRange(imagesToRemove);

            foreach (IFormFile file in sekiller)
            {
                if (!existingFiles.Contains(file.FileName))
                {
                    Photo image = new Photo();
                    image.PhotoNewsId = id;
                    image.PhotoUrl = SekilUpload(file);
                    _db.Photos.Add(image);
                }
            }

            if (!string.IsNullOrEmpty(xeberler.NewsTitleAz) && !string.IsNullOrEmpty(xeberler.NewsContentAz))
            {
                if (!string.IsNullOrEmpty(xeberler.NewsTitleEn) && !string.IsNullOrEmpty(xeberler.NewsContentAz))
                {
                    xeberler.NewsLangSupport = "inter";
                }
                else
                {
                    xeberler.NewsLangSupport = "az";
                }
            }
            else
            {
                xeberler.NewsLangSupport = "en";
            }

            oldnews.XeberlerAdvantageId = xeberler.XeberlerAdvantageId;
            oldnews.XeberlerVideoUrl = xeberler.XeberlerVideoUrl;
            oldnews.XeberTwitter = xeberler.XeberTwitter;
            oldnews.NewsContentAz = xeberler.NewsContentAz;
            oldnews.NewsContentEn = xeberler.NewsContentEn;
            oldnews.NewsTitleAz = xeberler.NewsTitleAz;
            oldnews.NewsTitleEn = xeberler.NewsTitleEn;
            oldnews.NewsLangSupport = xeberler.NewsLangSupport;

            if (SelectedColorId != 0)
            {
                var selectedColor = _db.TitleColors.FirstOrDefault(x => x.TitleColorId == SelectedColorId);

                if (selectedColor != null)
                {
                    oldnews.NewsColor = selectedColor.TitleColorCode;
                }
            }

            _db.SaveChanges();

            if (kateqoriyalar != null)
            {
                var oldkat = _db.CategoryForNews.Where(x => x.CategoryForNewsNewsId == id);
                _db.CategoryForNews.RemoveRange(oldkat);

                foreach (var item in kateqoriyalar)
                {
                    CategoryForNews categoryForNews = new CategoryForNews();
                    categoryForNews.CategoryForNewsCategoryId = item;
                    categoryForNews.CategoryForNewsNewsId = id;
                    _db.CategoryForNews.Add(categoryForNews);
                }

                _db.SaveChanges();
            }

            return RedirectToAction("News");
        }




        public void DeletePhoto(int id)
        {
            var photo = _db.Photos.SingleOrDefault(x => x.PhotoId == id);
            _db.Photos.Remove(photo);
            _db.SaveChanges();
        }

        public void RemoveNews(int id)
        {
            using (var item = new RmlubecoMediaContext())
            {
                var photos = item.Photos.Where(x => x.PhotoNewsId == id);
                var colors = item.TitleColors.Where(x => x.TitleColorNewsId == id);
                var categories = item.CategoryForNews.Where(x => x.CategoryForNewsNewsId == id);
                var News = item.Xeberlers.FirstOrDefault(x => x.NewsId == id);
                item.Photos.RemoveRange(photos);
                item.SaveChanges();
                item.CategoryForNews.RemoveRange(categories);
                item.SaveChanges();
                item.Xeberlers.Remove(News);
                item.SaveChanges();
                item.TitleColors.RemoveRange(colors);
                item.SaveChanges();


            }
        }


        public string SekilUpload(IFormFile sekil)
        {
            string filename = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(sekil.FileName);
            string filePath = Path.Combine("wwwroot/img/", filename);
            using (var image = Image.Load(sekil.OpenReadStream()))
            {
                var encoder = new JpegEncoder { Quality = 65 };
                image.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(800, 600), Mode = ResizeMode.Max }));
                image.Save(filePath);
            }
            return filename;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult CultureManagement(string culture, string returnUrl)
        {
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.Now.AddDays(30) });
            return LocalRedirect(returnUrl);
        }

    }
}