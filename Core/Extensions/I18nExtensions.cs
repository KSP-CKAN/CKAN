using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace CKAN.Extensions
{
    public static class I18nExtensions
    {

        public static string LocalizeDescription(this Enum val)
            => val.GetType()
                  ?.GetMember(val.ToString())
                  ?.First()
                   .GetCustomAttribute<DisplayAttribute>()
                  ?.GetDescription()
                  ?? "";

        public static string LocalizeName(this Enum val)
            => val.GetType()
                  ?.GetMember(val.ToString())
                  ?.First()
                   .GetCustomAttribute<DisplayAttribute>()
                  ?.GetName()
                  ?? "";

    }
}
