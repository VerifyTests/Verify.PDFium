// The neutralizing algorithm itself is owned and tested by the DeterministicPdf package. What is
// worth asserting here is the wiring: that this package applies it, and that a normalized document
// is still loadable by pdfium.
public class PdfNormalizerTests
{
    [Test]
    public async Task NormalizedDocumentStillLoads()
    {
        var data = PdfNormalizer.Normalize(File.ReadAllBytes("sample.pdf"));

        using var document = PdfiumDocument.Load(data);
        await Assert.That(document.PageCount).IsEqualTo(1);
    }

    [Test]
    public async Task NeutralizesVolatileValues()
    {
        var data = PdfNormalizer.Normalize(File.ReadAllBytes("sample.pdf"));

        var text = Encoding.Latin1.GetString(data);
        using (Assert.Multiple())
        {
            await Assert.That(text).DoesNotMatch(@"/CreationDate\s*\(D:[1-9]");
            await Assert.That(text).DoesNotMatch(@"/ModDate\s*\(D:[1-9]");
        }
    }

    [Test]
    public async Task IsIdempotent()
    {
        var once = PdfNormalizer.Normalize(File.ReadAllBytes("sample.pdf"));
        var twice = PdfNormalizer.Normalize(once);

        await Assert.That(twice).IsEquivalentTo(once);
    }
}
