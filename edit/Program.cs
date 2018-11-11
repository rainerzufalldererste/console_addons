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
    static int OriginX = 0, OriginY = 0;
    static string FileName;
    static string ErrorMessageString = "";
    static int SelectionStartLine = -1, SelectionEndLine = -1, SelectionStartChar = -1, SelectionEndChar = -1;
    static bool Selecting = false;
    static List<string> CopyBuffer = new List<string>();

    [STAThread]
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
                  OriginX = Console.CursorLeft;
                  OriginY = Console.CursorTop;
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
              SetCursorPosition(2, 1);
              Console.WriteLine("Message:");
              SetCursorPosition(4, 3);
              Console.WriteLine(ErrorMessageString);
              Console.WriteLine();
              SetCursorPosition(2, BufferHeight - 2);
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

        SetCursorPosition(ActivePositionX, ActivePositionY);
      }
      while (IsRunning && !((Key = Console.ReadKey()).Key == ConsoleKey.Q && Key.Modifiers == ConsoleModifiers.Control));

      if (!CustomHeight)
        Clear();
      else
        SetCursorPosition(0, BufferHeight - 2);
    }

    private static void SetCursorPosition(int x, int y)
    {
      Console.SetCursorPosition(OriginX + x, OriginY + y);
    }

    private static void Clear()
    {
      if (CustomHeight)
      {
        SetCursorPosition(0, 0);
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
        if (Selecting)
        {
          RemoveSelection();
          Selecting = false;
        }

        File[Line] = File[Line].Insert(Char, Key.KeyChar.ToString());
        Char++;
      }
      else
      {
        if (Key.Modifiers == ConsoleModifiers.Shift)
        {
          if (!Selecting)
          {
            SelectionStartLine = Line;
            SelectionEndLine = Line;
            SelectionStartChar = Char;
            SelectionEndChar = Char;
            Selecting = true;
          }
        }
      }

      switch (Key.Key)
      {
        case ConsoleKey.Escape:
          if (Selecting)
          {
            Selecting = false;
            forceRedraw = true;
          }
          break;

        case ConsoleKey.UpArrow:
          if (Char > Console.BufferWidth && !((Key.Modifiers & (ConsoleModifiers.Control)) != 0))
            Char -= Console.BufferWidth;
          else
            Line = Math.Max(Line - 1, 0);
          break;

        case ConsoleKey.DownArrow:
          if (Char + Console.BufferWidth <= File[Line].Length && !((Key.Modifiers & (ConsoleModifiers.Control)) != 0))
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
            if (Selecting)
            {
              RemoveSelection();
              Selecting = false;
            }

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
            if (Selecting)
            {
              forceRedraw = true;
              RemoveSelection();
              Selecting = false;
              break;
            }

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
            if (Selecting)
            {
              forceRedraw = true;
              RemoveSelection();
              Selecting = false;
              break;
            }

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
            bool jumped = false;

            if (File[Line].Length > Char)
            {
              int i = Char;

              for (; i < File[Line].Length; i++)
              {
                if (File[Line][i] != ' ' && File[Line][i] != '\t')
                {
                  if (i == Char)
                    break;
                  else
                  {
                    Char = i;
                    jumped = true;
                    break;
                  }
                }
              }

              if (i == File[Line].Length)
              {
                Char = i;
                jumped = true;
              }
            }

            if (!jumped)
            {
              File[Line] = File[Line].Insert(Char, "  ");
              Char += 2;
            }

            break;
          }

        case ConsoleKey.S:
          {
            if (Key.Modifiers == ConsoleModifiers.Control)
            {
              if (string.IsNullOrWhiteSpace(FileName) || Key.Modifiers == ConsoleModifiers.Alt)
              {
                Clear();
                SetCursorPosition(2, 1);
                Console.WriteLine("Message:");
                SetCursorPosition(4, 3);
                Console.WriteLine("Enter File Name:");
                Console.WriteLine();
                Console.WriteLine();
                SetCursorPosition(2, BufferHeight - 2);
                Console.WriteLine("Press [Enter] to continue.");
                SetCursorPosition(4, 5);

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

        case ConsoleKey.C:
          {
            if (Key.Modifiers == ConsoleModifiers.Control)
            {
              StringBuilder sb = new StringBuilder();

              if (SelectionStartLine < SelectionEndLine)
              {
                sb.AppendLine(File[SelectionStartLine].Substring(SelectionStartChar));

                for (int i = SelectionStartLine + 1; i < SelectionEndLine; i++)
                  sb.AppendLine(File[i]);

                sb.AppendLine(File[SelectionEndLine].Substring(0, SelectionEndChar));
              }
              else
              {
                sb.AppendLine(File[SelectionStartLine].Substring(SelectionStartChar, SelectionEndChar - SelectionStartChar));
              }

              System.Windows.Forms.Clipboard.SetText(sb.ToString());
              CopyBuffer.Add(sb.ToString());
            }

            break;
          }

        case ConsoleKey.X:
          {
            if (Key.Modifiers == ConsoleModifiers.Control)
            {
              StringBuilder sb = new StringBuilder();

              if (SelectionStartLine < SelectionEndLine)
              {
                sb.AppendLine(File[SelectionStartLine].Substring(SelectionStartChar));

                for (int i = SelectionStartLine + 1; i < SelectionEndLine; i++)
                  sb.AppendLine(File[i]);

                sb.AppendLine(File[SelectionEndLine].Substring(0, SelectionEndChar));
              }
              else
              {
                sb.AppendLine(File[SelectionStartLine].Substring(SelectionStartChar, SelectionEndChar - SelectionStartChar));
              }

              System.Windows.Forms.Clipboard.SetText(sb.ToString());
              CopyBuffer.Add(sb.ToString());

              RemoveSelection();
              Selecting = false;
              forceRedraw = true;
            }

            break;
          }

        case ConsoleKey.A:
          {
            if (Key.Modifiers == ConsoleModifiers.Control)
            {
              Selecting = true;
              SelectionStartLine = 0;
              SelectionStartChar = 0;
              SelectionEndLine = File.Count - 1;
              SelectionEndChar = File[File.Count - 1].Length;
              forceRedraw = true;
            }

            break;
          }

        case ConsoleKey.V:
          {
            if (Key.Modifiers == ConsoleModifiers.Control)
            {
              if (Selecting)
              {
                RemoveSelection();
                Selecting = false;
              }

              forceRedraw = true;
              string clipboard = null;

              if (System.Windows.Forms.Clipboard.ContainsText())
                clipboard = System.Windows.Forms.Clipboard.GetText();
              
              if (clipboard == null)
              {
                CurrentMode = EMode.MessageBox;
                ErrorMessageString = $"Failed to paste. To text currently copied.";
                break;
              }

              for (int i = 0; i < clipboard.Length; i++)
              {
                if (clipboard[i] == '\r')
                {
                  continue;
                }
                else if (clipboard[i] == '\n')
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
                else
                {
                  File[Line] = File[Line].Insert(Char, clipboard[i].ToString());
                  Char++;
                }
              }
            }

            break;
          }

        case ConsoleKey.F5:
          {
            forceRedraw = true;

            if (string.IsNullOrWhiteSpace(FileName))
            {
              CurrentMode = EMode.MessageBox;
              ErrorMessageString = $"Currently not attached to a file to reload from.";
              break;
            }

            try
            {
              File = System.IO.File.ReadAllLines(FileName).ToList();
            }
            catch (Exception e)
            {
              CurrentMode = EMode.MessageBox;
              ErrorMessageString = $"Failed to reload file '{FileName}'. ({e.Message})";
              break;
            }

            if (File.Count == 0)
              File.Add("");

            if (Line >= File.Count)
            {
              Line = 0;
              Char = 0;
            }
            else if (Char > File[Line].Length)
            {
              Char = 0;
            }

            break;
          }
      }

      if (Selecting && (Key.Modifiers & ConsoleModifiers.Shift) != 0)
      {
        if (Line == SelectionStartLine && Char < SelectionStartChar)
        {
          SelectionStartChar = Char;
        }
        else if (Line == SelectionEndLine && Char > SelectionEndChar)
        {
          SelectionEndChar = Char;
        }
        else if (Line < SelectionStartLine)
        {
          SelectionStartLine = Line;
          SelectionStartChar = Char;
        }
        else if (Line > SelectionEndLine)
        {
          SelectionEndLine = Line;
          SelectionEndChar = Char;
        }
      }
    }

    private static void RemoveSelection()
    {
      if (SelectionStartLine < SelectionEndLine)
      {
        if (File[SelectionStartLine].Length > 0)
          File[SelectionStartLine] = File[SelectionStartLine].Remove(SelectionStartChar);

        if (File[SelectionEndLine].Length > 0)
          File[SelectionEndLine] = File[SelectionEndLine].Substring(SelectionEndChar);

        File.RemoveRange(SelectionStartLine + 1, SelectionEndLine - SelectionStartLine - 1);
      }
      else
      {
        File[SelectionStartLine] = File[SelectionStartLine].Remove(SelectionStartChar, SelectionEndChar - SelectionStartChar);
      }

      Line = SelectionStartLine;
      Char = SelectionStartChar;
    }

    static void EditModeRender()
    {
      SetCursorPosition(0, 0);
      StringBuilder sb = new StringBuilder();

      if (Char > File[Line].Length)
        Char = Math.Min(Char - (Char / Console.BufferWidth) * Console.BufferWidth, File[Line].Length);

      int renderLine = 0;
      int characterIndex = 0;
      int drawnChar = 0;
      int textLine = Line;
      bool inSelection = false;

      // Find correct line to start rendering.
      {
        int remainingLines = (BufferHeight - 1) / 2;

        for (int i = Line; i >= 0; i--)
        {
          if (remainingLines <= 0)
            break;

          remainingLines--;
          textLine = i;
          characterIndex = 0;

          if (File[i].Length > Console.BufferWidth)
          {
            if (i == Line)
              characterIndex = Char - (Char % Console.BufferWidth);
            else
              characterIndex = File[i].Length - (File[i].Length % Console.BufferWidth);
            
            while (characterIndex > 0)
            {
              if (remainingLines > 0)
              {
                remainingLines--;
                characterIndex -= Console.BufferWidth;
              }
              else
              {
                break;
              }
            }
          }
        }
      }

      if (Selecting && (textLine > SelectionStartLine || (textLine == SelectionStartLine && characterIndex >= SelectionStartChar)))
      {
        if (textLine == Line)
        {
          Console.ForegroundColor = ConsoleColor.Cyan;
          Console.BackgroundColor = ConsoleColor.DarkCyan;
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.Cyan;
          Console.BackgroundColor = ConsoleColor.DarkBlue;
        }

        inSelection = true;
      }

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

          if (!inSelection)
          {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.White;
          }
          else
          {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.BackgroundColor = ConsoleColor.DarkCyan;
          }
        }

        for (; characterIndex < File[textLine].Length; characterIndex++)
        {
          if (Selecting)
          {
            bool currentCharInSelection = IsInSelection(textLine, characterIndex);

            if (!inSelection && currentCharInSelection)
            {
              Console.Write(sb.ToString());
              sb.Clear();
              inSelection = true;

              if (textLine == Line)
              {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.BackgroundColor = ConsoleColor.DarkCyan;
              }
              else
              {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.BackgroundColor = ConsoleColor.DarkBlue;
              }
            }
            else if (inSelection && !currentCharInSelection)
            {
              Console.Write(sb.ToString());
              sb.Clear();
              inSelection = false;

              if (textLine == Line)
              {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.ForegroundColor = ConsoleColor.White;
              }
              else
              {
                Console.ResetColor();
              }
            }
          }

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

            if (!inSelection)
            {
              Console.BackgroundColor = ConsoleColor.DarkGray;
              Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
              Console.ForegroundColor = ConsoleColor.Cyan;
              Console.BackgroundColor = ConsoleColor.DarkCyan;
            }
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
              if (!inSelection)
              {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.ForegroundColor = ConsoleColor.White;
              }
              else
              {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.BackgroundColor = ConsoleColor.DarkCyan;
              }

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

            if (!inSelection)
            {
              Console.BackgroundColor = ConsoleColor.DarkGray;
              Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
              Console.ForegroundColor = ConsoleColor.Cyan;
              Console.BackgroundColor = ConsoleColor.DarkCyan;
            }

            Console.Write(sb.ToString());
            sb.Clear();
          }

          if (drawnChar != Console.WindowWidth)
            sb.Append(new string(' ', Console.WindowWidth - (drawnChar % Console.WindowWidth)));
          
          if (!inSelection)
          {
            Console.ResetColor();
          }
          else
          {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
          }

          renderLine++;
          textLine++;
          characterIndex = 0;
          drawnChar = 0;
        }
      }

      Console.Write(sb.ToString());
      sb.Clear();
      Console.ResetColor();

      while (renderLine < BufferHeight - 1)
      {
        sb.Append(new string(' ', Console.WindowWidth));
        renderLine++;
      }

      Console.Write(sb.ToString());
      sb.Clear();
    }

    private static bool IsInSelection(int textLine, int characterIndex)
    {
      return (textLine > SelectionStartLine && textLine < SelectionEndLine) || (textLine == SelectionStartLine && SelectionStartLine != SelectionEndLine && characterIndex >= SelectionStartChar) || (textLine == SelectionEndLine && SelectionStartLine != SelectionEndLine && characterIndex <= SelectionEndChar) || (SelectionStartLine == SelectionEndLine && textLine == SelectionStartLine && characterIndex <= SelectionEndChar && characterIndex >= SelectionStartChar);
    }
  }
}
