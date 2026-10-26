#!/usr/bin/env python3
"""実装済みステップ・5ステップごとのコラム・PDF索引の整合性を検査する。"""
from pathlib import Path

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

if missing:
    raise SystemExit("教材の整合性検査失敗：\n" + "\n".join("- " + item for item in missing))
print(f"教材検査成功：ステップ{len(steps)}件・コラム{max(steps) // 5}件。")
