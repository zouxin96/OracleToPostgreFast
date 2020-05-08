using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OracleToPostgreFast
{
   static public class Helper
    {

        static public string[] SplitByFirst(this string str, string word)
        {

            int num = str.IndexOf(word);
            return new string[] { str.Substring(0, num), str.Substring(num + word.Length, str.Length - num - word.Length) };

        }
    }
}
