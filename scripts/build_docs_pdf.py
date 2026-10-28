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
    ("第1段階の概要：Web開発の前提", "docs/course-v3/stages/stage01.md"),
    ("学習順001：WinFormsとWebの処理場所を比較する", "docs/course-v3/lessons/Stage01/L001WebExecutionPage.md"),
    ("学習順002：URL・HTTPと要求／応答を理解する", "docs/course-v3/lessons/Stage01/L002HttpRequestResponsePage.md"),
    ("学習順003：HTML・CSS・フォームの基礎", "docs/course-v3/lessons/Stage01/L003HtmlCssFormPage.md"),
    ("学習順004：開発者ツールでDOM・Network・Consoleを見る", "docs/course-v3/lessons/Stage01/L004DomNetworkConsolePage.md"),
    ("学習順005：Visual Studio 2026でWebアプリを起動・デバッグする", "docs/course-v3/lessons/Stage01/L005VisualStudioDebugPage.md"),
    ("第1組の復習コラム：Webの基本", "docs/course-v3/reviews/review-01.md"),
    ("第2段階の概要：Blazorの基本と実行原理", "docs/course-v3/stages/stage02.md"),
    ("学習順006：最初のBlazor画面を表示する", "docs/course-v3/lessons/Stage02/L006FirstBlazorPage.md"),
    ("学習順007：URLと画面遷移を理解する", "docs/course-v3/lessons/Stage02/L007NavigationPage.md"),
    ("学習順008：RazorでC#の値を表示する", "docs/course-v3/lessons/Stage02/L008RazorSyntaxPage.md"),
    ("学習順009：Static SSRとInteractive Serverの違い", "docs/course-v3/lessons/Stage02/L009StaticSsrInteractivePage.md"),
    ("学習順010：ボタンのイベントを扱う", "docs/course-v3/lessons/Stage02/L010EventPage.md"),
    ("第2組の復習：Razorと実行方式の復習", "docs/course-v3/reviews/review-02.md"),
    ("学習順011：入力値を双方向に結び付ける", "docs/course-v3/lessons/Stage02/L011TwoWayBindingPage.md"),
    ("学習順012：条件分岐と繰り返しで表示する", "docs/course-v3/lessons/Stage02/L012ConditionalRenderingPage.md"),
    ("学習順013：親から子へ値を渡す", "docs/course-v3/lessons/Stage02/L013ComponentParametersPage.md"),
    ("学習順014：子から親へ通知する", "docs/course-v3/lessons/Stage02/L014EventCallbackPage.md"),
    ("学習順015：SignalR・circuit・再接続", "docs/course-v3/lessons/Stage02/L015CircuitReconnectPage.md"),
    ("第3組の復習：入力・部品・対話回線の復習", "docs/course-v3/reviews/review-03.md"),
    ("学習順016：初期化・更新・破棄を理解する", "docs/course-v3/lessons/Stage02/L016ComponentLifecyclePage.md"),
    ("学習順017：事前レンダリングと初期化の実行回数", "docs/course-v3/lessons/Stage02/L017PrerenderInitializationPage.md"),
    ("学習順018：入力フォームを日本語で検証する", "docs/course-v3/lessons/Stage02/L018ValidatedFormPage.md"),
    ("学習順019：URL・履歴・再読込と画面状態", "docs/course-v3/lessons/Stage02/L019UrlHistoryStatePage.md"),
    ("学習順020：circuit・ブラウザー・DBの状態保存の選択", "docs/course-v3/lessons/Stage02/L020StatePersistenceChoicePage.md"),
    ("第4組の復習：初期化・再読込・状態の復習", "docs/course-v3/reviews/review-04.md"),
    # 現行学習順の復習に役立つ補足コラムを収録する。
    ("補足コラム：イベントと状態の関係", "docs/columns/column01-05.md"),
    ("補足コラム：親子部品と入力検証", "docs/columns/column06-10.md"),
    ("補足資料：DIとサービス設計", "docs/stages/stage02.md"),
    ("第3段階の概要：DI・サービスと単体テスト", "docs/course-v3/stages/stage03.md"),
    ("学習順021：DI（依存性注入）の基本", "docs/course-v3/lessons/Stage03/L021DependencyInjectionPage.md"),
    ("学習順022：インターフェイスで部品を交換する", "docs/course-v3/lessons/Stage03/L022InterfaceClockPage.md"),
    ("学習順023：Singleton・Scoped・Transient", "docs/course-v3/lessons/Stage03/L023ServiceLifetimesPage.md"),
    ("学習順024：BlazorのScopedと対話回線", "docs/course-v3/lessons/Stage03/L024CircuitScopedPage.md"),
    ("学習順025：injectとコンストラクター注入", "docs/course-v3/lessons/Stage03/L025InjectionPatternsPage.md"),
    ("第5組の復習：DIの登録・交換・寿命と注入", "docs/columns/column11-15.md"),
    ("学習順026：Optionsで設定を受け取る", "docs/course-v3/lessons/Stage03/L026OptionsPatternPage.md"),
    ("学習順027：非同期処理とログ", "docs/course-v3/lessons/Stage03/L027AsyncLoggingPage.md"),
    ("学習順028：DIによる単体テスト", "docs/course-v3/lessons/Stage03/L028DependencyUnitTestPage.md"),
    ("学習順029：Applicationへ処理を分離", "docs/course-v3/lessons/Stage03/L029ApplicationLayerPage.md"),
    ("学習順030：DIで課題管理を作る", "docs/course-v3/lessons/Stage03/L030TodoServicePage.md"),
    ("学習コラム04：設定とサービス分離", "docs/columns/column16-20.md"),
    ("特別コラム05：ライブラリが提供するDI", "docs/columns/column-library-di.md"),
    ("第4段階の概要：MudBlazorとブラウザーの業務UI", "docs/course-v3/stages/stage04.md"),
    ("補足資料：MudBlazorによる業務画面", "docs/stages/stage03.md"),
    ("学習順031：MudBlazorと日本語翻訳を導入する", "docs/course-v3/lessons/Stage04/L031MudIntroductionPage.md"),
    ("学習順032：MudBlazorのレイアウトを学ぶ", "docs/course-v3/lessons/Stage04/L032MudLayoutPage.md"),
    ("学習順033：日本語で入力部品を使う", "docs/course-v3/lessons/Stage04/L033MudInputsPage.md"),
    ("学習順034：MudFormの入力検証を学ぶ", "docs/course-v3/lessons/Stage04/L034MudValidationPage.md"),
    ("学習順035：MudTableで一覧とソート", "docs/course-v3/lessons/Stage04/L035MudTablePage.md"),
    ("学習コラム06：MudBlazorのDIと翻訳", "docs/columns/column21-25.md"),
    ("学習順036：MudDataGridで検索・並べ替えする", "docs/course-v3/lessons/Stage04/L036DataGridPage.md"),
    ("学習順037：確認ダイアログで実行と中止を選ぶ", "docs/course-v3/lessons/Stage04/L037DialogPage.md"),
    ("学習順038：Snackbarと処理中の状態を表示する", "docs/course-v3/lessons/Stage04/L038NotificationPage.md"),
    ("学習順039：ダークモードとキーボード操作を学ぶ", "docs/course-v3/lessons/Stage04/L039AccessibilityPage.md"),
    ("学習順040：MudBlazor日本語UIを総合演習する", "docs/course-v3/lessons/Stage04/L040UiPracticePage.md"),
    ("第8組の復習：検索・確認・通知・操作性", "docs/columns/column26-30.md"),
    ("学習順041：JavaScript interopの利用判断", "docs/course-v3/lessons/Stage04/L041JsInteropPage.md"),
    ("学習順042：ファイルアップロードと入力制約", "docs/course-v3/lessons/Stage04/L042FileUploadPage.md"),
    ("学習順043：CSV・PDFファイルのダウンロード", "docs/course-v3/lessons/Stage04/L043DownloadPage.md"),
    ("学習順044：ErrorBoundary・取消・再試行", "docs/course-v3/lessons/Stage04/L044ErrorRecoveryPage.md"),
    ("学習順045：HTMLの意味・フォーカス・拡大表示の最終確認", "docs/course-v3/lessons/Stage04/L045AccessibilityReviewPage.md"),
    ("第9組の復習：ブラウザー機能と安全な操作", "docs/course-v3/reviews/review-09.md"),
    ("補足資料：PostgreSQLとEF Core", "docs/stages/stage04.md"),
    ("第5段階の概要：DapperからEF Core 10・PostgreSQL 18へ", "docs/course-v3/stages/stage05.md"),
    ("学習順046：DapperとEF Coreの役割・選定を比較する", "docs/course-v3/lessons/Stage05/L046OrmComparisonPage.md"),
    ("学習順047：モデルとDTOを理解する", "docs/course-v3/lessons/Stage05/L047EfModelPage.md"),
    ("学習順048：PostgreSQLで学習テーブルを定義する", "docs/course-v3/lessons/Stage05/L048SqlSchemaPage.md"),
    ("学習順049：EF Core 10をWebへ登録する", "docs/course-v3/lessons/Stage05/L049EfSetupPage.md"),
    ("学習順050：DbContextFactoryと接続プール", "docs/course-v3/lessons/Stage05/L050DbContextFactoryPage.md"),
    ("第10組の復習：DapperとEF Coreの入口", "docs/course-v3/reviews/review-10.md"),
    ("学習順051：LINQ検索とAsNoTracking", "docs/course-v3/lessons/Stage05/L051EfSearchPage.md"),
    ("学習順052：SQLのWHERE／JOIN／ORDER BYとLINQ・生成SQL", "docs/course-v3/lessons/Stage05/L052SqlAndLinqPage.md"),
    ("学習順053：Dapperの接続／ExecuteとDbContext・追跡・SaveChanges", "docs/course-v3/lessons/Stage05/L053PersistenceComparisonPage.md"),
    ("学習コラム08：WebプロジェクトだけでEF Coreを使う", "docs/columns/column31-35.md"),
    ("学習順054：EF Coreの登録・更新・削除", "docs/course-v3/lessons/Stage05/L054EfCrudPage.md"),
    ("学習順055：DateOnly・TimeOnly・Rangeの型変換", "docs/course-v3/lessons/Stage05/L055EfDateRangePage.md"),
    ("第11組の復習：LINQ・追跡・CRUD・型", "docs/course-v3/reviews/review-11.md"),
    ("学習順056：トランザクションを理解する", "docs/course-v3/lessons/Stage05/L056EfTransactionPage.md"),
    ("学習順057：xmin楽観的排他を確認する", "docs/course-v3/lessons/Stage05/L057EfConcurrencyPage.md"),
    ("学習順058：既存DB・ビュー・キーなしモデル・FDWを扱う", "docs/course-v3/lessons/Stage05/L058KeylessViewPage.md"),
    ("学習順059：FromSql／SqlQueryのパラメーターとDapper併用", "docs/course-v3/lessons/Stage05/L059ParameterizedSqlPage.md"),
    ("学習順060：PostgreSQL 18の実接続確認", "docs/course-v3/lessons/Stage05/L060EfDbCheckPage.md"),
    ("第12組の復習：トランザクション・xmin・安全SQL", "docs/course-v3/reviews/review-12.md"),
    ("学習コラム09：更新・日付Range・競合・DB確認", "docs/columns/column36-40.md"),
    ("特別コラム：用途から選ぶMudBlazorコンポーネント", "docs/columns/column-mudblazor-by-purpose.md"),
    ("補足資料：一覧データ連携", "docs/stages/stage05.md"),
    ("第6段階の概要：一覧データ連携・検索・編集", "docs/course-v3/stages/stage06.md"),
    ("学習順061：Itemsを使う一覧の条件", "docs/course-v3/lessons/Stage06/L061ItemsBoundaryPage.md"),
    ("学習順062：MudTableのServerDataでDB検索", "docs/course-v3/lessons/Stage06/L062TableServerDataPage.md"),
    ("学習順063：MudDataGridのServerDataとEF Core", "docs/course-v3/lessons/Stage06/L063GridServerDataPage.md"),
    ("学習順064：複数列ソートを安全に組み立てる", "docs/course-v3/lessons/Stage06/L064SafeSortPage.md"),
    ("学習順065：日付と完了状態で絞り込む", "docs/course-v3/lessons/Stage06/L065FilterSearchPage.md"),
    ("第13組の復習：ItemsとServerData・複合検索", "docs/columns/column41-45.md"),
    ("学習順066：関連テーブルをDTOへ射影する", "docs/course-v3/lessons/Stage06/L066RelatedDtoPage.md"),
    ("学習順067：キーセットページングを理解する", "docs/course-v3/lessons/Stage06/L067KeysetPage.md"),
    ("学習順068：取得済み一覧を部分描画する", "docs/course-v3/lessons/Stage06/L068VirtualizeItemsPage.md"),
    ("学習順069：DB取得と仮想スクロールを連携する", "docs/course-v3/lessons/Stage06/L069VirtualizeDatabasePage.md"),
    ("学習順070：デバウンスと障害時の再試行", "docs/course-v3/lessons/Stage06/L070SearchUxPage.md"),
    ("第14組の復習：関連DTO・キーセット・仮想化・検索UX", "docs/columns/column46-50.md"),
    ("学習順071：編集DTOをフォームで検証して保存する", "docs/course-v3/lessons/Stage06/L071EditDtoPage.md"),
    ("学習順072：ダイアログで保存と取消を分ける", "docs/course-v3/lessons/Stage06/L072EditDialogPage.md"),
    ("学習順073：複数選択と原子的な一括処理", "docs/course-v3/lessons/Stage06/L073BulkEditPage.md"),
    ("学習順074：xmin競合と最新値の確認", "docs/course-v3/lessons/Stage06/L074ConflictEditorPage.md"),
    ("学習順075：詳細DTOとN+1の防止", "docs/course-v3/lessons/Stage06/L075DetailProjectionPage.md"),
    ("第15組の復習：編集DTO・Dialog・一括・競合・詳細", "docs/columns/column51-55.md"),
    ("学習順076：日本語日付範囲とDB検索", "docs/course-v3/lessons/Stage06/L076RangeSearchPage.md"),
    ("学習順077：キャッシュと権限境界", "docs/course-v3/lessons/Stage06/L077CacheBoundaryPage.md"),
    ("学習順078：DB側集計とCSV出力", "docs/course-v3/lessons/Stage06/L078StatisticsExportPage.md"),
    ("学習順079：EXPLAINと応答時間計測", "docs/course-v3/lessons/Stage06/L079PerformancePage.md"),
    ("学習順080：一覧パターンを要件から選ぶ", "docs/course-v3/lessons/Stage06/L080PatternChoicePage.md"),
    ("第16組の復習：期間・キャッシュ・集計・性能・選定", "docs/columns/column56-60.md"),
