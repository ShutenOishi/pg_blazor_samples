#!/usr/bin/env python3
"""学習者向け日本語テキストを作成する。開発管理情報は別の照合データへ分離する。"""
import hashlib
import html
import json
import os
import re
import subprocess
from datetime import datetime, timezone
from pathlib import Path

from markdown_it import MarkdownIt
from weasyprint import CSS, HTML

ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "dist" / "blazor_learning_textbook_ja.pdf"
MANIFEST = ROOT / "dist" / "blazor_learning_source_manifest.json"

# 目次表示名は学習者向けの日本語で固定する。ファイル名は読者に表示しない。
CHAPTERS = [
    ("はじめに：この教材の使い方", "docs/learner-guide.md"),
    ("ステップ01：最初のBlazor画面を表示する", "docs/samples/step01.md"),
    ("ステップ02：URLと画面遷移を理解する", "docs/samples/step02.md"),
    ("ステップ03：RazorでC#の値を表示する", "docs/samples/step03.md"),
    ("ステップ04：ボタンのイベントを扱う", "docs/samples/step04.md"),
    ("ステップ05：入力値を双方向に結び付ける", "docs/samples/step05.md"),
    ("学習コラム01：イベントと状態の関係", "docs/columns/column01-05.md"),
    ("ステップ06：条件分岐と繰り返しで表示する", "docs/samples/step06.md"),
    ("ステップ07：親から子へ値を渡す", "docs/samples/step07.md"),
    ("ステップ08：子から親へ通知する", "docs/samples/step08.md"),
    ("ステップ09：初期化・更新・破棄を理解する", "docs/samples/step09.md"),
    ("ステップ10：入力フォームを日本語で検証する", "docs/samples/step10.md"),
    ("学習コラム02：親子部品と入力検証", "docs/columns/column06-10.md"),
    ("ステップ11：DI（依存性注入）の基本", "docs/samples/step11.md"),
    ("ステップ12：インターフェイスで部品を交換する", "docs/samples/step12.md"),
    ("ステップ13：Singleton・Scoped・Transient", "docs/samples/step13.md"),
    ("ステップ14：BlazorのScopedと対話回線", "docs/samples/step14.md"),
    ("ステップ15：injectとコンストラクター注入", "docs/samples/step15.md"),
    ("学習コラム03：DIとサービスの寿命", "docs/columns/column11-15.md"),
    ("ステップ16：Optionsで設定を受け取る", "docs/samples/step16.md"),
    ("ステップ17：非同期処理とログ", "docs/samples/step17.md"),
    ("ステップ18：DIによる単体テスト", "docs/samples/step18.md"),
    ("ステップ19：Applicationへ処理を分離", "docs/samples/step19.md"),
    ("ステップ20：DIで課題管理を作る", "docs/samples/step20.md"),
    ("学習コラム04：設定とサービス分離", "docs/columns/column16-20.md"),
    ("特別コラム05：ライブラリが提供するDI", "docs/columns/column-library-di.md"),
    ("ステップ21：MudBlazorと日本語翻訳を導入する", "docs/samples/step21.md"),
    ("ステップ22：MudBlazorのレイアウトを学ぶ", "docs/samples/step22.md"),
    ("ステップ23：日本語で入力部品を使う", "docs/samples/step23.md"),
    ("ステップ24：MudFormの入力検証を学ぶ", "docs/samples/step24.md"),
    ("ステップ25：MudTableで一覧とソート", "docs/samples/step25.md"),
    ("学習コラム06：MudBlazorのDIと翻訳", "docs/columns/column21-25.md"),
    ("ステップ26：MudDataGridで検索・並べ替えする", "docs/samples/step26.md"),
    ("ステップ27：確認ダイアログで実行と中止を選ぶ", "docs/samples/step27.md"),
    ("ステップ28：Snackbarと処理中の状態を表示する", "docs/samples/step28.md"),
    ("ステップ29：ダークモードとキーボード操作を学ぶ", "docs/samples/step29.md"),
    ("ステップ30：MudBlazor日本語UIを総合演習する", "docs/samples/step30.md"),
    ("学習コラム07：UIの部品をデータ連携につなぐ", "docs/columns/column26-30.md"),
    ("ステップ31：モデルとDTOを理解する", "docs/samples/step31.md"),
    ("ステップ32：PostgreSQLで学習テーブルを定義する", "docs/samples/step32.md"),
    ("ステップ33：EF Core 10をWebへ登録する", "docs/samples/step33.md"),
    ("ステップ34：DbContextFactoryと接続プール", "docs/samples/step34.md"),
    ("ステップ35：LINQ検索とAsNoTracking", "docs/samples/step35.md"),
    ("学習コラム08：WebプロジェクトだけでEF Coreを使う", "docs/columns/column31-35.md"),
    ("ステップ36：EF Coreの登録・更新・削除", "docs/samples/step36.md"),
    ("ステップ37：DateOnly・TimeOnly・Rangeの型変換", "docs/samples/step37.md"),
    ("ステップ38：トランザクションを理解する", "docs/samples/step38.md"),
    ("ステップ39：xmin楽観的排他を確認する", "docs/samples/step39.md"),
    ("ステップ40：PostgreSQL 18の実接続確認", "docs/samples/step40.md"),
    ("学習コラム09：更新・日付Range・競合・DB確認", "docs/columns/column36-40.md"),
    ("特別コラム：用途から選ぶMudBlazorコンポーネント", "docs/columns/column-mudblazor-by-purpose.md"),
    ("ステップ41：Itemsを使う一覧の条件", "docs/samples/step41.md"),
    ("ステップ42：MudTableのServerDataでDB検索", "docs/samples/step42.md"),
    ("ステップ43：MudDataGridのServerDataとEF Core", "docs/samples/step43.md"),
    ("ステップ44：複数列ソートを安全に組み立てる", "docs/samples/step44.md"),
    ("ステップ45：日付と完了状態で絞り込む", "docs/samples/step45.md"),
    ("学習コラム10：ItemsとServerDataの選び方", "docs/columns/column41-45.md"),
    ("ステップ46：関連テーブルをDTOへ射影する", "docs/samples/step46.md"),
    ("ステップ47：キーセットページングを理解する", "docs/samples/step47.md"),
    ("ステップ48：取得済み一覧を部分描画する", "docs/samples/step48.md"),
    ("ステップ49：DB取得と仮想スクロールを連携する", "docs/samples/step49.md"),
    ("ステップ50：デバウンスと障害時の再試行", "docs/samples/step50.md"),
    ("学習コラム11：関連DTO・キーセット・仮想化", "docs/columns/column46-50.md"),
    ("ステップ51：編集DTOをフォームで検証して保存する", "docs/samples/step51.md"),
    ("ステップ52：ダイアログで保存と取消を分ける", "docs/samples/step52.md"),
    ("ステップ53：複数選択と原子的な一括処理", "docs/samples/step53.md"),
    ("ステップ54：xmin競合と最新値の確認", "docs/samples/step54.md"),
    ("ステップ55：詳細DTOとN+1の防止", "docs/samples/step55.md"),
    ("学習コラム12：編集・一括処理・競合・詳細", "docs/columns/column51-55.md"),
    ("ステップ56：日本語日付範囲とDB検索", "docs/samples/step56.md"),
    ("ステップ57：キャッシュと権限境界", "docs/samples/step57.md"),
    ("ステップ58：DB側集計とCSV出力", "docs/samples/step58.md"),
    ("ステップ59：EXPLAINと応答時間計測", "docs/samples/step59.md"),
    ("ステップ60：一覧パターンを要件から選ぶ", "docs/samples/step60.md"),
    ("学習コラム13：期間検索・集計・性能・選定", "docs/columns/column56-60.md"),
    ("ステップ61：Windows統合認証を構成する", "docs/samples/step61.md"),
    ("ステップ62：認証済みSIDを扱う", "docs/samples/step62.md"),
    ("ステップ63：ADのSIDとDB利用者を結ぶ", "docs/samples/step63.md"),
    ("ステップ64：未登録・無効利用者を拒否する", "docs/samples/step64.md"),
    ("ステップ65：ロールと許可コードを確認する", "docs/samples/step65.md"),
    ("学習コラム14：認証と認可の分離", "docs/columns/column61-65.md"),
    ("先取りガイド：EF Core 10とNpgsqlのデータアクセス", "docs/efcore10-data-access.md"),
    ("先取りガイド：MudBlazor一覧とデータ連携方式の選択", "docs/data-grid-patterns.md"),
]

