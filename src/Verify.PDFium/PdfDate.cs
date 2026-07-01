/// <summary>
/// Parses a PDF date string (PDF 32000-1:2008 §7.9.4), for example <c>D:20240115093000+05'30'</c>,
/// into a <see cref="DateTimeOffset"/>. Every component after the year is optional.
/// </summary>
static class PdfDate
{
    public static bool TryParse(string value, out DateTimeOffset date)
    {
        date = default;

        var span = value.AsSpan();
        if (span.StartsWith("D:"))
        {
            span = span[2..];
        }

        if (!TryFixed(span, 0, 4, out var year) ||
            !TryOptional(span, 4, 1, out var month) ||
            !TryOptional(span, 6, 1, out var day) ||
            !TryOptional(span, 8, 0, out var hour) ||
            !TryOptional(span, 10, 0, out var minute) ||
            !TryOptional(span, 12, 0, out var second) ||
            !TryOffset(span, 14, out var offset))
        {
            return false;
        }

        try
        {
            date = new(year, month, day, hour, minute, second, offset);
            return true;
        }
        catch (ArgumentException)
        {
            // Out-of-range component (e.g. month 13) or offset.
            return false;
        }
    }

    // Parses a mandatory run of exactly length digits at start.
    static bool TryFixed(ReadOnlySpan<char> span, int start, int length, out int value)
    {
        value = 0;
        return span.Length >= start + length &&
               int.TryParse(span.Slice(start, length), out value);
    }

    // Parses an optional two-digit component; when the string ends before it, yields fallback.
    static bool TryOptional(ReadOnlySpan<char> span, int start, int fallback, out int value)
    {
        value = fallback;
        return span.Length <= start ||
               TryFixed(span, start, 2, out value);
    }

    static bool TryOffset(ReadOnlySpan<char> span, int start, out TimeSpan offset)
    {
        offset = TimeSpan.Zero;
        if (span.Length <= start)
        {
            return true;
        }

        var indicator = span[start];
        if (indicator is 'Z' or 'z')
        {
            return true;
        }

        if (indicator != '+' && indicator != '-')
        {
            return false;
        }

        if (!TryFixed(span, start + 1, 2, out var hours))
        {
            return false;
        }

        // Minutes follow the hours, separated by an apostrophe: HH'mm'.
        var minuteStart = start + 3;
        if (minuteStart < span.Length && span[minuteStart] == '\'')
        {
            minuteStart++;
        }

        var minutes = 0;
        if (minuteStart < span.Length &&
            !TryFixed(span, minuteStart, 2, out minutes))
        {
            return false;
        }

        offset = new(hours, minutes, 0);
        if (indicator == '-')
        {
            offset = -offset;
        }

        return true;
    }
}
