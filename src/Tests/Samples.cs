[TestFixture]
public class Samples
{
    #region VerifyPdf

    [Test]
    public Task VerifyPdf() =>
        VerifyFile("sample.pdf");

    #endregion

    #region VerifyPdfStream

    [Test]
    public Task VerifyPdfStream()
    {
        var stream = new MemoryStream(File.ReadAllBytes("sample.pdf"));
        return Verify(stream, "pdf");
    }

    #endregion

    [Test]
    public Task MultiPage() =>
        VerifyFile("multi-page.pdf");

    #region ExcludePdfDocument

    [Test]
    public Task ExcludePdfDocument() =>
        VerifyFile("sample.pdf")
            .ExcludePdfDocument();

    #endregion
}
