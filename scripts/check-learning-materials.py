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
LESSONS = SAMPLES.parent / "Lessons"
_STAGE01_MOVED_PAGES = {
    1: "L001WebExecutionPage.razor",
    2: "L002HttpRequestResponsePage.razor",
    3: "L003HtmlCssFormPage.razor",
    4: "L004DomNetworkConsolePage.razor",
    5: "L005VisualStudioDebugPage.razor",
}

_STAGE02_MOVED_PAGES = {
    6: "L006FirstBlazorPage.razor",
    7: "L007NavigationPage.razor",
    8: "L008RazorSyntaxPage.razor",
    9: "L009StaticSsrInteractivePage.razor",
    10: "L010EventPage.razor",
    11: "L011TwoWayBindingPage.razor",
    12: "L012ConditionalRenderingPage.razor",
    13: "L013ComponentParametersPage.razor",
    14: "L014EventCallbackPage.razor",
    15: "L015CircuitReconnectPage.razor",
    16: "L016ComponentLifecyclePage.razor",
    17: "L017PrerenderInitializationPage.razor",
    18: "L018ValidatedFormPage.razor",
    19: "L019UrlHistoryStatePage.razor",
    20: "L020StatePersistenceChoicePage.razor",
}

_STAGE03_MOVED_PAGES = {
    21: "L021DependencyInjectionPage.razor",
    22: "L022InterfaceClockPage.razor",
    23: "L023ServiceLifetimesPage.razor",
    24: "L024CircuitScopedPage.razor",
    25: "L025InjectionPatternsPage.razor",
    26: "L026OptionsPatternPage.razor",
    27: "L027AsyncLoggingPage.razor",
    28: "L028DependencyUnitTestPage.razor",
    29: "L029ApplicationLayerPage.razor",
    30: "L030TodoServicePage.razor",
}


def web_foundation_page(number: int) -> Path:
    """第1段階の主Razorを一か所の配置定義から解決する。"""
    if number in _STAGE01_MOVED_PAGES:
        return LESSONS / "Stage01" / _STAGE01_MOVED_PAGES[number]
    return SAMPLES / "WebFoundation" / f"Web{number:02d}Page.razor"
PDF_SCRIPT = (ROOT / "scripts" / "build_docs_pdf.py").read_text(encoding="utf-8")

# 以前の1〜4番相当の主Razorは現行学習順006〜008・010としてStage02へ移行済み。
# 残りの旧Stepフォルダーと併せ、従来の80教材の欠落を検出する。
disk_steps = sorted(int(path.name[4:]) for path in SAMPLES.glob("Step[0-9][0-9]") if path.is_dir())
moved_steps = {1: 6, 2: 7, 3: 8, 4: 10, 5: 11, 6: 12, 7: 13, 8: 14, 9: 16, 10: 18, **{i: i + 10 for i in range(11, 21)}}
steps = sorted(set(disk_steps) | set(moved_steps))
if not steps:
    raise SystemExit("教材検査エラー：実装済みのステップがありません。")
if steps != list(range(1, max(steps) + 1)):
    raise SystemExit("教材検査エラー：ステップ番号に欠番があります：" + repr(steps))

missing = []
for number in steps:
    if number in moved_steps:
        sequence = moved_steps[number]
        source = (LESSONS / "Stage02" / _STAGE02_MOVED_PAGES[sequence]) if sequence <= 20 else (LESSONS / "Stage03" / _STAGE03_MOVED_PAGES[sequence])
        if not source.is_file():
            missing.append(f"{source.relative_to(ROOT)}：移行済みRazor画面なし")
    else:
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
canonical_for_compatibility = {item["url"]: f'/learn/{item["sequence"]:03d}' for item in ordered}

def learner_text(body: str) -> str:
    """学習者PDFで除去する実装リンクだけを除き、管理情報の混入を検査する。"""
    return "\n".join(line for line in body.splitlines()
                     if not line.startswith("**実装ソース：** [対応するRazor実装]("))
if len(ordered) != 110 or [item["sequence"] for item in ordered] != list(range(1, 111)):
    missing.append("v3の110学習順が不正")
if len({item["id"] for item in ordered}) != 110:
    missing.append("v3の学習IDに重複がある")
if len(curriculum["stages"]) != 9 or len(curriculum["reviews"]) != 22:
    missing.append("v3の段階/復習組数が不正")
newpaths = []
for n in range(1, 6):
    item = ordered[n-1]
    razor = web_foundation_page(n)
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
    seq = {1: 9, 2: 15, 3: 17, 4: 19, 5: 20}[n]
    razor = LESSONS / "Stage02" / _STAGE02_MOVED_PAGES[seq]
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
    if "src/PgBlazorSamples.Web" in learner_text(body) or "github.com" in learner_text(body) or "レポジトリ" in learner_text(body):
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
    page = web_foundation_page(n)
    if not page.is_file() or f'@page "/learn/web-0{n}"' not in page.read_text(encoding="utf-8"):
        missing.append(f"web-0{n}：Razor画面とURLの不一致")
    marker = f'"docs/course-v3/lessons/web-0{n}.md"'
    if marker not in PDF_SCRIPT:
        missing.append(f"web-0{n}：PDF目次へ未収録")
web03 = web_foundation_page(3)
web03_css = web03.with_name(web03.name + ".css")
if not web03_css.is_file() or ".web-foundation-card" not in web03_css.read_text(encoding="utf-8"):
    missing.append("学習順003：主Razorと隣接するCSS isolationファイルが見つかりません")
if "PgBlazorSamples.Web.styles.css" not in (SAMPLES.parent / "App.razor").read_text(encoding="utf-8"):
    missing.append("学習順003：App.razorのCSS isolationバンドル参照がありません")
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
    if not all(token in text for token in ("演習", "解答", "切り分け", "WinForms", canonical_for_compatibility.get(url, url), symbol)):
        missing.append(filename + "：目標・URL・操作・解説が不足")
    if any(forbidden in learner_text(text) for forbidden in ("src/PgBlazorSamples.Web", "github.com", "レポジトリ")):
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
home_for_second_group = (ROOT / "src/PgBlazorSamples.Web/Components/Pages/Home.razor").read_text(encoding="utf-8")
second_urls = {item["url"]: f'/learn/{item["sequence"]:03d}' for item in ordered}
if not all(f'href="{second_urls.get(url, url)}"' in home_for_second_group for _, _, url, _ in second_group):
    missing.append("学習順006〜010のWeb目次が不足")
# 移行済み主Razorは学習順に対応する配置定義から参照する。
for number, target in [
    (6, 'href="/learn/007"'),
    (7, 'href="/learn/008"'),
    (8, 'href="/learn/009"'),
    (9, 'href="/learn/010"'),
    (10, 'href="/learn/column/runtime-10"'),
]:
    page = LESSONS / "Stage02" / _STAGE02_MOVED_PAGES[number]
    if not page.is_file() or target not in page.read_text(encoding="utf-8"):
        missing.append(str(page) + "：学習順の前後リンク不一致")
review_page = SAMPLES / "Runtime" / "RuntimeReview02Page.razor"
if not review_page.is_file() or 'href="/learn/011"' not in review_page.read_text(encoding="utf-8"):
    missing.append(str(review_page) + "：学習順の前後リンク不一致")



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
    if not all(token in body for token in ("WinForms", "演習", "解答", "切り分け", canonical_for_compatibility.get(url, url), required)):
        missing.append(filename + "：実装に対応した説明や問題が不足")
    if any(token in learner_text(body) for token in ("src/PgBlazorSamples.Web", "github.com", "レポジトリ")):
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
# ホームは新学習順の正規URLへ案内する。以前のURLは内部の互換用経路。
canonical_lookup = {item["url"]: f'/learn/{item["sequence"]:03d}' for item in ordered}
home = (ROOT / "src/PgBlazorSamples.Web/Components/Pages/Home.razor").read_text(encoding="utf-8")
def indexed_home(url):
    canonical = canonical_lookup.get(url, url)
    return f'href="{canonical}"' in home

for _, url, _ in third_group:
    if not indexed_home(url):
        missing.append(url + "：Web目次に不足")
if not (
    home.index('>学習順010を開く</a>') < home.index('>学習順006〜010の復習</a>')
    < home.index('>学習順011を開く</a>') < home.index('>学習順015を開く</a>')
    < home.index('>学習順011〜015の復習</a>') < home.index('>学習順016を開く</a>')
):
    missing.append("Web目次内の第2・第3組の復習位置が不正")
for number, target in [
    (11, 'href="/learn/012"'),
    (12, 'href="/learn/013"'),
    (13, 'href="/learn/014"'),
    (14, 'href="/learn/015"'),
    (15, 'href="/learn/column/runtime-15"'),
]:
    page = LESSONS / "Stage02" / _STAGE02_MOVED_PAGES[number]
    if not page.is_file() or target not in page.read_text(encoding="utf-8"):
        missing.append(str(page.relative_to(ROOT)) + "：第3組の前後リンクが不正")
review_page = SAMPLES / "Runtime" / "RuntimeReview03Page.razor"
if not review_page.is_file() or 'href="/learn/016"' not in review_page.read_text(encoding="utf-8"):
    missing.append(str(review_page.relative_to(ROOT)) + "：第3組の前後リンクが不正")

# 学習順016〜020・第4組の全面改稿品質とPDF・Web画面の導線を検査する。
fourth_group = [
    ("docs/samples/step09.md", "/learn/09", "OnInitializedAsync"),
    ("docs/course-v3/lessons/runtime-03.md", "/learn/runtime-03", "OnAfterRender"),
    ("docs/samples/step10.md", "/learn/10", "OnValidSubmit"),
    ("docs/course-v3/lessons/runtime-04.md", "/learn/runtime-04", "SupplyParameterFromQuery"),
    ("docs/course-v3/lessons/runtime-05.md", "/learn/runtime-05", "Select"),
]
for filename, url, symbol in fourth_group:
    body = (ROOT / filename).read_text(encoding="utf-8")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(" | ") < 8:
        missing.append(filename + "：3300文字・比較表・診断表が不足")
    if body.count("```") < 4 or body.count("```") % 2:
        missing.append(filename + "：実コード2ブロック以上が必要")
    if not all(word in body for word in ("WinForms", "演習", "解答", "切り分け", canonical_for_compatibility.get(url, url), symbol)):
        missing.append(filename + "：必須説明・学習順・実装構文が不足")
    if any(word in learner_text(body) for word in ("src/PgBlazorSamples.Web", "github.com", "レポジトリ")):
        missing.append(filename + "：管理情報を学習者本文に記載しないこと")
    if f'"{filename}"' not in PDF_SCRIPT:
        missing.append(filename + "：PDF章順に不足")
if [ordered[i]["id"] for i in range(15, 20)] != ["step09", "runtime-03", "step10", "runtime-04", "runtime-05"]:
    missing.append("第4組の学習順が不一致")
