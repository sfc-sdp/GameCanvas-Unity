#!/bin/bash
set -euo pipefail
project_dir="$(cd "$(dirname "$0")/.." && pwd)"
export DOTNET_ROLL_FORWARD="${DOTNET_ROLL_FORWARD:-Major}"
dotnet run --project "$project_dir/scripts/gen-api-docs" -- --source "$project_dir/Packages/jp.ac.keio.sfc.sdp/Runtime/Scripts" --output "$project_dir/docs-src/api"
cp "$project_dir/docs-src/api/api.json" "$project_dir/docs-src/public/api.json"
cd "$project_dir/docs-src"
npm ci
VITEPRESS_OUT_DIR="$project_dir/docs" npx vitepress build
