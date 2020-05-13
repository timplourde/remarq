using System;
using System.Threading.Tasks;

namespace Remarq
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if(args.Length < 3)
            {
                Console.WriteLine("Usage: sourceDir targetDir htmlTemplate");
                return 1;
            }
            var source = args[0];
            var target = args[1];
            var template = args[2];
            var generator = new Generator(source, target, template);
            var result = await generator.Generate();
            Console.WriteLine($"Wrote {result} files from {source} to {target}");
            return 0;
        }
    }
}
