using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APIGateway.Core.Phone
{
    public static class Phone
    {
        public static bool TryNormalizeCzech(string phone, out string output)
        {
            var rg = new Regex(
                @"((?:9[679]|8[035789]|6[789]|5[90]|42|3[578]|2[1-689])|9[0-58]|8[1246]|6[0-6]|5[1-8]|4[013-9]|3[0-469]|2[70]|7|1)(?:\W*\d){0,13}\d");
            var matchPhone = rg.Match(phone);

            if (matchPhone.Success)
            {
                var res = matchPhone.Value;
                res = res.Replace(" ", "");
                if (res.Length == 12)
                    res = "+" + res;

                if (res.Length == 9)
                    res = "+420" + res;
                output = res;
                return true;
            }
            output = null;
            return false;
        }
    }
}
