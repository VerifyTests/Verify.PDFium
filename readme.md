# <img src="/src/icon.png" height="30px"> Verify.PDFium

[![Discussions](https://img.shields.io/badge/Verify-Discussions-yellow?svg=true&label=)](https://github.com/orgs/VerifyTests/discussions)
[![Build status](https://github.com/VerifyTests/Verify.PDFium/actions/workflows/build.yml/badge.svg)](https://github.com/VerifyTests/Verify.PDFium/actions/workflows/build.yml)
[![NuGet Status](https://img.shields.io/nuget/v/Verify.PDFium.svg)](https://www.nuget.org/packages/Verify.PDFium/)

Extends [Verify](https://github.com/VerifyTests/Verify) to allow verification of PDF documents via [PDFium](https://pdfium.googlesource.com/pdfium/).<!-- singleLineInclude: intro. path: /docs/intro.include.md -->

Verifying a `pdf` produces:

 * A `.verified.txt` with the document information dictionary entries (Title, Author, Producer, dates, etc), the page count, and the size (in PDF points) and extracted text of each page.
 * The pdf itself as `.verified.pdf`. This can be omitted with [`ExcludeTargets`](#exclude-the-pdf-document).
 * A PNG render of every page as `#page_0001.verified.png`, `#page_0002.verified.png`, etc.

The page files are named, and the text placed, by Verify's [paged documents](https://github.com/VerifyTests/Verify/blob/main/docs/paged-documents.md) support, which every Verify plugin that splits a document into pages shares. So do the settings that [choose what is verified](#choosing-what-is-verified).

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


### Choosing what is verified

What a pdf is split into is controlled by Verify's settings for [paged documents](https://github.com/VerifyTests/Verify/blob/main/docs/paged-documents.md). Anything left out is not produced at all (pages are not rendered, text is not extracted), so these also save work.

The text of each page is in the info file by default. `PageText` moves it to a `#page_0001.verified.txt` per page, or leaves it out with `PageTextPlacement.None`:

<!-- snippet: PageTextPerPage -->
<a id='snippet-PageTextPerPage'></a>
```cs
[Test]
public Task PageTextPerPage() =>
    VerifyFile("sample.pdf")
        .PageText(PageTextPlacement.PerPage);
```
<sup><a href='/src/Tests/Samples.cs#L35-L42' title='Snippet source file'>snippet source</a> | <a href='#snippet-PageTextPerPage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

`PagesToInclude` limits the pages that are rendered and read, to the first pages of a document or to those a delegate accepts. The pdf itself is still verified whole:

<!-- snippet: PagesToInclude -->
<a id='snippet-PagesToInclude'></a>
```cs
[Test]
public Task PagesToInclude() =>
    VerifyFile(ProjectFiles.multi_page_pdf.Path)
        .PagesToInclude(2)
        .ExcludeDerivedTargets("png");
```
<sup><a href='/src/Tests/Samples.cs#L44-L52' title='Snippet source file'>snippet source</a> | <a href='#snippet-PagesToInclude' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

`ExcludeDerivedTargets("png")` leaves out the rendered pages, keeping the pdf and its text:

<!-- snippet: TextOnly -->
<a id='snippet-TextOnly'></a>
```cs
[Test]
public Task TextOnly() =>
    VerifyFile("sample.pdf")
        .ExcludeDerivedTargets("png");
```
<sup><a href='/src/Tests/Samples.cs#L54-L61' title='Snippet source file'>snippet source</a> | <a href='#snippet-TextOnly' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Each can also be set for every test, on `VerifierSettings`:

<!-- snippet: InitializeOutputs -->
<a id='snippet-InitializeOutputs'></a>
```cs
[ModuleInitializer]
public static void Initialize()
{
    VerifyPDFium.Initialize();

    // For every test: no text, so only the pages are verified
    VerifierSettings.PageText(PageTextPlacement.None);
}
```
<sup><a href='/src/StaticSettingsTests/ModuleInitializer.cs#L3-L14' title='Snippet source file'>snippet source</a> | <a href='#snippet-InitializeOutputs' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Verify a file

<!-- snippet: VerifyPdf -->
<a id='snippet-VerifyPdf'></a>
```cs
[Test]
public Task VerifyPdf() =>
    VerifyFile("sample.pdf");
```
<sup><a href='/src/Tests/Samples.cs#L3-L9' title='Snippet source file'>snippet source</a> | <a href='#snippet-VerifyPdf' title='Start of snippet'>anchor</a></sup>
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
<sup><a href='/src/Tests/Samples.cs#L11-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-VerifyPdfStream' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Exclude the pdf document

Some pdf producers embed non-deterministic bytes that cannot be neutralized. For example [Aspose.Cells](https://products.aspose.com/cells/) always embeds the machine's system fonts (it has no way to restrict font resolution to a bundled set), so the pdf bytes differ from one machine to the next even for the same input. Verify's `ExcludeTargets("pdf")` drops the `.verified.pdf` from the snapshot for that verification, while still verifying the deterministic rendered pages and info file. `VerifierSettings.ExcludeTargets("pdf")` does the same for every test:

<!-- snippet: ExcludePdfDocument -->
<a id='snippet-ExcludePdfDocument'></a>
```cs
[Test]
public Task ExcludePdfDocument() =>
    VerifyFile("sample.pdf")
        .ExcludeTargets("pdf");
```
<sup><a href='/src/Tests/Samples.cs#L26-L33' title='Snippet source file'>snippet source</a> | <a href='#snippet-ExcludePdfDocument' title='Start of snippet'>anchor</a></sup>
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
<sup><a href='/src/Tests/Samples.cs#L63-L70' title='Snippet source file'>snippet source</a> | <a href='#snippet-SkipPdfNormalization' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Only skip it when the producer is genuinely deterministic. Without normalization a freshly generated pdf carries a wall-clock `/CreationDate` and a fresh `/ID`, so the snapshot differs on every run.

The XMP canonicalization is worth calling out, because it is the pass that changes bytes even for an already-deterministic producer: it collapses the packet's whitespace. Turning this setting on for an existing suite therefore shifts every stored `.verified.pdf` once, without anything about the documents having changed.


## Reviewing changes

A change to a pdf is a change to several files: the pdf, its info file, and every page. Verify tells the diff tool that the pages and the info file were derived from the pdf, and [DiffEngineViewer](https://github.com/VerifyTests/DiffEngine/blob/main/docs/viewer.md#files-derived-from-a-document), which draws a pdf's pages itself, shows them as one row and accepts them together. Other diff tools are given each file, as before.


## Migrating from 1.x

Version 2 moves to the paged document support in Verify 33.3. The file names are unchanged. What differs:

| 1.x | 2.x |
| --- | --- |
| `Initialize(outputs: PdfiumOutputs.Png)` | `VerifierSettings.PageText(PageTextPlacement.None)` |
| `Initialize(outputs: PdfiumOutputs.Text)` | `VerifierSettings.ExcludeDerivedTargets("png")` |
| `Initialize(outputs: PdfiumOutputs.None)` | Both of the above |
| `.ExcludePdfDocument()` | `.ExcludeTargets("pdf")` |

The info file has the shape every paged document has: the document properties under `Document`, and each page as its `Number`, its size under `Info`, and its `Text`.

```
{
  PageCount: 1,                      {
  Properties: {                        Document: {
    Producer: PDFsharp 6.2.4             Producer: PDFsharp 6.2.4
  },                                   },
  Pages: [                             PageCount: 1,
    {                                  Pages: [
      Width: 612.0,                      {
      Height: 792.0,                       Number: 1,
      Text: Hello, World!                  Info: {
    }                                        Width: 612.0,
  ]                                          Height: 792.0
}                                          },
                                           Text: Hello, World!
                                         }
                                       ]
                                     }
```

So every `.verified.txt` changes once, and nothing else does.


## Icon

[PDF](https://thenounproject.com/icon/pdf-7564953//) designed by [Meilia](https://thenounproject.com/creator/meilia1/) from [The Noun Project](https://thenounproject.com).
