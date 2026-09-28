public class PdfDateTests
{
    [Test]
    public async Task ParsesFullDateWithPositiveOffset()
    {
        await Assert.That(PdfDate.TryParse("D:20240115093000+05'30'", out var date)).IsTrue();
        await Assert.That(date).IsEqualTo(new DateTimeOffset(2024, 1, 15, 9, 30, 0, new(5, 30, 0)));
    }

    [Test]
    public async Task ParsesUtcOffset()
    {
        await Assert.That(PdfDate.TryParse("D:20211105091500Z", out var date)).IsTrue();
        await Assert.That(date).IsEqualTo(new DateTimeOffset(2021, 11, 5, 9, 15, 0, TimeSpan.Zero));
    }

    [Test]
    public async Task ParsesNegativeOffset()
    {
        await Assert.That(PdfDate.TryParse("D:19991231235959-08'00'", out var date)).IsTrue();
        await Assert.That(date).IsEqualTo(new DateTimeOffset(1999, 12, 31, 23, 59, 59, new(-8, 0, 0)));
    }

    [Test]
    public async Task DefaultsOmittedComponents()
    {
        await Assert.That(PdfDate.TryParse("D:2024", out var date)).IsTrue();
        await Assert.That(date).IsEqualTo(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    [Test]
    public async Task RejectsNonDate() =>
        await Assert.That(PdfDate.TryParse("not a date", out _)).IsFalse();

    [Test]
    public async Task RejectsOutOfRangeComponents() =>
        await Assert.That(PdfDate.TryParse("D:20241350000000Z", out _)).IsFalse();
}
