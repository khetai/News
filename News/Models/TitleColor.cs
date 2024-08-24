using System;
using System.Collections.Generic;

namespace News.Models;

public partial class TitleColor
{
    public int TitleColorId { get; set; }

    public int? TitleColorNewsId { get; set; }

    public string? TitleColorName { get; set; }

    public string? TitleColorCode { get; set; }

    public virtual Xeberler? TitleColorNews { get; set; }
}
