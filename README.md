# remarq

Converts markdown notes to html

Usage: `remarq.exe [source directory] [target directory] [path-to-html-templte]`

## Building

To build a single exe:

```
dotnet publish -c Release /p:PublishSingleFile=true -r win-x64 -o publish
```