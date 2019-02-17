using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cat = testing1;

namespace Dummy
{
   public struct s
    {
        public int sa;
        int ab;
    };
    
    public enum Test
    {
        value,
        value0
    }
    class Program
    {

        static void Main(string[] args)
        {
            cat.asasa tt = new cat.asasa();
            tt.abcd();
            Test t1 = (Test) 0;
            Console.WriteLine("Testing in Program" + (int)t1);
            Class1 c1 = new Class1();
            c1.demo();
        }
        public void testing()
        {
            Console.WriteLine("Cyclic");
        }
    }

}


