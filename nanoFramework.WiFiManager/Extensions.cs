using System;
using System.Text;

namespace nanoFramework.WiFiManager
{
    public static class Extensions
    {
        public static string Replace(this string stringToSearch, string stringToFind, string stringToSubstitute)
        {
            StringBuilder stringBuilder = new StringBuilder(stringToSearch);
            stringBuilder.Replace(stringToFind, stringToSubstitute);
       
            return stringBuilder.ToString();
        }
    }
}
