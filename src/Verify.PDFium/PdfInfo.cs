class PdfInfo
{
    public required int PageCount { get; init; }
    public Dictionary<string, object>? Properties { get; init; }
    public required IReadOnlyList<PageInfo> Pages { get; init; }
}
