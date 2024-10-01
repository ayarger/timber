using CsvHelper;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using Yarn.Compiler;
using Yarn;
using YarnSpinnerGodot;


using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using File = System.IO.File;
using Path = System.IO.Path;
using System.Security.Cryptography;

public class YarnEditorUtilityDecoupled
{

    public static void UpdateLocalizationCSVs(YarnProject project)
    {
        if (project.LocaleCodeToCSVPath.Count > 0)
        {
            var modifiedFiles = new List<string>();

            foreach (var loc in project.LocaleCodeToCSVPath)
            {
                if (string.IsNullOrEmpty(loc.Value))
                {
                    GD.PrintErr($"Can't update localization for {loc.Key} because it doesn't have a strings file.");
                    continue;
                }

                var fileWasChanged = UpdateLocalizationFile(project.baseLocalization.GetStringTableEntries(),
                    loc.Key, loc.Value);

                if (fileWasChanged)
                {
                    modifiedFiles.Add(loc.Value);
                }
            }

            if (modifiedFiles.Count > 0)
            {
                GD.Print($"Updated the following files: {string.Join(", ", modifiedFiles)}");
            }
            else
            {
                GD.Print($"No Localization CSV files needed updating.");
            }
        }
    }
    public const string KEEP_IMPORT_TEXT = "[remap]\n\nimporter=\"keep\"";

