using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sarang
{
    public class Test
    {
        public void main()
        {
            string A = "CAC";
            string B = "";
            for (int i = A.Length - 1; i > 0; i--)
            {
                B = B + A[i];
            }
            string C = B;
            if (A == C)
            {
                Console.WriteLine("Palindrom");
            }
            else
            {
                Console.WriteLine("Not Pallindrom");
            }
        }
    }
}