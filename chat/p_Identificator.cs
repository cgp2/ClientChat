using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chat
{
    class p_Identificator
    {
        private const int POCKETTYPE = 1; //1 байта
        private int length; //2 байта
        public string name; //max 16 байт = 16 симолов
        public string password; //max 10 символов = 10 байт. MAX развем пакета = 28


        public p_Identificator(byte[] input)
        {
            if (int.Parse(ASCIIEncoding.GetEncoding(1251).GetString(input, 0, 1)) == POCKETTYPE)
            {
                length = int.Parse(ASCIIEncoding.GetEncoding(1251).GetString(input, 1, 3));
                string s = ASCIIEncoding.GetEncoding(1251).GetString(input, 4, length);
                name = s.Substring(0, s.IndexOf("/") - 1);
                password = s.Substring(s.IndexOf("/"));
            }
            else
            {
                Console.WriteLine("Неверный формат пакета");
            }
        }

        public p_Identificator(string n, string p)
        {
            name = n;
            password = p;

            length = (ASCIIEncoding.GetEncoding(1251).GetBytes(name + "/" + password)).Length;
        }

        public byte[] MakePocket()
        {
            byte[] bts0, bts1, bts2;

            bts0 = ASCIIEncoding.GetEncoding(1251).GetBytes(Convert.ToString(POCKETTYPE));

            if (length < 10)
            {
                bts1 = ASCIIEncoding.GetEncoding(1251).GetBytes(Convert.ToString("00" + length));
            }
            else
            {
                bts1 = ASCIIEncoding.GetEncoding(1251).GetBytes(Convert.ToString("0" + length));
            }

            bts2 = ASCIIEncoding.GetEncoding(1251).GetBytes(name + "/" + password);

            bts0 = bts0.Concat(bts1.Concat(bts2).ToArray()).ToArray();

            return bts0;
        }
    }
}
