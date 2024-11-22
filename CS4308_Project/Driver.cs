using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS4308_Project
{
    public class Driver
    {
        public static void Main(String[] args)
        {
            Console.WriteLine("[Julia Source Code Interpreter]\n");
            Interpreter i = new Interpreter();
            try
            {
                Console.WriteLine("This is the present working directory: " + PresentWorkingDir());
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred when attempting to locate present working directory:");
                Console.WriteLine(e.ToString());
                return;
            }
            try
            {
                Console.WriteLine("The default file name of the source code is: " + Scanner.DEFAULT_FILENAME);
                Console.Write("If you wish to use this file name, enter yes: ");
                if ((Console.ReadLine() ?? "").ToLower().Trim() == "yes")
                {
                    i.RunInterpreter();
                }
                else
                {
                    Console.Write("Enter filename (including file extension) of source code (.txt file expected): ");
                    i.RunInterpreter((Console.ReadLine() ?? "").Trim());
                }
                Console.WriteLine("\n\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occurred when attempting to run interpreter:");
                Console.WriteLine(e.ToString());
                return;
            }
        }
        public static string PresentWorkingDir()
        {
            string pwd_dll = System.Reflection.Assembly.GetExecutingAssembly().Location.ToString();
            while (pwd_dll[pwd_dll.Length - 1] != '\\')
            {
                pwd_dll = pwd_dll.Substring(0, pwd_dll.Length - 1);
            }
            return pwd_dll;
        }
    }
}
