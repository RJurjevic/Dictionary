# Dictionary (Webster / Project Gutenberg)

A small console dictionary lookup tool built around the **Project Gutenberg** text of **Webster’s Unabridged Dictionary** (the `29765-8.txt` source).

## What it does

- Looks up one or more words from the command line and prints the matching dictionary entry/entries.
- Supports multiple headwords/variants (e.g. `honor honour`, `color colour`, `center centre`).
- Outputs entries to the console with **colored highlighting** (the **headword** is emphasized; the **definition text** uses a readable contrasting color).
- Can optionally produce HTML output (if enabled in the existing code).

## Credits / authorship

- The original program was written by **Robert Jurjevic**.
- **ChatGPT** helped with:
  - **Reformatting / improving readability** (comments, small cleanup while keeping behavior intact)
  - Adding a simple **Console writer** helper and **colorized console output**

## Usage

From the build output folder (example):

```bat
Webster.exe home
Webster.exe honor honour color colour center centre
Webster.exe anchor ship sail mast
```
