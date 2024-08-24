using System;
using System.Collections.Generic;

namespace News.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? UserName { get; set; }

    public string? UserPassword { get; set; }

    public int? UserStatusId { get; set; }

    public virtual Status? UserStatus { get; set; }
}
