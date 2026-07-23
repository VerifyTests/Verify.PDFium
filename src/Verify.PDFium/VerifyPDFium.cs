namespace VerifyTests;

public static class VerifyPDFium
{
    static double dpi = 96;

    // Context key set by ExcludePdfDocument. When present the raw pdf is left out of the snapshot,
    // for producers (for example Aspose.Cells) that embed machine-specific system font bytes the
    // pdf can never be made byte-deterministic. The rendered pages and info file still verify.
    const string excludeDocumentKey = "VerifyPDFium.ExcludeDocument";

    // Context key set by SkipPdfNormalization. When present the pdf bytes are snapshotted as
    // produced, for producers that already emit byte-deterministic documents.
    const string skipNormalizationKey = "VerifyPDFium.SkipNormalization";

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

        VerifierSettings.RegisterStreamConverter("pdf", (_, target, context) => Convert(target, context));
    }

    /// <summary>
    /// Excludes the raw <c>.verified.pdf</c> from the snapshot for this verification, keeping only
    /// the rendered pages and the info file. Use it when the pdf bytes cannot be made deterministic
    /// (for example Aspose.Cells always embeds the machine's system fonts).
    /// </summary>
    public static SettingsTask ExcludePdfDocument(this SettingsTask settings)
    {
        settings.CurrentSettings.Context[excludeDocumentKey] = true;
        return settings;
    }

    /// <summary>
    /// Snapshots the pdf bytes exactly as produced, skipping the normalization that neutralizes the
    /// trailer <c>/ID</c>, the <c>/CreationDate</c> and <c>/ModDate</c>, and the XMP dates and
    /// identifiers. Use it when the producer already emits byte-deterministic documents, since
    /// normalizing them again copies the whole buffer, rescans it, and — when the XMP packet is
    /// canonicalized — rebuilds it and repairs the cross-reference table, all to change nothing.
    /// </summary>
    /// <remarks>
    /// Only skip this when the producer is genuinely deterministic. Without it a freshly generated
    /// pdf carries a wall-clock <c>/CreationDate</c> and a fresh <c>/ID</c>, so the snapshot differs
    /// on every run.
    /// <para>
    /// The XMP canonicalization is worth calling out because it is the pass that changes bytes for
    /// an already-deterministic producer: it collapses the packet's whitespace, so enabling or
    /// disabling this setting on an existing suite shifts the stored <c>.verified.pdf</c> even
    /// though nothing about the document changed. Expect to re-accept those snapshots once.
    /// </para>
    /// </remarks>
    public static SettingsTask SkipPdfNormalization(this SettingsTask settings)
    {
        settings.CurrentSettings.Context[skipNormalizationKey] = true;
        return settings;
    }

    static ConversionResult Convert(Stream stream, IReadOnlyDictionary<string, object> context)
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
                Properties = PdfProperties.Normalize(document.GetProperties())
            };
        }

        if (IncludeDocument(context))
        {
            if (Normalize(context))
            {
                // Neutralize the volatile fields for the pdf snapshot only once the document, which
                // reads lazily from the same buffer, has been released.
                bytes = PdfNormalizer.Normalize(bytes);
            }

            targets.Insert(
                0,
                new("pdf", new MemoryStream(bytes))
                {
                    BypassComparersForSubsequentOnDifference = true
                });
        }

        return new(info, targets);
    }

    static bool IncludeDocument(IReadOnlyDictionary<string, object> context) =>
        !context.TryGetValue(excludeDocumentKey, out var value) ||
        value is not true;

    static bool Normalize(IReadOnlyDictionary<string, object> context) =>
        !context.TryGetValue(skipNormalizationKey, out var value) ||
        value is not true;
}
