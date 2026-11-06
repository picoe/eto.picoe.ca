# Eto.Forms Website

This repository hosts [eto.picoe.ca](https://eto.picoe.ca), the public documentation and API reference for Eto.Forms. The site now runs on [Hugo](https://gohugo.io/), so you only need one binary to preview or build it.

## Prerequisites

Install the **extended** Hugo binary (for example with `brew install hugo` on macOS). Building the API reference also needs the .NET 8 SDK (or newer) and Node 18 (or newer).

## Local development

```bash
hugo server --disableFastRender
```

The server prints a local URL (usually `http://localhost:1313`). Changes inside the `content/`, `layouts/`, `assets/`, or `static/` folders hot-reload automatically.

`hugo server` doesn't include the API reference. To preview it, run `./build.sh` and serve `public/`, or run `dotnet tool run owl serve eto/src/Eto/Eto.csproj` on its own.

## Production build

```bash
./build.sh
```

This builds the site with Hugo, then the API reference with [Owl](https://github.com/mcneel/owl) into `public/api`, using Eto from the `eto` submodule. Deploy the contents of `public/`. Set `OWL_DIR` to a local Owl checkout to use that instead of the pinned version in `.config/dotnet-tools.json`.

## Repository layout

| Path | Purpose |
| --- | --- |
| `content/docs` | Markdown source for the guides, tutorials, and contributing docs. `_index.md` files describe section landing pages. |
| `eto` | Eto submodule, checked out at the release the API reference documents. |
| `content/owl-*.md` | The site header and footer as bare snippets, which `build.sh` passes to Owl for the API pages. |
| `layouts/` | Hugo templates (base layout, navigation, sidebar, etc.). |
| `assets/css/main.css` | Global styles compiled and fingerprinted by Hugo Pipes. |
| `assets/css/chrome.css` | Header and footer styles, shared with the API pages. |
| `static/` | Images and other assets copied as-is to the final site. |

## Updating the API reference

Check out a different Eto release in the submodule (`git -C eto checkout 2.10.3`), commit the submodule change, and run `./build.sh`.

## Tips

- Use front matter `weight` values on `_index.md` files to order sections in the docs sidebar.
- Hugo automatically adds every page under `content/docs` to the section sidebar, so keep file names and titles descriptive.
