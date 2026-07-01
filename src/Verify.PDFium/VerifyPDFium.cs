namespace VerifyTests;

public static class VerifyPDFium
{
    static double dpi = 96;

    public static bool Initialized { get; private set; }

    /// <param name="dpi">
    /// Render resolution for the page images. The default 96 renders an A4 page at 794 x 1123.
    /// </param>
    public static void Initialize(double dpi = 96)
    {
        if (Initialized)
        {
            throw new("Already Initialized");
        }

        if (dpi <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dpi), dpi, "dpi must be positive");
        }

        Initialized = true;
        VerifyPDFium.dpi = dpi;

        VerifierSettings.RegisterStreamConverter("pdf", (_, target, _) => Convert(target));
    }

    static ConversionResult Convert(Stream stream)
    {
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        var bytes = buffer.ToArray();

        List<Target> targets = [];
        PdfInfo info;
        using (var document = PdfiumDocument.Load(bytes))
        {
            var pageCount = document.PageCount;
            var pages = new List<PageInfo>(pageCount);
            for (var index = 0; index < pageCount; index++)
            {
                using var page = document.LoadPage(index);
                var size = page.Size;
                pages.Add(
                    new()
                    {
                        Width = size.Width,
                        Height = size.Height,
                        Text = page.GetText()
                    });

                var png = document.RenderPage(index, dpi);
                targets.Add(new("png", new MemoryStream(png), $"page_{index + 1:0000}"));
            }

            info = new()
            {
                PageCount = pageCount,
                Pages = pages,
                Properties = PdfNormalizer.NormalizeProperties(document.GetProperties())
            };
        }

        // Neutralize the volatile fields for the pdf snapshot only once the document, which reads
        // lazily from the same buffer, has been released.
        PdfNormalizer.Normalize(bytes);
        targets.Insert(
            0,
            new("pdf", new MemoryStream(bytes))
            {
                BypassComparersForSubsequentOnDifference = true
            });

        return new(info, targets);
    }
}
