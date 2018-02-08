using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace cdf
{
  class Program
  {
    [STAThread]
    static void Main(string[] args)
    {
      List<string> dirs = null;

      string searchPattern = null;
      int subDirs = 1;

      if (args.Length == 1)
      {
        searchPattern = args[0];
      }
      else if (args.Length == 2 && args[0] == "-t")
      {
        searchPattern = args[1];
        subDirs = 0;
      }
      else
      {
        Console.WriteLine("Invalid Parameter.\n\ncdf [-t(op directories only)] <directory-search-pattern>");

        return;
      }

      int last = 0;

      RETRY:
      dirs = GetDirectories(Environment.CurrentDirectory, subDirs).ToList();

      int found = dirs.Count;

      for (int i = 0; i < dirs.Count; i++)
        if (dirs[i].StartsWith(Environment.CurrentDirectory) && dirs[i].Length > Environment.CurrentDirectory.Length)
          dirs[i] = dirs[i].Substring(Environment.CurrentDirectory.Length + 1);

      Regex regex = new Regex(searchPattern.Replace("/", "\\").Replace("\\", "\\\\").Replace("*", "(.*)").Replace("?", "(.)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

      dirs = (from s in dirs where regex.IsMatch(s) select s).ToList();

      if(dirs.Count == 0)
      {
        if (subDirs > 0 && found > last)
        {
          subDirs++;
          last = found;
          goto RETRY;
        }
      }

      if (dirs.Count == 0)
      {
        Console.WriteLine("No matches found.");
      }
      else if (dirs.Count == 1)
      {
        string dir = dirs.First();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(dir);
        Console.ResetColor();

        System.Windows.Forms.Clipboard.SetText(dir);
        Console.WriteLine("\nCopied directory name to Clipboard.");
      }
      else
      {
        bool first = true;

        foreach (string d in dirs.OrderBy((s) => (from c in s where c == '\\' || c == '/' select 0).Count()))
        {
          string dir = d;

          if (first)
          {
            try
            {
              System.Windows.Forms.Clipboard.SetText(dir);
            }
            catch { }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{dir} (Copied)");
            Console.ResetColor();

            first = false;
          }
          else
          {
            Console.WriteLine(dir);
          }
        }
      }
    }

    static IEnumerable<string> GetDirectories(string directory, int depth = -1)
    {
      try
      {
        var dirs = Directory.EnumerateDirectories(directory);

        List<string> subDirs = new List<string>();

        if(depth > 0 || depth <= -1)
          foreach (string dir in dirs)
            subDirs.AddRange(GetDirectories(dir, depth - 1));

        var ret = dirs.ToList();
        ret.AddRange(subDirs);

        return ret;
      }
      catch { }

      return new string[0];
    }
  }
}
