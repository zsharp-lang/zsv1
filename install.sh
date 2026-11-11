#!/bin/sh
# Installs the Z# toolchain this is part of:
#   ./install.sh [folder] [--no-path]    or    ./install.sh --uninstall [folder]
here="$(cd "$(dirname "$0")" && pwd)"
chmod +x "$here/bin/zs" 2>/dev/null
exec "$here/bin/zs" "$here/install.zs" "$@"
