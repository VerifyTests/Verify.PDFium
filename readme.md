# <img src="/src/icon.png" height="30px"> Verify.PDFium

[![Discussions](https://img.shields.io/badge/Verify-Discussions-yellow?svg=true&label=)](https://github.com/orgs/VerifyTests/discussions)
[![Build status](https://img.shields.io/appveyor/build/SimonCropp/verify-pdfium)](https://ci.appveyor.com/project/SimonCropp/verify-pdfium)
[![NuGet Status](https://img.shields.io/nuget/v/Verify.PDFium.svg)](https://www.nuget.org/packages/Verify.PDFium/)

Extends [Verify](https://github.com/VerifyTests/Verify) to allow verification of PDF documents via [PDFium](https://pdfium.googlesource.com/pdfium/).<!-- singleLineInclude: intro. path: /docs/intro.include.md -->

Verifying a `pdf` produces:

 * A `.verified.txt` with the page count, per-page size (in PDF points) and extracted text, and document information dictionary entries (Title, Author, Producer, dates, etc).
 * The pdf itself as `.verified.pdf`. This can be omitted with [`ExcludePdfDocument`](#exclude-the-pdf-document).
 * A PNG render of every page as `#page_0001.verified.png`, `#page_0002.verified.png`, etc.

The non-deterministic fields of the pdf (the trailer `/ID`, the `/CreationDate` and `/ModDate`, and the equivalent XMP metadata) are neutralized so the same source document produces a byte-identical `.verified.pdf` across runs. A producer that already emits deterministic bytes can skip that work with [`SkipPdfNormalization`](#skip-pdf-normalization).

Rendering is provided by [Morph.PDFium](https://github.com/Papyrine/Morph.PDFium), which wraps the prebuilt PDFium binaries from [pdfium-binaries](https://github.com/bblanchon/pdfium-binaries) (Windows, Linux, and macOS). Rendering is deterministic for a given Morph.PDFium version: the same input produces byte-identical PNGs on every machine and OS, and no image library dependency is added.

**See [Milestones](../../milestones?state=closed) for release notes.**


## Sponsors

### Entity Framework Extensions<!-- include: sponsors. path: /docs/sponsors.include.md -->

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=simoncropp&utm_medium=Verify.PDFium) is a major sponsor and is proud to contribute to the development this project.

[![Entity Framework Extensions](https://raw.githubusercontent.com/VerifyTests/Verify.PDFium/refs/heads/main/docs/zzz.png)](https://entityframework-extensions.net/?utm_source=simoncropp&utm_medium=Verify.PDFium)

### Developed using JetBrains IDEs

[![JetBrains logo.](https://raw.githubusercontent.com/VerifyTests/Verify.PDFium/main/docs/jetbrains.png)](https://jb.gg/OpenSourceSupport)<!-- endInclude -->


## NuGet

 * https://nuget.org/packages/Verify.PDFium


## Usage


### Enable Verify.PDFium

<!-- snippet: enable -->
<a id='snippet-enable'></a>
```cs
[ModuleInitializer]
public static void Initialize() =>
    VerifyPDFium.Initialize();
```
<sup><a href='/src/Tests/ModuleInitializer.cs#L3-L9' title='Snippet source file'>snippet source</a> | <a href='#snippet-enable' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

`Initialize` optionally takes the render resolution: `VerifyPDFium.Initialize(dpi: 150)`. The default 96 dpi renders an A4 page at 794 x 1123.


### Verify a file

<!-- snippet: VerifyPdf -->
<a id='snippet-VerifyPdf'></a>
```cs
[Test]
public Task VerifyPdf() =>
    VerifyFile("sample.pdf");
```
<sup><a href='/src/Tests/Samples.cs#L4-L10' title='Snippet source file'>snippet source</a> | <a href='#snippet-VerifyPdf' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Verify a Stream

<!-- snippet: VerifyPdfStream -->
<a id='snippet-VerifyPdfStream'></a>
```cs
[Test]
public Task VerifyPdfStream()
{
    var stream = new MemoryStream(File.ReadAllBytes("sample.pdf"));
    return Verify(stream, "pdf");
}
```
<sup><a href='/src/Tests/Samples.cs#L12-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-VerifyPdfStream' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Exclude the pdf document

Some pdf producers embed non-deterministic bytes that cannot be neutralized. For example [Aspose.Cells](https://products.aspose.com/cells/) always embeds the machine's system fonts (it has no way to restrict font resolution to a bundled set), so the pdf bytes differ from one machine to the next even for the same input. `ExcludePdfDocument` drops the `.verified.pdf` from the snapshot for that verification, while still verifying the deterministic rendered pages and info file:

<!-- snippet: ExcludePdfDocument -->
<a id='snippet-ExcludePdfDocument'></a>
```cs
[Test]
public Task ExcludePdfDocument() =>
    VerifyFile("sample.pdf")
        .ExcludePdfDocument();
```
<sup><a href='/src/Tests/Samples.cs#L27-L34' title='Snippet source file'>snippet source</a> | <a href='#snippet-ExcludePdfDocument' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Skip pdf normalization

By default the pdf bytes are normalized before being snapshotted: the trailer `/ID`, the `/CreationDate` and `/ModDate`, and the XMP dates and identifiers are neutralized, and the XMP packet is canonicalized. A producer that already emits byte-deterministic documents gains nothing from that, and pays for a full buffer copy, a rescan, and — when the XMP is canonicalized — a rebuild plus a cross-reference repair. `SkipPdfNormalization` snapshots the bytes exactly as produced:

<!-- snippet: SkipPdfNormalization -->
<a id='snippet-SkipPdfNormalization'></a>
```cs
[Test]
public Task SkipPdfNormalization() =>
    VerifyFile("sample.pdf")
        .SkipPdfNormalization();
```
<sup><a href='/src/Tests/Samples.cs#L36-L43' title='Snippet source file'>snippet source</a> | <a href='#snippet-SkipPdfNormalization' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Only skip it when the producer is genuinely deterministic. Without normalization a freshly generated pdf carries a wall-clock `/CreationDate` and a fresh `/ID`, so the snapshot differs on every run.

The XMP canonicalization is worth calling out, because it is the pass that changes bytes even for an already-deterministic producer: it collapses the packet's whitespace. Turning this setting on for an existing suite therefore shifts every stored `.verified.pdf` once, without anything about the documents having changed.


## Icon

[PDF](https://thenounproject.com/icon/pdf-7564953//) designed by [Meilia](https://thenounproject.com/creator/meilia1/) from [The Noun Project](https://thenounproject.com).
