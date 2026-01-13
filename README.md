# Eto.Forms Website

This repository hosts [eto.picoe.ca](https://eto.picoe.ca), the public documentation and API reference for Eto.Forms. The site now runs on [Hugo](https://gohugo.io/), so you only need one binary to preview or build it.

## Prerequisites

Install the **extended** Hugo binary (for example with `brew install hugo` on macOS). .NET is only required if you plan to regenerate the API reference using the `GenerateApiDocs` tool.

## Local development

```bash
hugo server --disableFastRender
```

The server prints a local URL (usually `http://localhost:1313`). Changes inside the `content/`, `layouts/`, `assets/`, or `static/` folders hot-reload automatically.

## Production build

```bash
hugo --minify
```

The static files land in `public/`. Deploy the contents of that folder with your preferred hosting solution.

## Repository layout

| Path | Purpose |
| --- | --- |
| `content/docs` | Markdown source for the guides, tutorials, and contributing docs. `_index.md` files describe section landing pages. |
| `content/api` | Generated API reference. Each class/member lives in its own Markdown file. |
| `layouts/` | Hugo templates (base layout, navigation, sidebar, etc.). |
| `assets/css/main.css` | Global styles compiled and fingerprinted by Hugo Pipes. |
| `static/` | Images and other assets copied as-is to the final site. |
| `GenerateApiDocs/` | .NET CLI for regenerating the API reference from Eto assemblies. |

## Rebuilding the API reference

1. Build the Eto assemblies (or grab the packaged binaries) so each `.dll` has a matching XML documentation file next to it.
2. Run the `GenerateApiDocs` tool, pointing it at the assemblies you want to document and `content/api` as the output directory. See `GenerateApiDocs/README.md` for options.
3. Commit the updated Markdown files and rebuild the site with Hugo.

## Tips

- Use front matter `weight` values on `_index.md` files to order sections in the docs sidebar.
- Hugo automatically adds every page under `content/docs` and `content/api` to the section sidebar, so keep file names and titles descriptive.
- If you are editing API files manually, keep the Markdown plain—Hugo does not support MDX or React components.