fourth_positions = [PDF_SCRIPT.find(f'"{name}"') for name, _, _ in fourth_group]
fourth_review_pos = PDF_SCRIPT.find('"docs/course-v3/reviews/review-04.md"')
next_position = PDF_SCRIPT.find('"docs/samples/step11.md"')
if -1 in fourth_positions or fourth_positions != sorted(fourth_positions) or not (fourth_positions[-1] < fourth_review_pos < next_position):
    missing.append("第4組PDF章順・復習位置が不正")
review_four = (ROOT / "docs/course-v3/reviews/review-04.md").read_text(encoding="utf-8")
if len(review_four) < 2600 or review_four.count("| ---") < 2 or review_four.count("```") < 4 or "解答" not in review_four:
    missing.append("第4組の復習は長文・比較/診断表・コード・詳しい解答が必要")
for number, expected in [
    (16, 'href="/learn/017"'),
    (17, 'href="/learn/018"'),
    (18, 'href="/learn/019"'),
    (19, 'href="/learn/020"'),
    (20, 'href="/learn/column/runtime-20"'),
]:
    page = LESSONS / "Stage02" / _STAGE02_MOVED_PAGES[number]
    if not page.is_file() or expected not in page.read_text(encoding="utf-8"):
        missing.append(str(page.relative_to(ROOT)) + "：第4組の前後リンク不一致")
review_page = SAMPLES / "Runtime" / "RuntimeReview04Page.razor"
if not review_page.is_file() or 'href="/learn/021"' not in review_page.read_text(encoding="utf-8"):
    missing.append(str(review_page.relative_to(ROOT)) + "：第4組の前後リンク不一致")
for _, url, _ in fourth_group:
    if not indexed_home(url):
        missing.append(url + "：Web目次に第4組の学習URLが不足")
if "016〜020" not in (ROOT / stage2).read_text(encoding="utf-8"):
    missing.append("第2段階概要に第4組の学習順が不足")


# 学習順021〜025：第3段階のDI教材は、長文・実コード・診断・画面とPDF順を同時に照合する。
di_group = [
    ("docs/samples/step11.md", "/learn/11", "GreetingService", "L021DependencyInjectionPage.razor", 'href="/learn/022"'),
    ("docs/samples/step12.md", "/learn/12", "IClock", "L022InterfaceClockPage.razor", 'href="/learn/023"'),
    ("docs/samples/step13.md", "/learn/13", "GetRequiredService", "L023ServiceLifetimesPage.razor", 'href="/learn/024"'),
    ("docs/samples/step14.md", "/learn/14", "StateHasChanged", "L024CircuitScopedPage.razor", 'href="/learn/025"'),
    ("docs/samples/step15.md", "/learn/15", "GreetingFormatter", "L025InjectionPatternsPage.razor", 'href="/learn/column/15"'),
]
if [item["id"] for item in ordered[20:25]] != [f"step{n}" for n in range(11, 16)]:
    missing.append("第5組の学習順021〜025の対応に不一致")
for idx, (file_name, url, word, razor_name, next_link) in enumerate(di_group, start=21):
    file = ROOT / file_name
    if not file.is_file():
        missing.append(file_name + "：DI教材の本文がありません")
        continue
    body = file.read_text(encoding="utf-8")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(" | ") < 8:
        missing.append(file_name + "：比較・診断表または3300文字以上の本文が不足")
    if body.count("```") < 4 or body.count("```") % 2:
        missing.append(file_name + "：実コード2ブロック以上が必要")
    if not all(token in body for token in ("WinForms", "Dapper", "演習", "解答", "切り分け", canonical_for_compatibility.get(url, url), word)):
        missing.append(file_name + "：実画面・WinForms比較・演習・解答の説明が不足")
    if any(token in learner_text(body) for token in ("src/PgBlazorSamples.Web", "github.com", "レポジトリ")):
        missing.append(file_name + "：PDFへ開発管理情報を持ち込まないでください")
    razor = LESSONS / "Stage03" / razor_name
    if not razor.is_file():
        missing.append(razor_name + "：Razor画面不足")
    else:
        razor_text = razor.read_text(encoding="utf-8")
        if f'@page "{url}"' not in razor_text or next_link not in razor_text or f"学習順{idx:03d}" not in razor_text:
            missing.append(razor_name + "：URL・移動リンク・学習順が一致しません")
    if f'"{file_name}"' not in PDF_SCRIPT:
        missing.append(file_name + "：PDFに収録されていません")

di_stage = "docs/course-v3/stages/stage03.md"
di_review = "docs/columns/column11-15.md"
for name, minimum in ((di_stage, 2700), (di_review, 3200)):
    path = ROOT / name
    if not path.is_file():
        missing.append(name + "：第3段階の概要または第5組の復習不足")
        continue
    body = path.read_text(encoding="utf-8")
    if len(body) < minimum or body.count("| ---") < 2 or body.count("```") < 4 or "解答" not in body:
        missing.append(name + "：長文・比較/診断表・コード・解答が不足")
di_overview = SAMPLES / "Step11/DiOverviewPage.razor"
di_review_page = SAMPLES / "Columns/Column15Page.razor"
if not di_overview.is_file() or '@page "/learn/stage/di"' not in di_overview.read_text(encoding="utf-8"):
    missing.append("第3段階の概要RazorまたはURLがありません")
if not di_review_page.is_file() or 'href="/learn/026"' not in di_review_page.read_text(encoding="utf-8"):
    missing.append("第5組の復習Razorから学習順026への導線不足")
pdf_di = [PDF_SCRIPT.find(f'"{row[0]}"') for row in di_group]
pdf_stage3 = PDF_SCRIPT.find(f'"{di_stage}"')
pdf_review5 = PDF_SCRIPT.find(f'"{di_review}"')
pdf_next = PDF_SCRIPT.find('"docs/samples/step16.md"')
if -1 in pdf_di or not (0 <= pdf_stage3 < pdf_di[0]) or pdf_di != sorted(pdf_di) or not (pdf_di[-1] < pdf_review5 < pdf_next):
    missing.append("第3段階021〜025の概要・本文・復習とPDF順が不一致")
if not all(f'学習順{seq:03d}を開く' in home for seq in range(21, 26)) or "/learn/stage/di" not in home or "第5組の復習" not in home:
    missing.append("第3段階のWeb目次に学習順と第5組の復習が不足")
nav_di = (ROOT / "src/PgBlazorSamples.Web/Components/Layout/NavMenu.razor").read_text(encoding="utf-8")
if "/learn/stage/di" not in nav_di or not all(f"学習順{seq:03d}：" in nav_di for seq in range(21, 26)):
    missing.append("第3段階のナビゲーションに不足")
if 'href="/learn/column/runtime-20"' not in (LESSONS / "Stage03/L021DependencyInjectionPage.razor").read_text(encoding="utf-8"):
    missing.append("学習順020の復習から021への接続が不足")


# 第6組：学習順026〜030の本文・Razor画面・PDF・復習を検査する。
sixth_group = [
    ("docs/samples/step16.md", "/learn/16", "IOptions", "L026OptionsPatternPage.razor"),
    ("docs/samples/step17.md", "/learn/17", "CancellationToken", "L027AsyncLoggingPage.razor"),
    ("docs/samples/step18.md", "/learn/18", "IClock", "L028DependencyUnitTestPage.razor"),
    ("docs/samples/step19.md", "/learn/19", "StepSummaryService", "L029ApplicationLayerPage.razor"),
    ("docs/samples/step20.md", "/learn/20", "ITodoService", "L030TodoServicePage.razor"),
]
if [item["id"] for item in ordered[25:30]] != [f"step{n}" for n in range(16, 21)]:
    missing.append("第6組学習順026〜030の対応が不一致")
for seq, (filename, url, symbol, razor_name) in enumerate(sixth_group, start=26):
    content = (ROOT / filename).read_text(encoding="utf-8")
    if len(content) < 3300 or content.count("| ---") < 2 or content.count(" | ") < 8:
        missing.append(filename + "：3300文字・比較表・診断表が不足")
    if content.count("```") < 4 or content.count("```") % 2:
        missing.append(filename + "：実コード2ブロック以上が必要")
    if not all(token in content for token in ("WinForms", "Dapper", "演習", "解答", "切り分け", symbol, canonical_for_compatibility.get(url, url))):
        missing.append(filename + "：実装比較・切り分け・演習・解答が不足")
    if any(token in learner_text(content) for token in ("src/PgBlazorSamples.Web", "github.com", "レポジトリ")):
        missing.append(filename + "：教材へ管理情報を含めない")
    razor = (LESSONS / "Stage03" / razor_name).read_text(encoding="utf-8")
    if f'@page "{url}"' not in razor or f"学習順{seq}" not in razor or f'data-testid="di06-compare-{seq - 10}"' not in razor:
        missing.append(razor_name + "：URL・表示順・比較表が不一致")
    if not indexed_home(url) or f'"{filename}"' not in PDF_SCRIPT:
        missing.append(filename + "：Web/PDF索引が不足")
pdf_six = [PDF_SCRIPT.find(f'"{filename}"') for filename, _, _, _ in sixth_group]
pdf_review6 = PDF_SCRIPT.find('"docs/columns/column16-20.md"')
pdf_library_di = PDF_SCRIPT.find('"docs/columns/column-library-di.md"')
pdf_step21 = PDF_SCRIPT.find('"docs/samples/step21.md"')
if -1 in pdf_six or pdf_six != sorted(pdf_six) or not (pdf_six[-1] < pdf_review6 < pdf_library_di < pdf_step21):
    missing.append("第6組から復習・ライブラリDI・031へのPDF章順が不正")
review6 = (ROOT / "docs/columns/column16-20.md").read_text(encoding="utf-8")
if len(review6) < 3300 or review6.count("| ---") < 2 or review6.count("```") < 4 or "詳しい解答" not in review6:
    missing.append("第6組復習の長文・コード・表・詳しい解答が不足")
review_page = (SAMPLES / "Columns/Column20Page.razor").read_text(encoding="utf-8")
if 'data-testid="di06-review-table"' not in review_page or 'href="/learn/column/library-di"' not in review_page:
    missing.append("第6組復習Razorと特別コラムへの導線が不足")

# 第7組031〜035の実装・教材・復習・PDF章順をまとめて受入検査する。
mud_seventh = [
    ("docs/samples/step21.md", "/learn/21", "AddMudServices", "Step21/MudIntroductionPage.razor"),
    ("docs/samples/step22.md", "/learn/22", "MudDrawer", "Step22/MudLayoutPage.razor"),
    ("docs/samples/step23.md", "/learn/23", "MudDatePicker", "Step23/MudInputsPage.razor"),
    ("docs/samples/step24.md", "/learn/24", "ValidateAsync", "Step24/MudValidationPage.razor"),
    ("docs/samples/step25.md", "/learn/25", "RowsPerPage", "Step25/MudTablePage.razor"),
]
if [item["id"] for item in ordered[30:35]] != [f"step{n}" for n in range(21, 26)]:
    missing.append("第7組031〜035の学習順が不正")
