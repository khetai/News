using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using News.Models;
using News.ViewModel;

namespace News.ViewComponents
{
	public class RightSideViewComponent  : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			using (var item = new RmlubecoMediaContext())
			{
				ViewBag.Category = item.Categories.Include(x=>x.CategoryForNews).Select(x=> new CategoryWithCount
				{
					Category = x,
					Count = x.CategoryForNews.Count()
				}).ToList();
				ViewBag.News = item.Xeberlers.Take(4).ToList();
				return View();
			}
		}

	}
}