EXTENSIONS = {".cs", ".razor", ".csproj", ".slnx", ".sln", ".sql", ".props", ".targets", ".css", ".js", ".json"}
SOURCE_FOLDERS = ("src", "tests", "database")

def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()

def source_files():
    paths = []
    for folder in SOURCE_FOLDERS:
        target = ROOT / folder
        if target.exists():
            paths.extend(
                p for p in target.rglob("*")
                if p.is_file()
                and p.suffix.lower() in EXTENSIONS
                and not {"bin", "obj", ".vs", ".git"} & set(p.parts)
            )
    return sorted(set(paths))

def clean_lesson_markdown(text: str) -> str:
    """紙の教材には操作・概念・実コードを残し、作業管理へのリンクを出さない。"""
    text = re.sub(r"^# [^\n]*\n", "", text, count=1)
    lines = []
    for line in text.splitlines():
        if re.match(r"^\s*\*\*(実装|実装ソース|実装コード|対応するソース|ソースコードの場所)[：:]", line):
            continue
        if any(word in line.lower() for word in ("github.com", "レポジトリ", "pull request", "コミットハッシュ")):
            continue
        # 学習PDFでは開発用ディレクトリ指定を省き、Webプロジェクト内で実行する例として示す。
        line = re.sub(r"dotnet add src/[^\s]+\.csproj package ", "dotnet add package ", line)
        if line.strip() == "dotnet run --project src/PgBlazorSamples.Web":
            # IDEで起動する学習者に、開発ディレクトリ構造の知識を要求しない。
            line = "dotnet run"
        # 関連教材の相互参照は文字だけ残し、開発ファイルへのリンクを表示しない。
        line = re.sub(r"\[([^\]]+)\]\([^)]+\)", r"\1", line)
        lines.append(line)
    return "\n".join(lines)

