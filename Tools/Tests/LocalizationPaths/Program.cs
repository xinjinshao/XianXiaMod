using System.Reflection;
using System.Runtime.Loader;

if (args.Length != 2) throw new ArgumentException("Expected compiled XianXia.dll and tModLoader directory.");
string root = Path.GetFullPath(args[0]);
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

var engine=Assembly.LoadFrom(Path.Combine(engineRoot,"tModLoader.dll"));
var parser=engine.GetType("Terraria.ModLoader.LocalizationLoader",true).GetMethod("TryGetCultureAndPrefixFromPath",BindingFlags.Public|BindingFlags.Static);
int checkedFiles=0;
foreach(string file in Directory.EnumerateFiles(Path.Combine(root,"Localization"),"*.hjson",SearchOption.AllDirectories)) {
 string relative=Path.GetRelativePath(root,file).Replace('\\','/');object[] values={relative,null,null};
 if(!(bool)parser.Invoke(null,values)||!string.IsNullOrEmpty((string)values[2]))throw new Exception("Native loader cannot load complete-root keys at "+relative);
 checkedFiles++;
}
foreach(string bad in new[]{"Localization/worldgen.en-US.hjson","Localization/routes.zh-Hans.hjson"}){
 object[] values={bad,null,null};if((bool)parser.Invoke(null,values))throw new Exception("Negative path control unexpectedly accepted");
}
Console.WriteLine($"Native tModLoader localization path regression passed: {checkedFiles} files and 2 rejected legacy controls.");
