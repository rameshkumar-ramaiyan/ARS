using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USDA_ARS.Umbraco.Extensions.Helpers
{
    public static class LinqExtensions
    {
        public static List<IEnumerable<T>> Split<T>(this List<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits.ToList();
        }
    }
}
