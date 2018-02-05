using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace cdf
{
  class Program
  {
    [STAThread]
    static void Main(string[] args)
    {
      IEnumerable<string> dirs = null;
      
      if (args.Length == 1)
      {
        args[0] = args[0].Replace("/", "?").Replace("\\", "?");
        dirs = Directory.EnumerateDirectories(Environment.CurrentDirectory, args[0], SearchOption.AllDirectories);
      }
      else if (args.Length == 2 && args[0] == "-t")
      {
        args[1] = args[1].Replace("/", "?").Replace("\\", "?");
        dirs = Directory.EnumerateDirectories(Environment.CurrentDirectory, args[1], SearchOption.TopDirectoryOnly);
      }
      else
      {
        Console.WriteLine("Invalid Parameter.\n\ncdf [-t(op directories only)] <directory-search-pattern>");
      }

      if (dirs.Count() == 0)
      {
        Console.WriteLine("No matches found.");
      }
      else if (dirs.Count() == 1)
      {
        string dir = dirs.First();

        if (dir.StartsWith(Environment.CurrentDirectory))
          dir = dir.Substring(Environment.CurrentDirectory.Length + 1);

        Console.WriteLine(dir);

        //System.Threading.Thread.CurrentThread.SetApartmentState(System.Threading.ApartmentState.STA);
        System.Windows.Forms.Clipboard.SetText(dir);
        Console.WriteLine("\nCopied directory name to Clipboard.");
      }
      else
      {
        foreach (string d in dirs)
        {
          string dir = d;

          if (dir.StartsWith(Environment.CurrentDirectory))
            dir = dir.Substring(Environment.CurrentDirectory.Length + 1);

          Console.WriteLine(dir);
        }
      }
    }
  }
}
