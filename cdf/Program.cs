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
    static Dictionary<string, IEnumerable<string>> folderCache = new Dictionary<string, IEnumerable<string>>();

    [STAThread]
    static void Main(string[] args)
    {
      List<string> dirs = null;

      string searchPattern = null;
      int subDirs = 0;

      if (args.Length == 1)
      {
        searchPattern = args[0];
      }
      else
      {
        Console.WriteLine("Invalid Parameter.\n\ncdf <directory-search-pattern>");

        return;
      }

      int last = 0;

      RETRY:
      dirs = GetDirectories(Environment.CurrentDirectory, subDirs).ToList();

      int found = dirs.Count;

      for (int i = 0; i < dirs.Count; i++)
        if (dirs[i].StartsWith(Environment.CurrentDirectory) && dirs[i].Length > Environment.CurrentDirectory.Length)
          dirs[i] = dirs[i].Substring(Environment.CurrentDirectory.Length + (Environment.CurrentDirectory.EndsWith("\\") ? 0 : 1));

      Regex regex = null;

      try
      {
        regex = new Regex(searchPattern.Replace("/", "\\").Replace("\\", "\\\\").Replace("*", "(.*)").Replace("?", "(.)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
      }
      catch
      {
        Console.WriteLine("Invalid Search Pattern.\n\n* : \t multiple arbitrary characters\n? : \t single arbitrary character\n\nExample: \"data/build*Rele?se\" could match with \"project\\data\\build\\201\\Release\"");
        return;
      }

      dirs = (from s in dirs where regex.IsMatch(s) select s).ToList();

      if (dirs.Count == 0)
      {
        if (found > last)
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

    static IEnumerable<string> GetDirectories(string directory, int depth)
    {
      IEnumerable<string> dirs;

      if (folderCache.ContainsKey(directory))
      {
        dirs = folderCache[directory];
      }
      else
      {
        try
        {
          dirs = Directory.EnumerateDirectories(directory);
          folderCache.Add(directory, dirs);
        }
        catch
        {
          folderCache.Add(directory, new string[0]);
          return new string[0];
        }
      }

      List<string> subDirs = new List<string>();

      if (depth > 0)
        foreach (string dir in dirs)
          subDirs.AddRange(GetDirectories(dir, depth - 1));

      var ret = dirs.ToList();
      ret.AddRange(subDirs);

      return ret;
    }
  }
}
