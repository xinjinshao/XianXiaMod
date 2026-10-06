using System.Reflection;
using System.Runtime.Loader;

if (args.Length != 3) throw new ArgumentException("Expected package, tModLoader directory and repository root.");
string package = Path.GetFullPath(args[0]);
string root = Path.GetFullPath(args[2]);
string engineRoot = Path.GetFullPath(args[1]);
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    string engineAssembly = Path.Combine(engineRoot, name.Name + ".dll");
    if (File.Exists(engineAssembly)) return AssemblyLoadContext.Default.LoadFromAssemblyPath(engineAssembly);
    string dependency = Directory.EnumerateFiles(Path.Combine(engineRoot, "Libraries"), name.Name + ".dll", SearchOption.AllDirectories)
        .FirstOrDefault(path => !path.Contains(Path.DirectorySeparatorChar + "Native" + Path.DirectorySeparatorChar)
            && !path.Contains(Path.DirectorySeparatorChar + "runtime"));
    return dependency == null ? null : AssemblyLoadContext.Default.LoadFromAssemblyPath(dependency);
};


var engine = Assembly.LoadFrom(Path.Combine(engineRoot, "tModLoader.dll"));
var fileType = engine.GetType("Terraria.ModLoader.Core.TmodFile", true);
var file = Activator.CreateInstance(fileType, BindingFlags.Instance | BindingFlags.NonPublic, null,
    new object[] { package, null, null }, null);
using var opened = (IDisposable)fileType.GetMethod("Open").Invoke(file, null);
var names = (List<string>)fileType.GetMethod("GetFileNames").Invoke(file, null);
var nameSet = names.ToHashSet(StringComparer.Ordinal);
int checks = 0;
void Check(bool value, string message) { checks++; if (!value) throw new Exception(message); }
Check((string)fileType.GetProperty("Name").GetValue(file) == "XianXia", "Incorrect package internal name");
string version = File.ReadAllLines(Path.Combine(root, "build.txt"))
    .Single(line => line.StartsWith("version = ")).Split('=', 2)[1].Trim();
Check(fileType.GetProperty("Version").GetValue(file).ToString() == version, "Package version differs from build.txt");
Check((bool)fileType.GetMethod("VerifyHash", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(file, null), "Package hash failed");
foreach (string name in names) {
    Check(!name.Split('/').Any(part => part.StartsWith('.')) && !name.Contains(".."), "Hidden or traversing package entry: " + name);
    Check(!new[] { "Assets/", "Docs/", "Wiki/", "Tools/", "bin/", "obj/" }.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        && name != "README.md", "Development artifact in package: " + name);
    Check(!new[] { ".cs", ".csproj", ".sln", ".ps1", ".py", ".psd" }.Contains(Path.GetExtension(name), StringComparer.OrdinalIgnoreCase), "Source/tool file in package: " + name);
}
Check(nameSet.Contains("XianXia.dll") && nameSet.Contains("Info"), "Package missing assembly or metadata");
var getBytes = fileType.GetMethod("GetBytes", new[] { typeof(string) });
Check(((byte[])getBytes.Invoke(file, new object[] { "description.txt" })).SequenceEqual(File.ReadAllBytes(Path.Combine(root, "description.txt"))), "Stale packaged description");
int resources = 0;
foreach (string folder in new[] { "Common", "Content", "Localization" }) {
    foreach (string path in Directory.EnumerateFiles(Path.Combine(root, folder), "*", SearchOption.AllDirectories)) {
        if (!new[] { ".png", ".hjson" }.Contains(Path.GetExtension(path))) continue;
        string relative = Path.GetRelativePath(root, path).Replace('\\', '/');
        Check(nameSet.Contains(relative) || (relative.EndsWith(".png") && nameSet.Contains(Path.ChangeExtension(relative, ".rawimg"))), "Missing runtime resource: " + relative);
        resources++;
    }
}
Console.WriteLine($"Native package checks passed: {names.Count} entries, {resources} runtime resources, {checks} checks; no game world started.");