for seq, (filename, url, symbol, razor_name) in enumerate(mud_seventh, start=31):
    content = (ROOT / filename).read_text(encoding="utf-8")
    if len(content) < 3300 or content.count("| ---") < 2 or content.count(" | ") < 8:
        missing.append(filename + "：本文3300文字と比較・切り分け表が不足")
    if content.count("```") < 4 or content.count("```") % 2:
        missing.append(filename + "：実コード2ブロック以上が必要")
    if not all(token in content for token in ("WinForms", "Dapper", "演習", "解答", "切り分け", symbol, canonical_for_compatibility.get(url, url))):
        missing.append(filename + "：比較・コード・操作・演習・詳細解答の対応不足")
    if any(token in learner_text(content) for token in ("src/PgBlazorSamples.Web", "github.com", "レポジトリ")):
        missing.append(filename + "：学習者本文へ管理用情報を含めない")
    razor = (SAMPLES / razor_name).read_text(encoding="utf-8")
    if f'@page "{url}"' not in razor or f"学習順{seq:03d}" not in razor or f'data-testid="mud07-compare-{seq - 10}"' not in razor:
        missing.append(razor_name + "：URL・学習順・比較表が不一致")
    if f'"{filename}"' not in PDF_SCRIPT or not indexed_home(url):
        missing.append(filename + "：PDF/Webの学習順から欠落")
review7 = (ROOT / "docs/columns/column21-25.md").read_text(encoding="utf-8")
stage4 = (ROOT / "docs/course-v3/stages/stage04.md").read_text(encoding="utf-8")
if len(review7) < 3300 or review7.count("| ---") < 2 or review7.count("```") < 4 or "詳しい解答" not in review7:
    missing.append("第7組復習の長文・比較/診断表・実コード・詳答が不足")
if len(stage4) < 2900 or stage4.count("| ---") < 2 or stage4.count("```") < 4 or "031〜035" not in stage4:
    missing.append("第4段階概要の構成不足")
positions = [PDF_SCRIPT.find(f'"{filename}"') for filename, _, _, _ in mud_seventh]
if -1 in positions or positions != sorted(positions) or not (
    PDF_SCRIPT.find('"docs/course-v3/stages/stage04.md"') < positions[0]
    and positions[-1] < PDF_SCRIPT.find('"docs/columns/column21-25.md"') < PDF_SCRIPT.find('"docs/samples/step26.md"')
):
    missing.append("第7組の段階概要・本文・復習・次の章のPDF順が不正")
for relative, next_link in [
    ("Step25/MudTablePage.razor", 'href="/learn/column/25"'),
    ("Columns/Column25Page.razor", 'href="/learn/036"'),
    ("Browser/BrowserOverviewPage.razor", 'href="/learn/031"'),
]:
    if next_link not in (SAMPLES / relative).read_text(encoding="utf-8"):
        missing.append(relative + "：前後リンクの不足")
if not all(f"学習順{i:03d}を開く" in home for i in range(31, 36)) or "学習順031〜035の復習" not in home:
    missing.append("Web目次の第7組と復習の並びが不正")


# 学習順036〜040：第8組の本文・Razor・Web/PDF目次・復習を同じ条件で検査する。
mud_eighth = [
    ("docs/samples/step26.md", "/learn/26", "QuickFilter", "Step26/DataGridPage.razor"),
    ("docs/samples/step27.md", "/learn/27", "ShowMessageBoxAsync", "Step27/DialogPage.razor"),
    ("docs/samples/step28.md", "/learn/28", "MudProgressLinear", "Step28/NotificationPage.razor"),
    ("docs/samples/step29.md", "/learn/29", "ThemePreferenceService", "Step29/AccessibilityPage.razor"),
    ("docs/samples/step30.md", "/learn/30", "PracticeUser", "Step30/UiPracticePage.razor"),
]
if [item["id"] for item in ordered[35:40]] != [f"step{n}" for n in range(26, 31)]:
    missing.append("第8組の学習順036〜040が不一致")
for seq, (filename, url, symbol, razor_name) in enumerate(mud_eighth, start=36):
    content = (ROOT / filename).read_text(encoding="utf-8")
    if len(content) < 3300 or content.count("| ---") < 2 or content.count(" | ") < 8:
        missing.append(filename + "：本文3300文字または比較・切り分け表不足")
    if content.count("```") < 4 or content.count("```") % 2:
        missing.append(filename + "：実コード2ブロック以上が必要")
    if not all(token in content for token in ("WinForms", "Dapper", "演習", "解答", "切り分け", symbol, canonical_for_compatibility.get(url, url), f"学習順{seq}")):
        missing.append(filename + "：実装・演習・操作の対応不足")
    if any(token in learner_text(content) for token in ("src/PgBlazorSamples.Web", "github.com", "レポジトリ")):
        missing.append(filename + "：学習者本文へ管理情報を持ち込まない")
    razor = (LESSONS / "Stage03" / razor_name).read_text(encoding="utf-8")
    if f'@page "{url}"' not in razor or f"学習順{seq}" not in razor or f'data-testid="mud08-compare-{seq}"' not in razor:
        missing.append(razor_name + "：Razor学習順・表・URLの不一致")
    if f'"{filename}"' not in PDF_SCRIPT or f"学習順{seq:03d}を開く" not in home:
        missing.append(filename + "：PDF/Web索引の学習順不足")
review8 = (ROOT / "docs/columns/column26-30.md").read_text(encoding="utf-8")
review8_razor = (SAMPLES / "Columns/Column30Page.razor").read_text(encoding="utf-8")
if len(review8) < 3300 or review8.count("| ---") < 2 or review8.count("```") < 4 or "詳しい解答" not in review8:
    missing.append("第8組復習の長文・表・コード・詳しい解答が不足")
if 'data-testid="mud08-review-table"' not in review8_razor or 'href="/learn/041"' not in review8_razor:
    missing.append("第8組復習Razorと次の041への導線不足")
if "/learn/column/30" not in home or "学習順036〜040の復習" not in home:
    missing.append("Web目次の第8組復習が不足")
nav_eighth = (ROOT / "src/PgBlazorSamples.Web/Components/Layout/NavMenu.razor").read_text(encoding="utf-8")
if "第8組の復習：036〜040" not in nav_eighth:
    missing.append("第8組のナビゲーション不足")
pdf_eighth = [PDF_SCRIPT.find(f'"{x[0]}"') for x in mud_eighth]
pdf_review8 = PDF_SCRIPT.find('"docs/columns/column26-30.md"')
pdf_browser01 = PDF_SCRIPT.find('"docs/course-v3/lessons/browser-01.md"')
if not all(f'学習順{seq:03d}：' in PDF_SCRIPT for seq in range(36, 41)) or "第8組の復習：検索・確認・通知・操作性" not in PDF_SCRIPT:
    missing.append("第8組PDFの学習順・復習見出しが一致しません")
if -1 in pdf_eighth or pdf_eighth != sorted(pdf_eighth) or not (pdf_eighth[-1] < pdf_review8 < pdf_browser01):
    missing.append("第8組036〜040→復習→041のPDF章順が不正")
if 'href="/learn/column/30"' not in (SAMPLES / "Step30/UiPracticePage.razor").read_text(encoding="utf-8"):
    missing.append("学習順040から第8組復習への導線不足")



# 学習順041〜045：第9組の最終全面改稿は本文・Razor・Web・PDF・復習を一括検査する。
browser_ninth = [
    ("browser-01", "Browser01Page.razor", "JS.InvokeAsync"),
    ("browser-02", "Browser02Page.razor", "OpenReadStream"),
    ("browser-03", "Browser03Page.razor", "DotNetStreamReference"),
    ("browser-04", "Browser04Page.razor", "CancellationToken"),
    ("browser-05", "Browser05Page.razor", "focus-visible"),
]
if [item["id"] for item in ordered[40:45]] != [x[0] for x in browser_ninth]:
    missing.append("第9組の学習順041〜045がカリキュラムと一致しません")
for seq, (item_id, page_name, required_symbol) in enumerate(browser_ninth, start=41):
    relative = f"docs/course-v3/lessons/{item_id}.md"
    content = (ROOT / relative).read_text(encoding="utf-8")
    razor = (SAMPLES / "Browser" / page_name).read_text(encoding="utf-8")
    if len(content) < 3300 or content.count("| ---") < 2 or content.count(" | ") < 8:
        missing.append(relative + "：本文3300字・比較/診断表が不足")
    if content.count(chr(96) * 3) < 4 or content.count(chr(96) * 3) % 2:
        missing.append(relative + "：実コード2ブロック以上が不足")
    for keyword in ("WinForms", "Dapper", "操作", "演習", "解答", required_symbol):
        if keyword not in content:
            missing.append(relative + f"：必要な解説語 {keyword} がありません")
    if f'@page "/learn/{item_id}"' not in razor or f"学習順{seq:03d}" not in razor:
        missing.append(page_name + "：Razor URLか学習順が一致しません")
    if f'data-testid="browser09-compare-{seq:03d}"' not in razor:
        missing.append(page_name + "：第9組WinForms比較表がありません")
    if f"/learn/{seq:03d}" not in home or f"/learn/{seq:03d}" not in nav_eighth:
        missing.append(relative + "：WebトップかNavMenuの索引がありません")
    if f'"{relative}"' not in PDF_SCRIPT or f"学習順{seq:03d}：" not in PDF_SCRIPT:
        missing.append(relative + "：PDF章題/索引の学習順が不足")
review09_doc = (ROOT / "docs/course-v3/reviews/review-09.md").read_text(encoding="utf-8")
review09_page = (SAMPLES / "Browser/BrowserReview09Page.razor").read_text(encoding="utf-8")
stage04_page = (SAMPLES / "Browser/BrowserOverviewPage.razor").read_text(encoding="utf-8")
if len(review09_doc) < 3300 or review09_doc.count("| ---") < 2 or review09_doc.count(chr(96) * 3) < 4 or "詳しい解答" not in review09_doc:
    missing.append("第9組復習の本文・比較表・実コード・詳しい解答が不足")
if 'data-testid="browser09-review-table"' not in review09_page or 'href="/learn/046"' not in review09_page:
    missing.append("第9組復習Razorの表か学習順046へのリンクがありません")
if 'data-testid="stage04-browser-compare"' not in stage04_page:
    missing.append("第4段階概要の041〜045比較表がありません")
