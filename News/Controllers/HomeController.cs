using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using News.Models;
using News.ViewModel;
using System.Diagnostics;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Net.Mime.MediaTypeNames;

namespace News.Controllers
{
    public class HomeController : Controller
    {
        private readonly RmlubecoMediaContext _db;
        private readonly IHtmlLocalizer<HomeController> _localizer;

        public HomeController(RmlubecoMediaContext db, IHtmlLocalizer<HomeController> localizer)
        {
            _db = db;
            _localizer = localizer;

        }


        [HttpPost]
        public IActionResult Search(string Text)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                return RedirectToAction("Index");
            }
            return RedirectToAction("Axtaris", new { text = Text });
        }
        public IActionResult Axtaris(string text)
        {
            var news = _db.Xeberlers
                .Include(x => x.CategoryForNews)
                .ThenInclude(x => x.CategoryForNewsCategory)
                .Where(x =>
                    x.NewsTitleAz.Contains(text) ||
                    x.NewsTitleEn.Contains(text) ||

                    x.NewsContentAz.Contains(text) ||
                    x.NewsContentEn.Contains(text)
                )
                .Select(n => new NewsViewModel
                {
                    XeberlerModel = n,
                    CategoryForNewsModel = n.CategoryForNews.ToList(),
                })
                .ToList();
            ViewBag.FirstXeber = _db.Xeberlers.Include(x => x.CategoryForNews).OrderByDescending(x => x.NewsDate).FirstOrDefault(x => x.XeberlerAdvantageId == 1);
            return View(news);
        }
        public IActionResult Index()
        {
            var culture = CultureInfo.CurrentCulture; // Get the current culture info
            var languageCode = culture.TwoLetterISOLanguageName;
            var internationalNews = _db.Xeberlers.Include(x=>x.CategoryForNews).ThenInclude(x=>x.CategoryForNewsCategory).Where(x => x.NewsLangSupport == "inter").ToList();
            var specificLanguageNews = _db.Xeberlers.Include(x => x.CategoryForNews).ThenInclude(x => x.CategoryForNewsCategory).Where(x => x.NewsLangSupport == languageCode).ToList();

            var allNews = internationalNews.Concat(specificLanguageNews).ToList();
            var orderedNews = allNews.OrderByDescending(x => x.NewsDate);
            ViewBag.FirstXeber = orderedNews.Where(x=>x.XeberlerAdvantageId==1).ToList().Take(1);
            var news =  orderedNews.Select(n => new NewsViewModel
            {
                XeberlerModel = n,
                CategoryForNewsModel = n.CategoryForNews.ToList(),
            }).ToList();
            return View(news);
        }



        public IActionResult NewsDetails(int id)
        {
            var culture = HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>();
            var newsdetails = _db.Xeberlers
                .Include(x => x.CategoryForNews)
                .ThenInclude(x => x.CategoryForNewsCategory)
                .Include(x => x.Photos)
                .SingleOrDefault(x => x.NewsId == id && x.NewsLangSupport == culture.RequestCulture.UICulture.Name);

            if (newsdetails == null)
            {
                // Belirtilen id ve dilde haber bulunamazsa, kullanıcıyı Index'e yönlendir
                return RedirectToAction("Index");
            }

            // ViewModel'i doldurmak için mevcut kodlarınız
            var newsDetailsDto = new NewsDetailsViewModel
            {
                Xeberler = newsdetails,
                CategoryForNewsModel = newsdetails.CategoryForNews.ToList(),
                Photos = newsdetails.Photos.ToList(),
                SimilarXeberler = _db.CategoryForNews.Include(x => x.CategoryForNewsNews)
                    .Where(x => x.CategoryForNewsNewsId != id)
                    .Where(x => x.CategoryForNewsCategoryId == newsdetails.CategoryForNews.FirstOrDefault().CategoryForNewsCategoryId)
                    .Take(4)
                    .ToList()
            };

            return View(newsDetailsDto);
        }





        public IActionResult NewsForCategory(int id)
        {
            ViewBag.Name = _db.Categories.SingleOrDefault(x => x.CategoryId == id);
            var xeberler = _db.CategoryForNews.Where(x => x.CategoryForNewsCategoryId == id).OrderByDescending(x => x.CategoryForNewsNews.NewsDate).Select(x => new NewsViewModel
            {
                XeberlerModel = x.CategoryForNewsNews,
                CategoryForNewsModel = x.CategoryForNewsCategory.CategoryForNews.ToList(),
            }).Take(8).ToList();
            return View(xeberler);
        }
        public void Subscribe(string subsemail)
        {
            Subscribe s = new Subscribe();
            s.SubscribeEmail = subsemail;
            s.SubscribeDate = DateTime.Now;
            _db.Subscribes.Add(s);
            _db.SaveChanges();
        }
        public IActionResult ViewMoreForCategory(int page, int id)
        {
            var xeberler = _db.CategoryForNews.Where(x => x.CategoryForNewsCategoryId == id).OrderByDescending(x => x.CategoryForNewsNews.NewsDate).Select(x => new NewsViewModel
            {
                XeberlerModel = x.CategoryForNewsNews,
                CategoryForNewsModel = x.CategoryForNewsCategory.CategoryForNews.ToList(),
            }).Skip(page * 8).Take(8).ToList();

            if (xeberler == null)
            {
                return Ok("Empty");
            }
            return Ok(xeberler);
        }
        public IActionResult Contact()

        {
            return View();
        }
        public IActionResult About()

        {
            return View();
        }
        public IActionResult GetNewsLayout(int newsId)
        {
            var newsListFromDb = _db.Xeberlers.ToList();

            var viewModelList = newsListFromDb.Select(newsFromDb => new GetNewsViewModel
            {
                NewsId = newsFromDb.NewsId,
                NewsTitleAz = newsFromDb.NewsTitleAz
            }).ToList();
            return View(viewModelList);
        }

        [HttpPost]
        public IActionResult Contact(string contactName, string contactPhone, string contactEmail, string contactMessage)
        {
            var contactFormSubmission = new ContactFormSubmission
            {
                ContactName = contactName,
                ContactPhone = contactPhone,
                ContactEmail = contactEmail,
                ContactMessage = contactMessage,
                ContactSubmissionDate = DateTime.Now
            };


            _db.ContactFormSubmissions.Add(contactFormSubmission);
            _db.SaveChanges();

            return RedirectToAction("Contact");
        }



        public IActionResult ViewMore(int page)
        {
            var xeberler = _db.Xeberlers.Include(x => x.CategoryForNews).ThenInclude(x => x.CategoryForNewsCategory).OrderByDescending(x => x.NewsDate).Select(x => new NewsViewModel
            {
                XeberlerModel = x,
                CategoryForNewsModel = x.CategoryForNews.ToList(),
            }).Skip(page * 8).Take(8).ToList();

            if (xeberler == null)
            {
                return Ok("Empty");
            }
            return Ok(xeberler);
        }
        //Globalization
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


