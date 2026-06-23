#!/usr/bin/env bash
# build.sh — builds ShapeApp and all plugins, then places plugin DLLs
# into ShapeApp/bin/Debug/net10.0/plugins/ so the app finds them at runtime.

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=== Building ShapeContracts ==="
dotnet build ShapeContracts/ShapeContracts.csproj -c Debug

echo ""
echo "=== Building ShapeApp ==="
dotnet build ShapeApp/ShapeApp.csproj -c Debug

echo ""
echo "=== Building PentagonPlugin ==="
dotnet build Plugins/PentagonPlugin/PentagonPlugin.csproj -c Debug

echo ""
echo "=== Building StarPlugin ==="
dotnet build Plugins/StarPlugin/StarPlugin.csproj -c Debug

# Destination plugins folder next to the host executable
PLUGIN_DEST="ShapeApp/bin/Debug/net10.0/plugins"
mkdir -p "$PLUGIN_DEST"

echo ""
echo "=== Copying plugin DLLs to $PLUGIN_DEST ==="
cp Plugins/PentagonPlugin/bin/Debug/net10.0/PentagonPlugin.dll "$PLUGIN_DEST/"
cp Plugins/StarPlugin/bin/Debug/net10.0/StarPlugin.dll         "$PLUGIN_DEST/"

echo ""
echo "=== Done! Run with: ==="
echo "  cd ShapeApp && dotnet run"
echo ""
echo "  Or to point at a custom plugins folder:"
echo "  cd ShapeApp && dotnet run -- /path/to/plugins"