if "学習順041〜045を復習" not in home or "第9組の復習：041〜045" not in nav_eighth:
    missing.append("第9組のWeb目次かNavMenuの復習導線がありません")
positions09 = [PDF_SCRIPT.find(f'"docs/course-v3/lessons/{item_id}.md"') for item_id, _, _ in browser_ninth]
review09_pos = PDF_SCRIPT.find('"docs/course-v3/reviews/review-09.md"')
bridge_pos = PDF_SCRIPT.find('"docs/course-v3/lessons/db-bridge-01.md"')
if -1 in positions09 or positions09 != sorted(positions09) or not (positions09[-1] < review09_pos < bridge_pos):
    missing.append("第9組041〜045→復習→046のPDF順が不正")
if 'href="/learn/column/browser-45"' not in (SAMPLES / "Browser/Browser05Page.razor").read_text(encoding="utf-8"):
    missing.append("学習順045→第9組復習へのリンク不足")

# 第10組046〜050：本文、Razor、Web目次、日本語PDF順と復習を一括検査する。
db_unit_10 = [
    ("db-bridge-01", "docs/course-v3/lessons/db-bridge-01.md", "DbBridge/DbBridge01Page.razor"),
    ("step31", "docs/samples/step31.md", "Step31/EfModelPage.razor"),
    ("step32", "docs/samples/step32.md", "Step32/SqlSchemaPage.razor"),
    ("step33", "docs/samples/step33.md", "Step33/EfSetupPage.razor"),
    ("step34", "docs/samples/step34.md", "Step34/DbFactoryPage.razor"),
]
if [item["id"] for item in ordered[45:50]] != [item[0] for item in db_unit_10]:
    missing.append("第10組046〜050のカリキュラム順が不正")
for seq, (lesson_id, path, razor_name) in enumerate(db_unit_10, start=46):
    body = (ROOT / path).read_text(encoding="utf-8")
    razor = (SAMPLES / razor_name).read_text(encoding="utf-8")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(" | ") < 8:
        missing.append(path + "：3300文字・比較表・診断表の不足")
    if body.count(chr(96) * 3) < 4 or body.count(chr(96) * 3) % 2:
        missing.append(path + "：実コード2例の不足")
    for item in ("WinForms", "Dapper", "操作", "演習", "詳しい解答", "PostgreSQL"):
        if item not in body:
            missing.append(path + "：教材の必要項目なし：" + item)
    if f"学習順{seq:03d}" not in razor or f'data-testid="dbbridge10-compare-{seq:03d}"' not in razor:
        missing.append(razor_name + "：Razorの学習順か比較表の不足")
    if f"学習順{seq:03d}を開く" not in home or f"学習順{seq:03d}：" not in nav_eighth:
        missing.append(path + "：Web目次・NavMenuの学習順が不正")
    if f'"{path}"' not in PDF_SCRIPT:
        missing.append(path + "：PDFの学習順にありません")
review10 = (ROOT / "docs/course-v3/reviews/review-10.md").read_text(encoding="utf-8")
stage05 = (ROOT / "docs/course-v3/stages/stage05.md").read_text(encoding="utf-8")
review_page = (SAMPLES / "DbBridge/DbBridgeReview10Page.razor").read_text(encoding="utf-8")
stage_page = (SAMPLES / "DbBridge/DbBridgeOverviewPage.razor").read_text(encoding="utf-8")
if len(review10) < 3300 or review10.count("| ---") < 2 or review10.count(chr(96)*3) < 4:
    missing.append("第10組復習の教材品質不足")
if len(stage05) < 3000 or stage05.count("| ---") < 2 or stage05.count(chr(96)*3) < 4:
    missing.append("第5段階概要の教材品質不足")
if 'data-testid="dbbridge10-review-table"' not in review_page or 'data-testid="stage05-db-compare"' not in stage_page:
    missing.append("第10組・第5段階概要のWeb比較表不足")
if "学習順046〜050を復習" not in home or "第10組の復習：046〜050" not in nav_eighth:
    missing.append("第10組のWeb目次・NavMenuの復習リンク不足")
if 'href="/learn/column/db-bridge-50"' not in (SAMPLES / "Step34/DbFactoryPage.razor").read_text(encoding="utf-8"):
    missing.append("学習順050から復習への遷移がありません")
pos = [PDF_SCRIPT.find(f'"{path}"') for _, path, _ in db_unit_10]
review_pos = PDF_SCRIPT.find('"docs/course-v3/reviews/review-10.md"')
stage_pos = PDF_SCRIPT.find('"docs/course-v3/stages/stage05.md"')
if -1 in pos or pos != sorted(pos) or not (stage_pos < pos[0] < pos[-1] < review_pos):
    missing.append("第5段階概要→046〜050→第10組復習のPDF目次順序が不正")


# 第11組051〜055：PDF生成停止中は索引ソースの順序だけを検査する。
unit11 = [
    ("step35", "docs/samples/step35.md", "Step35/EfSearchPage.razor"),
    ("db-bridge-02", "docs/course-v3/lessons/db-bridge-02.md", "DbBridge/DbBridge02Page.razor"),
    ("db-bridge-03", "docs/course-v3/lessons/db-bridge-03.md", "DbBridge/DbBridge03Page.razor"),
    ("step36", "docs/samples/step36.md", "Step36/EfCrudPage.razor"),
    ("step37", "docs/samples/step37.md", "Step37/EfDateRangePage.razor"),
]
if [item["id"] for item in ordered[50:55]] != [x[0] for x in unit11]:
    missing.append("第11組051〜055の学習順が不正")
for sequence, (_, relative, page_name) in enumerate(unit11, start=51):
    body = (ROOT / relative).read_text(encoding="utf-8")
    view = (SAMPLES / page_name).read_text(encoding="utf-8")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(" | ") < 8:
        missing.append(relative + "：3300文字以上・比較表・切り分け表が必要")
    if body.count(chr(96) * 3) < 4 or body.count(chr(96) * 3) % 2:
        missing.append(relative + "：実コード2例以上が必要")
    for keyword in ("WinForms", "Dapper", "PostgreSQL", "操作", "演習", "詳しい解答"):
        if keyword not in body:
            missing.append(relative + "：必須の説明不足：" + keyword)
    if f"学習順{sequence:03d}" not in view or f'data-testid="dbbridge11-compare-{sequence:03d}"' not in view:
        missing.append(page_name + "：学習順表示・比較表がない")
    if f"学習順{sequence:03d}を開く" not in home or f"学習順{sequence:03d}：" not in nav_eighth:
        missing.append(relative + "：Web目次・NavMenuが学習順と一致しない")
    if f'"{relative}"' not in PDF_SCRIPT:
        missing.append(relative + "：PDF索引ソースに収録されていない")
review11_path = "docs/course-v3/reviews/review-11.md"
review11 = (ROOT / review11_path).read_text(encoding="utf-8")
review11_page = (SAMPLES / "DbBridge/DbBridgeReview11Page.razor").read_text(encoding="utf-8")
if len(review11) < 3300 or review11.count("| ---") < 2 or review11.count(chr(96) * 3) < 4 or "詳しい解答" not in review11:
    missing.append("第11組復習は長文・比較表・実コード・詳しい解答が必要")
if 'data-testid="dbbridge11-review-table"' not in review11_page or 'href="/learn/056"' not in review11_page:
    missing.append("第11組復習のRazor比較表・学習順056へのリンクなし")
if 'href="/learn/column/db-bridge-55"' not in (SAMPLES / "Step37/EfDateRangePage.razor").read_text(encoding="utf-8"):
    missing.append("学習順055から復習への遷移がない")
if "学習順051〜055を復習" not in home or "第11組の復習：051〜055" not in nav_eighth:
    missing.append("第11組復習のWeb目次・NavMenuからの導線がない")
positions11 = [PDF_SCRIPT.find(f'"{relative}"') for _, relative, _ in unit11]
review11_position = PDF_SCRIPT.find(f'"{review11_path}"')
review10_position = PDF_SCRIPT.find('"docs/course-v3/reviews/review-10.md"')
step38_position = PDF_SCRIPT.find('"docs/samples/step38.md"')
if (-1 in positions11 or positions11 != sorted(positions11)
        or not (review10_position < positions11[0] < positions11[-1] < review11_position < step38_position)):
    missing.append("PDF目次ソースの第10組→051〜055→第11組→056の順が不正")

# 第12組056〜060：PDF停止中も教材原稿と索引ソースのみを検査する。
unit12 = [
    ("step38", "docs/samples/step38.md", "Step38/EfTransactionPage.razor"),
    ("step39", "docs/samples/step39.md", "Step39/EfConcurrencyPage.razor"),
    ("db-bridge-04", "docs/course-v3/lessons/db-bridge-04.md", "DbBridge/DbBridge04Page.razor"),
    ("db-bridge-05", "docs/course-v3/lessons/db-bridge-05.md", "DbBridge/DbBridge05Page.razor"),
    ("step40", "docs/samples/step40.md", "Step40/EfDbCheckPage.razor"),
]
if [item["id"] for item in ordered[55:60]] != [entry[0] for entry in unit12]:
    missing.append("第12組056〜060の学習順が不正")
for sequence, (_, relative, page_name) in enumerate(unit12, start=56):
    body = (ROOT / relative).read_text(encoding="utf-8")
    view = (SAMPLES / page_name).read_text(encoding="utf-8")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(" | ") < 8:
        missing.append(relative + "：3300文字以上・比較表・切り分け表が必要")
    if body.count(chr(96) * 3) < 4 or body.count(chr(96) * 3) % 2:
        missing.append(relative + "：実コード2例以上が必要")
    for word in ("WinForms", "Dapper", "PostgreSQL", "操作", "演習", "詳しい解答"):
        if word not in body:
            missing.append(relative + "：必須の説明不足：" + word)
    if f"学習順{sequence:03d}" not in view or f'data-testid="dbbridge12-compare-{sequence:03d}"' not in view:
        missing.append(page_name + "：学習順とRazor比較表の同期が必要")
    if f"学習順{sequence:03d}を開く" not in home or f"学習順{sequence:03d}：" not in nav_eighth:
        missing.append(relative + "：ホームとNavMenuが学習順と一致しない")
    if f'"{relative}"' not in PDF_SCRIPT:
        missing.append(relative + "：PDF目次ソースに未収録")
review12_path = "docs/course-v3/reviews/review-12.md"
review12 = (ROOT / review12_path).read_text(encoding="utf-8")
review12_view = (SAMPLES / "DbBridge/DbBridgeReview12Page.razor").read_text(encoding="utf-8")
if len(review12) < 3300 or review12.count("| ---") < 2 or review12.count(chr(96) * 3) < 4 or "詳しい解答" not in review12:
    missing.append("第12組復習：長文・比較表・コード・詳答の不足")
