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
1. `PdfInfo` (page count, page sizes in points, document information dictionary) serialized as the info file
2. The original pdf bytes as a `pdf` target (`BypassComparersForSubsequentOnDifference` set, mirroring Verify.OpenXml)
3. One `png` target per page, named `page_0001` style

Key files:
- **VerifyPDFium.cs** — initialization and the converter
- **PdfInfo.cs** — info shape for the snapshot (uses `Morph.PDFium.PageSize`)

Style note: only public types get a namespace declaration (`VerifyTests`); internal types live in the global namespace.

### Tests (`src/Tests/`)

- **ModuleInitializer.cs** — calls `VerifyPDFium.Initialize()` via `[ModuleInitializer]` (also the readme `enable` snippet); PNGs compare via SSIM (`VerifierSettings.UseSsimForPng()`)
- **Samples.cs** — snapshot tests, also the readme usage snippets
- Test assets `sample.pdf` (1 page) and `multi-page.pdf` (4 pages), both Letter size, were produced by [Morph](https://github.com/Papyrine/Morph)'s PDF exporter with embedded font subsets, so rendering is machine-independent.

Engine-level unit tests (load/render/page sizes/metadata/threading) live in the Morph.PDFium repo, not here.

## Dependency notes

- Morph.PDFium pins the PDFium native version; bumping it may shift rendered pixels — expect to regenerate `*.verified.png` files in the same commit.
- Morph.PDFium is a Papyrine package: `Papyrine_SponsorshipLicenseIgnored` is set in `src/Directory.Build.props` and `SC023` is in `NoWarn` (same arrangement as Verify.OpenXml's Morph dependency).

## Build Configuration

- `TreatWarningsAsErrors` and `EnforceCodeStyleInBuild` are enabled
- Central Package Management in `src/Directory.Packages.props`
