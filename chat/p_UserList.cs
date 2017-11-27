using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chat
{
    class p_UserList
    {
        private const int POCKETTYPE = 3;
        private int length;
        public List<string> names = new List<string>();

        public p_UserList(byte[] input)
        {
            length = int.Parse(ASCIIEncoding.GetEncoding(1251).GetString(input, 1, 4));
            string s = ASCIIEncoding.GetEncoding(1251).GetString(input, 5, length);
            int n = s.Count(c => c == '/');
            for (var i = 0; i < n; i++)
            {
                string name = s.Substring(0, s.IndexOf("/"));
                s = s.Remove(0, s.IndexOf("/") + 1);
                names.Add(name);
            }
        }

    }
}
