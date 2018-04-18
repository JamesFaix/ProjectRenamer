using System;

namespace ProjectRenamer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var appArgs = ArgumentParser.ParseArgs(args);

                FileEditor.Rename(appArgs);
                FileEditor.UpdateProjectFileContents(appArgs);
                FileEditor.UpdateAssemblyInfo(appArgs);
                FileEditor.UpdateReferencingProjects(appArgs);
                FileEditor.UpdateReferencingSolutions(appArgs);

                Console.WriteLine("Done");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}\n{e.StackTrace}");
            }

            Console.Read();
        }
    }
}
