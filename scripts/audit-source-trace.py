#!/usr/bin/env python3
"""110教材のRazorから明示的なC#/SQL/JS/テスト参照を静的に調査する。

文字列で確認した参照は実行証拠ではない。間接的なDI依存、DB/JS実行、
正常系・異常系・現地試験をこの監査だけでPASSにしない。
"""
import argparse
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

# Windowsランナーのシステム既定エンコーディングに依存せず日本語の検査結果を表示する。
if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
if hasattr(sys.stderr, 'reconfigure'):
    sys.stderr.reconfigure(encoding='utf-8', errors='replace')


def contains_symbol(source, symbol):
    return re.search(r'(?<!\w)' + re.escape(symbol) + r'(?!\w)', source) is not None


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--report', type=Path, help='Write machine-readable trace JSON')
    args = parser.parse_args()
    mapping = json.loads((ROOT / 'docs/course-v3/source-map.json').read_text(encoding='utf-8'))['lessons']
    curriculum = json.loads((ROOT / 'docs/course-v3/curriculum.json').read_text(encoding='utf-8'))['steps']
    problems = []
    if len(mapping) != 110 or len(curriculum) != 110:
        problems.append('教材・索引が110件ではありません')
    curriculum_by_sequence = {entry['sequence']: entry for entry in curriculum}
    code = sorted(ROOT.glob('src/**/*.cs'))
    tests = sorted(ROOT.glob('tests/**/*.cs'))
    scripts = sorted(ROOT.glob('src/**/*.js'))
    sql = sorted(ROOT.glob('database/**/*.sql'))
    test_texts = {p: p.read_text(encoding='utf-8') for p in tests}
    code_symbols = {p: p.stem for p in code if len(p.stem) > 5 and p.stem not in {'GlobalUsings', 'AssemblyInfo', 'Program'}}
    rows = []
    for expected, entry in enumerate(mapping, 1):
        sequence = entry.get('sequence')
        name = f'{expected:03d}'
        if sequence != expected or sequence not in curriculum_by_sequence:
            problems.append(f'{name}: 学習順の欠番または重複')
            continue
        lesson_path = entry.get('lesson_path', '')
        razor_path = entry.get('razor_path', '')
        if curriculum_by_sequence[sequence].get('source') != lesson_path:
            problems.append(f'{name}: curriculumと教材パスが不一致')
        if not (ROOT / lesson_path).is_file() or not (ROOT / razor_path).is_file():
            problems.append(f'{name}: 教材またはRazorが見つかりません')
            continue
        source = (ROOT / razor_path).read_text(encoding='utf-8')
        canonical = f'/learn/{name}'
        declared_routes = re.findall(r'@page\s+"([^"]+)"', source)
        if canonical not in declared_routes or entry.get('canonical_url') != canonical:
            problems.append(f'{name}: 正規ルートが見つかりません')
        if entry.get('app_url') not in declared_routes:
            problems.append(f'{name}: 互換ルートが見つかりません')
        references = [(p, symbol) for p, symbol in code_symbols.items() if contains_symbol(source, symbol)]
        related_tests = []
        for test_path, body in test_texts.items():
            matched = sorted({symbol for _, symbol in references if contains_symbol(body, symbol)})
            if matched:
                related_tests.append({'path': test_path.relative_to(ROOT).as_posix(), 'matched_symbols': matched})
        rows.append({
            'sequence': name,
            'stage': entry.get('stage'),
            'lesson_path': lesson_path,
            'razor_path': razor_path,
            'canonical_route': canonical,
            'observed_routes': declared_routes,
            'declared_injections': re.findall(r'^\s*@inject\s+(.+)$', source, re.MULTILINE),
            'code_symbol_references': [p.relative_to(ROOT).as_posix() for p, _ in references],
            'test_symbol_candidates': related_tests,
            'js_filename_references': [p.relative_to(ROOT).as_posix() for p in scripts if p.name in source],
            'sql_filename_references': [p.relative_to(ROOT).as_posix() for p in sql if p.name in source],
            'evidence_level': 'STATIC_TEXT_MATCH_ONLY',
            'behavior_verified': False,
            'db_integration_verified': False,
            'site_verified': False,
        })
    # 利用者に見える過去番号らしい文言を抽出する。誤検知の可能性があるため人手の確認候補とする。
    legacy_label_candidates = []
    prose_paths = [ROOT / x['lesson_path'] for x in mapping]
    prose_paths += sorted((ROOT / 'docs/course-v3/reviews').glob('*.md'))
    prose_paths += sorted((ROOT / 'docs/course-v3/stages').glob('*.md'))
    prose_paths += [
        ROOT / 'README.md',
        ROOT / 'docs/course-v3/source-index.md',
        ROOT / 'src/PgBlazorSamples.Web/Components/Pages/Home.razor',
        ROOT / 'src/PgBlazorSamples.Web/Components/Layout/NavMenu.razor',
    ]
    legacy = re.compile(r'旧(?:Step|ステップ|番号|URL)|以前の(?:Step|ステップ|番号)|(?<![A-Za-z])Step\s*0?[1-9][0-9]?\b')
    for page in prose_paths:
        if not page.is_file():
            problems.append(f'{page.relative_to(ROOT)}: 表記監査対象が存在しません')
            continue
        in_code = False
        for line_no, line in enumerate(page.read_text(encoding='utf-8').splitlines(), 1):
            if line.lstrip().startswith(('~~~', chr(96) * 3)):
                in_code = not in_code
                continue
            if in_code:
                continue
            visible = re.sub(r'\]\([^)]+\)', ']', line)
            visible = re.sub(chr(96) + '[^' + chr(96) + ']*' + chr(96), '', visible)
            if legacy.search(visible):
                legacy_label_candidates.append({
                    'path': page.relative_to(ROOT).as_posix(),
                    'line': line_no,
                    'excerpt': visible.strip()[:180],
                })
    report = {
        'schema_version': 1,
        'source': 'docs/course-v3/source-map.json',
        'disclaimer': ('静的識別子一致であり、呼出しの意味解析、間接依存、'
                       '単体・Chrome・DB試験のPASS、実AD/IIS/FDW受入を証明しない。'),
        'coverage': {
            'lessons': len(rows),
            'source_cs_files_scanned': len(code),
            'test_cs_files_scanned': len(tests),
            'js_files_scanned': len(scripts),
            'sql_files_scanned': len(sql),
            'lessons_with_code_symbol_references': sum(bool(r['code_symbol_references']) for r in rows),
            'lessons_with_test_symbol_candidates': sum(bool(r['test_symbol_candidates']) for r in rows),
            'end_to_end_verified': 0,
            'legacy_label_candidates': len(legacy_label_candidates),
        },
        'lessons': rows,
        'legacy_label_candidates': legacy_label_candidates,
    }
    if args.report:
        args.report.parent.mkdir(parents=True, exist_ok=True)
        args.report.write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print('110教材の参照静的監査: ' + json.dumps(report['coverage'], ensure_ascii=False))
    if len(rows) != 110:
        problems.append('110件の完全追跡に失敗')
    if problems:
        for problem in problems:
            print('ERROR: ' + problem, file=sys.stderr)
        return 1
    return 0


if __name__ == '__main__':
    sys.exit(main())