if 'data-testid="dbbridge12-review-table"' not in review12_view or 'href="/learn/061"' not in review12_view:
    missing.append("第12組復習Razor比較表と次の学習順061への導線不足")
if "学習順056〜060を復習" not in home or "第12組の復習：056〜060" not in nav_eighth:
    missing.append("第12組復習のWeb目次・NavMenu導線不足")
positions12 = [PDF_SCRIPT.find(f'"{relative}"') for _, relative, _ in unit12]
review12_pos = PDF_SCRIPT.find(f'"{review12_path}"')
review11_pos = PDF_SCRIPT.find('"docs/course-v3/reviews/review-11.md"')
if (-1 in positions12 or positions12 != sorted(positions12)
        or not (review11_pos < positions12[0] < positions12[-1] < review12_pos)):
    missing.append("PDF目次ソースの第11組→056〜060→第12組順が不正")


# 第13組061〜065：旧80URLを守り、Markdown・Razor・Web・PDF索引のソースを検証する。
unit13 = [
    ("step41", "docs/samples/step41.md", "Step41/ItemsBoundaryPage.razor"),
    ("step42", "docs/samples/step42.md", "Step42/TableServerDataPage.razor"),
    ("step43", "docs/samples/step43.md", "Step43/GridServerDataPage.razor"),
    ("step44", "docs/samples/step44.md", "Step44/SafeSortPage.razor"),
    ("step45", "docs/samples/step45.md", "Step45/FilterSearchPage.razor"),
]
if [item["id"] for item in ordered[60:65]] != [entry[0] for entry in unit13]:
    missing.append("第13組061〜065の学習順が不正")
for sequence, (_, relative, page_name) in enumerate(unit13, start=61):
    body = (ROOT / relative).read_text(encoding="utf-8")
    view = (SAMPLES / page_name).read_text(encoding="utf-8")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(" | ") < 8:
        missing.append(relative + "：3300文字以上・比較表・切り分け表が必要")
    if body.count(chr(96) * 3) < 4 or body.count(chr(96) * 3) % 2:
        missing.append(relative + "：実コード2例以上が必要")
    for word in ("WinForms", "Dapper", "PostgreSQL", "操作", "演習", "詳しい解答"):
        if word not in body:
            missing.append(relative + "：必須の説明不足：" + word)
    if f"学習順{sequence:03d}" not in view or f'data-testid="stage06-compare-{sequence:03d}"' not in view:
        missing.append(page_name + "：Razor比較表・学習順が一致しない")
    if f"学習順{sequence:03d}を開く" not in home or f"学習順{sequence:03d}：" not in nav_eighth:
        missing.append(relative + "：HomeとNavMenuの導線不足")
    if f'"{relative}"' not in PDF_SCRIPT:
        missing.append(relative + "：PDF目次ソースから欠落")

review13_path = "docs/columns/column41-45.md"
review13 = (ROOT / review13_path).read_text(encoding="utf-8")
review13_view = (SAMPLES / "Columns/Column45Page.razor").read_text(encoding="utf-8")
stage06_path = "docs/course-v3/stages/stage06.md"
stage06 = (ROOT / stage06_path).read_text(encoding="utf-8")
stage06_view = (SAMPLES / "Step41/Stage06OverviewPage.razor").read_text(encoding="utf-8")
if len(review13) < 3300 or review13.count("| ---") < 2 or review13.count(chr(96) * 3) < 4 or "詳しい解答" not in review13:
    missing.append("第13組の復習：長文、比較表、コード、詳答の不足")
if len(stage06) < 3300 or stage06.count("| ---") < 2 or stage06.count(chr(96) * 3) < 4 or "詳しい解答" not in stage06:
    missing.append("第6段階概要：長文・比較表・実コード・詳答の不足")
if 'data-testid="stage06-review-table"' not in review13_view or 'href="/learn/066"' not in review13_view:
    missing.append("第13組の復習Razor・次の学習順066への導線不足")
if 'data-testid="stage06-overview-table"' not in stage06_view or 'href="/learn/061"' not in stage06_view:
    missing.append("第6段階概要Razor・学習順061への導線不足")
if "学習順061〜065を復習" not in home or "第13組の復習：061〜065" not in nav_eighth:
    missing.append("第13組復習のHome・NavMenu導線不足")
if "第6段階の概要ページを読む" not in home or "第6段階の概要" not in nav_eighth:
    missing.append("第6段階概要のWeb導線不足")
stage06_pos = PDF_SCRIPT.find('"docs/course-v3/stages/stage06.md"')
positions13 = [PDF_SCRIPT.find(f'"{relative}"') for _, relative, _ in unit13]
review13_pos = PDF_SCRIPT.find(f'"{review13_path}"')
review12_pos = PDF_SCRIPT.find('"docs/course-v3/reviews/review-12.md"')
step46_pos = PDF_SCRIPT.find('"docs/samples/step46.md"')
if (-1 in positions13 or positions13 != sorted(positions13)
        or not (review12_pos < stage06_pos < positions13[0] < positions13[-1] < review13_pos < step46_pos)):
    missing.append("PDF目次ソースの第12組→第6段階→061〜065→第13組→066順が不正")

# 第14組066〜070：PDF停止中も教材本文と索引ソースのみを検査する。
unit14 = [
    ("step46", "docs/samples/step46.md", "Step46/RelatedDtoPage.razor"),
    ("step47", "docs/samples/step47.md", "Step47/KeysetPage.razor"),
    ("step48", "docs/samples/step48.md", "Step48/VirtualizeItemsPage.razor"),
    ("step49", "docs/samples/step49.md", "Step49/VirtualizeDatabasePage.razor"),
    ("step50", "docs/samples/step50.md", "Step50/SearchUxPage.razor"),
]
if [item["id"] for item in ordered[65:70]] != [entry[0] for entry in unit14]:
    missing.append("第14組066〜070の学習順が不正")
for sequence, (_, relative, page_name) in enumerate(unit14, start=66):
    body = (ROOT / relative).read_text(encoding="utf-8")
    razor = (SAMPLES / page_name).read_text(encoding="utf-8")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(" | ") < 8:
        missing.append(relative + "：3300文字以上と比較・診断表が不足")
    if body.count(chr(96) * 3) < 4 or body.count(chr(96) * 3) % 2:
        missing.append(relative + "：実コード2例以上が必要")
    for keyword in ("WinForms", "Dapper", "PostgreSQL", "操作", "演習", "詳しい解答"):
        if keyword not in body:
            missing.append(relative + "：説明不足 " + keyword)
    if f"学習順{sequence:03d}" not in razor or f'data-testid="stage06-compare-{sequence:03d}"' not in razor:
        missing.append(page_name + "：Razorの学習順・比較表が不一致")
    if f"学習順{sequence:03d}を開く" not in home or f"学習順{sequence:03d}：" not in nav_eighth:
        missing.append(relative + "：Home/NavMenuの学習順が不一致")
    if f'"{relative}"' not in PDF_SCRIPT or f"学習順{sequence:03d}：" not in PDF_SCRIPT:
        missing.append(relative + "：PDF目次ソースに学習順がない")
review14_path = "docs/columns/column46-50.md"
review14 = (ROOT / review14_path).read_text(encoding="utf-8")
review14_view = (SAMPLES / "Columns/Column50Page.razor").read_text(encoding="utf-8")
if len(review14) < 3300 or review14.count("| ---") < 2 or review14.count(chr(96) * 3) < 4 or "詳しい解答" not in review14:
    missing.append("第14組復習の長文・比較表・コード・詳細解答が不足")
if 'data-testid="stage06-review14-table"' not in review14_view or 'href="/learn/071"' not in review14_view:
    missing.append("第14組復習Razorまたは学習順071への導線が不足")
if 'href="/learn/column/45"' not in (SAMPLES / "Step46/RelatedDtoPage.razor").read_text(encoding="utf-8"):
    missing.append("066から第13組復習への前後導線不足")
if "学習順066〜070を復習" not in home or "第14組の復習：066〜070" not in nav_eighth:
    missing.append("第14組のHome/NavMenu復習導線不足")
positions14 = [PDF_SCRIPT.find(f'"{relative}"') for _, relative, _ in unit14]
review14_pos = PDF_SCRIPT.find(f'"{review14_path}"')
review13_pos = PDF_SCRIPT.find('"docs/columns/column41-45.md"')
next71_pos = PDF_SCRIPT.find('"docs/samples/step51.md"')
if -1 in positions14 or positions14 != sorted(positions14) or not (
    review13_pos < positions14[0] < positions14[-1] < review14_pos < next71_pos
):
    missing.append("第13組→066〜070→第14組→071のPDF索引順が不正")

# 第15組071〜075：本文、Razor、Web/PDF索引のソースのみを検査。PDFは生成しない。
unit15 = [
    ("step51", "docs/samples/step51.md", "Step51/EditDtoPage.razor"),
    ("step52", "docs/samples/step52.md", "Step52/EditDialogPage.razor"),
    ("step53", "docs/samples/step53.md", "Step53/BulkEditPage.razor"),
    ("step54", "docs/samples/step54.md", "Step54/ConflictEditorPage.razor"),
    ("step55", "docs/samples/step55.md", "Step55/DetailProjectionPage.razor"),
]
if [item["id"] for item in ordered[70:75]] != [item[0] for item in unit15]:
    missing.append("第15組071〜075の学習順が不正")
for sequence, (_, relative, view_path) in enumerate(unit15, start=71):
    body = (ROOT / relative).read_text(encoding="utf-8")
    view = (SAMPLES / view_path).read_text(encoding="utf-8")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(" | ") < 8:
        missing.append(relative + "：3300字以上・比較と切り分け表が不足")
    if body.count(chr(96)*3) < 4 or "WinForms" not in body or "Dapper" not in body or "詳しい解答" not in body:
        missing.append(relative + "：実コード2例・比較・詳答が不足")
    if f'data-testid="stage06-compare-{sequence:03d}"' not in view:
        missing.append(relative + "：Razor比較表が不足")
    if f"学習順{sequence:03d}を開く" not in home or f"学習順{sequence:03d}：" not in nav_eighth:
        missing.append(relative + "：Web目次とNavMenuが不一致")
    if f'"{relative}"' not in PDF_SCRIPT or f"学習順{sequence:03d}：" not in PDF_SCRIPT:
        missing.append(relative + "：PDF目次ソースに学習順がない")
review15_path = "docs/columns/column51-55.md"
review15 = (ROOT / review15_path).read_text(encoding="utf-8")
review15_view = (SAMPLES / "Columns/Column55Page.razor").read_text(encoding="utf-8")
if len(review15) < 3300 or review15.count("| ---") < 2 or review15.count(chr(96)*3) < 4 or "詳しい解答" not in review15:
    missing.append("第15組復習の長文・表・実コード・詳答が不足")
