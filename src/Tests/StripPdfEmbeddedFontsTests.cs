// The Samples entry for StripPdfEmbeddedFonts stores the stripped document. These assert the wiring
// around it: that there is a font to strip, and that only the stored pdf loses it.
public class StripPdfEmbeddedFontsTests
{
    [Test]
    public async Task SampleEmbedsAFont()
    {
        // The premise the test below rests on. If sample.pdf embedded no font the setting would have
        // nothing to remove, and a snapshot of it would prove nothing.
        var raw = await File.ReadAllBytesAsync(ProjectFiles.sample_pdf);

        await Assert.That(Encoding.Latin1.GetString(raw)).Contains("/FontFile");
        await Assert.That(Encoding.Latin1.GetString(PdfNormalizer.Normalize(raw, stripEmbeddedFonts: true))).DoesNotContain("/FontFile");
    }

    [Test]
    public async Task StrippedDocumentStillLoads()
    {
        var raw = await File.ReadAllBytesAsync(ProjectFiles.sample_pdf);
        var stripped = PdfNormalizer.Normalize(raw, stripEmbeddedFonts: true);

        using var document = PdfiumDocument.Load(stripped);
        await Assert.That(document.PageCount).IsEqualTo(1);
    }

    // The page image and the text come from the document as produced, so they match the ones
    // Samples.VerifyPdf stores for the same file: stripping changes the stored pdf and nothing else.
    [Test]
    public Task PagesAreRenderedWithTheirFonts() =>
        Verify(new MemoryStream(File.ReadAllBytes(ProjectFiles.sample_pdf)), "pdf")
            .StripPdfEmbeddedFonts();
}
