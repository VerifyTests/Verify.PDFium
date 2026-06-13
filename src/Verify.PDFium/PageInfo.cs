class PageInfo
{
    /// <summary>Page width in PDF points (1/72 inch).</summary>
    public required double Width { get; init; }

    /// <summary>Page height in PDF points (1/72 inch).</summary>
    public required double Height { get; init; }

    /// <summary>Text extracted from the page, or null when the page has no text.</summary>
    public string? Text { get; init; }
}
