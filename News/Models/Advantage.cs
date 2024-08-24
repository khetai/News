using System;
using System.Collections.Generic;

namespace News.Models;

public partial class Advantage
{
    public int AdvantageId { get; set; }

    public string? AdvantageName { get; set; }

    public virtual ICollection<Xeberler> Xeberlers { get; set; } = new List<Xeberler>();
}
