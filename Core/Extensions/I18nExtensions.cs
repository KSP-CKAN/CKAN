using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace CKAN.Extensions
{
    public static class I18nExtensions
    {

        public static string Localize(this Enum val)
            => val.GetType()
                  .GetMember(val.ToString())
                  .FirstOrDefault()?
                  .GetCustomAttribute<DisplayAttribute>()
                  .GetDescription();

    }
}