def main():
    missing = [path for _, path in CHAPTERS if not (ROOT / path).is_file()]
    if missing:
        raise SystemExit("学習教材が見つかりません: " + ", ".join(missing))
    revision = os.environ.get("GITHUB_SHA")
    if not revision:
        try:
            revision = subprocess.check_output(
                ["git", "rev-parse", "HEAD"], cwd=ROOT, text=True
            ).strip()
        except (OSError, subprocess.CalledProcessError):
            revision = "unknown"

    markdown = MarkdownIt("commonmark", {"html": False}).enable("table")
    chapters_html = []
    toc = []
    for number, (title, relative_path) in enumerate(CHAPTERS, start=1):
        path = ROOT / relative_path
        content = clean_lesson_markdown(path.read_text(encoding="utf-8"))
        if "github.com" in content.lower() or "レポジトリ" in content:
            raise SystemExit(f"学習者用の不要な開発情報が残っています: {relative_path}")
        chapters_html.append(
            f'<section class="chapter" id="lesson-{number}">'
            f'<p class="chapter-kicker">{"先取り学習（未実装の設計）" if "先取りガイド" in title else ("学習コラム" if "学習コラム" in title else "ステップアップ学習")}</p>'
            f'<h1>{html.escape(title)}</h1>'
            f'{markdown.render(content)}</section>'
        )
        toc.append(f'<li><a href="#lesson-{number}">{html.escape(title)}</a></li>')

    body = f"""<!doctype html><html lang="ja"><head><meta charset="utf-8"></head><body>
    <section class="cover">
      <p class="brand">Visual Studio 2026 / .NET 10</p>
      <h1>Blazor 実践学習テキスト</h1>
      <p class="lead">画面づくりから日本語UIまで<br>65ステップ＋用途別MudBlazorとデータ連携の先取り学習</p>
      <div class="cover-note">各ステップ：目標 → 操作 → コードのしくみ → 演習<br>
      5ステップごとに理解を深める復習コラムを収録</div>
      <p class="version">学習範囲：ステップ01〜65／コラム01〜14／MudBlazor特別コラム／データ連携設計ガイド</p>
    </section>
    <section class="toc">
      <h1>目次</h1>
      <p>学びたいテーマの名前を選んでください。番号は実際の画面と対応しています。</p>
      <ol>{''.join(toc)}</ol>
    </section>
    {''.join(chapters_html)}
    </body></html>"""

    style = r"""
    @page { size: A4; margin: 19mm 18mm 20mm;
      @bottom-left { content: "Blazor 実践学習テキスト"; font-size: 9pt; color: #364c5d }
      @bottom-right { content: counter(page); font-size: 10pt; color: #233949 }
    }
    html { font-family: "Noto Sans CJK JP", sans-serif; color: #172b3b }
    body { font-size: 11.4pt; line-height: 1.75 }
    .cover { padding-top: 39mm; break-after: page; }
    .cover .brand { font-size: 13pt; font-weight: bold; letter-spacing: .04em; color: #1e536c }
    .cover h1 { font-size: 30pt; line-height: 1.38; color: #163b56; margin: 14mm 0 }
    .lead { font-size: 16pt; line-height: 1.7 }
    .cover-note { margin-top: 17mm; padding: 8mm; border-left: 5px solid #225773; background: #eaf2f7; }
    .version { margin-top: 16mm; color: #405868 }
    .toc { break-after: page; }
    .toc h1 { font-size: 24pt; margin-bottom: 4mm }
    .toc ol { padding-left: 1.45em }
    .toc li { margin: 2.2mm 0; break-inside: avoid; }
    .toc a { text-decoration: none; color: #164967; font-weight: 600 }
    .chapter { break-before: page; }
    .chapter-kicker { font-size: 10pt; font-weight: 700; color: #285a77; border-bottom: 1px solid #9aafbd }
    h1 { font-size: 21pt; line-height: 1.44; color: #113a56; break-after: avoid }
    h2 { font-size: 15pt; color: #154965; border-left: 4px solid #2d647f;
         padding-left: 3mm; break-after: avoid; margin-top: 8mm }
    h3 { font-size: 12.5pt; break-after: avoid; color: #234d64 }
    p { margin: 3mm 0; orphans: 2; widows: 2 }
    li { margin: 1.7mm 0; }
    code { font-family: "Noto Sans Mono CJK JP", monospace; font-size: .88em; overflow-wrap: anywhere }
    pre { font-size: 9pt; white-space: pre-wrap; overflow-wrap: anywhere; break-inside: avoid;
          padding: 4mm; background: #eef4f7; border-left: 2px solid #7597a9; line-height: 1.5 }
    table { border-collapse: collapse; width: 100%; font-size: 9.1pt; margin: 4mm 0 }
    th, td { border: 1px solid #9fb3c1; padding: 2mm; vertical-align: top; overflow-wrap: anywhere }
    th { background: #24485d; color: white }
    tr { break-inside: avoid }
    tr:nth-child(even) td { background: #f1f5f7 }
    blockquote { padding: 3mm 5mm; margin: 4mm 0; border-left: 3px solid #35708c; background: #eaf1f5 }
    a { color: #165a7a }
    hr { border: none; border-top: 1px solid #9baebb }
    """
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    HTML(string=body, base_url=ROOT.as_uri()).write_pdf(OUTPUT, stylesheets=[CSS(string=style)])

    # ハッシュなどの開発側の照合情報は別ファイルへ。学習者のPDF本文・目次には載せない。
    manifest = {
        "revision": revision,
        "generated_utc": datetime.now(timezone.utc).isoformat(),
        "chapters": [
            {"title": title, "path": path, "sha256": sha256(ROOT / path)}
            for title, path in CHAPTERS
        ],
        "source_files": [
            {"path": p.relative_to(ROOT).as_posix(), "sha256": sha256(p)}
            for p in source_files()
        ],
    }
    MANIFEST.write_text(json.dumps(manifest, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"日本語学習PDF生成成功: {OUTPUT} / 章数: {len(CHAPTERS)}")
    print(f"ソース照合情報は学習用PDFとは別に保存: {MANIFEST}")

if __name__ == "__main__":
    main()
