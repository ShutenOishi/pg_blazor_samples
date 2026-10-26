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


# 第5段階の新規5本は既存の10本の間に置く。PDFとRazorを学習順で照合する。
for n, seq in enumerate((46, 52, 53, 58, 59), 1):
    lesson_id = f"db-bridge-0{n}"
    item = ordered[seq - 1]
    page = SAMPLES / "DbBridge" / f"DbBridge0{n}Page.razor"
    if item["id"] != lesson_id or not item["implementation"].startswith("implemented"):
        missing.append(lesson_id + "：学習順または実装状態が不正")
    if not page.is_file() or f'@page "/learn/{lesson_id}"' not in page.read_text(encoding="utf-8"):
        missing.append(lesson_id + "：RazorとURLの不足")
    if not (ROOT / item["source"]).is_file() or f'"{item["source"]}"' not in PDF_SCRIPT:
        missing.append(lesson_id + "：本文・PDF索引不足")
stage05 = "docs/course-v3/stages/stage05.md"
if not (SAMPLES / "DbBridge" / "DbBridgeOverviewPage.razor").is_file() or f'"{stage05}"' not in PDF_SCRIPT:
    missing.append("第5段階の概要画面・本文・PDF索引不足")
for unit, end in ((10, 50), (11, 55), (12, 60)):
    review = f"docs/course-v3/reviews/review-{unit}.md"
    page = SAMPLES / "DbBridge" / f"DbBridgeReview{unit}Page.razor"
    if not page.is_file() or not (ROOT / review).is_file() or f'"{review}"' not in PDF_SCRIPT:
        missing.append(f"第{unit}組の復習本文・画面・PDF不足")
expected_stage05 = [x["source"] for x in ordered[45:60]]
actual_stage05 = [source for source in actual_v3_prefix if source in expected_stage05]
if actual_stage05 != expected_stage05:
    missing.append("第5段階の学習順046〜060とPDFのステップ順が不一致")
if f'"{stage05}"' in PDF_SCRIPT and PDF_SCRIPT.index(f'"{stage05}"') > PDF_SCRIPT.index(f'"{expected_stage05[0]}"'):
    missing.append("第5段階の概要は先頭に置いてください")
for unit, end in ((10, 50), (11, 55), (12, 60)):
    a = f'"docs/course-v3/reviews/review-{unit}.md"'
    b = f'"{ordered[end - 1]["source"]}"'
    if a in PDF_SCRIPT and b in PDF_SCRIPT and PDF_SCRIPT.index(a) < PDF_SCRIPT.index(b):
        missing.append(f"第{unit}組の復習が5ステップの末尾より前です")


# 第7段階のセキュリティ5本・段階概要・復習をRazor/PDF/学習順で照合する。
for n in range(1, 6):
    lesson_id = f"security-0{n}"
    item = ordered[80 + n - 1]
    razor = SAMPLES / "Security" / f"Security0{n}Page.razor"
    if item["id"] != lesson_id or not item["implementation"].startswith("implemented"):
        missing.append(lesson_id + "：順序・実装状態が不正")
    if not razor.is_file() or f'@page "/learn/{lesson_id}"' not in razor.read_text(encoding="utf-8"):
        missing.append(lesson_id + "：Razor・URLが不足")
    if not (ROOT / item["source"]).is_file() or f'"{item["source"]}"' not in PDF_SCRIPT:
        missing.append(lesson_id + "：教材・PDF索引が不足")
stage07 = "docs/course-v3/stages/stage07.md"
review17 = "docs/course-v3/reviews/review-17.md"
if not (SAMPLES / "Security" / "SecurityOverviewPage.razor").is_file() or f'"{stage07}"' not in PDF_SCRIPT:
    missing.append("第7段階の概要画面・PDFが不足")
if not (SAMPLES / "Security" / "SecurityReview17Page.razor").is_file() or not (ROOT / review17).is_file() or f'"{review17}"' not in PDF_SCRIPT:
    missing.append("第17組の復習画面・教材・PDFが不足")
if not (ROOT / "src/PgBlazorSamples.Web/Services/Learning/SecurityLessonPolicy.cs").is_file():
    missing.append("模擬安全判定の共通コードがありません")
expected_sec = [item["source"] for item in ordered[80:85]]
actual_sec = [source for source in actual_v3_prefix if source in expected_sec]
if actual_sec != expected_sec:
    missing.append("学習順081〜085のPDF順序が不正")
if f'"{stage07}"' in PDF_SCRIPT and f'"{expected_sec[0]}"' in PDF_SCRIPT and PDF_SCRIPT.index(f'"{stage07}"') > PDF_SCRIPT.index(f'"{expected_sec[0]}"'):
    missing.append("第7段階の概要は学習順081より前に配置")
