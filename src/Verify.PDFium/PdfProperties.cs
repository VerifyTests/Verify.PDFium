/// <summary>
/// Makes the pdfium-reported document property map deterministic. The byte-level neutralizing of the
/// document itself is done by <see cref="DeterministicPdf.PdfNormalizer"/>.
/// </summary>
static class PdfProperties
{
    // Projects the raw string properties to an object map, parsing the dates to DateTimeOffset so
    // Verify's built-in date scrubbing makes them deterministic. Non-dates stay as strings.
    public static Dictionary<string, object>? Normalize(Dictionary<string, string>? properties)
    {
        if (properties is null)
        {
            return null;
        }

        var result = new Dictionary<string, object>(properties.Count);
        foreach (var (key, value) in properties)
        {
            if (key is "CreationDate" or "ModDate" &&
                PdfDate.TryParse(value, out var date))
            {
                result[key] = date;
            }
            else
            {
                result[key] = value;
            }
        }

        return result;
    }
}
