public class OutputsTests
{
    [Test]
    public Task PngOnly() =>
        VerifyFile(ProjectFiles.sample_pdf.Path);
}
