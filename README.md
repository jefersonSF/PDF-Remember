# PDF Remember

A lightweight PDF reader for Windows that remembers the last page viewed in each PDF document, allowing you to resume reading exactly where you left off.

## Overview

**PDF Remember** is a desktop application developed with WPF and C# using .NET 10.

Its main purpose is to make reading PDF documents more convenient by automatically remembering the last page viewed. When you open the same document again, the application restores your previous reading position.

Instead of manually searching for the page where you stopped, you can simply reopen the document and continue reading.

## Features

- **Automatic Page Tracking:** Remembers the last page viewed in each PDF document.
- **Reading Position Restoration:** Automatically returns to the previously viewed page when reopening a document.
- **Persistent Reading Progress:** Keeps track of your reading position between application sessions.

## Technologies

- C#
- .NET 10
- WPF (Windows Presentation Foundation)
- XAML

## Download

You can download the compiled application from the [GitHub Releases](../../releases) section.

The published version is distributed as a **self-contained, single-file executable**, so installing the .NET 10 Runtime separately is not required.

**Target platform:** Windows x64.

## Requirements

- Windows 10 or later (64-bit).

No separate .NET Runtime installation is required to run the published self-contained version.

## Running the Application

1. Download the latest release from the [Releases](../../releases) page.
2. Extract the downloaded package, if necessary.
3. Run the application executable.
4. Open a PDF document and start reading.

The application will remember your last viewed page for subsequent sessions.

## Building from Source

To build the project locally, you need:

- .NET 10 SDK
- Windows environment with WPF support.

Clone the repository:

```bash
git clone https://github.com/jefersonSF/PDF-Remember.git
```

Navigate to the project directory and publish:

```bash
dotnet publish -c Release
```

The generated application can be found in the publish output directory.

## License

This repository currently has no explicit open-source license. Public availability of the source code does not grant permission to copy, modify, or redistribute it.