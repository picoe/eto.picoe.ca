#!/usr/bin/env bash
# Builds the site into public/, with the Eto API reference from Owl under public/api.
#   ./build.sh                    uses the Owl version pinned in .config/dotnet-tools.json
#   OWL_DIR=../Owl ./build.sh     uses a local Owl checkout instead
set -euo pipefail
cd "$(dirname "$0")"

# Only when missing, so a checkout with local Eto changes is documented as it is.
[[ -e eto/.git ]] || git submodule update --init eto

# Moved aside before deleting: Finder can rewrite .DS_Store mid-delete, which makes rm fail.
rm -rf .public-old 2>/dev/null || true
[[ -d public ]] && mv public .public-old
rm -rf .public-old 2>/dev/null || true
hugo --minify "$@"

if [[ -n "${OWL_DIR:-}" ]]; then
  owl=(dotnet run --project "$OWL_DIR/owl" --)
else
  dotnet tool restore
  owl=(dotnet tool run owl)
fi

# Source links follow the tag the submodule is on, so they match the code that was documented.
eto_ref=$(git -C eto describe --tags --exact-match 2>/dev/null || git -C eto rev-parse HEAD)

"${owl[@]}" build \
  eto/src/Eto/Eto.csproj \
  eto/src/Eto.Serialization.Xaml/Eto.Serialization.Xaml.csproj \
  eto/src/Eto.Serialization.Json/Eto.Serialization.Json.csproj \
  --name "Eto.Forms API" \
  --base /api \
  --site-url https://eto.picoe.ca \
  --source-refs "https://github.com/picoe/Eto/blob/$eto_ref" \
  --api-versions 2.13,2.12,2.11,2.10,2.9 \
  --site-header public/owl/header.html \
  --site-footer public/owl/footer.html \
  --out public/api \
  --quiet

# Only Owl needs the snippets.
rm -rf public/owl
