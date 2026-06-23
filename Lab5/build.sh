#!/usr/bin/env bash
# build.sh — Lab 5
# Builds ShapeContracts, ShapeApp, all shape plugins, and all processor plugins.
# Copies DLLs to the correct runtime sub-folders so the app finds them.

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=== Building ShapeContracts ==="
dotnet build ShapeContracts/ShapeContracts.csproj -c Debug

echo ""
echo "=== Building ShapeApp ==="
dotnet build ShapeApp/ShapeApp.csproj -c Debug

echo ""
echo "=== Building shape plugins ==="
dotnet build Plugins/PentagonPlugin/PentagonPlugin.csproj -c Debug
dotnet build Plugins/StarPlugin/StarPlugin.csproj         -c Debug

echo ""
echo "=== Building processor plugins ==="
dotnet build ProcessorPlugins/XmlToJsonPlugin/XmlToJsonPlugin.csproj     -c Debug
dotnet build ProcessorPlugins/JsonPrettifyPlugin/JsonPrettifyPlugin.csproj -c Debug

# ── Copy DLLs to runtime folders ─────────────────────────────────────────────
BIN="ShapeApp/bin/Debug/net10.0"

SHAPE_DEST="$BIN/plugins"
mkdir -p "$SHAPE_DEST"
echo ""
echo "=== Copying shape plugin DLLs → $SHAPE_DEST ==="
cp Plugins/PentagonPlugin/bin/Debug/net10.0/PentagonPlugin.dll "$SHAPE_DEST/"
cp Plugins/StarPlugin/bin/Debug/net10.0/StarPlugin.dll         "$SHAPE_DEST/"

PROC_DEST="$BIN/processors"
mkdir -p "$PROC_DEST"
echo ""
echo "=== Copying processor plugin DLLs → $PROC_DEST ==="
cp ProcessorPlugins/XmlToJsonPlugin/bin/Debug/net10.0/XmlToJsonPlugin.dll      "$PROC_DEST/"
cp ProcessorPlugins/JsonPrettifyPlugin/bin/Debug/net10.0/JsonPrettifyPlugin.dll "$PROC_DEST/"

echo ""
echo "=== Done! ==="
echo "  Run:  cd ShapeApp && dotnet run"
echo ""
echo "  Pipeline (save): XML → XmlToJson → JsonPrettify → Base64 → file"
echo "  Pipeline (load): file → Base64⁻¹ → JsonPrettify⁻¹ → XmlToJson⁻¹ → XML"
echo ""
echo "  To load only specific processors, move DLLs out of the 'processors/' folder."
