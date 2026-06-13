class PdfInfo
{
    public required int PageCount { get; init; }
    public required IReadOnlyList<PageInfo> Pages { get; init; }
    public Dictionary<string, string>? Properties { get; init; }
}
