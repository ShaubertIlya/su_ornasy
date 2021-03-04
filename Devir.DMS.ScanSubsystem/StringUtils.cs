using System;
using System.Collections.Generic;
using System.Text;

namespace Devir.DMS.ScanSubsystem
{
    public static class StringUtils
    {
        public static string TrimQuotes(string s)
        {
            return s.Trim('"');
        }
    }
}
