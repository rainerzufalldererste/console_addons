using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edit
{
  enum EMode
  {
    EditText,
    MessageBox,
  }

  class Program
  {
    static bool IsRunning = true;
    static List<string> File;
    static int Line;
    static int Char;
    static ConsoleKeyInfo Key = new ConsoleKeyInfo();
    static int ActivePositionX = 0, ActivePositionY = 0;
    static char[] JumpToChars = { ' ', ',', '-', '+', '*', '/', '\\', '\"', '\'', '&', '|', '`', '^', '%', '$', '#', '@', '!', ':', ';', '?', '<', '>', '(', ')', '~', '{', '}', '[', ']' };
    static EMode CurrentMode = EMode.EditText;
    static int BufferHeight = Console.WindowHeight;
    static bool CustomHeight = false;
    static string FileName;
    static string ErrorMessageString = "";

    static void Main(string[] args)
    {
      if (args.Length > 0)
      {
        FileName = args[0];

        try
        {
          if (System.IO.File.Exists(args[0]))
            File = System.IO.File.ReadAllLines(args[0]).ToList();
        }
        finally
        {
          if (File == null)
            File = new List<string>() { "" };
        }

        if (args.Length > 1)
        {
          int argIndex = 1;

          while (argIndex + 1 < args.Length)
          {
            int argsLeft = args.Length - argIndex;

            switch (args[argIndex])
            {
              case "-h":
              case "--buffer-height":
                if (argsLeft > 1)
                {
                  if (!int.TryParse(args[argIndex + 1], out BufferHeight))
                  {
                    Console.WriteLine("Invalid Parameter.");
                    Environment.Exit(-1);
                  }

                  CustomHeight = true;
                  argIndex += 2;
                }
                else
                {
                  Console.WriteLine("Invalid Parameter.");
                  Environment.Exit(-1);
                }
                break;

              default:
                Console.WriteLine("Invalid Parameter.");
                Environment.Exit(-1);
                break;
            }
          }
        }
      }
      else
      {
        FileName = "";
        File = new List<string>() { "" };
      }
        
      Console.Title = "Edit | " + FileName;
      Console.TreatControlCAsInput = true;

      int lastLine = -1;
      int lastChar = -1;
      bool forceRedraw = true;

      do
      {
        switch (CurrentMode)
        {
          case EMode.EditText:
          default:
            UpdateEditMode(out forceRedraw);
            break;

          case EMode.MessageBox:
            if (Key.Key == ConsoleKey.Enter)
            {
              CurrentMode = EMode.EditText;
              forceRedraw = true;
            }
            break;
        }

        switch (CurrentMode)
        {
          case EMode.MessageBox:
            Clear();
              Console.SetCursorPosition(2, 1);
              Console.WriteLine("Message:");
              Console.SetCursorPosition(4, 3);
              Console.WriteLine(ErrorMessageString);
              Console.WriteLine();
              Console.SetCursorPosition(2, BufferHeight - 2);
              Console.WriteLine("Press [Enter] to continue.");

            break;

          case EMode.EditText:
          default:
            if (lastChar != Char || lastLine != Line || forceRedraw)
            {
              EditModeRender();
              lastLine = Line;
              lastChar = Char;
              forceRedraw = false;
            }
            break;
        }

        Console.SetCursorPosition(ActivePositionX, ActivePositionY);
      }
      while (IsRunning && !((Key = Console.ReadKey()).Key == ConsoleKey.C && Key.Modifiers == ConsoleModifiers.Control));

      if (!CustomHeight)
        Clear();
    }

    private static void Clear()
    {
      if (CustomHeight)
      {
        Console.SetCursorPosition(0, 0);
        Console.Write(new string(' ', BufferHeight * Console.WindowWidth));
      }
      else
      {
        Console.Clear();
      }
    }

    private static void UpdateEditMode(out bool forceRedraw)
    {
      forceRedraw = false;

      if ((Key.Modifiers == ConsoleModifiers.Shift || Key.Modifiers == 0) && !char.IsControl(Key.KeyChar) && Key.KeyChar != '\0')
      {
        File[Line] = File[Line].Insert(Char, Key.KeyChar.ToString());
        Char++;
      }

      switch (Key.Key)
      {
        case ConsoleKey.UpArrow:
          if (Char > Console.BufferWidth)
            Char -= Console.BufferWidth;
          else
            Line = Math.Max(Line - 1, 0);
          break;

        case ConsoleKey.DownArrow:
          if (Char + Console.BufferWidth <= File[Line].Length)
            Char += Console.BufferWidth;
          else
            Line = Math.Min(Line + 1, File.Count - 1);
          break;

        case ConsoleKey.RightArrow:
          if ((Key.Modifiers & ConsoleModifiers.Control) == 0)
            Char++;
          else
            for (Char = Char + 1; Char < File[Line].Length; Char++)
              if (JumpToChars.Contains(File[Line][Char]))
                break;

          if (Char > File[Line].Length && Line + 1 < File.Count)
          {
            Char = 0;
            Line = Line + 1;
          }

          if (Char > File[Line].Length)
            Char = File[Line].Length;

          break;

        case ConsoleKey.LeftArrow:
          if ((Key.Modifiers & ConsoleModifiers.Control) == 0)
            Char = Char - 1;
          else
            for (Char = Char - 1; Char >= 0; Char--)
              if (JumpToChars.Contains(File[Line][Char]))
                break;

          if (Char < 0 && Line - 1 >= 0)
          {
            Line = Line - 1;
            Char = File[Line].Length;
          }

          if (Char < 0)
            Char = 0;

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
              if (Char == 0)
              {
                File.Insert(Line + 1, "");
                Line++;
              }
              else
              {
                File.Insert(Line, File[Line].Substring(0, Char));
                Line++;
                File[Line] = File[Line].Substring(Char);
                Char = 0;
              }
            }

            break;
          }

        case ConsoleKey.Backspace:
          {
            forceRedraw = true;

            if ((Key.Modifiers & ConsoleModifiers.Control) == 0)
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
                if (Char == File[Line].Length - 1)
                  File[Line] = File[Line].Remove(Char - 2);
                else
                  File[Line] = File[Line].Remove(Char - 1, 1);
                Char--;
              }
            }
            else
            {
              bool edited = false;

              if (Char > File[Line].Length)
                Char = Math.Max(0, File[Line].Length);

              for (int i = Char - 1; i >= 0; i--)
                if (JumpToChars.Contains(File[Line][i]))
                {
                  if (Char < File[Line].Length)
                    File[Line] = File[Line].Remove(i, Char - i);
                  else
                    File[Line] = File[Line].Remove(i);

                  Char = i + 1;
                  edited = true;
                  break;
                }

              if (!edited)
              {
                if (Char > 0)
                {
                  File[Line] = File[Line].Remove(0, Char);
                  Char = 0;
                }
                else
                {
                  if (Line > 0)
                  {
                    Char = File[Line - 1].Length;
                    File[Line - 1] += File[Line];
                    File.RemoveAt(Line);
                    Line--;
                  }
                }
              }
            }

            break;
          }

        case ConsoleKey.Delete:
          {
            if ((Key.Modifiers & ConsoleModifiers.Control) == 0)
            {
              if (Char == File[Line].Length)
              {
                if (Line + 1 < File.Count)
                {
                  File[Line] += File[Line + 1];
                  File.RemoveAt(Line + 1);
                  forceRedraw = true;
                }
              }
              else
              {
                File[Line] = File[Line].Remove(Char, 1);
                forceRedraw = true;
              }
            }
            else
            {
              bool edited = false;

              if (Char > File[Line].Length)
                Char = Math.Max(0, File[Line].Length);

              for (int i = Char + 1; i < File[Line].Length; i++)
                if (JumpToChars.Contains(File[Line][i]))
                {
                  File[Line] = File[Line].Remove(Char, i - Char);

                  Char = i - 1;
                  edited = true;
                  break;
                }

              if (!edited)
              {
                if (Char == File[Line].Length)
                {
                  if (Line + 1 < File.Count)
                  {
                    File[Line] += File[Line + 1];
                    File.RemoveAt(Line + 1);
                    forceRedraw = true;
                  }
                }
                else
                {
                  File[Line] = File[Line].Remove(Char);
                  forceRedraw = true;
                }
              }
            }

            break;
          }

        case ConsoleKey.Tab:
          {
            File[Line] += "  ";
            Char += 2;
            break;
          }

        case ConsoleKey.S:
          {
            if (Key.Modifiers == ConsoleModifiers.Control)
            {
              if (string.IsNullOrWhiteSpace(FileName) || Key.Modifiers == ConsoleModifiers.Alt)
              {
                Clear();
                Console.SetCursorPosition(2, 1);
                Console.WriteLine("Message:");
                Console.SetCursorPosition(4, 3);
                Console.WriteLine("Enter File Name:");
                Console.WriteLine();
                Console.WriteLine();
                Console.SetCursorPosition(2, BufferHeight - 2);
                Console.WriteLine("Press [Enter] to continue.");
                Console.SetCursorPosition(4, 5);

                FileName = Console.ReadLine();
              }

              if (!string.IsNullOrWhiteSpace(FileName))
              {
                try
                {
                  System.IO.File.WriteAllLines(FileName, File);
                  CurrentMode = EMode.MessageBox;
                  forceRedraw = true;
                  ErrorMessageString = $"Wrote contents to '{FileName}'.";
                }
                catch (Exception e)
                {
                  CurrentMode = EMode.MessageBox;
                  forceRedraw = true;
                  ErrorMessageString = $"Failed to write file to '{FileName}' ({e.Message})";
                }
              }
              else
              {
                CurrentMode = EMode.MessageBox;
                forceRedraw = true;
                ErrorMessageString = $"Aborted writing contents to file.";
              }
            }

            break;
          }
      }
    }

    static void EditModeRender()
    {
      Console.SetCursorPosition(0, 0);
      StringBuilder sb = new StringBuilder();

      int renderLine = 0;
      int characterIndex = 0;
      int drawnChar = 0;
      int textLine = Math.Max(Line - BufferHeight / 2, 0);

      Char = Math.Min(Char, File[Line].Length);

      while (renderLine < BufferHeight - 2)
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
          Console.ForegroundColor = ConsoleColor.White;
        }

        for (; characterIndex < File[textLine].Length; characterIndex++)
        {
          char c = File[textLine][characterIndex];

          if (textLine == Line && characterIndex == Char)
          {
            Console.Write(sb.ToString());
            sb.Clear();

            ActivePositionX = drawnChar % Console.BufferWidth;
            ActivePositionY = renderLine + (ActivePositionX != drawnChar ? 1 : 0);

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
          }

          if (textLine == Line && characterIndex > Char && characterIndex <= Char + 2) // to accommodate for tabs.
          {
            Console.Write(sb.ToString());
            sb.Clear();
            Console.ResetColor();
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
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
            if (Char == characterIndex)
            {
              Console.BackgroundColor = ConsoleColor.DarkGray;
              Console.Write(sb.ToString());
              sb.Clear();

              Console.BackgroundColor = ConsoleColor.White;
              Console.Write(" ");
              
              ActivePositionX = drawnChar % Console.BufferWidth;
              ActivePositionY = renderLine + (ActivePositionX != drawnChar ? 1 : 0);

              drawnChar++;

              if (drawnChar == Console.BufferWidth)
                renderLine++;
            }

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

      while (renderLine < BufferHeight - 1)
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
