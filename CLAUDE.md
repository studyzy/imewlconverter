# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

IME WL Converter (深蓝词库转换) is a cross-platform dictionary converter for Input Method Editors (IMEs). It converts dictionary files between 20+ different IME formats including Sougou Pinyin, QQ Pinyin, Rime, Google Pinyin, Baidu Pinyin, and many others.

**Language**: C# (.NET 10.0)
**License**: GPL-3.0
**Platforms**: Windows, Linux, macOS

## Common Commands

### Build Commands

```bash
# Restore NuGet packages
make restore

# Build all projects (Debug mode)
make build

# Build Release configuration
make build-release

# Build command-line tool only
make build-cmd

# Build macOS GUI version
make build-mac
```

### Testing

```bash
# Run unit tests
make test

# Run unit tests with verbose output
make test-verbose

# Run integration tests (requires CLI built first)
make integration-test

# Run integration tests with debug output
cd tests/integration
./run-tests.sh --all -v
```

### Running the Application

```bash
# Run CLI tool
make run-cmd

# Run macOS GUI
make run-mac

# Run CLI directly with dotnet
dotnet run --project src/ImeWlConverterCmd
```

### CLI Usage Examples

```bash
# Convert Sougou .scel to Google Pinyin text format
dotnet run --project src/ImeWlConverterCmd -- \
  -i:scel input.scel \
  -o:ggpy output.txt

# Convert multiple files
dotnet run --project src/ImeWlConverterCmd -- \
  -i:scel ./test/*.scel \
  -o:ggpy ./output/*

# Apply filters (length, rank, remove English/numbers)
dotnet run --project src/ImeWlConverterCmd -- \
  -i:scel input.scel \
  -o:ggpy output.txt \
  -ft:"len:1-100,rank:2-9999,rm:eng,rm:num"
```

### Code Quality

```bash
# Format code
make format

# Check code formatting (CI)
make lint
```

### Publishing and Packaging

```bash
# Publish macOS versions (ARM64 + x64)
make publish-mac

# Create macOS .app bundles
make app-mac

# Create distributable .zip packages
make package-mac
```

## High-Level Architecture

### Core Components

The codebase follows a plugin-style architecture with clear separation of concerns:

1. **ImeWlConverterCore** - Core library containing all conversion logic
2. **ImeWlConverterCmd** - Command-line interface
3. **ImeWlConverterMac** - macOS GUI application (Avalonia UI)
4. **IME WL Converter Win** - Windows GUI application (WinForms/WPF)
5. **ImeWlConverterCoreTest** - Unit tests

### Conversion Pipeline

The conversion process follows a pipeline architecture:

```
Input File → Import → Filters → Code Generation → Export → Output File
```

**Key Classes:**

- `MainBody` - Orchestrates the entire conversion pipeline
- `ConsoleRun` - Handles CLI argument parsing and execution
- `IWordLibraryImport` - Interface for all input format parsers
- `IWordLibraryExport` - Interface for all output format generators
- `WordLibrary` - Core data structure representing a dictionary entry (word + code + rank)
- `WordLibraryList` - Collection of WordLibrary entries

### Directory Structure

```
src/ImeWlConverterCore/
├── IME/              # Input format parsers (20+ formats)
│   ├── SougouPinyin.cs
│   ├── QQPinyin.cs
│   ├── Rime.cs
│   └── ...
├── Generaters/       # Output format generators
│   ├── PinyinGenerater.cs
│   ├── Wubi86Generater.cs
│   └── ...
├── Filters/          # Word filtering logic
│   ├── LengthFilter.cs
│   ├── RankFilter.cs
│   ├── EnglishFilter.cs
│   └── ...
├── Helpers/          # Utility functions
│   ├── PinyinHelper.cs
│   ├── BinFileHelper.cs
│   └── ...
├── Entities/         # Data models (WordLibrary, CodeType, etc.)
├── MainBody.cs       # Core orchestration logic
└── ConsoleRun.cs     # CLI argument handling
```

### Adding New Format Support

To add support for a new IME format:

1. **For Import**: Create a new class in `src/ImeWlConverterCore/IME/` that implements `IWordLibraryImport`
   - Inherit from `BaseImport` or `BaseTextImport` for common functionality
   - Implement `Import(string path)` to parse the file format
   - Add the format to the import registry in `ConsoleRun.LoadImeList()`

2. **For Export**: Create a new class that implements `IWordLibraryExport`
   - Implement `Export(WordLibraryList)` to generate output
   - Implement `ExportLine(WordLibrary)` for line-by-line export
   - Add the format to the export registry in `ConsoleRun.LoadImeList()`

3. Add the format constant to `ConstantString.cs` with a short code (e.g., `SOUGOU_XIBAO_SCEL_C = "scel"`)

### Code Generation System

The codebase supports multiple encoding methods (Pinyin, Wubi, Cangjie, etc.) through the `IWordCodeGenerater` interface:

