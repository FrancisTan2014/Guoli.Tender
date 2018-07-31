using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Guoli.Tender.Web.Utils
{
    public static class StringExt
    {
        public static int ToInt32(this string value, int def = default(int))
        {
            int res;
            if (!int.TryParse(value, out res))
            {
                res = def;
            }

            return res;
        }
    }
}