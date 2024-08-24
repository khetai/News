using System;
using System.Collections.Generic;

namespace News.Models;

public partial class ContactFormSubmission
{
    public int ContactId { get; set; }

    public string ContactName { get; set; } = null!;

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactMessage { get; set; }

    public DateTime ContactSubmissionDate { get; set; }
}
