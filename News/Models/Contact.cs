using System;
using System.Collections.Generic;

namespace News.Models;

public partial class Contact
{
    public int ContactId { get; set; }

    public string? ContactName { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactContent { get; set; }
}
