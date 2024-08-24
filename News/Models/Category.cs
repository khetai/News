using System;
using System.Collections.Generic;
using System.Reflection;

namespace News.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? CategoryNameAz { get; set; }

    public string? CategoryNameEn { get; set; }

    public string? CategoryNameRu { get; set; }

    public virtual ICollection<CategoryForNews> CategoryForNews { get; set; } = new List<CategoryForNews>();
    public object this[string propertyName]
    {
        get
        {
            // probably faster without reflection:
            // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
            // instead of the following
            Type myType = typeof(Category);
            PropertyInfo myPropInfo = myType.GetProperty(propertyName);
            return myPropInfo.GetValue(this, null);
        }
        set
        {
            Type myType = typeof(Category);
            PropertyInfo myPropInfo = myType.GetProperty(propertyName);
            myPropInfo.SetValue(this, value, null);
        }
    }
}
