using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POC.BLL;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var genBL = new GenericBusinessLogic();
            var t = genBL.GetTax(7,1);
        }
    }
}
