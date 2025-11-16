# GenerateApiDocs

> CLI that reads .NET assemblies + XML docs and emits MDX pages for a Docusaurus API site.

## Building

```bash
cd docs/GenerateApiDocs
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
  ../api
```

You can also supply `.nupkg` files instead of direct `.dll` paths. The tool will extract the matching assembly (preferring the DLL whose name matches the package ID) and its XML docs from the package's `lib/` folder before generating the pages.

### Options

| Option | Description |
| --- | --- |
| `--flatten-namespaces` | Write `Namespace.TypeName.mdx` at the root instead of under `Namespace/TypeName`. |
| `--include-explicit-implementations` | Adds a section that lists non-public explicit interface implementations. |
| `--move-deprecated-members` | Renders properties/methods/constructors marked with `[Obsolete]` in dedicated sections at the end. |
| `--split-members` | Generates one MDX file per constructor/field/property/event/method instead of a single page per type. |

The last argument must be the directory that will receive the generated MDX files (relative paths are accepted).

## Workflow

1. Build the target Eto assemblies in Release or Debug to ensure the XML doc files exist beside each `.dll`.
2. Run `GenerateApiDocs` with the binaries you want to document plus the output directory under `docs/artifacts/apidocs`.
3. Run `npm build` in the `docs` folder to make sure Docusaurus picks up the new content.
