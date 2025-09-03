# remarq

Converts markdown notes to html.


# Running Locally

```
dotnet run --project Remarq/Remarq.csproj -- sample/source test_output sample/template.html
```


## Packaging an EXE

To build a single exe:

```
dotnet publish -c Release /p:PublishSingleFile=true -r win-x64 -o publish
```

Usage: `remarq.exe [source directory] [target directory] [path-to-html-templte]`