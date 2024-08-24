using News.Models;

namespace News.ViewModel
{
	public class NewsDetailsViewModel
	{
        public Xeberler Xeberler { get; set; }
		public List<CategoryForNews> CategoryForNewsModel { get; set; }
        public List<Photo> Photos { get; set; }
        public List<CategoryForNews>? SimilarXeberler { get; set; }

    }
}
