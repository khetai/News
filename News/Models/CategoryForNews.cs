using System;
using System.Collections.Generic;

namespace News.Models;

public partial class CategoryForNews
{
    public int CategoryForNewsId { get; set; }

    public int? CategoryForNewsCategoryId { get; set; }

    public int? CategoryForNewsNewsId { get; set; }

    public virtual Category? CategoryForNewsCategory { get; set; }

    public virtual Xeberler? CategoryForNewsNews { get; set; }
}
