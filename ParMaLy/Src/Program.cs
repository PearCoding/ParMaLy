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

            Console.WriteLine("Tokens:");
            foreach(string str in env.Tokens)
            {
                Console.Write("'" + str + "', ");
            }
            Console.WriteLine();

            Console.WriteLine("Rules:");
            foreach(Rule r in env.Rules)
            {
                Console.Write("[" + r.ID + "] " + r.Name + ": ");
                foreach(RuleToken t in r.Tokens)
                {
                    if(t.Type == RuleTokenType.Rule)
                        Console.Write("<" + t.String + "> ");
                    else
                        Console.Write(t.String + " ");
                }
                Console.WriteLine();
            }
            return;
        }
    }
}
