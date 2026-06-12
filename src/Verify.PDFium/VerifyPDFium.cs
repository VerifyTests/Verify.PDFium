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

        using var document = PdfiumDocument.Load(bytes);

        var info = new PdfInfo
        {
            PageCount = document.PageCount,
            Pages = document.GetPageSizes(),
            Properties = document.GetProperties()
        };

        List<Target> targets =
        [
            new("pdf", new MemoryStream(bytes))
            {
                BypassComparersForSubsequentOnDifference = true
            }
        ];

        for (var index = 0; index < document.PageCount; index++)
        {
            var png = document.RenderPage(index, dpi);
            targets.Add(new("png", new MemoryStream(png), $"page_{index + 1:0000}"));
        }

        return new(info, targets);
    }
}
