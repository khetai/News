using News.Models;

namespace News.ViewModel
{
    public class NewsViewModel
    {
        public Xeberler XeberlerModel { get; set; }
        public List<CategoryForNews> CategoryForNewsModel { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
