namespace Flowkey
{
    internal class Program
    {
        // TODO : test on Linux and MacOs
        private static int Main(string[] args)
        {
            // 1. Parse command line arguments
            var parameters = args.Parse();
            if (parameters == null)
            {
                return 1;
            }

            // 2. Init
            var destDir = Path.Combine(parameters.DestDir, parameters.Title);
            var imageDir = Path.Combine(destDir, "images");
            var sheetDir = Path.Combine(destDir, "sheets");
            var downloader = new FlowkeySheetDownloader(parameters);
            var renderer = new FlowkeySheetRenderer(parameters);

            // 3. Download images from flowkey
            if (!downloader.Download(imageDir))
            {
                Console.WriteLine("Exit: error when downloading images from flowkey url");
                return 1;
            }

            // 4. Arrange downloaded images into big image
            if (!renderer.CreateMusicSheets(imageDir, sheetDir))
            {
                Console.WriteLine("Exit: error when generating music sheets");
                return 1;
            }

            return 0;
        }
    }
}
