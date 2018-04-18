## Project Renamer

If you've ever tried to rename a C# project in Visual Studio, you know it's not as easy as it should be. Especially when working in a large repository where a single project can be referenced in many places.

Project Renamer makes this easier.


## How to use

Make sure the given project and any projects or solutions referencing it are not open in Visual Studio.

Call `ProjectRenamer.exe` from the command line with the following arguments
- Target project path
- New project name
- (Optional) Directory to search for references in

Example:
`ProjectRenamer.exe C:\git\myRepo\myProject\myProject.csproj myNewProjectName C:\git\myRepo`

You don't need to include the `.csproj` extension.

What it will update
- The `csproj` file will be renamed
- The directory containing the `csproj` file will be renamed to match
- The `RootNamespace` and `AssemblyName` values in the `csproj` will be updated to match
- The `AssemblyTitle` and `AssemblyProduct` attributes values in `Properties/AssemblyInfo.cs` will be updated to match (will be skipped for .NET Core/Standard projects)
- If a search directory is provided, any other `csproj` or `sln` files in that directory will have project references updated to the new name and path 