#!/usr/bin/env bash
# ローカルHTTPで日本語画面のサーバー描画と配信を確認する。
set -euo pipefail
base="http://127.0.0.1:5183"
log="${RUNNER_TEMP:-/tmp}/pg-blazor-smoke.log"
tmp="$(mktemp -d)"
pid=""
cleanup() {
  if [[ -n "$pid" ]]; then
    kill "$pid" 2>/dev/null || true
    wait "$pid" 2>/dev/null || true
  fi
  rm -rf "$tmp"
}
trap cleanup EXIT

ASPNETCORE_ENVIRONMENT=Development dotnet run \
  --project src/PgBlazorSamples.Web/PgBlazorSamples.Web.csproj \
  --configuration Release --no-build --no-launch-profile \
  --urls "$base" > "$log" 2>&1 &
pid="$!"

started=false
for _ in $(seq 1 60); do
  if curl --fail --silent "$base/" > "$tmp/home.html"; then
    started=true
    break
  fi
  if ! kill -0 "$pid" 2>/dev/null; then
    break
  fi
  sleep 1
done

if [[ "$started" != "true" ]]; then
  echo "起動確認に失敗しました。"
  cat "$log"
  exit 1
fi
check_page() {
  local path="$1"
  local value="$2"
  local output="$tmp/page.html"
  curl --fail --silent --show-error "$base$path" > "$output"
  if ! grep -Fq "$value" "$output"; then
    echo "画面 $path に日本語の期待値がありません: $value"
    cat "$log"
    exit 1
  fi
  if ! grep -Fq 'lang="ja"' "$output"; then
    echo "画面 $path のHTML言語設定が日本語ではありません。"
    exit 1
  fi
  echo "確認成功: $path"
}
check_page "/" "Blazorを基礎から学ぶ"
check_page "/learn/01" "はじめてのBlazor画面"
check_page "/learn/02" "URLとルーティング"
check_page "/learn/03" "C#の値を画面へ表示する"
check_page "/learn/99" "ページが見つかりません"

curl --fail --silent --show-error "$base/learn/03" > "$tmp/step03.html"
if grep -Fq '<重要>' "$tmp/step03.html"; then
  echo "Razor文字列がHTMLタグとして出力されました。"
  exit 1
fi
if ! grep -Fq 'blazor.web.js' "$tmp/home.html"; then
  echo "対話型サーバー用のスクリプトが確認できません。"
  exit 1
fi
echo "日本語画面のHTTP疎通確認が完了しました。"
