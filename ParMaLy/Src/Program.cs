using System;
using System.IO;

namespace PML
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 1)
            {
                Console.Error.Write("Not enough arguments given.");
                return;
            }

            string file = args[0];

            Environment env = new Environment(new Logger());
            try
            {
                string source = File.ReadAllText(file);
                env.Parse(source);

            }
            catch(Error err)
            {
                Console.Error.Write(err.Type.ToString());
                return;
            }
            catch(FileNotFoundException ex)
            {
                Console.Error.Write(ex.Message);
                return;
            }
            
            Output.SimpleBreakdown.Print(File.CreateText("out.txt"), env);
        }
    }
}
