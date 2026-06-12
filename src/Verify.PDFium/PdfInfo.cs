class PdfInfo
{
    public required int PageCount { get; init; }
    /// <summary>Page dimensions in PDF points (1/72 inch).</summary>
    public required IReadOnlyList<PageSize> Pages { get; init; }
    public Dictionary<string, string>? Properties { get; init; }
}
