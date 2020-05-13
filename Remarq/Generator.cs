using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Remarq
{
    public class Generator
    {
        private readonly DirectoryInfo _sourceDirectory;
        private readonly DirectoryInfo _destDirectory;
        private NoteConverter _noteConverter;

        public Generator(string sourcePath, string destPath, string templateHtmlFilePath)
        {
            if (!Directory.Exists(sourcePath))
            {
                throw new ArgumentException($"Source path {_sourceDirectory} does not exist");
            }
            _sourceDirectory = new DirectoryInfo(sourcePath);
            if (!destPath.EndsWith(Path.DirectorySeparatorChar))
            {
                destPath = destPath + Path.DirectorySeparatorChar;
            }
            _destDirectory = new DirectoryInfo(destPath);
            _noteConverter = new NoteConverter(templateHtmlFilePath);
        }

        public async Task<int> Generate()
        {
            var sourceFiles = ReadSourceFiles();
            if (_destDirectory.Exists)
            {
                _destDirectory.Delete(true);
            }
            var tasks = BuildTasks(sourceFiles);
            await Task.WhenAll(tasks);
            return sourceFiles.Count;
        }

        private List<Task> BuildTasks(List<FileInfo> sourceFiles)
        {
            var tasks = new List<Task>();
            var markdownFiles = sourceFiles.Where(f => f.Extension == ".md").ToList();
            var otherFiles = sourceFiles.Except(markdownFiles).ToList();

            tasks.AddRange(otherFiles.Select(FileCopyTask));
            tasks.AddRange(markdownFiles.Select(MarkdownFileTask));
            
            return tasks;
        }

        private Task MarkdownFileTask(FileInfo sourceFile)
        {
           return Task.Run(() =>
           {
               var markdownSource = File.ReadAllText(sourceFile.FullName);
               var html = _noteConverter.Convert(markdownSource, sourceFile.Name);
               var destPath = SourceFileInfoToDestFileInfo(sourceFile, ".html");
               if (!destPath.Directory.Exists)
               {
                   Directory.CreateDirectory(destPath.Directory.FullName);
               }
               File.WriteAllText(destPath.FullName, html);
           });
        }

        private Task FileCopyTask(FileInfo sourceFile)
        {
            return Task.Run(() =>
            {
                var destPath = SourceFileInfoToDestFileInfo(sourceFile, null);
                if (!destPath.Directory.Exists)
                {
                    Directory.CreateDirectory(destPath.Directory.FullName);
                }
                sourceFile.CopyTo(destPath.FullName, true);
            });
        }

        private List<FileInfo> ReadSourceFiles()
        {
            var paths = Directory.GetFileSystemEntries(_sourceDirectory.FullName, "*", SearchOption.AllDirectories);
            return paths.Select(p => new FileInfo(p))
                .Where(f => f.Attributes.HasFlag(FileAttributes.Directory) == false)
                .ToList();
        }

        private FileInfo SourceFileInfoToDestFileInfo(FileInfo sourceFile, string newExtension)
        {
            var destPath = sourceFile.FullName.Replace(_sourceDirectory.FullName, _destDirectory.FullName);
            if(newExtension != null)
            {
                destPath = Path.ChangeExtension(destPath, newExtension);
            }
            return new FileInfo(destPath);
        }
    }
}
