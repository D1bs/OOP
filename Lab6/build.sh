#!/usr/bin/env bash
# build.sh — Lab 6 (Patterns: Adapter, Decorator, Facade)
set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=== Building ShapeContracts ==="
dotnet build ShapeContracts/ShapeContracts.csproj -c Debug

echo ""
echo "=== Building FriendPlugin (classmate's project) ==="
dotnet build FriendPlugin/FriendPlugin.csproj -c Debug

echo ""
echo "=== Building ShapeApp (with Adapter, Decorator, Facade) ==="
dotnet build ShapeApp/ShapeApp.csproj -c Debug

echo ""
echo "=== Building shape plugins ==="
dotnet build Plugins/PentagonPlugin/PentagonPlugin.csproj -c Debug
dotnet build Plugins/StarPlugin/StarPlugin.csproj         -c Debug

echo ""
echo "=== Building processor plugins ==="
dotnet build ProcessorPlugins/XmlToJsonPlugin/XmlToJsonPlugin.csproj      -c Debug
dotnet build ProcessorPlugins/JsonPrettifyPlugin/JsonPrettifyPlugin.csproj -c Debug
dotnet build ProcessorPlugins/Base64Plugin/Base64Plugin.csproj             -c Debug

BIN="ShapeApp/bin/Debug/net10.0"

mkdir -p "$BIN/plugins"
echo ""
echo "=== Copying shape plugin DLLs → plugins/ ==="
cp Plugins/PentagonPlugin/bin/Debug/net10.0/PentagonPlugin.dll "$BIN/plugins/"
cp Plugins/StarPlugin/bin/Debug/net10.0/StarPlugin.dll         "$BIN/plugins/"

mkdir -p "$BIN/processors"
echo ""
echo "=== Copying processor plugin DLLs → processors/ ==="
cp ProcessorPlugins/XmlToJsonPlugin/bin/Debug/net10.0/XmlToJsonPlugin.dll       "$BIN/processors/"
cp ProcessorPlugins/JsonPrettifyPlugin/bin/Debug/net10.0/JsonPrettifyPlugin.dll  "$BIN/processors/"
cp ProcessorPlugins/Base64Plugin/bin/Debug/net10.0/Base64Plugin.dll             "$BIN/processors/"

echo ""
echo "=== Build complete! ==="
echo "  Run:  cd ShapeApp && dotnet run"
echo ""
echo "  Patterns in this build:"
echo "    Adapter   — FriendPluginAdapter wraps CsvAnnotatorProcessor (IFriendProcessor → IProcessorPlugin)"
echo "    Decorator — LoggingProcessorDecorator adds timing logs to the adapted plugin"
echo "    Facade    — SerializationFacade.Save() / .Load() hide the pipeline internals"