if f'"{review17}"' in PDF_SCRIPT and f'"{expected_sec[-1]}"' in PDF_SCRIPT and PDF_SCRIPT.index(f'"{review17}"') < PDF_SCRIPT.index(f'"{expected_sec[-1]}"'):
    missing.append("第17組の復習は学習順085より後に配置")
if f'"{review17}"' in PDF_SCRIPT and '"docs/samples/step61.md"' in PDF_SCRIPT and PDF_SCRIPT.index(f'"{review17}"') > PDF_SCRIPT.index('"docs/samples/step61.md"'):
    missing.append("第17組復習は次の学習順086より前に配置")


# 最後の5ステップ：総合演習と段階概要・第22組を教材/PDF/Webに同期する。
for n in range(1, 6):
    lesson_id = f"capstone-0{n}"
    item = ordered[104 + n]
    page = SAMPLES / "Capstone" / f"Capstone0{n}Page.razor"
    if item["sequence"] != 105 + n or item["id"] != lesson_id or not item["implementation"].startswith("implemented"):
        missing.append(f"{lesson_id}：学習順または実装状態の不一致")
    if not page.is_file() or f'@page "/learn/{lesson_id}"' not in page.read_text(encoding="utf-8"):
        missing.append(f"{lesson_id}：Razor画面とURLが不足")
    if not (ROOT / item["source"]).is_file() or f'"{item["source"]}"' not in PDF_SCRIPT:
        missing.append(f"{lesson_id}：MarkdownかPDFへの収録が不足")
stage09 = "docs/course-v3/stages/stage09.md"
review22 = "docs/course-v3/reviews/review-22.md"
if not (SAMPLES / "Capstone" / "CapstoneOverviewPage.razor").is_file() or f'"{stage09}"' not in PDF_SCRIPT:
    missing.append("第9段階の概要画面・PDF索引が不足")
if not (SAMPLES / "Capstone" / "CapstoneReview22Page.razor").is_file() or not (ROOT / review22).is_file() or f'"{review22}"' not in PDF_SCRIPT:
    missing.append("第22組の復習画面・本文・PDF索引が不足")
expected_capstone = [x["source"] for x in ordered[105:110]]
actual_capstone = [source for source in actual_v3_prefix if source in expected_capstone]
if actual_capstone != expected_capstone:
    missing.append("総合演習106〜110のPDF学習順が不正")
if f'"{stage09}"' in PDF_SCRIPT and PDF_SCRIPT.index(f'"{stage09}"') > PDF_SCRIPT.index(f'"{expected_capstone[0]}"'):
    missing.append("第9段階概要を学習順106より先に配置してください")
if f'"{review22}"' in PDF_SCRIPT and PDF_SCRIPT.index(f'"{review22}"') < PDF_SCRIPT.index(f'"{expected_capstone[-1]}"'):
    missing.append("第22組復習を学習順110の後に置いてください")
if not (ROOT / "tests/PgBlazorSamples.UnitTests/CapstoneIntegrationTests.cs").is_file():
    missing.append("総合演習のPostgreSQL18否定試験が不足")


# 学習順001〜005の全面改稿では、読み物としての分量・表・コード・演習・解答を必須にする。
# この品質基準は次の5件単位の全面改稿にも同様に展開する。
for n in range(1, 6):
    item = ROOT / f"docs/course-v3/lessons/web-0{n}.md"
    if not item.is_file():
        missing.append(str(item.relative_to(ROOT)) + "：本文不足")
        continue
    body = item.read_text(encoding="utf-8")
    if len(body) < 3300:
        missing.append(item.name + "：長文解説が不足（最低3300文字）")
    if body.count("| ---") < 2 or body.count(" | ") < 8:
        missing.append(item.name + "：比較・切り分け表が不足")
    if body.count("```") < 4 or body.count("```") % 2:
        missing.append(item.name + "：実装に沿うコード例が2件以上必要")
    if "演習" not in body or "解答" not in body or not any(word in body for word in ("失敗", "切り分け", "症状", "うまく動かない")):
        missing.append(item.name + "：演習・解答・失敗時の説明が不足")
    if "src/PgBlazorSamples.Web" in body or "github.com" in body or "レポジトリ" in body:
        missing.append(item.name + "：学習者に不要な管理情報を含んでいます")
for path, minimum in [
    ("docs/course-v3/stages/stage01.md", 2200),
    ("docs/course-v3/reviews/review-01.md", 2300),
]:
    file = ROOT / path
    if not file.is_file():
        missing.append(path + "：段階概要・復習が不足")
        continue
    body = file.read_text(encoding="utf-8")
    if len(body) < minimum or " | " not in body or "解答" not in body or "```" not in body:
        missing.append(path + "：長文・表・コード・解答のいずれかが不足")
