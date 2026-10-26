#!/usr/bin/env python3
"""実装済みステップ・5ステップごとのコラム・PDF索引の整合性を検査する。"""
import json
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

# 旧第1段階はv3の第2段階へ統合済み。旧第2〜7段階のURL・本文は移行中も保持する。
# v3の第1・第2段階は別検査で先頭配置を照合し、旧第1段階概要のPDF重複を要求しない。
stage_starts = (11, 21, 31, 41, 61, 71)
for stage, start in enumerate(stage_starts, start=2):
    if max(steps) < start:
        continue
    page = f"docs/stages/stage{stage:02d}.md"
    page_marker = f'"{page}"'
    first_step_marker = f'"docs/samples/step{start:02d}.md"'
    if not (ROOT / page).is_file():
        missing.append(page + "：段階概要がありません")
    if page_marker not in PDF_SCRIPT:
        missing.append(page + "：PDF目次に収録されていません")
    elif first_step_marker not in PDF_SCRIPT or PDF_SCRIPT.index(page_marker) > PDF_SCRIPT.index(first_step_marker):
        missing.append(page + "：段階概要は先頭ステップの前に配置してください")

# v3の学習順・実装済み新規ステップとPDF目次の整合性を保証する。
curriculum = json.loads((ROOT / "docs/course-v3/curriculum.json").read_text(encoding="utf-8"))
ordered = curriculum["steps"]
if len(ordered) != 110 or [item["sequence"] for item in ordered] != list(range(1, 111)):
    missing.append("v3の110学習順が不正")
if len({item["id"] for item in ordered}) != 110:
    missing.append("v3の学習IDに重複がある")
if len(curriculum["stages"]) != 9 or len(curriculum["reviews"]) != 22:
    missing.append("v3の段階/復習組数が不正")
newpaths = []
for n in range(1, 6):
    item = ordered[n-1]
    razor = SAMPLES / "WebFoundation" / f"Web{n:02d}Page.razor"
    if item["id"] != f"web-{n:02d}" or not item["implementation"].startswith("implemented"):
        missing.append(f"web-{n:02d}の順序・実装状態が不一致")
    if not razor.exists() or f'@page "/learn/web-{n:02d}"' not in razor.read_text(encoding="utf-8"):
        missing.append(str(razor.relative_to(ROOT)) + "：URL定義なし")
    if not (ROOT / item["source"]).exists() or f'"{item["source"]}"' not in PDF_SCRIPT:
        missing.append(item["source"] + "：教材・PDF収録なし")
    else:
        newpaths.append(PDF_SCRIPT.index(f'"{item["source"]}"'))
review = 'docs/course-v3/reviews/review-01.md'
stage = 'docs/course-v3/stages/stage01.md'
if not (SAMPLES / "WebFoundation" / "WebFoundationReviewPage.razor").exists():
    missing.append("Web基礎の復習画面なし")
if not (ROOT / review).exists() or f'"{review}"' not in PDF_SCRIPT:
    missing.append("Web基礎の復習PDFなし")
if not (ROOT / stage).exists() or f'"{stage}"' not in PDF_SCRIPT:
    missing.append("Web基礎の概要ページなし")
if len(newpaths) == 5:
    if newpaths != sorted(newpaths):
        missing.append("Web基礎のPDF学習順が不正")
    if f'"{stage}"' in PDF_SCRIPT and PDF_SCRIPT.index(f'"{stage}"') > newpaths[0]:
        missing.append("段階概要はWeb01より前に配置")
    if f'"{review}"' in PDF_SCRIPT and PDF_SCRIPT.index(f'"{review}"') < newpaths[4]:
        missing.append("復習はWeb05より後に配置")


# 第2段階では新規5件を既存10件の間に挿入する。Razor・教材・復習・PDF順を検証する。
runtime_ids = ["runtime-01", "runtime-02", "runtime-03", "runtime-04", "runtime-05"]
for n, lesson_id in enumerate(runtime_ids, 1):
    item = next((item for item in ordered if item["id"] == lesson_id), None)
    razor = SAMPLES / "Runtime" / f"Runtime{n:02d}Page.razor"
    if not item or not item["implementation"].startswith("implemented"):
        missing.append(lesson_id + "：学習順の実装状態が不正")
        continue
    if not razor.is_file() or f'@page "/learn/{lesson_id}"' not in razor.read_text(encoding="utf-8"):
        missing.append(lesson_id + "：Razor URLが不正")
    if not (ROOT / item["source"]).is_file() or f'"{item["source"]}"' not in PDF_SCRIPT:
        missing.append(lesson_id + "：本文またはPDF索引が不足")
