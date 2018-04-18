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

            if (IsClassicProject(xml))
            {
                xml.GetDescendantByLocalName("RootNamespace")
                    .SetValue(args.NewProjectName);

                xml.GetDescendantByLocalName("AssemblyName")
                    .SetValue(args.NewProjectName);

                text = xml.ToString();
                File.WriteAllText(args.NewProjectPath, text);
            }
            else
            {
                Console.WriteLine("Project is .NET Standard or .NET Core, no update necessary.");
            }
        }

        private static bool IsClassicProject(XDocument projectFile)
        {
            //Only look at top-level Project elements
            var projectElement = projectFile.GetElementByLocalName("Project");

            return !projectElement.Attributes()
                .Any(attr => attr.Name.LocalName == "Sdk");
        }

        public static void UpdateAssemblyInfo(AppArguments args)
        {
            var asmInfoPath = Path.Combine(Path.GetDirectoryName(args.NewProjectPath), "Properties", "AssemblyInfo.cs");

            if (File.Exists(asmInfoPath))
            {
                Console.WriteLine("Updating assembly info file");
                var text = File.ReadAllText(asmInfoPath);

                text = Regex.Replace(text, 
                    @"(\[assembly: AssemblyTitle\("")[^""]+(""\)\])", 
                    "${1}" + args.NewProjectName + "${2}");


                text = Regex.Replace(text, 
                    @"(\[assembly: AssemblyProduct\("")[^""]+(""\)\])", 
                    "${1}" + args.NewProjectName + "${2}");

                File.WriteAllText(asmInfoPath, text);
            }
            else
            {
                Console.WriteLine("No assembly info found. Is this a .NET Core or .NET Standard project?");
            }
        }

        public static void UpdateReferencingProjects(AppArguments args)
        {
            if (args.SearchDirectory == null)
            {
                Console.WriteLine("No search directory provided, skipping updates to referencing projects");
            }
            else
            {
                Console.WriteLine("Updating referencing project files");
                var projects = Directory.GetFiles(args.SearchDirectory, "*.csproj", SearchOption.AllDirectories);

                foreach (var p in projects)
                {
                    var text = File.ReadAllText(p);
                    var xml = XDocument.Parse(text);

                    var projRefs = xml.GetDescendantsByLocalName("ProjectReference");

                    var renamedProjRef = projRefs.SingleOrDefault(projRef => 
                        projRef.GetAttributeByLocalName("Include")
                            .Value.Contains(args.OldProjectName + ".csproj"));

                    if (renamedProjRef != null)
                    {
                        var attr = renamedProjRef.GetAttributeByLocalName("Include");

                        var oldPath = attr.Value;
                        var pathBase = Path.GetDirectoryName(Path.GetDirectoryName(oldPath));
                        var newPath = Path.Combine(pathBase, args.NewProjectName, $"{args.NewProjectName}.csproj");

                        attr.Value = newPath;

                        if (IsClassicProject(xml))
                        {
                            renamedProjRef.GetDescendantByLocalName("Name")
                                .SetValue(args.NewProjectName);
                        }

                        text = xml.ToString();
                        File.WriteAllText(p, text);
                    }                    
                }
            }
        }

        public static void UpdateReferencingSolutions(AppArguments args)
        {
            if (args.SearchDirectory == null)
            {
                Console.WriteLine("No search directory provided, skipping updates to referencing solutions");
            }
            else
            {
                Console.WriteLine("Updating referencing solution files");
                var solutions = Directory.GetFiles(args.SearchDirectory, "*.sln", SearchOption.AllDirectories);

                foreach (var s in solutions)
                {
                    var oldText = File.ReadAllText(s);

                    var newText = Regex.Replace(oldText,
                        @"(?<Header>Project\(""\{[^""]+\}""\) = "")" + args.OldProjectName + @"(?<Middle>"", "")(?<PathBase>[^""]\\+)?(?<Dir>[^\\]+)\\(?<Name>[^""]+).csproj""",
                        $"${{Header}}{args.NewProjectName}${{Middle}}${{PathBase}}{args.NewProjectName}\\{args.NewProjectName}.csproj");

                    if (oldText != newText)
                    {
                        File.WriteAllText(s, newText);
                    }
                }
            }
        }
    }
}