# 学習順081〜085・復習17・概要7のソース順だけを維持。PDF生成は禁止のまま。
    ("第7段階の概要：Webセキュリティ・AD認証とDB認可", "docs/course-v3/stages/stage07.md"),
    ("学習順081：Windows認証・Cookie・HTTP・circuit", "docs/course-v3/lessons/Stage07/L081WindowsCookieCircuitPage.md"),
    ("学習順082：Middlewareの順序・HTTPS・IISの経路", "docs/course-v3/lessons/Stage07/L082MiddlewareHttpsPage.md"),
    ("学習順083：XSS・CSRF・SQL注入の防御境界", "docs/course-v3/lessons/Stage07/L083XssCsrfSqlPage.md"),
    ("学習順084：環境別設定・Secret・Data Protection鍵", "docs/course-v3/lessons/Stage07/L084SecretsKeysPage.md"),
    ("学習順085：接続切断・セッション失効・権限再検証", "docs/course-v3/lessons/Stage07/L085ReauthorizationPage.md"),
    ("第17組の復習：Webの安全境界", "docs/course-v3/reviews/review-17.md"),
    ("補足資料：Windows認証とDBによる業務認可", "docs/stages/stage06.md"),
    ("学習順086：Windows統合認証を構成する", "docs/course-v3/lessons/Stage07/L086NegotiatePage.md"),
    ("学習順087：認証済みドメインユーザー名を扱う", "docs/course-v3/lessons/Stage07/L087SidPage.md"),
    ("学習順088：認証済みユーザー名と既存台帳を結ぶ", "docs/course-v3/lessons/Stage07/L088UserMappingPage.md"),
    ("学習順089：未登録・無効利用者を拒否する", "docs/course-v3/lessons/Stage07/L089UserRefusalPage.md"),
    ("学習順090：既存台帳の業務権限を確認する", "docs/course-v3/lessons/Stage07/L090RolePermissionPage.md"),
    ("第18組の復習：Windows本人認証と既存台帳", "docs/columns/column61-65.md"),
    ("特別コラム：1万人規模の認証・既存台帳認可・セッション・監査", "docs/columns/column-auth-at-scale.md"),
    ("学習順091：サービス側で認可を強制する", "docs/course-v3/lessons/Stage07/L091AuthorizedEntryPage.md"),
    ("学習順092：部署ごとのCountとItems", "docs/course-v3/lessons/Stage07/L092UnitScopedPage.md"),
    ("学習順093：集計とCSVにも認可を適用", "docs/course-v3/lessons/Stage07/L093ScopedExportPage.md"),
    ("学習順094：保存直前の権限とxminを再確認", "docs/course-v3/lessons/Stage07/L094SecuredEditPage.md"),
    ("学習順095：Windows Server 2025の実機受入を分ける", "docs/course-v3/lessons/Stage07/L095DeploymentGatePage.md"),
    ("第19組の復習：業務サービスの操作・行単位認可", "docs/columns/column66-70.md"),
    ("補足資料：監査・競合制御・運用", "docs/stages/stage07.md"),
    ("第8段階の概要：監査・競合制御・Windows Server運用", "docs/course-v3/stages/stage08.md"),
    ("学習順096：既存台帳のキー正規化・重複拒否と索引", "docs/course-v3/lessons/Stage08/L096DirectoryNormalizationPage.md"),
    ("学習順097：明示Windowsログインと短命Cookie", "docs/course-v3/lessons/Stage08/L097ExplicitWindowsLoginPage.md"),
    ("学習順098：権限行ロックと更新の原子性", "docs/course-v3/lessons/Stage08/L098TransactionalAuthorizationPage.md"),
    ("学習順099：本人・操作・結果を永続監査する", "docs/course-v3/lessons/Stage08/L099DurableAuditPage.md"),
    ("学習順100：IIS・Kerberos・負荷・障害の実機受入", "docs/course-v3/lessons/Stage08/L100IisAcceptancePage.md"),
    ("第20組の復習：認証・認可・監査の本番移行判断", "docs/columns/column71-75.md"),
    ("学習順101：二人の編集競合とxmin", "docs/course-v3/lessons/Stage08/L101ConcurrentEditPage.md"),
    ("学習順102：キャッシュの有効期限と権限失効", "docs/course-v3/lessons/Stage08/L102AuthorizedCachePage.md"),
    ("学習順103：ヘルスチェック・構造化ログ・運用監視", "docs/course-v3/lessons/Stage08/L103DiagnosticsPage.md"),
    ("学習順104：実DB・ブラウザー・認可の結合/E2E試験", "docs/course-v3/lessons/Stage08/L104EndToEndAcceptancePage.md"),
    ("学習順105：Windows Server 2025/IISの配布・切替・復旧", "docs/course-v3/lessons/Stage08/L105DeploymentPage.md"),
    ("第21組の復習：競合・失効・監視・試験・配布", "docs/columns/column76-80.md"),
    ("第9段階の概要：業務アプリの通し演習", "docs/course-v3/stages/stage09.md"),
    ("学習順106：業務要件・画面遷移・WinForms移行の設計", "docs/course-v3/lessons/Stage09/L106RequirementFlowPage.md"),
    ("学習順107：PostgreSQL検索とページング・生成SQL", "docs/course-v3/lessons/Stage09/L107SearchPagingPage.md"),
    ("学習順108：DTO編集・競合・取消・例外の統合", "docs/course-v3/lessons/Stage09/L108DtoEditPage.md"),
    ("学習順109：認証済み本人と既存台帳による権限・監査", "docs/course-v3/lessons/Stage09/L109PermissionAuditPage.md"),
    ("学習順110：E2E・障害復旧・IIS受入表", "docs/course-v3/lessons/Stage09/L110AcceptanceLedgerPage.md"),
    ("第22組の復習：総合演習", "docs/course-v3/reviews/review-22.md"),
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
        is_stage = "段階の概要" in title
        chapters_html.append(
            f'<section class="chapter" id="lesson-{number}">'
            f'<p class="chapter-kicker">{"各段階の学習案内" if is_stage else ("先取り学習（未実装の設計）" if "先取りガイド" in title else ("学習コラム" if "学習コラム" in title else "ステップアップ学習"))}</p>'
            f'<h1>{html.escape(title)}</h1>'
            f'{markdown.render(content)}</section>'
        )
        toc_class = "toc-stage" if is_stage else "toc-topic"
        toc.append(f'<li class="{toc_class}"><a href="#lesson-{number}">{html.escape(title)}</a></li>')

    step_count = sum(1 for title, _ in CHAPTERS if title.startswith(("ステップ", "学習順")))
    column_count = sum(1 for title, _ in CHAPTERS if "コラム" in title)
    stage_count = sum(1 for title, _ in CHAPTERS if "段階の概要" in title)
    body = f"""<!doctype html><html lang="ja"><head><meta charset="utf-8"></head><body>
    <section class="cover">
      <p class="brand">Visual Studio 2026 / .NET 10</p>
      <h1>Blazor 実践学習テキスト</h1>
      <p class="lead">WinForms・Dapperの経験を生かしてWeb開発を学ぶ<br>学習順{step_count}項目・概要{stage_count}本・各種学習コラム</p>
      <div class="cover-note">各ステップ：目標 → 操作 → コードのしくみ → 演習<br>
      5ステップごとに理解を深める復習コラムを収録</div>
      <p class="version">学習範囲：学習順{step_count}項目／コラム{column_count}本／概要{stage_count}本</p>
    </section>
    <section class="toc">
      <h1>目次</h1>
      <p>学習順001から順に進めてください。</p>
      <ol>{''.join(toc)}</ol>
    </section>
    {''.join(chapters_html)}
    </body></html>"""

    style = r"""
    @page { size: A4; margin: 19mm 18mm 20mm;
      @bottom-left { content: "Blazor 実践学習テキスト"; font-size: 9pt; color: #364c5d }
      @bottom-right { content: counter(page); font-size: 10pt; color: #233949 }
    }
    html { font-family: "Noto Sans CJK JP", "IPAGothic", sans-serif; color: #172b3b }
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
    .toc li.toc-stage { margin-top: 6mm; font-weight: bold; border-bottom: 1px solid #9aafbd; padding-bottom: 2mm; }
    .toc li.toc-topic { margin-left: 4mm; }
    .toc a { text-decoration: none; color: #164967; font-weight: 600 }
    .chapter { break-before: page; }
    .chapter-kicker { font-size: 10pt; font-weight: 700; color: #285a77; border-bottom: 1px solid #9aafbd }
    h1 { font-size: 21pt; line-height: 1.44; color: #113a56; break-after: avoid }
    h2 { font-size: 15pt; color: #154965; border-left: 4px solid #2d647f;
         padding-left: 3mm; break-after: avoid; margin-top: 8mm }
    h3 { font-size: 12.5pt; break-after: avoid; color: #234d64 }
    p { margin: 3mm 0; orphans: 2; widows: 2 }
    li { margin: 1.7mm 0; }
    code { font-family: "Noto Sans Mono CJK JP", "IPAGothic", monospace; font-size: .88em; overflow-wrap: anywhere }
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
