# GenerateApiDocs

CLI that reads .NET assemblies + XML docs and emits Markdown pages for the Hugo-powered Eto.Forms API reference.

## Building

```bash
cd GenerateApiDocs
dotnet build
```

## Running

```bash
dotnet run -- \
  --flatten-namespaces \
  --include-explicit-implementations \
  --move-deprecated-members \
  ../../artifacts/core/Eto/Debug/netstandard2.0/Eto.dll \
  ../../artifacts/core/Eto.Serialization.Xaml/Debug/netstandard2.0/Eto.Serialization.Xaml.dll \
  ../../artifacts/core/Eto.Serialization.Json/Debug/netstandard2.0/Eto.Serialization.Json.dll \
  ../content/api
```

You can also supply `.nupkg` files instead of direct `.dll` paths. The tool will extract the matching assembly (preferring the DLL whose name matches the package ID) and its XML docs from the package's `lib/` folder before generating the pages.

If you don't have the package locally, pass the NuGet package ID (optionally in the form `PackageId@1.2.3`). The CLI will download the package from nuget.org on the fly, pull in its non-system NuGet dependencies, and use those assemblies + XML docs so referenced types can be resolved.

Need to preload framework components or utility packages (e.g., `System.Windows.Forms`) that aren't direct dependencies of your target assemblies? Use `--reference` with either a DLL path, `.nupkg`, or package ID so those assemblies are available while generating docs.

### Options

| Option | Description |
| --- | --- |
| `--flatten-namespaces` | Write `Namespace.TypeName.mdx` at the root instead of under `Namespace/TypeName`. |
| `--include-explicit-implementations` | Adds a section that lists non-public explicit interface implementations. |
| `--move-deprecated-members` | Renders properties/methods/constructors marked with `[Obsolete]` in dedicated sections at the end. |
| `--split-members` | Generates one Markdown file per constructor/field/property/event/method instead of a single page per type. |
| `--reference <spec>` | Preload additional assemblies or packages (e.g., `System.Windows.Forms` or `PackageId@1.2.3`) so their types are available during generation. |

The last argument must be the directory that will receive the generated Markdown files (relative paths are accepted).

## Workflow

1. Build the target Eto assemblies in Release or Debug to ensure the XML doc files exist beside each `.dll`.
2. Run `GenerateApiDocs` with the binaries you want to document and point the output directory at `content/api`.
3. Rebuild the Hugo site (`hugo --minify`) so the updated Markdown is rendered.