    public static IEnumerable<StringTableEntry> ParseFromCSV(string sourceText)
    {
        try
        {
            using (var stringReader = new System.IO.StringReader(sourceText))
            using (var csv = new CsvReader(stringReader, StringTableEntry.GetConfiguration()))
            {
                /*
                Do the below instead of GetRecords<T> due to
                incompatibility with IL2CPP See more:
                https://github.com/YarnSpinnerTool/YarnSpinner-Unity/issues/36#issuecomment-691489913
                */
                var records = new List<StringTableEntry>();
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    // Fetch values; if they can't be found, they'll be
                    // defaults.
                    csv.TryGetField<string>("language", out var language);
                    csv.TryGetField<string>("lock", out var lockString);
                    csv.TryGetField<string>("comment", out var comment);
                    csv.TryGetField<string>("id", out var id);
                    csv.TryGetField<string>("text", out var text);
                    csv.TryGetField<string>("file", out var file);
                    csv.TryGetField<string>("node", out var node);
                    csv.TryGetField<string>("lineNumber", out var lineNumber);

                    var record = new StringTableEntry();

                    record.Language = language ?? string.Empty;
                    record.ID = id ?? string.Empty;
                    record.Text = text ?? string.Empty;
                    record.File = file ?? string.Empty;
                    record.Node = node ?? string.Empty;
                    record.LineNumber = lineNumber ?? string.Empty;
                    record.Lock = lockString ?? string.Empty;
                    record.Comment = comment ?? string.Empty;

                    records.Add(record);
                }

                return records;
            }
        }
        catch (CsvHelperException e)
        {
            throw new System.ArgumentException($"Error reading CSV file: {e}");
        }
    }
    private static bool UpdateLocalizationFile(IEnumerable<StringTableEntry> baseLocalizationStrings,
            string language, string csvResourcePath)
    {
        var absoluteCSVPath = ProjectSettings.GlobalizePath(csvResourcePath);

        IEnumerable<StringTableEntry> translatedStrings = new List<StringTableEntry>();
        if (File.Exists(absoluteCSVPath))
        {
            var existingCSVText = File.ReadAllText(absoluteCSVPath);
            translatedStrings = ParseFromCSV(existingCSVText);
        }
        else
        {
            GD.Print(
                $"CSV file {csvResourcePath} did not exist for locale {language}. A new file will be created at that location.");
        }

        // Convert both enumerables to dictionaries, for easier lookup
        var baseDictionary = baseLocalizationStrings.ToDictionary(entry => entry.ID);
        var translatedDictionary = translatedStrings.ToDictionary(entry => entry.ID);

        // The list of line IDs present in each localisation
        var baseIDs = baseLocalizationStrings.Select(entry => entry.ID);
        foreach (var str in translatedStrings)
        {
            if (baseDictionary.ContainsKey(str.ID))
            {
                str.Original = baseDictionary[str.ID].Text;
            }
        }

        var translatedIDs = translatedStrings.Select(entry => entry.ID);

        // The list of line IDs that are ONLY present in each
        // localisation
        var onlyInBaseIDs = baseIDs.Except(translatedIDs);
        var onlyInTranslatedIDs = translatedIDs.Except(baseIDs);

        // Tracks if the translated localisation needed modifications
        // (either new lines added, old lines removed, or changed lines
        // flagged)
        var modificationsNeeded = false;

        // Remove every entry whose ID is only present in the
        // translated set. This entry has been removed from the base
        // localization.
        foreach (var id in onlyInTranslatedIDs.ToList())
        {
            translatedDictionary.Remove(id);
            modificationsNeeded = true;
        }

        // Conversely, for every entry that is only present in the base
        // localisation, we need to create a new entry for it.
        foreach (var id in onlyInBaseIDs)
        {
            StringTableEntry baseEntry = baseDictionary[id];
            var newEntry = new StringTableEntry(baseEntry)
            {
                // Empty this text, so that it's apparent that a
                // translated version needs to be provided.
                Text = string.Empty,
                Original = baseEntry.Text,
                Language = language,
            };
            translatedDictionary.Add(id, newEntry);
            modificationsNeeded = true;
        }

        // Finally, we need to check for any entries in the translated
        // localisation that:
        // 1. have the same line ID as one in the base, but
        // 2. have a different Lock (the hash of the text), which
        //    indicates that the base text has changed.

        // First, get the list of IDs that are in both base and
        // translated, and then filter this list to any where the lock
        // values differ
        var outOfDateLockIDs = baseDictionary.Keys
            .Intersect(translatedDictionary.Keys)
            .Where(id => baseDictionary[id].Lock != translatedDictionary[id].Lock);

        // Now loop over all of these, and update our translated
        // dictionary to include a note that it needs attention
        foreach (var id in outOfDateLockIDs)
        {
            // Get the translated entry as it currently exists
            var entry = translatedDictionary[id];

            // Include a note that this entry is out of date
            entry.Text = $"(NEEDS UPDATE) {entry.Text}";

            // update the base language text
            entry.Original = baseDictionary[id].Text;
            // Update the lock to match the new one
            entry.Lock = baseDictionary[id].Lock;

            // Put this modified entry back in the table
            translatedDictionary[id] = entry;

            modificationsNeeded = true;
        }

        // We're all done!

        if (modificationsNeeded == false)
        {
            GenerateGodotTranslation(language, csvResourcePath);
            // No changes needed to be done to the translated string
            // table entries. Stop here.
            return false;
        }

        // We need to produce a replacement CSV file for the translated
        // entries.

        var outputStringEntries = translatedDictionary.Values
            .OrderBy(entry => entry.File)
            .ThenBy(entry => int.Parse(entry.LineNumber));

        string outputCSV = CreateCSV(outputStringEntries);

        // Write out the replacement text to this existing file,
        // replacing its existing contents
        File.WriteAllText(absoluteCSVPath, outputCSV, System.Text.Encoding.UTF8);
        var csvImport = $"{absoluteCSVPath}.import";
        if (!File.Exists(csvImport))
        {
            File.WriteAllText(csvImport, KEEP_IMPORT_TEXT);
        }

        GenerateGodotTranslation(language, csvResourcePath);
        // Signal that the file was changed
        return true;
    }

    public static string CreateCSV(IEnumerable<StringTableEntry> entries)
    {
        using (var textWriter = new System.IO.StringWriter())
        {
            // Generate the localised .csv file

            // Use the invariant culture when writing the CSV
            var csv = new CsvHelper.CsvWriter(
                textWriter, // write into this stream
                StringTableEntry.GetConfiguration() // use this configuration
            );

            var fieldNames = new[]
            {
                    "language",
                    "id",
                    "text",
                    "original",
                    "file",
                    "node",
                    "lineNumber",
                    "lock",
                    "comment"
                };

            foreach (var field in fieldNames)
            {
                csv.WriteField(field);
            }

            csv.NextRecord();

            foreach (var entry in entries)
            {
                var values = new[]
                {
                        entry.Language,
                        entry.ID,
                        entry.Text,
                        entry.Original,
                        entry.File,
                        entry.Node,
                        entry.LineNumber,
                        entry.Lock,
                        entry.Comment,
                    };
                foreach (var value in values)
                {
                    csv.WriteField(value);
                }

                csv.NextRecord();
            }

            return textWriter.ToString();
        }
    }

    private static void GenerateGodotTranslation(string language, string csvFilePath)
    {
        var absoluteCSVPath = ProjectSettings.GlobalizePath(csvFilePath);
        var translation = new Translation();
        translation.Locale = language;

        var csvText = File.ReadAllText(absoluteCSVPath);
        IEnumerable<StringTableEntry> stringEntries = ParseFromCSV(csvText);
        foreach (var entry in stringEntries)
        {
            translation.AddMessage(entry.ID, entry.Text);
        }

        var extensionRegex = new Regex(@".csv$");
        var translationPath = extensionRegex.Replace(absoluteCSVPath, ".translation");
        var translationResPath = ProjectSettings.LocalizePath(translationPath);
        ResourceSaver.Save(translationResPath, translation);
        GD.Print($"Wrote translation file for {language} to {translationResPath}.");
    }

    public static void CompileAllScripts(YarnProject project)
    {
        List<FunctionInfo> newFunctionList = new List<FunctionInfo>();
        var assetPath = project.ResourcePath;
        GD.Print($"Compiling all scripts in {assetPath}");

        project.ResourceName = Path.GetFileNameWithoutExtension(assetPath);
        //project.SearchForSourceScripts();

        if (!project.SourceScripts.Any())
        {
            GD.Print($"No .yarn files in directories below {project.ResourcePath}");
            return;
        }

        var library = new Library();
        ActionManager.ClearAllActions();
        ActionManager.AddActionsFromAssemblies();
        ActionManager.RegisterFunctions(library);
        var existingFunctions = project.ListOfFunctions ?? Array.Empty<FunctionInfo>();
        var pretedermined = predeterminedFunctions().ToArray();
        foreach (var func in pretedermined)
        {
            FunctionInfo existingFunc = null;
            foreach (var existing in existingFunctions)
            {
                if (existing.Name == func.Name)
                {
                    existingFunc = existing;
                    existingFunc.Parameters = func.Parameters;
                    existingFunc.ReturnType = func.ReturnType;
                    break;
                }
            }

            newFunctionList.Add(existingFunc ?? func);
        }

        IEnumerable<Diagnostic> errors;
        project.ProjectErrors = Array.Empty<YarnProjectError>();

        // We now now compile!
        var scriptAbsolutePaths = project.SourceScripts.ToList().Where(s => s != null)
            .Select(scriptResource => ProjectSettings.GlobalizePath(scriptResource)).ToList();
        // Store the compiled program
        byte[] compiledBytes = null;
        CompilationResult? compilationResult = GD.Load<CSharpScript>("res://addons/YarnSpinner-Godot/Runtime/Yarn/YarnSpinner.Compiler/CompilationResult.cs").New() as CompilationResult?;
        if (scriptAbsolutePaths.Count > 0)
        {
            var job = CompilationJob.CreateFromFiles(scriptAbsolutePaths);
            // job.VariableDeclarations = localDeclarations;
            job.CompilationType = CompilationJob.Type.FullCompilation;
            job.Library = library;
            compilationResult = Yarn.Compiler.Compiler.Compile(job);

            errors = compilationResult.Value.Diagnostics.Where(d =>
                d.Severity == Diagnostic.DiagnosticSeverity.Error);

            if (errors.Count() > 0)
            {
                var errorGroups = errors.GroupBy(e => e.FileName);
                foreach (var errorGroup in errorGroups)
                {
                    var errorMessages = errorGroup.Select(e => e.ToString());

                    foreach (var message in errorMessages)
                    {
                        GD.PushError($"Error compiling: {message}");
                    }
                }

                var projectErrors = errors.ToList().ConvertAll(e =>
                    new YarnProjectError
                    {
                        Context = e.Context,
                        Message = e.Message,
                        FileName = ProjectSettings.LocalizePath(e.FileName)
                    });
                project.ProjectErrors = projectErrors.ToArray();
                return;
            }

            if (compilationResult.Value.Program == null)
            {
                GD.PushError(
                    "public error: Failed to compile: resulting program was null, but compiler did not report errors.");
                return;
            }

            // Store _all_ declarations - both the ones in this
            // .yarnproject file, and the ones inside the .yarn files.

            // While we're here, filter out any declarations that begin with our
            // Yarn public prefix. These are synthesized variables that are
            // generated as a result of the compilation, and are not declared by
            // the user.

            var newDeclarations = new List<Declaration>() //localDeclarations
                .Concat(compilationResult.Value.Declarations)
                .Where(decl => !decl.Name.StartsWith("$Yarn.Internal."))
                .Where(decl => !(decl.Type is FunctionType))
                .Select(decl =>
                {
                    SerializedDeclaration existingDeclaration = null;
                    // try to re-use a declaration if one exists to avoid changing the .tres file so much
                    foreach (var existing in project.SerializedDeclarations)
                    {
                        if (existing.name == decl.Name)
                        {
                            existingDeclaration = existing;
                            break;
                        }
                    }

                    var serialized = existingDeclaration ?? new SerializedDeclaration();
                    serialized.SetDeclaration(decl);
                    return serialized;
                }).ToArray();
            project.SerializedDeclarations = newDeclarations;
            // Clear error messages from all scripts - they've all passed
            // compilation
            project.ProjectErrors = Array.Empty<YarnProjectError>();

            CreateYarnInternalLocalizationAssets(project, compilationResult.Value);

            using (var memoryStream = new MemoryStream())
            using (var outputStream = new CodedOutputStream(memoryStream))
            {
                // Serialize the compiled program to memory
                compilationResult.Value.Program.WriteTo(outputStream);
                outputStream.Flush();

                compiledBytes = memoryStream.ToArray();
            }
        }

        project.ListOfFunctions = newFunctionList.ToArray();
        project.CompiledYarnProgramBase64 = compiledBytes == null ? "" : Convert.ToBase64String(compiledBytes);

        ResourceSaver.Save(project.ResourcePath, project, ResourceSaver.SaverFlags.ReplaceSubresourcePaths);
    }

    private static Localization developmentLocalization;
    public static void CreateYarnInternalLocalizationAssets(YarnProject project,
            CompilationResult compilationResult)
    {
        // Will we need to create a default localization? This variable
        // will be set to false if any of the languages we've
        // configured in languagesToSourceAssets is the default
        // language.
        var shouldAddDefaultLocalization = true;
        if (project.LocaleCodeToCSVPath == null)
        {
            project.LocaleCodeToCSVPath = new Godot.Collections.Dictionary<string, string>();
        }


        if (shouldAddDefaultLocalization)
        {
            // We didn't add a localization for the default language.
            // Create one for it now.
            var stringTableEntries = GetStringTableEntries(project, compilationResult);

            developmentLocalization = project.baseLocalization ?? GD.Load<CSharpScript>("res://addons/YarnSpinner-Godot/Runtime/Localization.cs").New() as YarnSpinnerGodot.Localization;
            developmentLocalization.Clear();
            developmentLocalization.ResourceName = $"Default ({project.defaultLanguage})";
            developmentLocalization.LocaleCode = project.defaultLanguage;

            // Add these new lines to the development localisation's asset
            foreach (StringTableEntry entry in stringTableEntries)
            {
                developmentLocalization.AddLocalisedStringToAsset(entry.ID, entry);
            }

            project.baseLocalization = developmentLocalization;

            // Since this is the default language, also populate the line metadata.
            if (project.LineMetadata == null) { project.LineMetadata = GD.Load<CSharpScript>("res://addons/YarnSpinner-Godot/Runtime/LineMetadata.cs").New() as LineMetadata; }
            project.LineMetadata.Clear();
            project.LineMetadata.AddMetadata(LineMetadataTableEntriesFromCompilationResult(compilationResult));
        }
    }
    private static IEnumerable<LineMetadataTableEntry> LineMetadataTableEntriesFromCompilationResult(
           CompilationResult result)
    {
        return result.StringTable.Select(x =>
        {
            var meta = new LineMetadataTableEntry();
            meta.ID = x.Key;
            meta.File = ProjectSettings.LocalizePath(x.Value.fileName);
            meta.Node = x.Value.nodeName;
            meta.LineNumber = x.Value.lineNumber.ToString();
            meta.Metadata = RemoveLineIDFromMetadata(x.Value.metadata).ToArray();
            return meta;
        }).Where(x => x.Metadata.Length > 0);
    }
    private static IEnumerable<StringTableEntry> GetStringTableEntries(YarnProject project,
            CompilationResult result)
    {
        return result.StringTable.Select(x =>
        {
            StringTableEntry entry = new StringTableEntry();

            entry.ID = x.Key;
            entry.Language = project.defaultLanguage;
            entry.Text = x.Value.text;
            entry.File = ProjectSettings.LocalizePath(x.Value.fileName);
            entry.Node = x.Value.nodeName;
            entry.LineNumber = x.Value.lineNumber.ToString();
            entry.Lock = GetHashString(x.Value.text, 8);
            entry.Comment = GenerateCommentWithLineMetadata(x.Value.metadata);
            return entry;
        }
        );
    }

    private static byte[] GetHash(string inputString)
    {
        using (HashAlgorithm algorithm = SHA256.Create())
        {
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
    }

    /// <summary>
    /// Returns a string containing the hexadecimal representation of a
    /// SHA-256 hash of <paramref name="inputString"/>.
    /// </summary>
    /// <param name="inputString">The string to produce a hash
    /// for.</param>
    /// <param name="limitCharacters">The length of the string to
    /// return. The returned string will be at most <paramref
    /// name="limitCharacters"/> characters long. If this is set to -1,
    /// the entire string will be returned.</param>
    /// <returns>A string version of the hash.</returns>
    public static string GetHashString(string inputString, int limitCharacters = -1)
    {
        var sb = new StringBuilder();
        foreach (byte b in GetHash(inputString))
        {
            sb.Append(b.ToString("x2"));
        }

        if (limitCharacters == -1)
        {
            // Return the entire string
            return sb.ToString();
        }
        else
        {
            // Return a substring (or the entire string, if
            // limitCharacters is longer than the string)
            return sb.ToString(0, Mathf.Min(sb.Length, limitCharacters));
        }
    }

    private static string GenerateCommentWithLineMetadata(string[] metadata)
    {
        var cleanedMetadata = RemoveLineIDFromMetadata(metadata);

        if (cleanedMetadata.Count() == 0)
        {
            return string.Empty;
        }

        return $"Line metadata: {string.Join(" ", cleanedMetadata)}";
    }

    private static IEnumerable<string> RemoveLineIDFromMetadata(string[] metadata)
    {
        return metadata.Where(x => !x.StartsWith("line:"));
    }

    public const string YARN_PROJECT_PATHS_SETTING_KEY = "YarnSpinnerGodot/YarnProjectPaths";

    public static List<YarnProject> LoadAllYarnProjects()
    {
        var projects = new List<YarnProject>();
        CleanUpMovedOrDeletedProjects();
        var allProjects = (Godot.Collections.Array)ProjectSettings.GetSetting(YARN_PROJECT_PATHS_SETTING_KEY);
        foreach (var path in YarnManager.projects.Values)
        {
            //projects.Add(ResourceLoader.Load<YarnProject>(path.ToString()));
            projects.Add(path);
        }

        return projects;
    }


    public static void AddLineTagsToFilesInYarnProject(YarnProject project)
    {
        // First, gather all existing line tags across ALL yarn
        // projects, so that we don't accidentally overwrite an
        // existing one. Do this by finding all yarn scripts in all
        // yarn projects, and get the string tags inside them.

        var allYarnFiles =
            // get all yarn projects across the entire project
            LoadAllYarnProjects()
                // Get all of their source scripts, as a single sequence
                .SelectMany(i => i.SourceScripts)
                // Get the path for each asset
                // remove any nulls, in case any are found
                .Where(path => path != null);

#if YARNSPINNER_DEBUG
            var stopwatch = Stopwatch.StartNew();
#endif

        var library = new Library();
        ActionManager.ClearAllActions();
        ActionManager.RegisterFunctions(library);

        // Compile all of these, and get whatever existing string tags
        // they had. Do each in isolation so that we can continue even
        // if a file contains a parse error.
        var allExistingTags = allYarnFiles.SelectMany(path =>
        {
            // Compile this script in strings-only mode to get
            // string entries
            var compilationJob = CompilationJob.CreateFromFiles(ProjectSettings.GlobalizePath(path));
            compilationJob.CompilationType = CompilationJob.Type.StringsOnly;
            compilationJob.Library = library;
            var result = Yarn.Compiler.Compiler.Compile(compilationJob);

            bool containsErrors = result.Diagnostics
                .Any(d => d.Severity == Diagnostic.DiagnosticSeverity.Error);

            if (containsErrors)
            {
                GD.PrintErr($"Can't check for existing line tags in {path} because it contains errors.");
                return new string[]
                {
                };
            }

            return result.StringTable.Where(i => i.Value.isImplicitTag == false).Select(i => i.Key);
        }).ToList(); // immediately execute this query so we can determine timing information

#if YARNSPINNER_DEBUG
            stopwatch.Stop();
            GD.Print($"Checked {allYarnFiles.Count()} yarn files for line tags in {stopwatch.ElapsedMilliseconds}ms");
#endif

        var modifiedFiles = new List<string>();

        try
        {
            foreach (var script in project.SourceScripts)
            {
                var assetPath = ProjectSettings.GlobalizePath(script);
                var contents = File.ReadAllText(assetPath);

                // Produce a version of this file that contains line
                // tags added where they're needed.
                var taggedVersion = Yarn.Compiler.Utility.AddTagsToLines(contents, allExistingTags);

                // if the file has an error it returns null
                // we want to bail out then otherwise we'd wipe the yarn file
                if (taggedVersion == null)
                {
                    continue;
                }

                // If this produced a modified version of the file,
                // write it out and re-import it.
                if (contents != taggedVersion)
                {
                    modifiedFiles.Add(Path.GetFileNameWithoutExtension(assetPath));

                    File.WriteAllText(assetPath, taggedVersion, Encoding.UTF8);
                }
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Encountered an error when updating scripts: {e}");
            return;
        }

        // Report on the work we did.
        if (modifiedFiles.Count > 0)
        {
            GD.Print($"Updated the following files: {string.Join(", ", modifiedFiles)}");
        }
        else
        {
            GD.Print("No files needed updating.");
        }
    }

    private string GenerateUniqueDirectoryName()
    {
        var uniqueID = Guid.NewGuid().ToString();
        string uniqueFolderName = $"YarnProject_{uniqueID}";
        return uniqueFolderName;
    }


    private static List<FunctionInfo> predeterminedFunctions()
    {
        var functions = ActionManager.FunctionsInfo();

        List<FunctionInfo> f = new List<FunctionInfo>();
        foreach (var func in functions)
        {
            f.Add(CreateFunctionInfoFromMethodGroup(func));
        }

        return f;
    }

    public static FunctionInfo CreateFunctionInfoFromMethodGroup(MethodInfo method)
    {
        var returnType = $"-> {method.ReturnType.Name}";

        var parameters = method.GetParameters();
        var p = new string[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var q = parameters[i].ParameterType;
            p[i] = parameters[i].Name;
        }

        var info = new FunctionInfo();
        info.Name = method.Name;
        info.ReturnType = returnType;
        info.Parameters = p;
        return info;
    }

    private static void CleanUpMovedOrDeletedProjects()
    {
        if (ProjectSettings.HasSetting(YARN_PROJECT_PATHS_SETTING_KEY))
        {
            var projects = (Godot.Collections.Array)ProjectSettings.GetSetting(YARN_PROJECT_PATHS_SETTING_KEY);
            var removeProjects = new List<string>();
            foreach (var path in projects)
            {
                if (!File.Exists(ProjectSettings.GlobalizePath((string)path)))
                {
                    removeProjects.Add((string)path);
                }
            }

            var newProjects = new Godot.Collections.Array();
            foreach (var project in projects)
            {
                if (!removeProjects.Contains(project.ToString()))
                {
                    newProjects.Add(project);
                }
            }
            ProjectSettings.SetSetting(YARN_PROJECT_PATHS_SETTING_KEY, newProjects);
        }

    }
}
