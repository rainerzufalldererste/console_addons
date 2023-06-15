using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace findif
{
  class Program
  {
    private static int amountOfLines;

    static void Main(string[] args)
    {
      SearchOption searchOption = SearchOption.AllDirectories;
      string fileSearchPattern = "";
      string excludeText = null;
      string textSearch = "";
      bool ignoreCase = true;
      bool allowNullChars = false;
      ListingMethod listingMethod = (ListingMethod)5;

      if (args.Length == 0)
        goto INVALID_PARAMETER;

      int index = 0;

      while (index < args.Length - 1)
      {
        if (args[index] == "-t")
        {
          searchOption = SearchOption.TopDirectoryOnly;
          index++;
        }
        else if (args[index] == "-f" && index < args.Length - 2 && string.IsNullOrEmpty(fileSearchPattern))
        {
          fileSearchPattern = args[index + 1];
          index += 2;
        }
        else if (args[index] == "-x" && index < args.Length - 2 && string.IsNullOrEmpty(excludeText))
        {
          excludeText = args[index + 1];
          index += 2;
        }
        else if (args[index] == "-0")
        {
          allowNullChars = true;
          index++;
        }
        else if (args[index] == "-c" || args[index] == "--case-sensitive")
        {
          ignoreCase = false;
          index++;
        }
        else if (args[index] == "-n")
        {
          listingMethod = ListingMethod.JustListFiles;
          index++;
        }
        else if (args[index].StartsWith("-l=") && args[index].Length > 3 && int.TryParse(args[index].Substring(3), out amountOfLines) && amountOfLines > 0)
        {
          listingMethod = (ListingMethod)amountOfLines;
          index++;
        }
        else goto INVALID_PARAMETER;
      }

      textSearch += args[index];
      
      if (ignoreCase)
        textSearch = textSearch.ToLower();

      if (string.IsNullOrEmpty(fileSearchPattern))
        fileSearchPattern = "*";

      int matches = 0;
      var allFiles = Directory.EnumerateFiles(Environment.CurrentDirectory, fileSearchPattern, searchOption);

      foreach (string filename in allFiles)
      {
        if (excludeText != null && filename.Contains(excludeText))
          continue;

        try
        {
          string[] file = File.ReadAllLines(filename);

          if (!allowNullChars)
          {
            bool found = false;

            for (int line = 0; line < file.Length; line++)
            {
              if (file[line].Contains('\0'))
              {
                if (line == file.Length - 1) // Last char of last line is allowed to be '\0'.
                {
                  for (int i = 0; i < file[line].Length - 1; i++)
                  {
                    if (file[line][i] == '\0')
                    {
                      found = true;
                      break;
                    }
                  }

                  if (found)
                    break;
                }

                found = true;
                break;
              }
            }

            if (found)
              continue;
          }

          for (int line = 0; line < file.Length; line++)
          {
            if ((ignoreCase && file[line].ToLower().Contains(textSearch)) || (!ignoreCase && file[line].Contains(textSearch)))
            {
              matches++;

              if (listingMethod == ListingMethod.JustListFiles)
              {
                Console.WriteLine($"{filename}: Line {line + 1}");
              }
              else
              {
                Console.WriteLine($"\n{filename}:");

                int min = Math.Max(0, line - (int)listingMethod);
                int max = Math.Min(file.Length - 1, line + (int)listingMethod);

                for (int i = min; i <= max; i++)
                {
                  if ((ignoreCase && file[i].ToLower().Contains(textSearch)) || (!ignoreCase && file[i].Contains(textSearch)))
                  {
                    if (i > line)
                    {
                      line = i;
                      matches++;
                      max = Math.Min(file.Length - 1, max + (int)listingMethod);
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                  }

                  Console.WriteLine($"{(i + 1).ToString().PadLeft(5, ' ')}: {file[i]}");

                  Console.ResetColor();
                }

                line = max;
              }
            }
          }
        }
        catch (Exception e)
        {
          Console.WriteLine($"Error for {filename}: {e.Message}");
        }
      }

      if(matches == 0)
        Console.WriteLine($"No matches found in {allFiles.Count()} file(s).");
      else
        Console.WriteLine($"\n\n{matches} match(es) found in {allFiles.Count()} file(s).");

#if DEBUG
      Console.ReadKey();
#endif
      return;

      INVALID_PARAMETER:
      Console.WriteLine("Invalid Parameter.\n\nfindif \n\n\t[-f <file-search-pattern>] [-x <excluded-path-string>] [-t(op directory only)] [-0 (allow null chars in files)]\n\t[-c (case sensitive) | --case-sensitive] \n\t[-n (display filenames only) / -l=<n> (display n lines before and after match.)] \n\t<string-to-find>");
#if DEBUG
      Console.ReadKey();
#endif
      return;
    }

    enum ListingMethod : int
    {
      JustListFiles = 0, DisplayXLines
    }
  }
}