stage2 = "docs/course-v3/stages/stage02.md"
if not (SAMPLES / "Runtime" / "RuntimeOverviewPage.razor").is_file() or f'"{stage2}"' not in PDF_SCRIPT:
    missing.append("第2段階の概要画面またはPDFが不足")
for unit, suffix in [(2, "10"), (3, "15"), (4, "20")]:
    source = f"docs/course-v3/reviews/review-0{unit}.md"
    view = SAMPLES / "Runtime" / f"RuntimeReview0{unit}Page.razor"
    if not view.is_file() or not (ROOT / source).is_file() or f'"{source}"' not in PDF_SCRIPT:
        missing.append(f"第{unit}組の復習画面・本文・PDFが不足")
expected_v3_prefix = [item["source"] for item in ordered[:20]]
import re
actual_v3_prefix = re.findall(r'^    \(".*?", "(docs/(?:samples|course-v3/lessons)/[^"]+)"\),$', PDF_SCRIPT, flags=re.MULTILINE)
actual_v3_steps = [source for source in actual_v3_prefix if source in expected_v3_prefix]
if actual_v3_steps != expected_v3_prefix:
    missing.append("第1〜2段階のPDF学習順001〜020が不正")

# 第4段階のブラウザー新規5件と概要・復習を確認する。
for n in range(1, 6):
    lesson_id = f"browser-0{n}"
    item = next((x for x in ordered if x["id"] == lesson_id), None)
    page = SAMPLES / "Browser" / f"Browser0{n}Page.razor"
    if not item or not item["implementation"].startswith("implemented"):
        missing.append(lesson_id + "：未実装または学習順不正")
        continue
    if not page.is_file() or f'@page "/learn/{lesson_id}"' not in page.read_text(encoding="utf-8"):
        missing.append(lesson_id + "：Razor画面とURL不足")
    if not (ROOT / item["source"]).is_file() or f'"{item["source"]}"' not in PDF_SCRIPT:
        missing.append(lesson_id + "：教材かPDF索引が不足")
overview = 'docs/course-v3/stages/stage04.md'
review09 = 'docs/course-v3/reviews/review-09.md'
for path, view in [(overview, "BrowserOverviewPage.razor"), (review09, "BrowserReview09Page.razor")]:
    if not (ROOT / path).is_file() or f'"{path}"' not in PDF_SCRIPT:
        missing.append(path + "：第4段階の概要・復習がPDFに不足")
    if not (SAMPLES / "Browser" / view).is_file():
        missing.append(view + "：概要・復習画面なし")
if not (ROOT / "src/PgBlazorSamples.Web/wwwroot/js/browser-learning.js").is_file():
    missing.append("ブラウザーJSモジュールがありません")
v3_stage4 = [x["source"] for x in ordered if 31 <= x["sequence"] <= 45]
pdf_stage4 = [source for source in actual_v3_prefix if source in v3_stage4]
if pdf_stage4 != v3_stage4:
    missing.append("第4段階のPDF学習順031〜045と画面の並び順が一致しません")
if f'"{overview}"' in PDF_SCRIPT and f'"{v3_stage4[0]}"' in PDF_SCRIPT:
    if PDF_SCRIPT.index(f'"{overview}"') > PDF_SCRIPT.index(f'"{v3_stage4[0]}"'):
        missing.append("第4段階の概要は学習順031より前に収録してください")
if f'"{review09}"' in PDF_SCRIPT and f'"{v3_stage4[-1]}"' in PDF_SCRIPT:
    if PDF_SCRIPT.index(f'"{review09}"') < PDF_SCRIPT.index(f'"{v3_stage4[-1]}"'):
        missing.append("第9組の復習は学習順045の後に収録してください")

if missing:
    raise SystemExit("教材の整合性検査失敗：\n" + "\n".join("- " + item for item in missing))
print(f"教材検査成功：ステップ{len(steps)}件・コラム{max(steps) // 5 + (1 if max(steps) >= 20 else 0) + (1 if max(steps) >= 40 else 0) + (1 if max(steps) >= 65 else 0)}件。")