if 'data-testid="stage06-review15-table"' not in review15_view or 'href="/learn/076"' not in review15_view:
    missing.append("第15組復習Razorと学習順076への導線不足")
if 'href="/learn/column/50"' not in (SAMPLES / "Step51/EditDtoPage.razor").read_text(encoding="utf-8"):
    missing.append("学習順071から第14組復習への前後リンク不足")
if "学習順071〜075を復習" not in home or "第15組の復習：071〜075" not in nav_eighth:
    missing.append("第15組のHome/NavMenu復習導線が不足")
positions15 = [PDF_SCRIPT.find(f'"{relative}"') for _, relative, _ in unit15]
review15_pos = PDF_SCRIPT.find(f'"{review15_path}"')
review14_pos = PDF_SCRIPT.find('"docs/columns/column46-50.md"')
step56_pos = PDF_SCRIPT.find('"docs/samples/step56.md"')
if -1 in positions15 or positions15 != sorted(positions15) or not (
    review14_pos < positions15[0] < positions15[-1] < review15_pos < step56_pos
):
    missing.append("第14組→071〜075→第15組→076のPDF索引順が不正")



# 第16組076〜080：PDFを生成せず索引のソースと教材・Razor・Webを照合する。
unit16 = [
    ("step56", "docs/samples/step56.md", "Step56/RangeSearchPage.razor"),
    ("step57", "docs/samples/step57.md", "Step57/CacheBoundaryPage.razor"),
    ("step58", "docs/samples/step58.md", "Step58/StatisticsExportPage.razor"),
    ("step59", "docs/samples/step59.md", "Step59/PerformancePage.razor"),
    ("step60", "docs/samples/step60.md", "Step60/PatternChoicePage.razor"),
]
if [item["id"] for item in ordered[75:80]] != [item[0] for item in unit16]:
    missing.append("第16組076〜080の学習順が不正")
for sequence, (_, relative, view_path) in enumerate(unit16, start=76):
    body = (ROOT / relative).read_text(encoding="utf-8")
    view = (SAMPLES / view_path).read_text(encoding="utf-8")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(chr(96)*3) < 4:
        missing.append(relative + "：3300字・比較/切り分け表・コード2例が不足")
    for term in ("WinForms", "Dapper", "操作手順", "障害の切り分け", "詳しい解答"):
        if term not in body:
            missing.append(relative + "：" + term + "不足")
    if f'data-testid="stage06-compare-{sequence:03d}"' not in view:
        missing.append(relative + "：Razor比較表が不足")
    if f"学習順{sequence:03d}を開く" not in home or f"学習順{sequence:03d}：" not in nav_eighth:
        missing.append(relative + "：Home/NavMenuの学習順が不一致")
    if f'"{relative}"' not in PDF_SCRIPT or f"学習順{sequence:03d}：" not in PDF_SCRIPT:
        missing.append(relative + "：PDF目次ソースの順序・名称が不一致")
review16_path = "docs/columns/column56-60.md"
review16 = (ROOT / review16_path).read_text(encoding="utf-8")
review16_view = (SAMPLES / "Columns/Column60Page.razor").read_text(encoding="utf-8")
if len(review16) < 3300 or review16.count("| ---") < 2 or review16.count(chr(96)*3) < 4 or "詳しい解答" not in review16:
    missing.append("第16組復習の長文・表・コード・詳答が不足")
if 'data-testid="stage06-review16-table"' not in review16_view or 'href="/learn/081"' not in review16_view:
    missing.append("第16組復習Razorまたは081導線が不足")
if 'href="/learn/column/55"' not in (SAMPLES / "Step56/RangeSearchPage.razor").read_text(encoding="utf-8"):
    missing.append("学習順076→第15組復習へのリンク不足")
if 'href="/learn/column/60"' not in (SAMPLES / "Step60/PatternChoicePage.razor").read_text(encoding="utf-8"):
    missing.append("学習順080→第16組復習へのリンク不足")
if "学習順076〜080を復習" not in home or "第16組の復習：076〜080" not in nav_eighth:
    missing.append("第16組復習のHome/NavMenu不一致")
positions16 = [PDF_SCRIPT.find(f'"{relative}"') for _, relative, _ in unit16]
review16_pos = PDF_SCRIPT.find(f'"{review16_path}"')
if -1 in positions16 or positions16 != sorted(positions16) or not (
    review15_pos < positions16[0] < positions16[-1] < review16_pos < PDF_SCRIPT.find('"docs/course-v3/lessons/security-01.md"')
):
    missing.append("第15組→076〜080→第16組→081のPDF目次ソース順序が不正")


# 第17組081〜085：最終全面改稿の文章量・コード・画面と索引を検査する。
security_unit17 = [(81, "security-01"), (82, "security-02"), (83, "security-03"), (84, "security-04"), (85, "security-05")]
if [item["id"] for item in ordered[80:85]] != [key for _, key in security_unit17]:
    missing.append("学習順081〜085が第17組と一致しません")
for sequence, lesson_id in security_unit17:
    relative = f"docs/course-v3/lessons/{lesson_id}.md"
    body = (ROOT / relative).read_text(encoding="utf-8")
    view = (SAMPLES / "Security" / f"Security0{sequence - 80}Page.razor").read_text(encoding="utf-8")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(chr(96)*3) < 4:
        missing.append(relative + "：3300字・表・実コード2件が不足")
    for term in ("WinForms", "Dapper", "操作手順", "障害の切り分け", "詳しい解答"):
        if term not in body:
            missing.append(relative + "：" + term + "不足")
    if f'data-testid="stage07-compare-{sequence}"' not in view:
        missing.append(relative + "：Razor比較表不足")
    if f"学習順{sequence:03d}を開く" not in home or f"学習順{sequence:03d}：" not in nav_eighth:
        missing.append(relative + "：Web目次とNavMenuの学習順不一致")
    if f'"{relative}"' not in PDF_SCRIPT or f"学習順{sequence:03d}：" not in PDF_SCRIPT:
        missing.append(relative + "：PDF目次ソースと本文の照合失敗")
for relative in ("docs/course-v3/reviews/review-17.md", "docs/course-v3/stages/stage07.md"):
    body = (ROOT / relative).read_text(encoding="utf-8")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(chr(96)*3) < 4:
        missing.append(relative + "：3300字・表・実コード2件が不足")
    if "WinForms" not in body or "Dapper" not in body or "障害の切り分け" not in body or "詳しい解答" not in body:
        missing.append(relative + "：比較・切り分け・詳答が不足")
rev17_view = (SAMPLES / "Security/SecurityReview17Page.razor").read_text(encoding="utf-8")
stage7_view = (SAMPLES / "Security/SecurityOverviewPage.razor").read_text(encoding="utf-8")
if 'data-testid="stage07-review17-table"' not in rev17_view or 'href="/learn/086"' not in rev17_view:
    missing.append("第17組の復習表または学習順086への導線不足")
if 'data-testid="stage07-overview-table"' not in stage7_view or 'href="/learn/081"' not in stage7_view:
    missing.append("第7段階の概要表または081への導線不足")
if "学習順081〜085を復習する" not in home or "第17組の復習：081〜085" not in nav_eighth:
    missing.append("第17組のHome/NavMenu導線不足")
positions17 = [PDF_SCRIPT.find(f'"docs/course-v3/lessons/{key}.md"') for _, key in security_unit17]
stage7_pos = PDF_SCRIPT.find('"docs/course-v3/stages/stage07.md"')
review17_pos = PDF_SCRIPT.find('"docs/course-v3/reviews/review-17.md"')
if -1 in positions17 or positions17 != sorted(positions17) or not (
    PDF_SCRIPT.find('"docs/columns/column56-60.md"') < stage7_pos
    < positions17[0] < positions17[-1] < review17_pos
    < PDF_SCRIPT.find('"docs/samples/step61.md"')
):
    missing.append("第16組→第7段階概要→081〜085→第17組→086のPDF目次ソース順が不正")


# 第18組086〜090：長文教材・Razor・Web/PDF目次のソースを検査する。PDFは生成しない。
for index, view_rel in enumerate([
    "Step61/NegotiatePage.razor", "Step62/SidPage.razor",
    "Step63/UserMappingPage.razor", "Step64/UserRefusalPage.razor",
    "Step65/RolePermissionPage.razor"
]):
    seq, old = 86 + index, 61 + index
    item = ordered[seq - 1]
    source = f"docs/samples/step{old}.md"
    body = (ROOT / source).read_text(encoding="utf-8")
    view = (SAMPLES / view_rel).read_text(encoding="utf-8")
    if item["sequence"] != seq or item["id"] != f"step{old}" or item["rewrite"] != "final-reviewed-ci-passed":
        missing.append(source + "：学習順と改稿状態不一致")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(chr(96) * 3) < 4:
        missing.append(source + "：3300字・比較表・実コード2例不足")
    for term in ("WinForms", "Dapper", "操作手順", "障害の切り分け", "詳しい解答"):
        if term not in body:
            missing.append(source + "：" + term + "不足")
    if f'data-testid="stage07-compare-{seq}"' not in view:
        missing.append(source + "：比較表不足")
    if f"学習順{seq:03d}を開く" not in home or f"学習順{seq:03d}：" not in nav_eighth:
        missing.append(source + "：Web目次・NavMenu不足")
    if f'"{source}"' not in PDF_SCRIPT or f"学習順{seq:03d}：" not in PDF_SCRIPT:
        missing.append(source + "：PDF目次ソース不足")
review18_path = "docs/columns/column61-65.md"
review18 = (ROOT / review18_path).read_text(encoding="utf-8")
review18_view = (SAMPLES / "Columns/Column65Page.razor").read_text(encoding="utf-8")
if len(review18) < 3300 or review18.count("| ---") < 2 or review18.count(chr(96) * 3) < 4 or "詳しい解答" not in review18:
    missing.append("第18組復習：長文・比較表・コード・詳答不足")
if 'data-testid="stage07-review18-table"' not in review18_view or 'href="/learn/column/auth-at-scale"' not in review18_view:
    missing.append("第18組復習：Razor比較表または次コラム導線不足")
if "学習順086〜090を復習する" not in home or "第18組の復習：086〜090" not in nav_eighth:
    missing.append("第18組復習：Home/NavMenu不足")
if 'href="/learn/column/security-85"' not in (SAMPLES / "Step61/NegotiatePage.razor").read_text(encoding="utf-8"):
    missing.append("学習順086の前リンクがありません")
if 'href="/learn/column/65"' not in (SAMPLES / "Step65/RolePermissionPage.razor").read_text(encoding="utf-8"):
    missing.append("学習順090の次リンクがありません")
