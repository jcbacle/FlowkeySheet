using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Flowkey
{
    internal class FlowkeySheetRenderer
    {
        private readonly FlowkeySheetParams flowkeyParams;

        public FlowkeySheetRenderer(FlowkeySheetParams flowkeyParams)
        {
            this.flowkeyParams = flowkeyParams;
        }

        public bool CreateMusicSheets(string imageDir, string sheetDir)
        {
            Directory.CreateDirectory(sheetDir);

            // 1. Load images
            var imageFiles = Directory.GetFiles(imageDir, "*.png").OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f)));
            if (!imageFiles.Any())
            {
                Console.WriteLine($"No image files found in {imageDir}");
                return false;
            }

            var images = imageFiles.Select(f => Image.Load(f)).ToArray();
            var maxWidth = images.Max(i => i.Width);
            var maxHeight = images.Max(i => i.Height);

            // 2. Compute music sheet width and height
            var rowSpacing = (int)(maxHeight * 0.5);
            var rowHeight = maxHeight + rowSpacing;
            var margin = (int)(flowkeyParams.NbMeasurePerRow * maxWidth * 0.075);
            var sheetWidth = (flowkeyParams.NbMeasurePerRow * maxWidth) + (2 * margin);
            var sheetHeight = (int)Math.Floor(sheetWidth * Math.Sqrt(2));       // A4 heigth/width ratio is equals to sqrt(2)
            var fontSize = (int)(sheetHeight * 0.012);

#if DEBUG
            var nbImage = 0;
            Console.WriteLine($"sheetHeight {sheetHeight} sheetWidth {sheetWidth}");
#endif

            // 3. Create music sheets
            var nbSheet = 1;
            var currentSheet = CreateFirstSheet(sheetWidth, sheetHeight, rowSpacing, margin, rowHeight, fontSize);
            int x = margin;
            int y = rowHeight;
            foreach (var image in images)
            {
                if (x + image.Width > sheetWidth)
                {
                    // merge image in a new row
                    x = margin;
                    y += rowHeight;
                }

                if (y + image.Height + rowSpacing > sheetHeight)
                {
                    // save current image and create a new image
                    SaveSheet(currentSheet, nbSheet, sheetDir);
                    nbSheet++;
                    currentSheet = CreateSheet(sheetWidth, sheetHeight, nbSheet, rowSpacing, margin, fontSize);
                    y = rowSpacing;
                }
#if DEBUG
                Console.WriteLine($"Draw image {nbImage++} to sheet {nbSheet} {x} {y}");
#endif

                currentSheet.Mutate(o => o.DrawImage(image, new Point(x, y), 1f));
                x += image.Width;
            }

            SaveSheet(currentSheet, nbSheet, sheetDir);

            Console.WriteLine($"{nbSheet} music sheets written to '{sheetDir}'");

            return true;
        }

        private Image CreateFirstSheet(int sheetWidth, int sheetHeight, int rowSpacing, int margin, int rowHeight, int fontSize)
        {
            var image = CreateSheet(sheetWidth, sheetHeight, 1, rowSpacing, margin, fontSize);

            // draw title
            image.DrawText(flowkeyParams.Title, fontSize, FontStyle.Bold, sheetWidth / 2, rowSpacing, HorizontalAlignment.Center, VerticalAlignment.Top);

            // draw author and level if set
            var lines = new List<string>();
            if (!string.IsNullOrEmpty(flowkeyParams.Author))
            {
                lines.Add($"{flowkeyParams.Author}");
            }
            if (!string.IsNullOrEmpty(flowkeyParams.Level))
            {
                lines.Add($"{flowkeyParams.Level}");
            }

            if (lines.Count != 0)
            {
                var text = string.Join(Environment.NewLine, lines);
                image.DrawText(text, fontSize, FontStyle.Regular, sheetWidth - margin, rowHeight - rowSpacing / 2, HorizontalAlignment.Right, VerticalAlignment.Bottom);
            }

            // draw bpm if set
            if (flowkeyParams.Bpm != null)
            {
                image.DrawText($"Bpm = {flowkeyParams.Bpm}", fontSize, FontStyle.Regular, margin, rowHeight - rowSpacing / 2, HorizontalAlignment.Left, VerticalAlignment.Bottom);
            }

            return image;
        }

        private static Image CreateSheet(int sheetWidth, int sheetHeight, int nbSheet, int rowSpacing, int margin, int fontSize)
        {
            var image = new Image<Rgba32>(sheetWidth, sheetHeight, Color.White);
            image.DrawText($"{nbSheet}", fontSize, FontStyle.Regular, sheetWidth - margin, sheetHeight - rowSpacing / 2, HorizontalAlignment.Right, VerticalAlignment.Bottom);
            return image;
        }

        private static void SaveSheet(Image currentSheet, int nbSheet, string destDir)
        {
            var fileName = Path.Combine(destDir, $"sheet_{nbSheet}.png");
            currentSheet.Save(fileName);
            currentSheet.Dispose();
        }
    }

    internal static class ImageSharpExtensions
    {
        private const string DefaultFont = "Arial";
        private static Color DefaultFontColor = Color.Black;

        public static void DrawText(this Image image, string text, float fontSize, FontStyle fontStyle, float x, float y, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            var textOptions = new DrawingOptions
            {
                TextOptions = new TextOptions
                {
                    HorizontalAlignment = horizontalAlignment,
                    VerticalAlignment = verticalAlignment,
                }
            };

            if (!SystemFonts.TryFind(DefaultFont, out var fontFamily))
            {
                Console.WriteLine($"Unable to find font '{DefaultFont}' to draw text");
                return;
            }

            var textFont = fontFamily.CreateFont(fontSize, fontStyle);

            image.Mutate(o => o.DrawText(textOptions, text, textFont, DefaultFontColor, new PointF(x, y)));
        }
    }
}
