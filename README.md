ResourceExtractorDotNet
=======================


Summary
-------

ResourceExtractorDotNet is a command-line tool for extracting embedded resources
from a .NET assembly and writing them to disk.  For embedded resources which are
compiled .NET resources, the files will be extracted as .resx (XML format
resource) files.  Any other resources (e.g. JPEG images, WAV audio) will be
extracted as raw binaries.


Usage
-----

`ResourceExtractorDotNet.exe <assembly> <output directory>`

ResourceExtractorDotNet requires two command-line parameters.

The first parameter is the .NET assembly to extract the embedded resources from.
Wildcards are currently not supported.

The second parameter is the output directory to extract the embedded resources
to.

**Both parameters are required.**

For example, the following invocation will
attempt to extract embedded resources from the assembly `TestAssembly.dll` into
the directory `.\resources`:

`ResourceExtractorDotNet.exe TestAssembly.dll .\resources`

If the directory `.\resources` does not exist, it will be automatically created.
If the directory `.\resources` already exists, any existing files in the
directory will be overwritten without warning.

For example, if the .NET assembly `TestAssembly.dll` contained the following
embedded resources:

* `Audio.wav`
* `Image.jpg`
* `Strings.resources`

Then the output would be:

* `.\resources\Audio.wav`
* `.\resources\Image.jpg`
* `.\resources\Strings.resx` *(NOTE: The compiled .NET resource was extracted
  as a XML-format resource file.)*


Compiling
---------

This tool was developed using the [Atom text editor](https://atom.io) and
[Microsoft Build Tools 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48159) on
Windows 10.  After installing the tools, open a Developer Command Prompt,
change directory to the source location and invoke `MSBuild`.  Assuming the tool
successfully builds, the binary will be written to
`.\build\bin\ResourceExtractorDotNet.exe`.


Troubleshooting at runtime
--------------------------

To help in troubleshooting, tracing can be enabled via the application config
file (`ResourceExtractorDotNet.exe.config`).  Open the config file in a text
editor and uncomment the `<listeners/>` to enable additional logging to the
console or to a text file.
