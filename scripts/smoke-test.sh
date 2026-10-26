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
check_page "/learn/04" "ボタンのクリックで数値を変える"
check_page "/learn/05" "入力した文字をすぐに表示する"
check_page "/learn/06" "条件で表示を切り替え、一覧を作る"
check_page "/learn/07" "親から子へ値を渡す"
check_page "/learn/08" "子の操作を親へ知らせる"
check_page "/learn/09" "初期化・更新・破棄の違いを見る"
check_page "/learn/10" "日本語メッセージで入力を検証する"
check_page "/learn/11" "部品を直接作る方法と、DIで受け取る方法を比べる"
check_page "/learn/12" "同じ約束で時計の実装を入れ替える"
check_page "/learn/13" "サービスはいつ作られ、いつ共有される？"
check_page "/learn/14" "親子で状態を共有し、別の回線とは分ける"
check_page "/learn/15" "画面とC#クラスでサービスを受け取る"
check_page "/learn/column/15" "DIを使うと、画面とサービスの何が変わる？"
check_page "/learn/16" "設定ファイルの値を型付きサービスで受け取る"
check_page "/learn/17" "処理の完了と取り消しを画面で観察する"
check_page "/learn/18" "固定時計で時間帯に左右されないテストを作る"
check_page "/learn/19" "画面とは別のクラスライブラリへ処理を分ける"
check_page "/learn/20" "DIサービスで学習課題の一覧を操作する"
check_page "/learn/column/20" "DIを利用して画面の責任を小さくする"
check_page "/learn/column/library-di" "ライブラリの機能は、登録して受け取って使う"
check_page "/learn/column/05" "画面の表示は、何がきっかけで変わる？"
check_page "/learn/column/10" "親子コンポーネントと入力検証を整理する"
check_page "/learn/99" "ページが見つかりません"

curl --fail --silent --show-error "$base/learn/03" > "$tmp/step03.html"
if grep -Fq '<重要>' "$tmp/step03.html"; then
  echo "Razor文字列がHTMLタグとして出力されました。"
  exit 1
fi
if ! grep -Fq 'blazor.web' "$tmp/home.html"; then
  echo "対話型サーバー用のスクリプトが確認できません。"
  exit 1
fi
echo "日本語画面のHTTP疎通確認が完了しました。"
