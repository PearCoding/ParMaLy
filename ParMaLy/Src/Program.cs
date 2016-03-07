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

            Console.Write("Tokens: ");
            foreach(string str in env.Tokens)
            {
                Console.Write("'" + str + "', ");
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.Write("Groups: ");
            foreach (RuleGroup grp in env.Groups)
            {
                Console.Write(grp.Name + ", ");
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Start: " + (env.Start == null ? "NOT SET!" : env.Start.Name));
            Console.WriteLine();

            Console.WriteLine("Rules:");
            foreach(Rule r in env.Rules)
            {
                Console.Write("[" + r.ID + "] " + r.Group.Name + ": ");
                if (r.IsEmpty)
                    Console.Write("/*EMPTY*/");
                else
                {
                    foreach (RuleToken t in r.Tokens)
                    {
                        if (t.Type == RuleTokenType.Rule)
                            Console.Write("<" + t.String + "> ");
                        else
                            Console.Write(t.String + " ");
                    }
                }
                Console.WriteLine();
            }
            Console.WriteLine();

            Console.WriteLine("First Sets:");
            FirstSet.Setup(env);
            foreach (RuleGroup grp in env.Groups)
            {
                Console.Write(grp.Name + ": { ");
                foreach (string t2 in grp.FirstSet)
                {
                    if (t2 == null)
                        Console.Write("/*EMPTY*/ ");
                    else
                        Console.Write(t2 + " ");
                }
                Console.WriteLine("}");
            }
            return;
        }
    }
}
