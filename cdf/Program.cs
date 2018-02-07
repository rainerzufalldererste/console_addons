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
        dirs = Directory.EnumerateDirectories(Environment.CurrentDirectory, args[0], SearchOption.AllDirectories).Catch(typeof(UnauthorizedAccessException)).ToArray();
      }
      else if (args.Length == 2 && args[0] == "-t")
      {
        args[1] = args[1].Replace("/", "?").Replace("\\", "?");
        dirs = Directory.EnumerateDirectories(Environment.CurrentDirectory, args[1], SearchOption.TopDirectoryOnly).Catch(typeof(UnauthorizedAccessException)).ToArray();
      }
      else
      {
        Console.WriteLine("Invalid Parameter.\n\ncdf [-t(op directories only)] <directory-search-pattern>");

        return;
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

        System.Windows.Forms.Clipboard.SetText(dir);
        Console.WriteLine("\nCopied directory name to Clipboard.");
      }
      else
      {
        bool first = true;

        foreach (string d in dirs.OrderBy((s) => (from c in s where c == '\\' || c == '/' select 0).Count()))
        {
          string dir = d;

          if (dir.StartsWith(Environment.CurrentDirectory))
            dir = dir.Substring(Environment.CurrentDirectory.Length + 1);

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
  }

  // See: http://qaru.site/questions/127277/c-handle-systemunauthorizedaccessexception-in-linq
  static class ExceptionExtensions
  {
    public static IEnumerable<TIn> Catch<TIn>(this IEnumerable<TIn> source, Type exceptionType)
    {
      using (var enumerator = source.GetEnumerator())
      {
        while (true)
        {
          bool ok = false;

          try
          {
            ok = enumerator.MoveNext();
          }
          catch (Exception e)
          {
            if (e.GetType() != exceptionType)
              throw;
            continue;
          }

          if (!ok)
            yield break;

          yield return enumerator.Current;
        }
      }
    }
  }
}