# HTML画面、教材、PDF索引の実装を対応させる。
web01 = SAMPLES / "WebFoundation"
for n in range(1, 6):
    page = web01 / f"Web0{n}Page.razor"
    if not page.is_file() or f'@page "/learn/web-0{n}"' not in page.read_text(encoding="utf-8"):
        missing.append(f"web-0{n}：Razor画面とURLの不一致")
    marker = f'"docs/course-v3/lessons/web-0{n}.md"'
    if marker not in PDF_SCRIPT:
        missing.append(f"web-0{n}：PDF目次へ未収録")
if not (web01 / "WebFoundationOverviewPage.razor").is_file() or not (web01 / "WebFoundationReviewPage.razor").is_file():
    missing.append("第1段階概要・第1組復習画面の欠落")


# 学習順006〜010の全面改稿を第1組と同じ基準で受入検査する。
second_group = [
    ("step01", "docs/samples/step01.md", "/learn/01", "@page"),
    ("step02", "docs/samples/step02.md", "/learn/02", "NavLink"),
    ("step03", "docs/samples/step03.md", "/learn/03", "sampleText"),
    ("runtime-01", "docs/course-v3/lessons/runtime-01.md", "/learn/runtime-01", "Interactive Server"),
    ("step04", "docs/samples/step04.md", "/learn/04", "Decrement"),
]
for lesson_id, filename, url, symbol in second_group:
    file = ROOT / filename
    if not file.is_file():
        missing.append(filename + "：全面改稿本文が不足")
        continue
    text = file.read_text(encoding="utf-8")
    if len(text) < 3300:
        missing.append(filename + "：長文教材が3300文字未満")
    if text.count("| ---") < 2 or text.count(" | ") < 8:
        missing.append(filename + "：比較表・診断表が不足")
    if text.count("```") < 4 or text.count("```") % 2:
        missing.append(filename + "：コードブロック2件以上が必要")
    if not all(token in text for token in ("演習", "解答", "切り分け", "WinForms", url, symbol)):
        missing.append(filename + "：目標・URL・操作・解説が不足")
    if any(forbidden in text for forbidden in ("src/PgBlazorSamples.Web", "github.com", "レポジトリ")):
        missing.append(filename + "：学習者PDFに不要な管理用パス")
    if f'"{filename}"' not in PDF_SCRIPT:
        missing.append(filename + "：PDF学習目次にありません")

for filename, minimum in [
    ("docs/course-v3/stages/stage02.md", 2500),
    ("docs/course-v3/reviews/review-02.md", 2500),
]:
    file = ROOT / filename
    if not file.is_file():
        missing.append(filename + "：第2段階概要・復習が不足")
        continue
    text = file.read_text(encoding="utf-8")
    if len(text) < minimum or text.count("| ---") < 2 or text.count("```") < 2 or "解答" not in text:
        missing.append(filename + "：説明・表・コード・解答のいずれかが不足")

second_sources = [x[1] for x in second_group]
pdf_positions = [PDF_SCRIPT.find(f'"{name}"') for name in second_sources]
review_pos = PDF_SCRIPT.find('"docs/course-v3/reviews/review-02.md"')
stage_pos = PDF_SCRIPT.find('"docs/course-v3/stages/stage02.md"')
if -1 in pdf_positions or pdf_positions != sorted(pdf_positions):
    missing.append("学習順006〜010のPDF収録順が不正")
if stage_pos < 0 or stage_pos > pdf_positions[0]:
    missing.append("第2段階概要が006の前にありません")
if review_pos < pdf_positions[-1] or review_pos > PDF_SCRIPT.find('"docs/samples/step05.md"'):
    missing.append("第2組復習が010と011の間にありません")
if not all(url in (ROOT / "src/PgBlazorSamples.Web/Components/Pages/Home.razor").read_text(encoding="utf-8") for _, _, url, _ in second_group):
    missing.append("学習順006〜010のWeb目次が不足")
for path, target in [
    ("Step01/FirstPage.razor", 'href="/learn/02"'),
    ("Step02/NavigationPage.razor", 'href="/learn/03"'),
    ("Step03/RazorSyntaxPage.razor", 'href="/learn/runtime-01"'),
    ("Runtime/Runtime01Page.razor", 'href="/learn/04"'),
    ("Step04/EventPage.razor", 'href="/learn/column/runtime-10"'),
    ("Runtime/RuntimeReview02Page.razor", 'href="/learn/05"'),
]:
    source = SAMPLES / path
    if not source.is_file() or target not in source.read_text(encoding="utf-8"):
        missing.append(path + "：学習順の前後リンク不一致")



