namespace VerifyTests;

public static class VerifyPDFium
{
    static double dpi = 96;

    // Context key set by SkipPdfNormalization. When present the pdf bytes are snapshotted as
    // produced, for producers that already emit byte-deterministic documents.
    const string skipNormalizationKey = "VerifyPDFium.SkipNormalization";

    // Context key set by StripPdfEmbeddedFonts, for a single verification. The stripEmbeddedFonts
    // argument of Initialize turns the same thing on for every one.
    const string stripEmbeddedFontsKey = "VerifyPDFium.StripEmbeddedFonts";

    static bool stripEmbeddedFonts;

    public static bool Initialized { get; private set; }

    /// <param name="dpi">
    /// Render resolution for the page images. The default 96 renders an A4 page at 794 x 1123.
    /// </param>
    /// <param name="stripEmbeddedFonts">
    /// Removes the embedded font programs from every snapshotted pdf. See
    /// <see cref="StripPdfEmbeddedFonts"/>, which does the same for a single verification.
    /// </param>
    public static void Initialize(double dpi = 96, bool stripEmbeddedFonts = false)
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
        VerifyPDFium.stripEmbeddedFonts = stripEmbeddedFonts;

        VerifierSettings.RegisterStreamConverter("pdf", (_, target, context) => Convert(target, context));
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

    /// <summary>
    /// Removes the embedded font programs from the snapshotted pdf, leaving each font named but not
    /// embedded. Use it when the producer embeds the machine's installed fonts, so that the same
    /// document renders to different pdf bytes on a machine with a different version of a font.
    /// </summary>
    /// <remarks>
    /// Only the <c>.verified.pdf</c> is affected. The pages are rendered, and the text read, from the
    /// document as it was produced, with its fonts.
    /// <para>
    /// The stored pdf is no longer a faithful copy: a viewer opening it substitutes fonts of its own.
    /// Turning this on for an existing suite changes every stored <c>.verified.pdf</c> that embeds a
    /// font, so expect to re-accept those once.
    /// </para>
    /// <para>
    /// It is part of the normalization, so it does nothing alongside
    /// <see cref="SkipPdfNormalization"/>.
    /// </para>
    /// </remarks>
    public static SettingsTask StripPdfEmbeddedFonts(this SettingsTask settings)
    {
        settings.CurrentSettings.Context[stripEmbeddedFontsKey] = true;
        return settings;
    }

    static ConversionResult Convert(Stream stream, IReadOnlyDictionary<string, object> context)
    {
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        var bytes = buffer.ToArray();

        // Names the pages, places their text, and says which pages and which of their outputs the
        // verification wants, so a page that is not wanted is neither rendered nor read
        var conversion = new PagedConversion(context);
        var includeImages = conversion.IncludeImages;
        var includeText = conversion.IncludeText;
        using (var document = PdfiumDocument.Load(bytes))
        {
            conversion.Info = PdfProperties.Normalize(document.GetProperties());
            foreach (var number in conversion.Pages(document.PageCount))
            {
                var index = number - 1;
                using var page = document.LoadPage(index);
                var size = page.Size;

                Stream? image = null;
                if (includeImages)
                {
                    image = new MemoryStream(document.RenderPage(index, dpi));
                }

                string? text = null;
                if (includeText)
                {
                    text = page.GetText();
                }

                conversion.AddPage(
                    number,
                    image,
                    text,
                    new PageSize
                    {
                        Width = size.Width,
                        Height = size.Height
                    });
            }
        }

        if (!context.IsTargetExcluded("pdf"))
        {
            if (Normalize(context))
            {
                // Neutralize the volatile fields for the pdf snapshot only once the document, which
                // reads lazily from the same buffer, has been released.
                bytes = PdfNormalizer.Normalize(bytes, StripEmbeddedFonts(context));
            }

            conversion.Source(new("pdf", new MemoryStream(bytes)));
        }

        return conversion.Build();
    }

    static bool Normalize(IReadOnlyDictionary<string, object> context) =>
        !context.TryGetValue(skipNormalizationKey, out var value) ||
        value is not true;

    static bool StripEmbeddedFonts(IReadOnlyDictionary<string, object> context) =>
        stripEmbeddedFonts ||
        context.TryGetValue(stripEmbeddedFontsKey, out var value) &&
        value is true;
}
