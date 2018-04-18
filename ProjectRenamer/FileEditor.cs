using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

            var rootNamespace = xml.Descendants().Single(el => el.Name.LocalName == "RootNamespace");
            var assemblyName = xml.Descendants().Single(el => el.Name.LocalName == "AssemblyName");

            rootNamespace.SetValue(args.NewProjectName);
            assemblyName.SetValue(args.NewProjectName);

            text = xml.ToString();
            File.WriteAllText(args.NewProjectPath, text);
        }

        public static void UpdateAssemblyInfo(AppArguments args)
        {
            var asmInfoPath = Path.Combine(Path.GetDirectoryName(args.NewProjectPath), "Properties", "AssemblyInfo.cs");

            if (File.Exists(asmInfoPath))
            {
                Console.WriteLine("Updating assembly info file");
                var text = File.ReadAllText(asmInfoPath);

                text = Regex.Replace(text, @"(\[assembly: AssemblyTitle\("")[^""]+(""\)\])", "${1}" + args.NewProjectName + "${2}");
                text = Regex.Replace(text, @"(\[assembly: AssemblyProduct\("")[^""]+(""\)\])", "${1}" + args.NewProjectName + "${2}");

                File.WriteAllText(asmInfoPath, text);
            }
            else
            {
                Console.WriteLine("No assembly info found. Is this a .NET Core or .NET Standard project?");
            }
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
