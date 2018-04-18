using System;
using System.IO;

namespace ProjectRenamer
{
    static class ArgumentParser
    {
        private const int _minArgs = 2;
        private const int _maxArgs = 3;
        private const string _projExtension = ".csproj";

        public static AppArguments ParseArgs(string[] args)
        {
            if (args.Length < _minArgs || args.Length > _maxArgs)
            {
                throw new ArgumentException(
                    "Required arguments are:\n" +
                    "\t<projectPath> <newName> [<searchDirectory>]\n" +
                    "\n" +
                    "File extension is optional on the first two arguments.");
            }

            var oldPath = args[0];
            if (!oldPath.ToLower().EndsWith(_projExtension))
            {
                oldPath += _projExtension;
            }

            var oldName = Path.GetFileNameWithoutExtension(oldPath);
            var oldParentDir = Path.GetFileName(Path.GetDirectoryName(oldPath));

            var newName = args[1];
            if (newName.EndsWith(_projExtension))
            {
                newName = newName.Substring(0, newName.IndexOf(_projExtension) - 1);
            }

            var newPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(oldPath)), newName, $"{newName}{_projExtension}");
            
            var searchDir = args.Length == 3 ? args[2] : null;

            if (!File.Exists(oldPath))
            {
                throw new ArgumentException($"Project path '{oldPath}' does not exist.");
            }

            if (!IsValidPath(oldParentDir, newName))
            {
                throw new ArgumentException($"New name '{newName}' is not valid.");
            }

            if (searchDir != null && !Directory.Exists(searchDir))
            {
                throw new ArgumentException($"Search directory '{searchDir}' does not exist.");
            }

            return new AppArguments
            {
                OldProjectPath = oldPath,
                OldProjectName = oldName,
                OldProjectDirectParentDirectory = oldParentDir,
                NewProjectName = newName,
                NewProjectPath = newPath,
                SearchDirectory = searchDir
            };
        }

        private static bool IsValidPath(string dir, string filename)
        {
            return filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
                && !File.Exists(Path.Combine(dir, filename));
        }
    }
}
