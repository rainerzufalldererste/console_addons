using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edit
{
  class Program
  {
    static bool IsRunning = true;
    static List<string> File;
    static int Line;
    static int Char;
    static ConsoleKeyInfo Key = new ConsoleKeyInfo();

    static void Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine("Usage: edit <Filename>");
        return;
      }

      try
      {
        File = System.IO.File.ReadAllLines(args[0]).ToList();
      }
      catch (Exception e)
      {
        Console.WriteLine($"Failed to open file. ({e.Message})");
      }

      Console.Title = "Edit | " + args[0];
      Console.TreatControlCAsInput = true;

      int lastLine = -1;
      int lastChar = -1;

      do
      {
        if (Key.Modifiers == ConsoleModifiers.Shift || Key.Modifiers == 0 && !char.IsControl(Key.KeyChar))
        {
          File[Line] = File[Line].Insert(Char, Key.KeyChar.ToString());
          Char++;
        }

        switch (Key.Key)
        {
          case ConsoleKey.UpArrow:
            Line = Math.Max(Line - 1, 0);
            break;

          case ConsoleKey.DownArrow:
            Line = Math.Min(Line + 1, File.Count - 1);
            break;

          case ConsoleKey.RightArrow:
            Char = Math.Min(Char + 1, File[Line].Length);
            break;

          case ConsoleKey.LeftArrow:
            Char = Math.Max(Char - 1, 0);
            break;

          case ConsoleKey.Enter:
            {
              if (Key.Modifiers == ConsoleModifiers.Control)
              {
                File.Insert(Line, "");
                Line++;
              }
              else
              {
                File.Insert(Line + 1, "");
                Line++;
              }

              break;
            }

          case ConsoleKey.Backspace:
            {
              if (Char == 0)
              {
                if (Line > 0)
                {
                  Char = File[Line - 1].Length;
                  File[Line - 1] += File[Line];
                  File.RemoveAt(Line);
                  Line--;
                }
              }
              else
              {
                File[Line] = File[Line].Remove(Char - 1, 1);
                Char--;
              }

              break;
            }
        }

        if (lastChar != Char || lastLine != Line)
        {
          UpdateRender();
          lastLine = Line;
          lastChar = Char;
        }
        else
        {
          Console.SetCursorPosition(0, 0);
        }
      }
      while (IsRunning && !((Key = Console.ReadKey()).Key == ConsoleKey.C && Key.Modifiers == ConsoleModifiers.Control));

      Console.Clear();
    }

    static void UpdateRender()
    {
      Console.SetCursorPosition(0, 0);
      StringBuilder sb = new StringBuilder();

      int renderLine = 0;
      int characterIndex = 0;
      int drawnChar = 0;
      int textLine = Math.Max(Line - Console.WindowHeight / 2, 0);

      Char = Math.Min(Char, File[Line].Length);

      while (renderLine < Console.WindowHeight - 2)
      {
        if (textLine >= File.Count)
          break;

        if (textLine == Line)
        {
          if (characterIndex == 0)
          {
            Console.Write(sb.ToString());
            sb.Clear();
          }

          Console.BackgroundColor = ConsoleColor.DarkGray;
        }

        for (; characterIndex < File[textLine].Length; characterIndex++)
        {
          char c = File[textLine][characterIndex];

          if (textLine == Line && characterIndex == Char)
          {
            Console.Write(sb.ToString());
            sb.Clear();

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
          }

          if (textLine == Line && characterIndex > Char && characterIndex <= Char + 2) // to accommodate for tabs.
          {
            Console.Write(sb.ToString());
            sb.Clear();
            Console.ResetColor();
            Console.BackgroundColor = ConsoleColor.DarkGray;
          }

          if (c == '\t')
          {
            if (drawnChar + 1 < Console.WindowWidth && (drawnChar & 1) == 0)
            {
              sb.Append("  ");
              drawnChar += 2;
            }
            else
            {
              sb.Append(' ');
              drawnChar++;
            }
          }
          else
          {
            sb.Append(c);
            drawnChar++;
          }

          if (drawnChar % Console.WindowWidth == 0 && drawnChar > 0)
          {
            characterIndex++;
            renderLine++;
            break;
          }
        }

        if (characterIndex == File[textLine].Length)
        {
          if (textLine == Line)
          {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.Write(sb.ToString());
            sb.Clear();
          }

          if (drawnChar != Console.WindowWidth)
            sb.Append(new string(' ', Console.WindowWidth - (drawnChar % Console.WindowWidth)));

          renderLine++;
          textLine++;
          characterIndex = 0;
          drawnChar = 0;
        }
      }

      while (renderLine < Console.WindowHeight - 2)
      {
        sb.Append(new string(' ', Console.WindowWidth));
        renderLine++;
      }

      Console.ResetColor();
      Console.Write(sb.ToString());
      sb.Clear();
    }
  }
}
