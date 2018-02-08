using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace gitchanges
{
  class Program
  {
    static bool excludeSubmodules = true;

    static void Main(string[] args)
    {
      string gitRepositoryPath = Repository.Discover(Environment.CurrentDirectory);

      if (string.IsNullOrEmpty(gitRepositoryPath))
      {
        Console.WriteLine($"No git-Repository found in '{Environment.CurrentDirectory}'.");
        return;
      }

      Repository repo = new Repository(gitRepositoryPath);
      Branch currentBranch = repo.Head;

      if (currentBranch == null)
      {
        Console.WriteLine("No branch specified in git-HEAD is marked as current branch. (possibly detached HEAD)");
        return;
      }

      string tracking = "";

      if (currentBranch.TrackedBranch != null)
        tracking = $"(tracking '{currentBranch.TrackedBranch.FriendlyName}')";
      else
        tracking = "(no tracked branch)";

      Console.WriteLine($"Current Branch: {currentBranch.FriendlyName} {tracking}\n");

      RepositoryStatus repositoryStatus = repo.RetrieveStatus(new StatusOptions() { ExcludeSubmodules = excludeSubmodules, IncludeUnaltered = false, IncludeIgnored = false });
      bool stopAfterStep = false;
      int modifiedChanges = 0;

      foreach (var item in repositoryStatus)
      {
        if (item.State == FileStatus.NewInWorkdir)
        {
          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"NEW UNSTAGED FILE: {item.FilePath}");
          Console.ResetColor();

          stopAfterStep = true;
        }
      }

      Patch patch = repo.Diff.Compare<Patch>(currentBranch.Commits.First().Tree, DiffTargets.WorkingDirectory);

      modifiedChanges = (from f in patch where (f.Status == ChangeKind.Modified || f.Status == ChangeKind.Added || f.Status == ChangeKind.Deleted) && modifiedChanges++ > -1 select f).Count();

      if (modifiedChanges > 0)
      {
        foreach (var patchElement in patch)
        {
          switch (patchElement.Status)
          {
            case ChangeKind.Renamed:
              Console.ForegroundColor = ConsoleColor.DarkYellow;
              Console.WriteLine($"{patchElement.Path}: <- {patchElement.OldPath} (renamed)");
              Console.ResetColor();

              stopAfterStep = true;
              break;

            case ChangeKind.Copied:
              Console.ForegroundColor = ConsoleColor.DarkYellow;
              Console.WriteLine($"{patchElement.Path}: (copied)");
              Console.ResetColor();

              stopAfterStep = true;
              break;

            case ChangeKind.Deleted:
              Console.ForegroundColor = ConsoleColor.DarkRed;
              Console.WriteLine($"{patchElement.Path}: (deleted)");
              Console.ResetColor();

              stopAfterStep = true;
              break;

            //case ChangeKind.Added:
            //  Console.ForegroundColor = ConsoleColor.Green;
            //  Console.WriteLine($"{patchElement.Path}: (added)");
            //  Console.ResetColor();
            //
            //  stopAfterStep = true;
            //  break;

            case ChangeKind.Conflicted:
              Console.ForegroundColor = ConsoleColor.Red;
              Console.WriteLine($"{patchElement.Path}: (conflicted)");
              Console.ResetColor();

              stopAfterStep = true;
              break;

            case ChangeKind.Modified:
              modifiedChanges++;
              break;
          }
        }

        Console.WriteLine();

        if (modifiedChanges > 0)
        {
          if (stopAfterStep)
          {
            Console.WriteLine($"{modifiedChanges} modified unstaged file(s). Press Enter to continue.\n");
            Console.ReadKey();
          }
          else
          {
            Console.WriteLine($"{modifiedChanges} modified unstaged file(s):\n");
          }
        }

        stopAfterStep = false;

        var modDiffs = (from f in patch where f.Status == ChangeKind.Modified select f);

        foreach (var diff in modDiffs)
        {
          if (diff.IsBinaryComparison)
          {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{diff.Path}: (modified binary)\n");
            Console.ResetColor();
            continue;
          }

          Console.ForegroundColor = ConsoleColor.Blue;
          Console.WriteLine($"{diff.Path}: (+ {diff.LinesAdded} | - {diff.LinesDeleted})");
          Console.ResetColor();
          Console.WriteLine($"{diff.Patch}\n");

          if (diff != modDiffs.Last())
          {
            Console.WriteLine($"Press Enter to continue.\n");
            Console.ReadKey();
          }

          stopAfterStep = true;
        }
      }
      else
      {
        Console.WriteLine("No unstashed changes on current branch.");
      }
      
      repositoryStatus = repo.RetrieveStatus(new StatusOptions() { IncludeIgnored = false, IncludeUnaltered = false, ExcludeSubmodules = false, Show = StatusShowOption.IndexOnly });
      
      var stagedChanges = (from rs in repositoryStatus where (rs.State == FileStatus.ModifiedInIndex || rs.State == FileStatus.NewInIndex || rs.State == FileStatus.DeletedFromIndex) select rs);
      
      if (stagedChanges.Count() > 0)
      {
        stopAfterStep = false;
        modifiedChanges = 0;

        foreach(var change in stagedChanges)
        {
          switch (change.State)
          {
            case FileStatus.NewInIndex:
              Console.ForegroundColor = ConsoleColor.Green;
              Console.WriteLine($"{change.FilePath}: (added)");
              Console.ResetColor();
              modifiedChanges++;
              break;

            case FileStatus.DeletedFromIndex:
              Console.ForegroundColor = ConsoleColor.Red;
              Console.WriteLine($"{change.FilePath}: (deleted)");
              Console.ResetColor();
              modifiedChanges++;
              break;

            default:
              stopAfterStep = true;
              break;
          }
        }

        if(stopAfterStep && modifiedChanges > 0)
        {
          Console.WriteLine($"{stagedChanges.Count() - modifiedChanges} modified staged file(s). Press Enter to continue.\n");
          Console.ReadKey();
        }
        else
        {
          Console.WriteLine($"{stagedChanges.Count() - modifiedChanges} modified staged file(s).\n");
        }

        var modifiedStagedChanges = (from s in stagedChanges where s.State == FileStatus.ModifiedInIndex select s);

        foreach (var change in modifiedStagedChanges)
        {
          Console.WriteLine($"{change.FilePath}: (modified)");

          if(change != modifiedStagedChanges.Last())
          {
            Console.WriteLine($"Press Enter to continue.\n");
            Console.ReadKey();
          }
        }
      }
      else
      {
        Console.WriteLine("No stashed changes on current branch.");
      }
    }
  }
}
