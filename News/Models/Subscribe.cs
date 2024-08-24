using System;
using System.Collections.Generic;

namespace News.Models;

public partial class Subscribe
{
    public int SubscribeId { get; set; }

    public string? SubscribeEmail { get; set; }

    public DateTime? SubscribeDate { get; set; }
}
