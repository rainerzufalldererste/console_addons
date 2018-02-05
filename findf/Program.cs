using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace findf
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length == 1)
        foreach (string s in Directory.EnumerateFiles(Environment.CurrentDirectory, args[0], SearchOption.AllDirectories))
          Console.WriteLine(s);
      else if (args.Length == 2 && args[1] == "-t")
        foreach (string s in Directory.EnumerateFiles(Environment.CurrentDirectory, args[0], SearchOption.TopDirectoryOnly))
          Console.WriteLine(s);
      else
        Console.WriteLine("Invalid Parameter.\n\nfindf <filename> [-t(op directory only)]");
    }
  }
}
