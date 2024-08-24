using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using News.Controllers;
using News.Models;

namespace News.ViewComponents
{

    public class CategoryViewComponent :ViewComponent
    {

        public IViewComponentResult Invoke()
        {
            using (var item = new RmlubecoMediaContext())
            {
                ViewBag.Category = item.Categories.ToList();
                return View();
            }
        }

    }
}
