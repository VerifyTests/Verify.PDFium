public class OutputsTests
{
    [Test]
    public Task PngOnly() =>
        VerifyFile("sample.pdf");
}
