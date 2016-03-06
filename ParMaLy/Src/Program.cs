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
            string source = File.ReadAllText(file);

            var parser = new Parser.Parser(source, new Logger());

            Parser.SyntaxTree tree;
            try
            {
                tree = parser.Parse();
                return;
            }
            catch(Error err)
            {
                Console.Error.Write(err.Type.ToString());
                return;
            }
        }
    }
}
