#!/usr/bin/env python3
"""既存Markdownから日本語PDFを再作成し、参照したソースの照合情報を付録へ付ける。"""
import hashlib
import html
import os
import subprocess
from datetime import datetime, timezone
from pathlib import Path
from markdown_it import MarkdownIt
from weasyprint import HTML, CSS

ROOT = Path(__file__).resolve().parents[1]
DOCS = [
    "README.md", "docs/implementation-handbook.md",
    "docs/first-code-implementation-plan.md",
    "docs/next-chat-handoff.md",
    "docs/samples/step01.md", "docs/samples/step02.md",
    "docs/samples/step03.md",
    "docs/samples/step04.md", "docs/samples/step05.md",
    "docs/samples/step06.md",
    "docs/samples/step07.md", "docs/samples/step08.md",
    "docs/samples/step09.md", "docs/samples/step10.md",
    "docs/columns/README.md", "docs/columns/column01-05.md",
    "docs/columns/column06-10.md",
    "docs/learning-path.md", "docs/sample-catalog.md",
    "docs/di-for-beginners.md", "docs/architecture.md",
    "docs/environment.md", "docs/versions.md", "docs/database.md",
    "docs/authentication.md", "docs/windows-server-2025-iis.md",
    "docs/quality.md", "docs/conventions.md",
    "docs/implementation-plan.md", "docs/sample-specification.md",
    "docs/references.md", "docs/decisions/0001-learning-app-structure.md",
    "CONTRIBUTING.md", "SECURITY.md",
]
OUTPUT = ROOT / "dist" / "pg_blazor_samples_technical_handbook.pdf"
EXTS = {".cs", ".razor", ".csproj", ".slnx", ".sln", ".sql", ".props", ".targets", ".css", ".js", ".json"}


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def code_files():
    result = []
    for folder in ("src", "tests", "database"):
        base = ROOT / folder
        if base.exists():
            for path in base.rglob("*"):
                if (path.is_file() and path.suffix.lower() in EXTS
                        and not {"bin", "obj", ".vs", ".git"} & set(path.parts)):
                    result.append(path)
    result.extend(p for p in ROOT.iterdir() if p.is_file() and (p.suffix in {".sln", ".slnx", ".props", ".targets"} or p.name == "global.json"))
    return sorted(set(result))


def main():
    md = MarkdownIt("commonmark", {"html": False}).enable("table")
    try:
        revision = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip()
    except (OSError, subprocess.CalledProcessError):
        revision = os.environ.get("GITHUB_SHA", "コミット未取得")
    missing = [name for name in DOCS if not (ROOT / name).is_file()]
    if missing:
        raise SystemExit("PDFの生成元が不足: " + ", ".join(missing))
    sources = code_files()
    chapters = []
    for i, name in enumerate(DOCS, 1):
        path = ROOT / name
        contents = md.render(path.read_text(encoding="utf-8"))
        chapters.append(f'<section class="chapter" id="chapter-{i}"><p class="source">第{i}章 / {html.escape(name)} / SHA-256: {digest(path)[:16]}</p>{contents}</section>')
    entries = "".join(f'<li><a href="#chapter-{i}">{html.escape(name)}</a></li>' for i, name in enumerate(DOCS, 1))
    if sources:
        source_rows = "".join(f'<tr><td>{html.escape(p.relative_to(ROOT).as_posix())}</td><td>{digest(p)[:16]}</td></tr>' for p in sources)
        status = f"ソース関連ファイルを{len(sources)}件検出しました。コードの意味と説明文の整合性は別途レビューが必要です。"
    else:
        source_rows = '<tr><td colspan="2">ソースコードなし：現段階では設計・学習計画のみ</td></tr>'
        status = "実装ソースコードはまだありません。本文に記載のコード例・構成は設計用です。"
    now = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M UTC")
    body = f"""<!doctype html><html lang="ja"><head><meta charset="utf-8"></head><body>
      <section class="cover"><p class="edition">pg_blazor_samples / 技術ドキュメント</p>
      <h1>Blazor × PostgreSQL 18<br>日本語技術ハンドブック</h1>
      <p class="lead">Visual Studio 2026 / .NET 10 / MudBlazor / linq2db / Npgsql<br>Active Directory / Windows Server 2025</p>
      <hr><p><strong>資料の状態：Step01〜10のソースを追加・Step11以降は設計中（受入状況はサンプル一覧）</strong></p>
      <p>{html.escape(status)}</p>
      <p class="small">Gitコミット：{html.escape(revision)}<br>生成：{now}</p></section>
      <section class="toc"><h1>目次</h1><ol>{entries}<li><a href="#inventory">ソース整合性付録</a></li></ol></section>
      {''.join(chapters)}
      <section class="chapter" id="inventory"><p class="source">自動生成付録</p><h1>ソース整合性の確認</h1>
      <p>対象コミット：{html.escape(revision)}</p><p>{html.escape(status)}</p>
      <table><thead><tr><th>実際に存在するソース関連ファイル</th><th>SHA-256先頭16文字</th></tr></thead><tbody>{source_rows}</tbody></table>
      <p>Markdownの内容からPDFを作成しています。ファイルとハッシュの照合は、文章の意味的な正しさやビルド成功を自動で保証しません。実装変更時は文章・テストをレビューしてください。</p>
      </section></body></html>"""
    style = """@page {size:A4;margin:18mm 16mm 20mm;@bottom-left{content:"pg_blazor_samples | 日本語技術資料";font-size:8pt;color:#354d60}@bottom-right{content:counter(page);font-size:9pt}}
      html{font-family:"Noto Sans CJK JP",sans-serif;color:#102739}body{font-size:11pt;line-height:1.72}
      .cover{padding-top:35mm;page-break-after:always}.cover h1{font-size:29pt;line-height:1.4;color:#113f59}.edition{color:#1c637d;font-size:11pt;font-weight:700}.lead{font-size:13pt;line-height:1.8}.small{font-size:9pt;overflow-wrap:anywhere;color:#405b6c}
      .toc{page-break-after:always}.toc ol{columns:2}.toc li{break-inside:avoid;padding:1mm}
      .chapter{break-before:page}.source{font-size:9pt;color:#246782;border-bottom:1px solid #a9c3d1}
      h1{font-size:20pt;color:#113f59;break-after:avoid}h2{font-size:15pt;color:#176385;break-after:avoid;border-left:3px solid #297894;padding-left:3mm}h3{font-size:12pt;break-after:avoid}
      table{border-collapse:collapse;width:100%;table-layout:fixed;font-size:8.6pt;margin:4mm 0}th,td{border:1px solid #b8c8d2;padding:2mm;overflow-wrap:anywhere;vertical-align:top}th{background:#173e58;color:white}tr:nth-child(even){background:#eef4f7}tr{break-inside:avoid}
      code{font-family:"Noto Sans Mono CJK JP",monospace;font-size:.85em;overflow-wrap:anywhere}pre{font-size:8.5pt;white-space:pre-wrap;overflow-wrap:anywhere;background:#e9f1f5;padding:3mm;break-inside:avoid}blockquote{border-left:3px solid #307595;background:#edf4f7;padding:2mm 4mm}a{color:#0a5e86;text-decoration:none}li{margin-bottom:1mm}p{margin:3mm 0}hr{border:0;border-top:2px solid #1d6484;margin:6mm 0}"""
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    HTML(string=body, base_url=ROOT.as_uri()).write_pdf(OUTPUT, stylesheets=[CSS(string=style)])
    print(f"生成成功: {OUTPUT} / 章:{len(DOCS)} / ソース関連ファイル:{len(sources)}")


if __name__ == "__main__":
    main()