positions18 = [PDF_SCRIPT.find(f'"docs/samples/step{x}.md"') for x in range(61, 66)]
review18_pos = PDF_SCRIPT.find('"docs/columns/column61-65.md"')
if -1 in positions18 or positions18 != sorted(positions18) or not (
    review17_pos < positions18[0] < positions18[-1] < review18_pos < PDF_SCRIPT.find('"docs/columns/column-auth-at-scale.md"')
):
    missing.append("第17組→第18組→特別コラムのPDF目次順不正")



# 第19組091〜095：全面改稿・画面比較表・Web/PDF目次を原本ソースで検査。PDFは生成しない。
unit19 = [
    (91, 66, "Step66/AuthorizedEntryPage.razor"),
    (92, 67, "Step67/UnitScopedPage.razor"),
    (93, 68, "Step68/ScopedExportPage.razor"),
    (94, 69, "Step69/SecuredEditPage.razor"),
    (95, 70, "Step70/DeploymentGatePage.razor"),
]
if [item["id"] for item in ordered[90:95]] != [f"step{old}" for _, old, _ in unit19]:
    missing.append("第19組091〜095の学習順が不正")
for sequence, old, view_path in unit19:
    source = f"docs/samples/step{old}.md"
    body = (ROOT / source).read_text(encoding="utf-8")
    view = (SAMPLES / view_path).read_text(encoding="utf-8")
    if ordered[sequence - 1]["rewrite"] != "final-reviewed-ci-passed":
        missing.append(source + "：全面改稿の受入状態が不一致")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(chr(96) * 3) < 4:
        missing.append(source + "：3300字・比較/切り分け表・コード2例不足")
    for term in ("WinForms", "Dapper", "操作手順", "障害の切り分け", "詳しい解答"):
        if term not in body:
            missing.append(source + "：" + term + "不足")
    if f'data-testid="stage07-compare-{sequence}"' not in view:
        missing.append(source + "：Razor比較表不足")
    if f"学習順{sequence:03d}を開く" not in home or f"学習順{sequence:03d}：" not in nav_eighth:
        missing.append(source + "：Home/NavMenuに新学習順がない")
    if f'"{source}"' not in PDF_SCRIPT or f"学習順{sequence:03d}：" not in PDF_SCRIPT:
        missing.append(source + "：PDF目次ソースに新学習順がない")

review19_path = "docs/columns/column66-70.md"
review19 = (ROOT / review19_path).read_text(encoding="utf-8")
review19_view = (SAMPLES / "Columns/Column70Page.razor").read_text(encoding="utf-8")
if len(review19) < 3300 or review19.count("| ---") < 2 or review19.count(chr(96) * 3) < 4 or "詳しい解答" not in review19:
    missing.append("第19組復習の長文・表・実コード・詳答が不足")
if 'data-testid="stage07-review19-table"' not in review19_view or 'href="/learn/096"' not in review19_view:
    missing.append("第19組復習の比較表または096への導線がない")
if "学習順091〜095を復習する" not in home or "第19組の復習：091〜095" not in nav_eighth:
    missing.append("第19組復習のHome/NavMenu導線がない")
if 'href="/learn/column/auth-at-scale"' not in (SAMPLES / "Step66/AuthorizedEntryPage.razor").read_text(encoding="utf-8"):
    missing.append("学習順091の前リンクがない")
if 'href="/learn/column/70"' not in (SAMPLES / "Step70/DeploymentGatePage.razor").read_text(encoding="utf-8"):
    missing.append("学習順095の復習リンクがない")
positions19 = [PDF_SCRIPT.find(f'"docs/samples/step{x}.md"') for x in range(66, 71)]
review19_pos = PDF_SCRIPT.find('"docs/columns/column66-70.md"')
if -1 in positions19 or positions19 != sorted(positions19) or not (
    PDF_SCRIPT.find('"docs/columns/column-auth-at-scale.md"') < positions19[0]
    < positions19[-1] < review19_pos < PDF_SCRIPT.find('"docs/samples/step71.md"')
):
    missing.append("特別コラム→091〜095→第19組復習→096のPDF索引順が不正")



# 第20組096〜100：全面改稿・実画面・目次を検査する。PDFの生成はしない。
unit20 = [
    (96, 71, "Step71/DirectoryNormalizationPage.razor"),
    (97, 72, "Step72/ExplicitWindowsLoginPage.razor"),
    (98, 73, "Step73/TransactionalAuthorizationPage.razor"),
    (99, 74, "Step74/DurableAuditPage.razor"),
    (100, 75, "Step75/IisAcceptancePage.razor"),
]
if [item["id"] for item in ordered[95:100]] != [f"step{old}" for _, old, _ in unit20]:
    missing.append("第20組096〜100の学習順が不正")
for seq, old, view_rel in unit20:
    source = f"docs/samples/step{old}.md"
    body = (ROOT / source).read_text(encoding="utf-8")
    view = (SAMPLES / view_rel).read_text(encoding="utf-8")
    if ordered[seq - 1]["rewrite"] != "final-reviewed-ci-passed":
        missing.append(source + "：改稿状態の不一致")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(chr(96) * 3) < 4:
        missing.append(source + "：3300文字以上・比較表・コード2例不足")
    for term in ("WinForms", "Dapper", "操作手順", "障害の切り分け", "詳しい解答"):
        if term not in body:
            missing.append(source + "：" + term + "不足")
    if f'data-testid="stage08-compare-{seq:03d}"' not in view:
        missing.append(source + "：Razor比較表不足")
    if f"学習順{seq:03d}を開く" not in home or f"学習順{seq:03d}：" not in nav_eighth:
        missing.append(source + "：Web目次・NavMenu不足")
    if f'"{source}"' not in PDF_SCRIPT or f"学習順{seq:03d}：" not in PDF_SCRIPT:
        missing.append(source + "：PDF目次ソース不足")
    previous = "/learn/column/70" if seq == 96 else f"/learn/{seq - 1:03d}"
    following = "/learn/column/75" if seq == 100 else f"/learn/{seq + 1:03d}"
    if f'href="{previous}"' not in view or f'href="{following}"' not in view:
        missing.append(source + "：前後導線不足")

review20_path = "docs/columns/column71-75.md"
review20 = (ROOT / review20_path).read_text(encoding="utf-8")
review20_view = (SAMPLES / "Columns/Column75Page.razor").read_text(encoding="utf-8")
if len(review20) < 3300 or review20.count("| ---") < 2 or review20.count(chr(96) * 3) < 4 or "詳しい解答" not in review20:
    missing.append("第20組復習：長文・表・実コード・詳答不足")
if 'data-testid="stage08-review20-table"' not in review20_view or 'href="/learn/101"' not in review20_view:
    missing.append("第20組復習：比較表または101への次リンク不足")
if "学習順096〜100を復習する" not in home or "第20組の復習：096〜100" not in nav_eighth:
    missing.append("第20組復習：Web目次・NavMenu不足")
stage08 = (ROOT / "docs/course-v3/stages/stage08.md").read_text(encoding="utf-8")
stage08_view = (SAMPLES / "Step71/Stage08OverviewPage.razor").read_text(encoding="utf-8")
if len(stage08) < 3300 or stage08.count("| ---") < 2 or stage08.count(chr(96) * 3) < 4 or "詳しい解答" not in stage08:
    missing.append("第8段階概要：教材の長文・コード・表・詳答不足")
if 'data-testid="stage08-overview-table"' not in stage08_view or 'href="/learn/096"' not in stage08_view:
    missing.append("第8段階概要：Razor比較表・096導線不足")
if "第8段階の概要を先に読む" not in home or 'href="/learn/stage/operations"' not in nav_eighth:
    missing.append("第8段階概要：Web目次・NavMenu不足")
positions20 = [PDF_SCRIPT.find(f'"docs/samples/step{x}.md"') for x in range(71, 76)]
review20_pos = PDF_SCRIPT.find('"docs/columns/column71-75.md"')
stage08_pos = PDF_SCRIPT.find('"docs/course-v3/stages/stage08.md"')
if -1 in positions20 or positions20 != sorted(positions20) or not (
    PDF_SCRIPT.find('"docs/columns/column66-70.md"') < stage08_pos
    < positions20[0] < positions20[-1] < review20_pos
    < PDF_SCRIPT.find('"docs/samples/step76.md"')
):
    missing.append("第19組復習→第8段階概要→第20組096〜100→復習→101のPDF索引順不正")


# 第21組101〜105：全面改稿本文・Razor・Web/PDF目次のソースだけを検査する。
unit21 = [(101, 76, "Step76/ConcurrentEditPage.razor"),
          (102, 77, "Step77/AuthorizedCachePage.razor"),
          (103, 78, "Step78/DiagnosticsPage.razor"),
          (104, 79, "Step79/EndToEndAcceptancePage.razor"),
          (105, 80, "Step80/DeploymentPage.razor")]
if [x["id"] for x in ordered[100:105]] != [f"step{old}" for _, old, _ in unit21]:
    missing.append("第21組101〜105の学習順が不正")
for seq, old, view_rel in unit21:
    source = f"docs/samples/step{old}.md"
    text_body = (ROOT / source).read_text(encoding="utf-8")
    view = (SAMPLES / view_rel).read_text(encoding="utf-8")
    if ordered[seq - 1]["rewrite"] != "final-reviewed-ci-passed":
        missing.append(source + "：改稿状態不一致")
    if len(text_body) < 3300 or text_body.count("| ---") < 2 or text_body.count(chr(96) * 3) < 4:
        missing.append(source + "：3300字・比較表・実コード2例不足")
    for term in ("WinForms", "Dapper", "操作手順", "障害の切り分け", "詳しい解答"):
        if term not in text_body:
            missing.append(source + "：" + term + "不足")
    if f'data-testid="stage08-compare-{seq}"' not in view:
        missing.append(source + "：Razor比較表不足")
    if f"学習順{seq:03d}を開く" not in home or f"学習順{seq:03d}：" not in nav_eighth:
        missing.append(source + "：Web目次・NavMenu不足")
    if f'"{source}"' not in PDF_SCRIPT or f"学習順{seq:03d}：" not in PDF_SCRIPT:
        missing.append(source + "：PDF目次ソース不足")
    previous = "/learn/100" if seq == 101 else f"/learn/{seq - 1:03d}"
    following = "/learn/column/80" if seq == 105 else f"/learn/{seq + 1:03d}"
    if f'href="{previous}"' not in view or f'href="{following}"' not in view:
        missing.append(source + "：前後リンク不足")
review21 = (ROOT / "docs/columns/column76-80.md").read_text(encoding="utf-8")
review21_view = (SAMPLES / "Columns/Column80Page.razor").read_text(encoding="utf-8")
if len(review21) < 3300 or review21.count("| ---") < 2 or review21.count(chr(96) * 3) < 4 or "詳しい解答" not in review21:
    missing.append("第21組復習：本文・比較表・実コード・詳答不足")
if 'data-testid="stage08-review21-table"' not in review21_view or 'href="/learn/106"' not in review21_view:
    missing.append("第21組復習：Razor比較表と学習順106へのリンク不足")
