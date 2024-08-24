using System;
using System.Collections.Generic;
using System.Reflection;

namespace News.Models;

public partial class Xeberler
{
    public int NewsId { get; set; }

    public string? NewsTitleAz { get; set; }

    public string? NewsTitleEn { get; set; }

    public string? NewsContentAz { get; set; }

    public string? NewsContentEn { get; set; }

    public DateTime? NewsDate { get; set; }

    public string? NewsPhoto { get; set; }

    public int? NewsView { get; set; }

    public string? XeberTwitter { get; set; }

    public string? XeberlerVideoUrl { get; set; }

    public int? XeberlerAdvantageId { get; set; }

    public DateTime? XeberlerFutureDate { get; set; }

    public string? NewsColor { get; set; }

    public string? NewsLangSupport { get; set; }

    public virtual ICollection<CategoryForNews> CategoryForNews { get; set; } = new List<CategoryForNews>();

    public virtual ICollection<Photo> Photos { get; set; } = new List<Photo>();

    public virtual ICollection<TitleColor> TitleColors { get; set; } = new List<TitleColor>();

    public virtual Advantage? XeberlerAdvantage { get; set; }
    public object this[string propertyName]
    {
        get
        {
            // probably faster without reflection:
            // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
            // instead of the following
            Type myType = typeof(Xeberler);
            PropertyInfo myPropInfo = myType.GetProperty(propertyName);
            return myPropInfo.GetValue(this, null);
        }
        set
        {
            Type myType = typeof(Xeberler);
            PropertyInfo myPropInfo = myType.GetProperty(propertyName);
            myPropInfo.SetValue(this, value, null);
        }
    }
}
