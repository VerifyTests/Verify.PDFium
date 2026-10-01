namespace VerifyTests;

/// <summary>
/// The kinds of output a pdf is split into. Passed to <see cref="VerifyPDFium.Initialize"/>.
/// The raw pdf target is not controlled by this; use <c>VerifierSettings.ExcludeTargets("pdf")</c>.
/// </summary>
[Flags]
public enum PdfiumOutputs
{
    /// <summary>No outputs. Only the source document and info are emitted.</summary>
    None = 0,

    /// <summary>A rendered png per page. When omitted pages are not rendered.</summary>
    Png = 1,

    /// <summary>The extracted text of each page in the info file. When omitted text is not extracted.</summary>
    Text = 2,

    /// <summary>All outputs. The default.</summary>
    All = Png | Text
}