# 学習順011〜015と第3組を受入候補として品質検査する。
third_group = [
    ("docs/samples/step05.md", "/learn/05", "@bind"),
    ("docs/samples/step06.md", "/learn/06", "@foreach"),
    ("docs/samples/step07.md", "/learn/07", "[Parameter]"),
    ("docs/samples/step08.md", "/learn/08", "EventCallback"),
    ("docs/course-v3/lessons/runtime-02.md", "/learn/runtime-02", "SignalR"),
]
for filename, url, required in third_group:
    body = (ROOT / filename).read_text(encoding="utf-8")
    if len(body) < 3300:
        missing.append(filename + "：第3組の本文が3300文字未満")
    if body.count("| ---") < 2 or body.count(" | ") < 8:
        missing.append(filename + "：比較表・診断表が不足")
    if body.count("```") < 4 or body.count("```") % 2:
        missing.append(filename + "：実コードが2ブロック未満")
    if not all(token in body for token in ("WinForms", "演習", "解答", "切り分け", url, required)):
        missing.append(filename + "：実装に対応した説明や問題が不足")
    if any(token in body for token in ("src/PgBlazorSamples.Web", "github.com", "レポジトリ")):
        missing.append(filename + "：PDF本文に学習者向けではない管理用情報")
    if f'"{filename}"' not in PDF_SCRIPT:
        missing.append(filename + "：PDFの章順に存在しません")
third_review = ROOT / "docs/course-v3/reviews/review-03.md"
review_body = third_review.read_text(encoding="utf-8")
if len(review_body) < 2600 or review_body.count("| ---") < 2 or review_body.count("```") < 4 or "解答" not in review_body:
    missing.append("第3組の復習に比較表・コード・詳しい解答が不足")
stage2_text = (ROOT / "docs/course-v3/stages/stage02.md").read_text(encoding="utf-8")
if len(stage2_text) < 3000 or "011〜015" not in stage2_text or "第3組" not in stage2_text:
    missing.append("第2段階の概要が第3組に未対応")
if [ordered[i]["id"] for i in range(10, 15)] != ["step05", "step06", "step07", "step08", "runtime-02"]:
    missing.append("第3組の学習順011〜015が不正")
third_sources = [name for name, _, _ in third_group]
third_pdf_indices = [PDF_SCRIPT.find(f'"{name}"') for name in third_sources]
third_review_pos = PDF_SCRIPT.find('"docs/course-v3/reviews/review-03.md"')
next_group_pos = PDF_SCRIPT.find('"docs/samples/step09.md"')
if -1 in third_pdf_indices or third_pdf_indices != sorted(third_pdf_indices) or not(third_pdf_indices[-1] < third_review_pos < next_group_pos):
    missing.append("第3組のPDF章順または復習位置が不正")
home = (ROOT / "src/PgBlazorSamples.Web/Components/Pages/Home.razor").read_text(encoding="utf-8")
for _, url, _ in third_group:
    if url not in home:
        missing.append(url + "：Web目次に不足")
if not (
    home.index('>学習順010を開く</a>') < home.index('>学習順006〜010の復習</a>')
    < home.index('>学習順011を開く</a>') < home.index('>学習順015を開く</a>')
    < home.index('>学習順011〜015の復習</a>') < home.index('>学習順016を開く</a>')
):
    missing.append("Web目次内の第2・第3組の復習位置が不正")
for relative, target in [
    ("Step05/BindingPage.razor", 'href="/learn/06"'),
    ("Step06/ConditionsPage.razor", 'href="/learn/07"'),
    ("Step07/ParametersPage.razor", 'href="/learn/08"'),
    ("Step08/CallbackPage.razor", 'href="/learn/runtime-02"'),
    ("Runtime/Runtime02Page.razor", 'href="/learn/column/runtime-15"'),
    ("Runtime/RuntimeReview03Page.razor", 'href="/learn/09"'),
]:
    page = SAMPLES / relative
    if not page.is_file() or target not in page.read_text(encoding="utf-8"):
        missing.append(relative + "：第3組の前後リンクが不正")

if missing:
    raise SystemExit("教材の整合性検査失敗：\n" + "\n".join("- " + item for item in missing))
print(f"教材検査成功：ステップ{len(steps)}件・コラム{max(steps) // 5 + (1 if max(steps) >= 20 else 0) + (1 if max(steps) >= 40 else 0) + (1 if max(steps) >= 65 else 0)}件。")
