using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ProjectRenamer
{
    static class FileEditor
    {
        public static void Rename(AppArguments args)
        {
            Console.WriteLine("Renaming project file");

            var oldDir = Path.GetDirectoryName(args.OldProjectPath);
            var newDir = Path.GetDirectoryName(args.NewProjectPath);

            Directory.Move(oldDir, newDir);

            var oldPath = Path.Combine(newDir, $"{args.OldProjectName}.csproj");

            File.Move(oldPath, args.NewProjectPath);
        }

        public static void UpdateProjectFileContents(AppArguments args)
        {
            Console.WriteLine("Updating project file contents");

            var text = File.ReadAllText(args.NewProjectPath);
            var xml = XDocument.Parse(text);

            var rootNamespace = xml.Root.Descendants("RootNamespace").Single();
            var assemblyName = xml.Root.Descendants("AssemblyName").Single();

            rootNamespace.SetValue(args.NewProjectName);
            assemblyName.SetValue(args.NewProjectName);

            text = xml.ToString();
            File.WriteAllText(args.NewProjectPath, text);
        }

        public static void UpdateAssemblyInfo(AppArguments args)
        {
            Console.WriteLine("Updating assembly info file");

        }

        public static void UpdateReferencingProjects(AppArguments args)
        {
            Console.WriteLine("Updating referencing project files");
        }

        public static void UpdateReferencingSolutions(AppArguments args)
        {
            Console.WriteLine("Updating referencing solution files");
        }
    }
}
