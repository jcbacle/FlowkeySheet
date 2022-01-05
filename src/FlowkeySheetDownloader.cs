using System.Net;

namespace Flowkey
{
    internal class FlowkeySheetDownloader
    {
        private readonly FlowkeySheetParams flowkeyParams;

        public FlowkeySheetDownloader(FlowkeySheetParams flowkeyParams)
        {
            this.flowkeyParams = flowkeyParams;
        }
        
        internal bool Download(string imageDir)
        {
            Directory.CreateDirectory(imageDir);

            if (flowkeyParams.NoDownload)
            {
                Console.WriteLine("Skip downloading");
                return true;
            }                

            var nbImage = 0;
            var uriBuilder = new UriBuilder(flowkeyParams.Url);

            // check url
            var file = uriBuilder.Uri.Segments.Last();
            if (file != "0.png")
            {
                Console.WriteLine($"Expected file '0.png' actual '{file}' from url '{flowkeyParams.Url}'");
                return false;
            }

            using (var client = new WebClient())
            {
                var rootPath = string.Join("/", uriBuilder.Uri.Segments[..^1]);

                while (true)
                {
                    var fileName = $"{nbImage}.png";
                    uriBuilder.Path = $"{rootPath}/{fileName}";

                    try
                    {
                        client.DownloadFile(uriBuilder.Uri, Path.Combine(imageDir, fileName));
                    }
                    catch (WebException)
                    {
                        break;
                    }

                    nbImage++;
                }
            }

            Console.WriteLine($"{nbImage} images downloaded to '{imageDir}'");

            return true;
        }
    }
}
