if (args.Length < 2)
{
    Console.WriteLine("Usage: makem3u <directory> <mode>");
    Console.WriteLine("Modes:");
    Console.WriteLine("  1 - Include only the specified directory");
    Console.WriteLine("  2 - Include subdirectories, generate M3U per subfolder");
    Console.WriteLine("  3 - Include subdirectories, generate one M3U in specified folder with all files");
    Console.WriteLine("  4 - Include subdirectories, generate M3U per subfolder AND main M3U in specified folder");
    return;
}

var directory = args[0];
var mode = args[1];

if (!Directory.Exists(directory))
{
    Console.WriteLine($"Error: Directory '{directory}' does not exist.");
    return;
}

if (!int.TryParse(mode, out int modeInt) || modeInt < 1 || modeInt > 4)
{
    Console.WriteLine("Error: Mode must be 1, 2, 3, or 4.");
    return;
}

var directoryName = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
if (string.IsNullOrEmpty(directoryName))
{
    directoryName = Path.GetFileName(Path.GetDirectoryName(directory));
}
if (string.IsNullOrEmpty(directoryName))
{
    directoryName = "playlist";
}

switch (modeInt)
{
    case 1:
        GenerateM3UForDirectory(directory, directory);
        break;
    case 2:
        GenerateM3UForSubdirectories(directory, directoryName);
        break;
    case 3:
        GenerateM3UForAllSubdirectories(directory, directoryName);
        break;
    case 4:
        GenerateM3UForSubdirectories(directory, directoryName);
        GenerateM3UForAllSubdirectories(directory, directoryName);
        break;
}

static void GenerateM3UForDirectory(string targetDirectory, string outputDirectory, string? customFileName = null)
{
    var lines = new List<string>();
    var audioFiles = Directory.EnumerateFiles(targetDirectory)
        .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".flac", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".m4a", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
        .OrderBy(f => f)
        .ToList();

    foreach (var f in audioFiles)
    {
        try
        {
            var tagFile = TagLib.File.Create(f);
            var duration = Math.Round(tagFile.Properties.Duration.TotalSeconds);
            var title = tagFile.Tag.Title;
            if (string.IsNullOrEmpty(title))
            {
                title = Path.GetFileNameWithoutExtension(f);
            }
            var relativePath = Path.GetRelativePath(outputDirectory, f);
            lines.Add($"#EXTINF:{duration},{title}");
            lines.Add(relativePath.Replace('\\', '/'));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not read tags from '{f}': {ex.Message}");
            var relativePath = Path.GetRelativePath(outputDirectory, f);
            lines.Add($"#EXTINF:-1,{Path.GetFileNameWithoutExtension(f)}");
            lines.Add(relativePath.Replace('\\', '/'));
        }
    }

    if (lines.Count > 0)
    {
        lines.Insert(0, "#EXTM3U");
        string outputFileName;
        if (!string.IsNullOrEmpty(customFileName))
        {
            outputFileName = customFileName;
        }
        else
        {
            outputFileName = Path.GetFileName(targetDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (string.IsNullOrEmpty(outputFileName))
            {
                var dirName = Path.GetDirectoryName(targetDirectory);
                if (!string.IsNullOrEmpty(dirName))
                {
                    outputFileName = Path.GetFileName(dirName);
                }
            }
            if (string.IsNullOrEmpty(outputFileName))
            {
                outputFileName = "playlist";
            }
        }
        var outputPath = Path.Combine(outputDirectory, $"{outputFileName}.m3u");
        File.WriteAllText(outputPath, string.Join(Environment.NewLine, lines));
        Console.WriteLine($"Generated: {outputPath}");
    }
}

static void GenerateM3UForSubdirectories(string rootDirectory, string mainFolderName)
{
    var subdirectories = Directory.EnumerateDirectories(rootDirectory);
    foreach (var subdir in subdirectories)
    {
        var subfolderName = Path.GetFileName(subdir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrEmpty(subfolderName))
        {
            var dirName = Path.GetDirectoryName(subdir);
            if (!string.IsNullOrEmpty(dirName))
            {
                subfolderName = Path.GetFileName(dirName);
            }
        }
        if (string.IsNullOrEmpty(subfolderName))
        {
            subfolderName = "subfolder";
        }
        var playlistName = $"{mainFolderName} - {subfolderName}";
        GenerateM3UForDirectory(subdir, subdir, playlistName);
    }
}

static void GenerateM3UForAllSubdirectories(string rootDirectory, string outputFileName)
{
    var lines = new List<string>();
    var audioFiles = new List<string>();

    // Collect all audio files from root and subdirectories
    CollectAudioFiles(rootDirectory, rootDirectory, audioFiles);

    audioFiles = audioFiles.OrderBy(f => f).ToList();

    foreach (var f in audioFiles)
    {
        try
        {
            var tagFile = TagLib.File.Create(f);
            var duration = Math.Round(tagFile.Properties.Duration.TotalSeconds);
            var title = tagFile.Tag.Title;
            if (string.IsNullOrEmpty(title))
            {
                title = Path.GetFileNameWithoutExtension(f);
            }
            var relativePath = Path.GetRelativePath(rootDirectory, f);
            lines.Add($"#EXTINF:{duration},{title}");
            lines.Add(relativePath.Replace('\\', '/'));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not read tags from '{f}': {ex.Message}");
            var relativePath = Path.GetRelativePath(rootDirectory, f);
            lines.Add($"#EXTINF:-1,{Path.GetFileNameWithoutExtension(f)}");
            lines.Add(relativePath.Replace('\\', '/'));
        }
    }

    if (lines.Count > 0)
    {
        lines.Insert(0, "#EXTM3U");
        var outputPath = Path.Combine(rootDirectory, $"{outputFileName}.m3u");
        File.WriteAllText(outputPath, string.Join(Environment.NewLine, lines));
        Console.WriteLine($"Generated: {outputPath}");
    }
}

static void CollectAudioFiles(string rootDirectory, string currentDirectory, List<string> audioFiles)
{
    // Add files from current directory
    var files = Directory.EnumerateFiles(currentDirectory)
        .Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".flac", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".m4a", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ||
                    f.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase));
                   
    audioFiles.AddRange(files);

    // Recursively process subdirectories
    foreach (var subdir in Directory.EnumerateDirectories(currentDirectory))
    {
        CollectAudioFiles(rootDirectory, subdir, audioFiles);
    }
}