- `PinyinGenerater` - Full Pinyin and Shuangpin (double Pinyin)
- `Wubi86Generater`, `Wubi98Generater` - Wubi input methods
- `SelfDefiningCodeGenerater` - Custom user-defined encoding

Generators can read encoding tables from resource files or user-provided files.

### Filter System

Filters are applied after import but before export:

- **Single Filters** (`ISingleFilter`): Process individual entries (e.g., `LengthFilter`, `RankFilter`)
- **Batch Filters** (`IBatchFilter`): Process the entire word list (e.g., deduplication)
- Filters are configured via CLI with `-ft:` parameter

### Chinese Character Conversion

Built-in support for simplified/traditional Chinese conversion through the `IChineseConverter` interface.

## Integration Tests

The project uses a shell-based integration test framework located in `tests/integration/`.

### Test Structure

- `test-cases/` - Test case definitions organized by format or feature
  - `1-imports/` - Tests for importing various formats to CSV
  - `2-exports/` - Tests for exporting CSV to various formats
  - `3-advanced/` - Advanced features (filters, encoding, large files)
- Each test suite has a `test-config.yaml` defining test cases
- Test data is sourced from `src/ImeWlConverterCoreTest/Test/` (shared with unit tests)

### Running Integration Tests

```bash
# Run all tests
cd tests/integration
./run-tests.sh --all

# Run specific suite
./run-tests.sh -s 1-imports

# Run with verbose output
./run-tests.sh -s 1-imports -v

# Keep output files for debugging
./run-tests.sh -s 1-imports --keep-output

# Generate JUnit XML report (for CI)
./run-tests.sh --all --xml
```

### Adding Integration Tests

1. Choose or create a test suite directory under `tests/integration/test-cases/`
2. Create or update `test-config.yaml` with your test case
3. Reference existing test files from `src/ImeWlConverterCoreTest/Test/` using relative paths
4. Generate expected output by running the converter manually
5. Run `./run-tests.sh -s <suite-name>` to verify

See `tests/integration/README.md` for detailed documentation.

## Version Management

Version numbers are automatically generated from Git tags using MinVer:

- Tags follow semantic versioning (e.g., `v2.9.0`)
- Version is embedded in assemblies at build time
- Check version: `make version`

## Build Configuration

- **Debug**: Default, includes debugging symbols
- **Release**: Optimized builds with:
  - Code trimming (`PublishTrimmed=true`)
  - Single-file publishing (`PublishSingleFile=true`)
  - ReadyToRun compilation for faster startup
  - No PDB files

## CI/CD Workflow

GitHub Actions workflow (`.github/workflows/ci.yml`):

1. **Lint** - Code formatting check (fast-fail)
2. **Build and Test** - Build + unit tests on Ubuntu
3. **Platform Builds** - Parallel builds for Windows (x64/x86), Linux (x64/arm64), macOS (x64/arm64)
4. **Integration Tests** - Run on Linux and macOS builds
5. Artifacts retained for 7-30 days

## OpenSpec Integration

This project uses OpenSpec workflow for specification-driven development:

- Specs located in `specs/` directory
- Integration test specification in `specs/001-integration-tests/`
- Uses YAML contracts for test case schemas
- OpenSpec configuration in `.specify/` and `openspec/` directories

## Important Notes

### Character Encoding

The codebase handles multiple encodings (UTF-8, GBK, GB2312, Big5). Always register the CodePagesEncodingProvider:

```csharp
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
```

This is done in `Program.cs` and `MainBody.cs` constructors.

### Binary File Formats

Several IME formats use proprietary binary formats (e.g., `.scel`, `.bdict`, `.qpyd`). Parsers for these are in:

- `BinFileHelper.cs` - Binary reading utilities
- Individual format classes (e.g., `SougouPinyin.cs` for .scel files)

Reverse engineering documentation for these formats is not provided in the codebase but can be found in related Chinese forums.

### Resource Files

Encoding tables and dictionaries are embedded as resources:

- `src/ImeWlConverterCore/Resources/` - Contains .txt files for various encoding tables
- Access via `DictionaryHelper.GetResourceContent(filename)`

### Cross-Platform Considerations

- Use `Path.Combine()` for file paths, never hardcode separators
- The CLI tool is framework-dependent (requires .NET runtime)
- macOS app bundles include the full .NET runtime (self-contained)
- Integration tests work on Linux, macOS, and Windows (Git Bash)

## Debugging Tips

### Integration Test Failures

```bash
# Keep test output for inspection
cd tests/integration
./run-tests.sh -s <suite> --keep-output

# Manually run conversion to debug
dotnet run --project ../../src/ImeWlConverterCmd -- \
  -i:<format> <input-file> \
  -o:<format> <output-file>

# Compare output
diff -u expected.txt test-output/actual.txt
```

### Unit Test Debugging

Test data is in `src/ImeWlConverterCoreTest/Test/` with real dictionary files from various IME formats. Tests cover:

- Format parsing (import)
- Format generation (export)
- Filters
- Helpers (Pinyin, Wubi, etc.)
