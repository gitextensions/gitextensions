#!/usr/bin/env bash
set -euo pipefail

publish_directory=${1:-}
output_archive=${2:-}
bundle_version=${3:-}
display_version=${4:-}

if [[ -z "$publish_directory" || -z "$output_archive" || -z "$bundle_version" || -z "$display_version" ]]; then
    echo "usage: package-macos-app.sh <publish-directory> <output-archive> <bundle-version> <display-version>" >&2
    exit 2
fi

for tool in codesign ditto iconutil mktemp plutil; do
    if ! command -v "$tool" >/dev/null 2>&1; then
        echo "error: required command '$tool' is not installed" >&2
        exit 1
    fi
done

logo_directory="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../setup/assets/Logo" && pwd)"

if [[ ! -x "$publish_directory/GitExtensions.Avalonia" ]]; then
    echo "error: publish directory does not contain the GitExtensions.Avalonia executable" >&2
    exit 1
fi

if [[ ! "$bundle_version" =~ ^[0-9]+([.][0-9]+)*$ ]]; then
    echo "error: bundle version must contain only decimal components" >&2
    exit 2
fi

work_directory=$(mktemp -d)
cleanup()
{
    rm -rf -- "$work_directory"
}
trap cleanup EXIT

app_directory="$work_directory/Git Extensions Avalonia.app"
contents_directory="$app_directory/Contents"
macos_directory="$contents_directory/MacOS"
mkdir -p "$macos_directory" "$contents_directory/Resources"
cp -a "$publish_directory/." "$macos_directory/"
chmod +x "$macos_directory/GitExtensions.Avalonia"

iconset_directory="$work_directory/GitExtensions.iconset"
mkdir -p "$iconset_directory"
for icon in "16 16x16" "32 16x16@2x" "32 32x32" "64 32x32@2x" "128 128x128" \
            "256 128x128@2x" "256 256x256" "512 256x256@2x" "512 512x512"; do
    read -r pixels name <<<"$icon"
    cp "$logo_directory/git-extensions-logo-${pixels}px.png" "$iconset_directory/icon_${name}.png"
done
iconutil -c icns "$iconset_directory" -o "$contents_directory/Resources/GitExtensions.icns"

cat > "$contents_directory/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "https://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleDisplayName</key>
  <string>Git Extensions Avalonia</string>
  <key>CFBundleExecutable</key>
  <string>GitExtensions.Avalonia</string>
  <key>CFBundleIconFile</key>
  <string>GitExtensions.icns</string>
  <key>CFBundleIdentifier</key>
  <string>com.github.gitextensions.GitExtensions.Avalonia</string>
  <key>CFBundleInfoDictionaryVersion</key>
  <string>6.0</string>
  <key>CFBundleName</key>
  <string>Git Extensions Avalonia</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>CFBundleShortVersionString</key>
  <string>$display_version</string>
  <key>CFBundleVersion</key>
  <string>$bundle_version</string>
  <key>LSMinimumSystemVersion</key>
  <string>10.15</string>
  <key>NSHighResolutionCapable</key>
  <true/>
</dict>
</plist>
EOF

plutil -lint "$contents_directory/Info.plist"

# The publish step signs only the apphost, ad hoc, so its signature does not cover the bundle
# assembled around it. Apple Silicon then refuses to launch the bundle from Finder or the Dock,
# although starting the executable from a terminal still works. Sign the finished bundle.
codesign --force --deep --sign - "$app_directory"
codesign --verify --deep --strict "$app_directory"

mkdir -p "$(dirname "$output_archive")"
rm -f -- "$output_archive"
ditto -c -k --sequesterRsrc --keepParent "$app_directory" "$output_archive"
