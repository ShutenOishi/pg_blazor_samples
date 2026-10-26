#!/usr/bin/env python3
"""実装済みステップ・5ステップごとのコラム・PDF索引の整合性を検査する。"""
import sys
from pathlib import Path

# Windowsランナーがcp1252で起動した場合も日本語の診断を出力できるようにする。
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")

ROOT = Path(__file__).resolve().parents[1]
SAMPLES = ROOT / "src" / "PgBlazorSamples.Web" / "Components" / "Samples"
PDF_SCRIPT = (ROOT / "scripts" / "build_docs_pdf.py").read_text(encoding="utf-8")

steps = sorted(int(path.name[4:]) for path in SAMPLES.glob("Step[0-9][0-9]") if path.is_dir())
if not steps:
    raise SystemExit("教材検査エラー：実装済みのステップがありません。")
if steps != list(range(1, max(steps) + 1)):
    raise SystemExit("教材検査エラー：ステップ番号に欠番があります：" + repr(steps))

missing = []
for number in steps:
    source_dir = SAMPLES / f"Step{number:02d}"
    if not list(source_dir.glob("*.razor")):
        missing.append(f"{source_dir.relative_to(ROOT)}：Razor画面なし")
    doc = f"docs/samples/step{number:02d}.md"
    if not (ROOT / doc).is_file():
        missing.append(doc)
    if f'"{doc}"' not in PDF_SCRIPT:
        missing.append(doc + "：PDF索引に不足")

for number in range(5, max(steps) + 1, 5):
    doc = f"docs/columns/column{number - 4:02d}-{number:02d}.md"
    view = SAMPLES / "Columns" / f"Column{number:02d}Page.razor"
    if not (ROOT / doc).is_file():
        missing.append(doc)
    if not view.is_file():
        missing.append(str(view.relative_to(ROOT)))
    if f'"{doc}"' not in PDF_SCRIPT:
        missing.append(doc + "：PDF索引に不足")

if max(steps) >= 20:
    extra = 'docs/columns/column-library-di.md'
    if not (ROOT / extra).is_file() or f'"{extra}"' not in PDF_SCRIPT:
        missing.append(extra + '：特別コラム未収録')
    page = SAMPLES / 'Columns' / 'ColumnLibraryDiPage.razor'
    if not page.is_file():
        missing.append(str(page.relative_to(ROOT)))

# Step40の後・Step41の前に入れる用途別MudBlazor特別コラムを検査する。
if max(steps) >= 40:
    special = "docs/columns/column-mudblazor-by-purpose.md"
    page = SAMPLES / "Columns" / "ColumnMudBlazorGuidePage.razor"
    if not (ROOT / special).is_file() or f'"{special}"' not in PDF_SCRIPT:
        missing.append(special + "：特別コラム未収録")
    if not page.is_file():
        missing.append(str(page.relative_to(ROOT)))

if missing:
    raise SystemExit("教材の整合性検査失敗：\n" + "\n".join("- " + item for item in missing))
print(f"教材検査成功：ステップ{len(steps)}件・コラム{max(steps) // 5 + (1 if max(steps) >= 20 else 0) + (1 if max(steps) >= 40 else 0) + (1 if max(steps) >= 65 else 0)}件。")
