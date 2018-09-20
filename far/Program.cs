using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace far
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine("Usage:\nfar.exe [IGNORED] <URL>");
      }
      else
      {
        try
        {
          System.Diagnostics.Process.Start(args.Last().Trim());
        }
        catch (Exception e)
        {
          Console.WriteLine($"Execution Failed. ({e.Message})");
        }
      }
    }
  }
}
