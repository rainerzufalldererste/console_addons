using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace linemod
{
    class Program
    {
        static bool keep = true;
        static bool removeEmpty = false;
        static string startsWith = null;
        static string contains = null;
        static string endsWith = null;
        static string outFile = null;

        static void Main(string[] args)
        {
            if(args.Length < 4)
            {
                Console.WriteLine("Usage: linemod <file> (-o <output file>) -k(eep)/-r(emove) [-s \"starts with\" | -c \"contains\" | -e \"ends with\"] (-re {removeEmpty})");
                return;
            }

            string[] lines = null;

            try
            {
                lines = System.IO.File.ReadAllLines(args[0]);
                outFile = args[0];
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not open file '{args[0]}'. ({e.Message})");
                return;
            }

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-k":
                        keep = true;
                        break;

                    case "-r":
                        keep = false;
                        break;

                    case "-re":
                        removeEmpty = true;
                        break;

                    case "-s":
                    case "-e":
                    case "-c":
                    case "-o":
                        {
                            if (i + 1 < args.Length)
                            {
                                switch (args[i])
                                {
                                    case "-s":
                                        startsWith = args[i + 1];
                                        break;
                                    case "-e":
                                        endsWith = args[i + 1];
                                        break;
                                    case "-c":
                                        contains = args[i + 1];
                                        break;
                                    case "-o":
                                        outFile = args[i + 1];
                                        break;
                                }

                                i++;
                            }
                            else
                            {
                                Console.WriteLine($"Invalid Parameter: {args[i]} must be followed by string.");
                                return;
                            }

                            break;
                        }

                    default:
                        Console.WriteLine($"Invalid Parameter: '{args[i]}'.");
                        return;

                }
            }

            try
            {
                if (keep)
                    System.IO.File.WriteAllLines(outFile, (from l in lines where (startsWith == null || l.StartsWith(startsWith)) && (contains == null || l.Contains(contains)) && (endsWith == null || l.EndsWith(endsWith)) && (!removeEmpty || l.Length > 0) select l));
                else
                    System.IO.File.WriteAllLines(outFile, (from l in lines where (startsWith == null || !l.StartsWith(startsWith)) && (contains == null || !l.Contains(contains)) && (endsWith == null || !l.EndsWith(endsWith)) && (!removeEmpty || l.Length > 0) select l));
            }
            catch(Exception e)
            {
                Console.WriteLine($"Could not write file '{outFile}'. ({e.Message})");
                return;
            }
        }
    }
}
