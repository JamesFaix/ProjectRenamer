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
            Directory.Move(args.OldProjectDirectory, args.NewProjectDirectory);
            var tempPath = Path.Combine(args.NewProjectDirectory, $"{args.OldProjectName}.csproj");
            File.Move(tempPath, args.NewProjectPath);
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
                
                File.WriteAllText(args.NewProjectPath, xml.ToString());
            }
            else
            {
                Console.WriteLine("Project is .NET Standard or .NET Core, no update necessary.");
            }
        }

        private static bool IsClassicProject(XDocument projectFile)
        {
            //Only look at top-level Project elements
            return !projectFile.GetElementByLocalName("Project")
                .GetAttributesByLocalName("Sdk")
                .Any();
        }

        public static void UpdateAssemblyInfo(AppArguments args)
        {
            var asmInfoPath = Path.Combine(args.NewProjectDirectory, "Properties", "AssemblyInfo.cs");

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

                        var oldRelativePath = attr.Value;
                        var relativePathBase = Path.GetDirectoryName(Path.GetDirectoryName(oldRelativePath));
                        var newRelativePath = Path.Combine(relativePathBase, args.NewProjectName, $"{args.NewProjectName}.csproj");

                        attr.Value = newRelativePath;

                        if (IsClassicProject(xml))
                        {
                            renamedProjRef.GetDescendantByLocalName("Name")
                                .SetValue(args.NewProjectName);
                        }

                        File.WriteAllText(p, xml.ToString());
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