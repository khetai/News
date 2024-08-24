using System;
using System.Collections.Generic;

namespace News.Models;

public partial class Photo
{
    public int PhotoId { get; set; }

    public int? PhotoNewsId { get; set; }

    public string? PhotoUrl { get; set; }

    public virtual Xeberler? PhotoNews { get; set; }
}