if "学習順101〜105を復習する" not in home or "第21組の復習：101〜105" not in nav_eighth:
    missing.append("第21組復習：Home/NavMenu導線不足")
pdf21 = [PDF_SCRIPT.find(f'"docs/samples/step{x}.md"') for x in range(76, 81)]
if -1 in pdf21 or pdf21 != sorted(pdf21) or not (
    review20_pos < pdf21[0] < pdf21[-1]
    < PDF_SCRIPT.find('"docs/columns/column76-80.md"')
    < PDF_SCRIPT.find('"docs/course-v3/stages/stage09.md"')
):
    missing.append("第21組101〜105→第21組復習→第9段階のPDF索引順不正")


# 最終第22組106〜110・第9段階概要の長文・実コード・WebとPDF索引ソース検査。
final_nav = (ROOT / "src/PgBlazorSamples.Web/Components/Layout/NavMenu.razor").read_text(encoding="utf-8")
final_home = (ROOT / "src/PgBlazorSamples.Web/Components/Pages/Home.razor").read_text(encoding="utf-8")
for seq in range(106, 111):
    n = seq - 105
    source = f"docs/course-v3/lessons/capstone-0{n}.md"
    body = (ROOT / source).read_text(encoding="utf-8")
    view = (SAMPLES / f"Capstone/Capstone0{n}Page.razor").read_text(encoding="utf-8")
    if ordered[seq - 1]["rewrite"] != "final-reviewed-ci-passed":
        missing.append(source + "：全面改稿状態が不一致")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(chr(96)*3) < 4:
        missing.append(source + "：3300文字・比較/障害表・実コード2例の不足")
    for term in ("WinForms", "Dapper", "操作手順", "障害の切り分け", "詳しい解答"):
        if term not in body:
            missing.append(source + "：" + term + "不足")
    if f'data-testid="capstone-compare-{seq}"' not in view:
        missing.append(source + "：Razor比較表が不足")
    if f"学習順{seq}を開く" not in final_home or f"学習順{seq:03d}：" not in final_nav:
        missing.append(source + "：Home/NavMenuの学習順が不足")
    if f'"{source}"' not in PDF_SCRIPT or f"学習順{seq:03d}：" not in PDF_SCRIPT:
        missing.append(source + "：PDF目次ソースが不足")
    previous = "/learn/column/80" if seq == 106 else f"/learn/{seq - 1:03d}"
    following = "/learn/column/capstone-110" if seq == 110 else f"/learn/{seq + 1:03d}"
    if f'href="{previous}"' not in view or f'href="{following}"' not in view:
        missing.append(source + "：前後リンクが不足")

for source, razor, marker in [
    ("docs/course-v3/stages/stage09.md", "Capstone/CapstoneOverviewPage.razor", "capstone-stage-overview-table"),
    ("docs/course-v3/reviews/review-22.md", "Capstone/CapstoneReview22Page.razor", "capstone-review22-table"),
]:
    body = (ROOT / source).read_text(encoding="utf-8")
    view = (SAMPLES / razor).read_text(encoding="utf-8")
    if len(body) < 3300 or body.count("| ---") < 2 or body.count(chr(96)*3) < 4 or "詳しい解答" not in body:
        missing.append(source + "：3300文字・比較表・コード2例・詳答が不足")
    if f'data-testid="{marker}"' not in view:
        missing.append(source + "：Razor比較表不足")

final_curriculum = json.loads((ROOT / "docs/course-v3/curriculum.json").read_text(encoding="utf-8"))
if final_curriculum["stages"][-1].get("final_review") != "ci-passed":
    missing.append("第9段階の改稿状態が不一致")
if final_curriculum["reviews"][-1].get("final_review") != "ci-passed":
    missing.append("第22組の改稿状態が不一致")
pdf_last = [PDF_SCRIPT.find(f'"docs/course-v3/lessons/capstone-0{n}.md"') for n in range(1, 6)]
pdf_stage = PDF_SCRIPT.find('"docs/course-v3/stages/stage09.md"')
pdf_review = PDF_SCRIPT.find('"docs/course-v3/reviews/review-22.md"')
if -1 in pdf_last or pdf_last != sorted(pdf_last) or not (
    PDF_SCRIPT.find('"docs/columns/column76-80.md"') < pdf_stage
    < pdf_last[0] < pdf_last[-1] < pdf_review
):
    missing.append("第9段階概要→106〜110→第22組のPDF目次ソース順序が不正")

# 学習順001〜110のソース参照表を現在の教材順序と突き合わせる。
# 利用者が番号を換算して探す必要をなくし、改名時の参照切れを検出する。
source_map_path = ROOT / "docs/course-v3/source-map.json"
source_index_path = ROOT / "docs/course-v3/source-index.md"
if not source_map_path.is_file() or not source_index_path.is_file():
    missing.append("学習順001〜110の教材・実装直接索引がありません")
else:
    source_map = json.loads(source_map_path.read_text(encoding="utf-8"))
    source_index = source_index_path.read_text(encoding="utf-8")
    mapped_lessons = source_map.get("lessons", [])
    if len(mapped_lessons) != 110:
        missing.append("実装直接索引が110件ではありません")
    if [entry.get("sequence") for entry in mapped_lessons] != list(range(1, 111)):
        missing.append("実装直接索引の学習順が連続していません")
    if len({entry.get("razor_path") for entry in mapped_lessons}) != 110:
        missing.append("実装直接索引の主Razorへの対応が重複しています")
    for expected, actual in zip(ordered, mapped_lessons):
        seq = expected["sequence"]
        if (actual.get("lesson_path") != expected["source"]
                or actual.get("app_url") != expected["url"]
                or actual.get("title") != expected["title"]
                or actual.get("stage") != expected["stage"]):
            missing.append(f"学習順{seq:03d}：実装直接索引がcurriculumと一致しません")
        if not (ROOT / actual.get("lesson_path", "")).is_file():
            missing.append(f"学習順{seq:03d}：教材ファイルがありません")
        razor_file = ROOT / actual.get("razor_path", "")
        if not razor_file.is_file():
            missing.append(f"学習順{seq:03d}：Razorソースがありません")
        elif f'@page "/learn/{seq:03d}"' not in razor_file.read_text(encoding="utf-8"):
            missing.append(f"学習順{seq:03d}：正規URLがRazorにありません")
        if actual.get("canonical_url") != f"/learn/{seq:03d}":
            missing.append(f"学習順{seq:03d}：正規URLの対応が不正")
        if f"| {seq:03d} | " not in source_index or f"`/learn/{seq:03d}`" not in source_index:
            missing.append(f"学習順{seq:03d}：直接索引の表示がありません")
    # 実際のメニュー/ホームに110件すべての正規URLが現れることを検証する。
    actual_home = (ROOT / "src/PgBlazorSamples.Web/Components/Pages/Home.razor").read_text(encoding="utf-8")
    actual_nav = (ROOT / "src/PgBlazorSamples.Web/Components/Layout/NavMenu.razor").read_text(encoding="utf-8")
    for entry in mapped_lessons:
        seq = entry["sequence"]
        new = f'href="/learn/{seq:03d}"'
        old = f'href="{entry["app_url"]}"'
        if new not in actual_home or new not in actual_nav:
            missing.append(f"学習順{seq:03d}：正規URLのホーム・メニューへの参照が不足")
        if old in actual_home or old in actual_nav:
            missing.append(f"学習順{seq:03d}：ホーム・メニューに互換URLが残っています")
    import re
    for path in (ROOT / "README.md", source_index_path):
        visible = re.sub(r"\]\([^)]+\)", "]", path.read_text(encoding="utf-8"))
        if re.search(r"(?:旧(?:Step|ステップ)|(?:Step|ステップ)\s*\d{1,3})", visible):
            missing.append(f"{path.relative_to(ROOT)}：現在の学習順以外の番号表記があります")

# 現行教材の直接ソースリンク、正規URL、互換URLの残存を検査する。
for entry in mapped_lessons:
    doc_file = ROOT / entry["lesson_path"]
    view_file = ROOT / entry["razor_path"]
    if not doc_file.is_file() or not view_file.is_file():
        continue
    doc_text = doc_file.read_text(encoding="utf-8")
    source_links = re.findall(r"\[対応するRazor実装\]\(([^)]+)\)", doc_text)
    if len(source_links) != 1:
        missing.append(f'学習順{entry["sequence"]:03d}：直接Razorリンクが一意でありません')
    elif (doc_file.parent / source_links[0]).resolve() != view_file.resolve():
        missing.append(f'学習順{entry["sequence"]:03d}：教材の直接Razorリンクが不正')
    if f'`{entry["canonical_url"]}`' not in doc_text:
        missing.append(f'学習順{entry["sequence"]:03d}：教材に正規URLがありません')
    if f'@page "{entry["app_url"]}"' not in view_file.read_text(encoding="utf-8"):
        missing.append(f'学習順{entry["sequence"]:03d}：互換URLの@pageが失われています')

# 互換用のページ定義は維持し、画面内の移動先は正規URLへ統一する。
compatibility = {entry["app_url"] for entry in mapped_lessons}
for view_file in list(SAMPLES.rglob("*.razor")) + list(LESSONS.rglob("*.razor")):
    view_text = view_file.read_text(encoding="utf-8")
    for target in re.findall(r'href="([^"]+)"', view_text):
        if target in compatibility:
            missing.append(f'{view_file.relative_to(ROOT)}：互換URLへの利用者向けリンクが残っています: {target}')


# 現行Web目次・NavMenuを学習順だけで公開し、旧番号を案内しない。
for page_path in ("src/PgBlazorSamples.Web/Components/Pages/Home.razor",
                  "src/PgBlazorSamples.Web/Components/Layout/NavMenu.razor"):
    ui = (ROOT / page_path).read_text(encoding="utf-8")
    if re.search(r"(?:旧ステップ番号から探す|旧URLから探す|(?:Step|ステップ)\\s*\\d{1,2}|学習順\\d{1,2}(?!\\d))", ui):
        missing.append(f"{page_path}：利用者向けの旧番号表記が残っています")
if missing:
    raise SystemExit("教材の整合性検査失敗：\n" + "\n".join("- " + item for item in missing))
print(f"教材検査成功：ステップ{len(steps)}件・コラム{max(steps) // 5 + (1 if max(steps) >= 20 else 0) + (1 if max(steps) >= 40 else 0) + (1 if max(steps) >= 65 else 0)}件。")

# 全110件のRazorと明示的なサービス/SQL/JS/テスト参照を静的照合する。
# この検査のSUCCESSを、実処理や実DB・現地受入の証明には使わない。
import subprocess
subprocess.run([sys.executable, str(ROOT / "scripts/audit-source-trace.py")], check=True)
