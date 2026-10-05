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
        VerifyFile(ProjectFiles.multi_page_pdf.Path);

    #region ExcludePdfDocument

    [Test]
    public Task ExcludePdfDocument() =>
        VerifyFile("sample.pdf")
            .ExcludeTargets("pdf");

    #endregion

    #region PageTextPerPage

    [Test]
    public Task PageTextPerPage() =>
        VerifyFile("sample.pdf")
            .PageText(PageTextPlacement.PerPage);

    #endregion

    #region PagesToInclude

    [Test]
    public Task PagesToInclude() =>
        VerifyFile(ProjectFiles.multi_page_pdf.Path)
            .PagesToInclude(2)
            .ExcludeDerivedTargets("png");

    #endregion

    #region TextOnly

    [Test]
    public Task TextOnly() =>
        VerifyFile("sample.pdf")
            .ExcludeDerivedTargets("png");

    #endregion

    #region SkipPdfNormalization

    [Test]
    public Task SkipPdfNormalization() =>
        VerifyFile("sample.pdf")
            .SkipPdfNormalization();

    #endregion
}
