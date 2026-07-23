# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Verify.PDFium is a [Verify](https://github.com/VerifyTests/Verify) plugin that converts PDF documents into snapshot-testable output: a metadata info file, the pdf binary, and a deterministic PNG render of every page. Rendering is provided by [Morph.PDFium](https://github.com/Papyrine/Morph.PDFium) (`Morph.PDFium.PdfiumDocument`), which wraps the [PDFium](https://pdfium.googlesource.com/pdfium/) C API over the [bblanchon.PDFium.*](https://github.com/bblanchon/pdfium-binaries) native binaries (Windows/Linux/macOS).

## Build & Test Commands

**Important:** run from the repo root (its `global.json` pins the SDK). The Morph repo's `global.json` pins the Microsoft.Testing.Platform runner, so running `dotnet test` from unrelated directories can pick up the wrong config.

```bash
# Build
dotnet build src --configuration Release

# Run all tests
dotnet test src/Tests/Tests.csproj

# Run a single test
dotnet test src/Tests/Tests.csproj --filter "FullyQualifiedName~Samples.VerifyPdf"
```

Tests use NUnit and target `net10.0`. The library targets `net10.0` (constrained by Morph.PDFium).

## Architecture

All source lives under `src/`. Solution file is `src/Verify.PDFium.slnx`.

### Library (`src/Verify.PDFium/`)

Entry point is `VerifyPDFium.Initialize(dpi = 96)` which registers a stream converter for the `pdf` extension. The converter loads the document with `Morph.PDFium.PdfiumDocument` and returns a `ConversionResult` containing:
1. `PdfInfo` (page count, per-page size in points and extracted text, document information dictionary) serialized as the info file
2. The pdf bytes as a `pdf` target (`BypassComparersForSubsequentOnDifference` set, mirroring Verify.OpenXml)
3. One `png` target per page, named `page_0001` style

To keep snapshots stable for PDFs freshly generated at test time, the non-deterministic fields are neutralized two ways:
- **In the `pdf` bytes** (`DeterministicPdf.PdfNormalizer.Normalize`): the trailer `/ID`, the info-dictionary `/CreationDate`/`/ModDate`, and the XMP dates plus `xmpMM:DocumentID`/`InstanceID`/`OriginalDocumentID` and `dc:date` are overwritten by an in-place byte scan (no string round-trip, no regex). That value-zeroing is length-preserving, so cross-reference offsets survive it. A final `CanonicalizeXmp` pass then collapses the XMP packet's whitespace — Apache FOP serializes the packet through the platform's XML writer, so indentation varies by JRE — and that pass is **not** length-preserving: it rebuilds the buffer and repairs both the metadata stream length and the classic cross-reference table. A document it cannot safely rewrite (cross-reference stream, incremental update, unlocatable stream length) comes back unchanged. `Normalize` copies its input, so a caller keeps ownership of the array passed in; the call is still made only after the `PdfiumDocument` (which reads lazily from the same buffer) is disposed. Values compressed away inside an `/ObjStm` or a flate-compressed XMP packet are not reachable by the plaintext scan and are left alone.
- **In the info file** (`PdfProperties.Normalize`): `Properties` is a `Dictionary<string, object>` whose `CreationDate`/`ModDate` values are parsed (`PdfDate`) into `DateTimeOffset`, so Verify's built-in date scrubbing renders them deterministically (`DateTimeOffset_1` etc.). Properties are read from the original bytes, before the byte-level pass zeroes them.

Both can be opted out of per verification, via a context key set by a `SettingsTask` extension and read in `Convert`:
- `ExcludePdfDocument` — drops the `.verified.pdf` entirely, for producers whose bytes can never be made deterministic (Aspose.Cells embeds the machine's system fonts).
- `SkipPdfNormalization` — keeps the `.verified.pdf` but snapshots the producer's own bytes, for producers that are already byte-deterministic. Worth knowing when changing this: `CanonicalizeXmp` is the pass that alters bytes even for such a producer, so toggling the setting on an existing suite shifts every stored `.verified.pdf` once.

Key files:
- **VerifyPDFium.cs** — initialization, the converter, and the two opt-out extensions
- **PdfProperties.cs** — projects the pdfium-reported property map to the scrubbable object map
- **PdfDate.cs** — parses PDF date strings (`D:YYYYMMDD…`) to `DateTimeOffset`
- **PdfInfo.cs** / **PageInfo.cs** — info shape for the snapshot (per-page width/height in points and text via `PdfPage.GetText()`)

The byte-level normalizer is **not** a type in this repo — it is `PdfNormalizer` from the [DeterministicPdf](https://github.com/SimonCropp/DeterministicPdf) package, which owns and tests the algorithm.

Style note: only public types get a namespace declaration (`VerifyTests`); internal types live in the global namespace.

### Tests (`src/Tests/`)

- **ModuleInitializer.cs** — calls `VerifyPDFium.Initialize()` via `[ModuleInitializer]` (also the readme `enable` snippet); PNGs compare via SSIM (`VerifierSettings.UseSsimForPng()`)
- **Samples.cs** — snapshot tests, also the readme usage snippets
- **PdfNormalizerTests.cs** — asserts the wiring rather than the algorithm: that a normalized document still loads in pdfium, that volatile values are neutralized, and that the pass is idempotent
- **SkipPdfNormalizationTests.cs** — asserts the opposite pairing, that the skipped snapshot holds the producer's own bytes while the default one holds the neutralized bytes. `SampleIsNotAlreadyNormalized` pins the premise, so the pair cannot both pass vacuously if `sample.pdf` ever becomes normalization-invariant
- Test assets `sample.pdf` (1 page) and `multi-page.pdf` (4 pages), both Letter size, were produced by [Morph](https://github.com/Papyrine/Morph)'s PDF exporter with embedded font subsets, so rendering is machine-independent.

Engine-level unit tests (load/render/page sizes/metadata/threading) live in the Morph.PDFium repo, not here.

## Dependency notes

- Morph.PDFium pins the PDFium native version; bumping it may shift rendered pixels — expect to regenerate `*.verified.png` files in the same commit.
- DeterministicPdf owns the byte-level normalization, so bumping it may shift `.verified.pdf` files across every consuming suite without any document having changed. The 1.3.0 release did exactly that by adding `CanonicalizeXmp`: downstream snapshots moved by the size of the collapsed XMP whitespace while the rendered pages and info files stayed identical. When a bump changes the `.verified.pdf` but leaves the pages and info alone, that is the expected shape of it — not a rendering regression.
- Morph.PDFium is a Papyrine package: `Papyrine_SponsorshipLicenseIgnored` is set in `src/Directory.Build.props` and `SC023` is in `NoWarn` (same arrangement as Verify.OpenXml's Morph dependency).

## Build Configuration

- `TreatWarningsAsErrors` and `EnforceCodeStyleInBuild` are enabled
- Central Package Management in `src/Directory.Packages.props`
