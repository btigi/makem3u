# makemu3

A command-line app to create mu3 playlist files.

## Usage

makemu3 is a command line application and should be run from a terminal session. Application usage is

```
makem3u <directory> <mode>
Modes:
  1 - Include only the specified directory
  2 - Include subdirectories, generate M3U per subfolder
  3 - Include subdirectories, generate one M3U in specified folder with all files
  4 - Include subdirectories, generate M3U per subfolder AND main M3U in specified folder
```


Usage examples:

 ```makem3u.exe "C:\music\billie piper\Honey to the B" 1```

 ```makem3u.exe "C:\music\billie piper" 4```
 
## Requirements

- .NET 10.0
- Windows OS

## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/makemu3

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```

## Licencing

makemu3 is licensed under the MIT license. Full licence details are available in license.md