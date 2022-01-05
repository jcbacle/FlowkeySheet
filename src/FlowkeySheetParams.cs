namespace Flowkey
{
    internal sealed class FlowkeySheetParams
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public int NbMeasurePerRow { get; set; } = 3;
        public string DestDir { get; set; } = Path.Combine(Path.GetTempPath(), "flowkey");
        public string? Author { get; set; }
        public string? Level { get; set; }
        public int? Bpm { get; set; }
        public bool NoDownload { get; internal set; }
    }

    internal static class FlowkeySheetParamsParser
    {
        internal static FlowkeySheetParams? Parse(this string[] args)
        {
            var parameters = new FlowkeySheetParams();


            if (args.Length == 0 || args.HasOption("-h") || args.HasOption("--help") || args.HasOption("/?"))
            {
                DisplayHelp();
                return null;
            }

            var url = args.ParseArgument("--url");
            if (string.IsNullOrEmpty(url))
            {
                Console.WriteLine("-url is mandatory");
                DisplayHelp();
                return null;
            }
            parameters.Url = url;

            var title = args.ParseArgument("--title");
            if (string.IsNullOrEmpty(title))
            {
                Console.WriteLine("-title is mandatory");
                DisplayHelp();
                return null;
            }
            parameters.Title = title;

            var measure = args.ParseArgument("--measure");
            if (int.TryParse(measure, out var nbMeasure))
            {
                parameters.NbMeasurePerRow = nbMeasure;
            }

            var destDir = args.ParseArgument("--dest");
            if (!string.IsNullOrEmpty(destDir))
            {
                parameters.DestDir = destDir;
            }

            parameters.Author = args.ParseArgument("--author");
            parameters.Level = args.ParseArgument("--level");
            parameters.NoDownload = args.HasOption("--nodl");
            parameters.Bpm = int.TryParse(args.ParseArgument("--bpm"), out var bpm) ? bpm : null;

            DisplayParameters(parameters);

            return parameters;
        }

        private static void DisplayHelp()
        {
            var defaultParameters = new FlowkeySheetParams();

            Console.WriteLine($"Usage:");
            Console.WriteLine($"  flowkey [options]");
            Console.WriteLine($"");
            Console.WriteLine($"Options:");
            Console.WriteLine($"  --url       Url of image, mandatory");
            Console.WriteLine($"  --title     Title, mandatory");
            Console.WriteLine($"  --measure   Nb measures per row, default to '{defaultParameters.NbMeasurePerRow}' if not set");
            Console.WriteLine($"  --dest      Destination directory, default to '{defaultParameters.DestDir}' if not set");
            Console.WriteLine($"  --author    Author, optional");
            Console.WriteLine($"  --level     Level, optional");
            Console.WriteLine($"  --bpm       Bpm, optional");
            Console.WriteLine($"  --nodl      skip downloading, default to '{defaultParameters.NoDownload}' if not set");
            Console.WriteLine();
        }

        private static void DisplayParameters(FlowkeySheetParams parameters)
        {
            Console.WriteLine($"Title:             {parameters.Title}");
            Console.WriteLine($"Author:            {parameters.Author}");
            Console.WriteLine($"Level:             {parameters.Level}");
            Console.WriteLine($"Bpm:               {parameters.Bpm}");
            Console.WriteLine($"Url:               {parameters.Url}");
            Console.WriteLine($"Dest:              {parameters.DestDir}");
            Console.WriteLine($"NbMeasurePerRow:   {parameters.NbMeasurePerRow}");
            Console.WriteLine($"NoDownload:        {parameters.NoDownload}");
            Console.WriteLine();
        }

        private static string? ParseArgument(this string[] args, string option) => args.SkipWhile(a => a != option).Skip(1).Take(1).FirstOrDefault();

        private static bool HasOption(this string[] args, string option) => args.Contains(option);
    }
}
