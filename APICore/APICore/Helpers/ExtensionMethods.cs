using System.Collections.Generic;
using System.Linq;
using WebMvcPluginUser.Entities;

namespace WebMvcPluginUser.Helpers
{
    public static class ExtensionMethods
    {   
        public static bool IsExistAndNotEmpty(this string str)
        {
            return str != null && str.Length > 0;
        }
    }
}
