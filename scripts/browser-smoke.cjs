#!/usr/bin/env node
// 実ブラウザーの操作により、Interactive Serverでイベントが処理されることを確認する。
// この試験は開発用の架空データだけを使用し、社内AD・DBには接続しない。
const { chromium } = require("playwright-core");
const { spawn, execFileSync } = require("node:child_process");
const { mkdirSync } = require("node:fs");
const path = require("node:path");

const root = path.resolve(__dirname, "..");
const base = "http://127.0.0.1:5185";

// GitHub ActionsのUbuntuランナーに導入済みのブラウザーを使う。
function findChrome() {
    for (const name of ["google-chrome", "google-chrome-stable", "chromium", "chromium-browser"]) {
        try {
            return execFileSync("which", [name], { encoding: "utf8" }).trim();
        } catch {
            // 該当するコマンドがなければ次の候補を確認する。
        }
    }
    throw new Error("ブラウザーが見つかりません。UbuntuランナーにChromeを導入してください。");
}
const server = spawn("dotnet", [
    "run", "--project", "src/PgBlazorSamples.Web/PgBlazorSamples.Web.csproj",
    "--configuration", "Release", "--no-build", "--no-launch-profile", "--urls", base
], {
    cwd: root,
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: "Development" },
    stdio: ["ignore", "pipe", "pipe"]
});
let serverLog = "";
server.stdout.on("data", data => { serverLog += data.toString(); });
server.stderr.on("data", data => { serverLog += data.toString(); });

async function waitForServer() {
    for (let tries = 0; tries < 75; tries++) {
        if (server.exitCode !== null) break;
        try {
            const response = await fetch(base + "/learn/04");
            if (response.ok) return;
        } catch {
            // 起動中はポートへ接続できないことがある。
        }
        await new Promise(resolve => setTimeout(resolve, 1000));
    }
    throw new Error("Webアプリの起動を確認できませんでした。\n" + serverLog);
}
async function waitText(page, selector, expected) {
    await page.waitForFunction(
        ([css, value]) => document.querySelector(css)?.textContent?.includes(value),
        [selector, expected],
        { timeout: 15000 }
    );
}

(async () => {
    let browser;
    try {
        await waitForServer();
        // Step78：匿名で確認できるのはプロセスの稼働だけ。業務readinessではない。
        const live = await fetch(base + "/health/live");
        if (!live.ok) throw new Error("Step78：/health/liveが正常応答しません。");
        browser = await chromium.launch({
            executablePath: findChrome(),
            headless: true,
            args: ["--no-sandbox", "--disable-dev-shm-usage"]
        });
        const page = await browser.newPage({ locale: "ja-JP", viewport: { width: 1280, height: 850 } });
        const evidence = path.resolve(root, "dist", "browser-evidence");
        mkdirSync(evidence, { recursive: true });

        // 新第1段階の概要・Web基礎・復習を実ブラウザーで確認する。
        await page.goto(base + "/learn/stage/web");
        await page.getByRole("table").first().getByText("ブラウザーがHTML/CSSを描画").waitFor();
        await page.getByRole("heading", { name: "第1段階の概要：Web開発の前提" }).waitFor();
        await page.goto(base + "/learn/web-01");
        await page.waitForTimeout(1000);
        await page.getByRole("button", { name: "サーバー側で1増やす" }).click();
        await waitText(page, '[data-testid="web01-count"]', "1");
        await page.goto(base + "/learn/web-02");
        await waitText(page, '[data-testid="web02-path"]', "/learn/web-02");
        await page.goto(base + "/learn/web-03");
        await page.waitForTimeout(850);
        await page.getByRole("textbox", { name: "練習用の名前" }).fill("練習太郎");
        await waitText(page, '[data-testid="web03-preview"]', "練習太郎");
        await page.goto(base + "/learn/web-04");
        await page.waitForTimeout(850);
        await page.getByRole("button", { name: "追加の説明を表示・非表示" }).click();
        await waitText(page, '[data-testid="web04-state"]', "表示中");
        await page.getByTestId("web04-added").waitFor();
        await page.goto(base + "/learn/web-05");
        await page.waitForTimeout(850);
        await page.getByRole("button", { name: "デバッグ対象の処理を実行する" }).click();
        await waitText(page, '[data-testid="web05-count"]', "1");
        await page.goto(base + "/learn/column/web-05");
        await page.getByRole("table").first().getByText("NetworkのDocumentとStatus").waitFor();
        await page.getByRole("heading", { name: "Webの処理場所・通信・画面表示を整理する" }).waitFor();
        await page.screenshot({ path: path.join(evidence, "web-foundation-review.png") });
        console.log("新第1段階のWeb基礎5画面と復習を確認");

        // 第2段階：新規5ページの表示、対話状態、URL復元と3組の復習を検証する。
        await page.goto(base + "/learn/stage/runtime");
        await page.getByRole("heading", { name: "第2段階の概要：Blazorの基本と実行原理" }).waitFor();
        // 全面改稿した学習順006〜010の表示・遷移・正常/拒否操作を確認する。
        await page.getByRole("table").first().getByRole("cell", { name: "Form", exact: true }).waitFor();
        await page.goto(base + "/learn/01");
        await page.getByRole("heading", { name: "はじめてのBlazor画面" }).waitFor();
        await page.getByRole("link", { name: "次へ：学習順007" }).click();
        await page.getByRole("heading", { name: "URLとルーティング" }).waitFor();
        await page.getByRole("link", { name: "学習順008へ進む" }).click();
        await page.getByRole("heading", { name: "C#の値を画面へ表示する" }).waitFor();
        await page.getByText("2026年10月26日").first().waitFor();
        await page.getByRole("link", { name: "次へ：学習順009" }).click();
        await page.getByRole("heading", { name: "Static SSRとInteractive Serverの違い" }).waitFor();
        await page.getByRole("link", { name: "次へ：学習順010" }).click();
        await page.getByRole("button", { name: "1増やす" }).click();
        await page.getByRole("button", { name: "1増やす" }).click();
        await waitText(page, '[data-testid="counter-value"]', "2");
        await page.getByRole("button", { name: "1減らす" }).click();
        await waitText(page, '[data-testid="counter-value"]', "1");
        await page.getByRole("button", { name: "0に戻す" }).click();
        await waitText(page, '[data-testid="counter-value"]', "0");
        if (!(await page.getByRole("button", { name: "1減らす" }).isDisabled())) {
            throw new Error("学習順010の0未満防止が無効です");
        }
        await page.getByRole("link", { name: "次へ：第2組の復習" }).click();
        await page.getByRole("heading", { name: "第2組の復習：Razorと実行方式の復習" }).waitFor();
        await page.getByRole("table").first().getByText("disabledとif").waitFor();
        console.log("全面改稿006〜010の実画面・学習順・復習・拒否を検証");
        // 第3組：親子の値・通知と表示件数、再接続画面への学習順を実ブラウザーで確認する。
        await page.goto(base + "/learn/05");
        await page.waitForTimeout(800);
        await page.getByLabel("表示する名前（架空の名前を入力）").fill("テスト太郎");
        await waitText(page, '[data-testid="name-preview"]', "テスト太郎");
        await page.getByRole("button", { name: "入力を消去する" }).click();
        await waitText(page, '[data-testid="name-preview"]', "まだ名前が入力されていません。");
        await page.getByRole("link", { name: "次へ：学習順012" }).click();
        await waitText(page, '[data-testid="visible-count"]', "4");
        await page.getByLabel("未完了の課題だけ表示する").check();
        await waitText(page, '[data-testid="visible-count"]', "2");
        await page.getByLabel("未完了の課題だけ表示する").uncheck();
        await waitText(page, '[data-testid="visible-count"]', "4");
        await page.getByRole("link", { name: "次へ：学習順013" }).click();
        await page.getByLabel("練習用の表示名").fill("練習花子");
        await page.getByTestId("greeting-card").first().getByText("練習花子").waitFor();
        await page.getByTestId("greeting-card").last().getByText("架空の利用者").waitFor();
        await page.getByRole("link", { name: "次へ：学習順014" }).click();
        await page.getByRole("button", { name: "Razor構文" }).click();
        await waitText(page, '[data-testid="step08-selection"]', "Razor構文");
        await page.getByRole("button", { name: "Razor構文" }).click();
        await waitText(page, '[data-testid="step08-count"]', "2");
        await page.getByRole("link", { name: "次へ：学習順015" }).click();
        await page.getByRole("button", { name: "対話回数を1増やす" }).click();
        await waitText(page, '[data-testid="runtime02-count"]', "1");
        await page.getByRole("link", { name: "次へ：第3組の復習" }).click();
        await page.getByRole("heading", { name: "第3組の復習：入力・部品・対話回線の復習" }).waitFor();
        await page.getByRole("table").first().getByText("EventCallback<string>").waitFor();
        await page.getByRole("link", { name: "次へ：学習順016" }).click();
        if (!page.url().includes("/learn/09")) throw new Error("第3組の復習から学習順016へ移動できません");
        console.log("全面改稿011〜015の実画面・親子通知・一覧フィルター・復習への遷移を検証");
        await page.goto(base + "/learn/runtime-01");
        await page.waitForTimeout(900);
        await page.getByRole("button", { name: "サーバー側の回数を増やす" }).click();
        await waitText(page, '[data-testid="runtime01-count"]', "1");
        await page.goto(base + "/learn/runtime-02");
        await page.waitForTimeout(900);
        await page.getByRole("button", { name: "対話回数を1増やす" }).click();
        await waitText(page, '[data-testid="runtime02-count"]', "1");
        await page.reload();
        await waitText(page, '[data-testid="runtime02-count"]', "0");
        // 第4組：学習順016から017へ進み、ライフサイクル表と対話を確認する。
        await page.goto(base + "/learn/09");
        await waitText(page, '[data-testid="step09-init"]', "1");
        await page.getByRole("button", { name: "テーマを変更する" }).click();
        await waitText(page, '[data-testid="step09-topic"]', "変更後のテーマ 1");
        await page.getByRole("link", { name: "次へ：学習順017" }).click();
        await page.getByRole("heading", { name: "事前レンダリングと初期化の実行回数" }).waitFor();
        await page.getByRole("table").first().getByText("OnInitialized").waitFor();
        await page.goto(base + "/learn/runtime-03");
        await waitText(page, '[data-testid="runtime03-init"]', "1");
        await waitText(page, '[data-testid="runtime03-after"]', "1");
        await page.waitForTimeout(900);
        await page.getByRole("button", { name: "状態を変更する" }).click();
        await waitText(page, '[data-testid="runtime03-change"]', "1");
        await page.goto(base + "/learn/runtime-04");
        await page.waitForTimeout(900);
        await page.getByRole("button", { name: "履歴をURLで選ぶ" }).click();
        await waitText(page, '[data-testid="runtime04-tab"]', "履歴");
        await page.getByRole("button", { name: "操作回数を1増やす" }).click();
        await waitText(page, '[data-testid="runtime04-count"]', "1");
        await page.reload();
        await waitText(page, '[data-testid="runtime04-tab"]', "履歴");
        await waitText(page, '[data-testid="runtime04-count"]', "0");
        await page.goto(base + "/learn/runtime-05");
        await page.waitForTimeout(900);
        await page.getByRole("button", { name: "業務の永続データ" }).click();
        await waitText(page, '[data-testid="runtime05-choice"]', "データベース");
        await page.getByRole("link", { name: "次へ：第4組の復習" }).click();
        await page.getByRole("heading", { name: "第4組の復習：初期化・再読込・状態の復習" }).waitFor();
        await page.getByRole("table").first().getByText("日本語入力検証").waitFor();
        await page.getByRole("link", { name: "次へ：学習順021" }).waitFor();
        for (const suffix of ["10", "15", "20"]) {
            await page.goto(base + "/learn/column/runtime-" + suffix);
            await page.getByRole("heading", { name: /第[234]組の復習/ }).waitFor();
        }
        await page.screenshot({ path: path.join(evidence, "runtime-review.png") });
        console.log("第2段階の新規実行原理5画面と復習3件を検証");

        // 第4段階：JS interop、アップロード制限、実ファイル出力、例外復旧と入力操作。
        await page.goto(base + "/learn/stage/browser");
        await page.getByRole("heading", { name: "第4段階の概要：MudBlazorとブラウザーの業務UI" }).waitFor();
        await page.goto(base + "/learn/browser-01");
        await page.waitForTimeout(950);
        await page.getByRole("button", { name: "C#だけで回数を増やす" }).click();
        await waitText(page, '[data-testid="browser01-count"]', "1");
        await page.getByRole("button", { name: "JavaScriptでHTMLの言語設定を読む" }).click();
        await waitText(page, '[data-testid="browser01-language"]', "ja");

        await page.goto(base + "/learn/browser-02");
        await page.waitForTimeout(950);
        const uploadInput = page.locator('#browser02-file');
        await uploadInput.setInputFiles({ name: "練習.csv", mimeType: "text/csv", buffer: Buffer.from("番号,内容\\n1,テスト\\n") });
        await waitText(page, '[data-testid="browser02-result"]', "検査完了");
        await page.getByTestId("browser02-hash").waitFor();
        await uploadInput.setInputFiles({ name: "拒否.png", mimeType: "image/png", buffer: Buffer.from("NG") });
        await waitText(page, '[data-testid="browser02-result"]', "拒否");

        await page.goto(base + "/learn/browser-03");
        await page.waitForTimeout(950);
        const csvDownload = page.waitForEvent("download");
        await page.getByRole("button", { name: "学習用CSVを保存する" }).click();
        const csvFile = await csvDownload;
        if (csvFile.suggestedFilename() !== "blazor_practice.csv") throw new Error("CSVのファイル名が違います。");
        const pdfDownload = page.waitForEvent("download");
        await page.getByRole("button", { name: "学習用PDFを保存する" }).click();
        const pdfFile = await pdfDownload;
        if (pdfFile.suggestedFilename() !== "blazor_practice.pdf") throw new Error("PDFのファイル名が違います。");
        await waitText(page, '[data-testid="browser03-result"]', "blazor_practice.pdf");

        await page.goto(base + "/learn/browser-04");
        await page.waitForTimeout(950);
        await page.getByRole("button", { name: "学習用の例外を発生させる" }).click();
        await page.getByTestId("browser04-error").waitFor();
        await page.getByRole("button", { name: "エラー表示を復旧する" }).click();
        await page.getByRole("button", { name: "学習用の例外を発生させる" }).waitFor();
        await page.getByRole("button", { name: "学習用の処理を開始する" }).click({ noWaitAfter: true });
        await waitText(page, '[data-testid="browser04-state"]', "処理中");
        await page.getByRole("button", { name: "進行中の処理を取り消す" }).click();
        await waitText(page, '[data-testid="browser04-state"]', "取り消しました");

        await page.goto(base + "/learn/browser-05");
        await page.waitForTimeout(950);
        await page.getByLabel("練習用の表示内容").fill("読み上げを確認");
        await waitText(page, '[data-testid="browser05-preview"]', "読み上げを確認");
        await page.getByRole("button", { name: "文字の大きさを切り替える" }).click();
        await page.getByRole("button", { name: "表示のコントラストを切り替える" }).click();
        await waitText(page, '[data-testid="browser05-settings"]', "高コントラスト");
        await page.goto(base + "/learn/column/browser-45");
        await page.getByRole("heading", { name: "第9組の復習：ブラウザー機能と安全な操作" }).waitFor();
        await page.screenshot({ path: path.join(evidence, "browser-review09.png") });
        // 第9組：本文と同じ学習順、比較表、復習、前後リンク、Web目次を確認する。
        await page.goto(base + "/learn/stage/browser");
        await page.locator('[data-testid="stage04-browser-compare"]').waitFor();
        for (let seq = 41; seq <= 45; seq++) {
            const id = "browser-0" + (seq - 40);
            await page.goto(base + "/learn/" + id);
            const serial = String(seq).padStart(3, "0");
            await page.locator('[data-testid="browser09-compare-' + serial + '"]').waitFor();
            await page.getByText("学習順" + serial + "：WinForms＋Dapperとの比較と操作確認").waitFor();
        }
        await page.getByRole("link", { name: "次へ" }).waitFor();
        await page.goto(base + "/learn/column/browser-45");
        await page.locator('[data-testid="browser09-review-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順046" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順041〜045を復習" }).waitFor();
        console.log("第9組041〜045の比較表・復習・Web導線をChromeで検証");
        console.log("第4段階browser-01〜05と第9組の復習を実ブラウザーで確認");

        // 第5段階：生成SQLと変更追跡、KeylessモデルはDB未接続でも試す。
        await page.goto(base + "/learn/stage/db-bridge");
        await page.getByRole("heading", { name: "第5段階の概要：DapperからEF Core 10・PostgreSQL 18へ" }).waitFor();
        await page.goto(base + "/learn/db-bridge-01");
        await page.waitForTimeout(900);
        await page.getByRole("button", { name: "一般的なCRUD" }).click();
        await waitText(page, '[data-testid="dbbridge01-result"]', "一般的なCRUD");
        await page.goto(base + "/learn/db-bridge-02");
        await page.waitForTimeout(900);
        await page.getByRole("button", { name: "LINQからSQLを生成する" }).click();
        await waitText(page, '[data-testid="dbbridge02-state"]', "SQL生成のみ完了");
        await waitText(page, '[data-testid="dbbridge02-sql"]', "SELECT");
        await page.goto(base + "/learn/db-bridge-03");
        await page.waitForTimeout(900);
        await page.getByRole("button", { name: "追跡状態を再現する" }).click();
        await waitText(page, '[data-testid="dbbridge03-state"]', "Added");
        await page.goto(base + "/learn/db-bridge-04");
        await page.waitForTimeout(900);
        await page.getByRole("button", { name: "キーなしモデル定義を確認する" }).click();
        await waitText(page, '[data-testid="dbbridge04-state"]', "キーなし");
        await page.goto(base + "/learn/db-bridge-05");
        await page.waitForTimeout(900);
        await page.getByRole("button", { name: "安全なSQLを生成する" }).click();
        await waitText(page, '[data-testid="dbbridge05-state"]', "パラメーター化");
        await waitText(page, '[data-testid="dbbridge05-fromsql"]', "@");
        await waitText(page, '[data-testid="dbbridge05-sqlquery"]', "@");
        for (const end of ["50", "55", "60"]) {
            await page.goto(base + "/learn/column/db-bridge-" + end);
            await page.getByRole("heading", { name: /第1[012]組の復習/ }).waitFor();
        }
        await page.screenshot({ path: path.join(evidence, "db-bridge-review.png") });
        console.log("第5段階：db-bridge5画面と復習3件を確認");

        // 第10組：比較表、実際のクリック、復習と旧URLの連続確認。
        await page.goto(base + "/learn/stage/db-bridge");
        await page.locator('[data-testid="stage05-db-compare"]').waitFor();
        for (const [seq, url] of [[46, "db-bridge-01"], [47, "31"], [48, "32"], [49, "33"], [50, "34"]]) {
            await page.goto(base + "/learn/" + url);
            await page.locator('[data-testid="dbbridge10-compare-' + String(seq).padStart(3, "0") + '"]').waitFor();
        }
        await page.goto(base + "/learn/33");
        await page.locator('[data-testid="step33-config"]').waitFor();
        await page.goto(base + "/learn/34");
        await page.getByRole("button", { name: "接続して一覧を取得" }).click();
        await page.waitForFunction(() =>
            document.querySelector('[data-testid="step34-result"]')?.textContent?.trim() !== "まだ実行していません.");
        await page.goto(base + "/learn/column/db-bridge-50");
        await page.locator('[data-testid="dbbridge10-review-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順051" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順046〜050を復習" }).waitFor();
        console.log("第10組046〜050の5画面・比較表・実操作・復習・Web目次をChromeで確認");


        // 第11組：051〜055の比較表、実動作、保存されない操作と復習導線。
        for (const [seq, lesson] of [[51, "35"], [52, "db-bridge-02"], [53, "db-bridge-03"], [54, "36"], [55, "37"]]) {
            await page.goto(base + "/learn/" + lesson);
            await page.locator('[data-testid="dbbridge11-compare-' + String(seq).padStart(3, "0") + '"]').waitFor();
        }
        await page.goto(base + "/learn/db-bridge-02");
        await page.waitForTimeout(900);
        await page.getByRole("button", { name: "LINQからSQLを生成する" }).click();
        await waitText(page, '[data-testid="dbbridge02-state"]', "DBには接続していません");
        await page.goto(base + "/learn/db-bridge-03");
        await page.waitForTimeout(900);
        await page.getByRole("button", { name: "追跡状態を再現する" }).click();
        await waitText(page, '[data-testid="dbbridge03-state"]', "Added");
        await waitText(page, '[data-testid="dbbridge03-state"]', "Detached");
        await page.goto(base + "/learn/column/db-bridge-55");
        await page.locator('[data-testid="dbbridge11-review-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順056" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順051〜055を復習" }).waitFor();
        console.log("第11組051〜055の比較表・DB操作の有無・Web目次をChromeで確認");


        // 第12組：旧URLを維持したまま5画面・比較表・拒否・復習導線を検査。
        for (const [seq, lesson] of [[56, "38"], [57, "39"], [58, "db-bridge-04"], [59, "db-bridge-05"], [60, "40"]]) {
            await page.goto(base + "/learn/" + lesson);
            await page.locator('[data-testid="dbbridge12-compare-' + String(seq).padStart(3, "0") + '"]').waitFor();
        }
        await page.goto(base + "/learn/db-bridge-04");
        await page.getByRole("button", { name: "キーなしモデル定義を確認する" }).click();
        await waitText(page, '[data-testid="dbbridge04-state"]', "データは取得していません");
        await page.goto(base + "/learn/db-bridge-05");
        await page.getByRole("button", { name: "安全なSQLを生成する" }).click();
        await waitText(page, '[data-testid="dbbridge05-state"]', "実DBは操作していません");
        await page.locator('[data-testid="dbbridge05-fromsql"]').waitFor();
        await page.locator('[data-testid="dbbridge05-sqlquery"]').waitFor();
        await page.goto(base + "/learn/column/db-bridge-60");
        await page.locator('[data-testid="dbbridge12-review-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順061" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順056〜060を復習" }).waitFor();
        console.log("第12組056〜060の比較表・DB非接続・復習導線をChromeで確認");


        // 第13組：第6段階概要、旧URLの5画面、比較表と復習への導線。
        await page.goto(base + "/learn/stage/data-list");
        await page.locator('[data-testid="stage06-overview-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順061" }).waitFor();
        for (const [seq, oldId] of [[61, 41], [62, 42], [63, 43], [64, 44], [65, 45]]) {
            await page.goto(base + "/learn/" + oldId);
            await page.locator('[data-testid="stage06-compare-' + seq.toString().padStart(3, "0") + '"]').waitFor();
        }
        await page.goto(base + "/learn/column/45");
        await page.locator('[data-testid="stage06-review-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順066" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順061〜065を復習" }).waitFor();
        await page.getByRole("link", { name: "第6段階の概要ページを読む" }).waitFor();
        console.log("第13組061〜065の比較表、段階概要、復習導線をChromeで確認");

        // 第7段階：本物の認証/認可を偽装せず、教材の模擬拒否とRazorのエンコードを確認。
        await page.goto(base + "/learn/stage/security");
        await page.getByRole("heading", { name: "第7段階の概要：Webセキュリティ・AD認証とDB認可" }).waitFor();
        await page.goto(base + "/learn/security-01");
        await page.waitForTimeout(950);
        await page.getByRole("button", { name: "架空の条件を判定する" }).click();
        await waitText(page, '[data-testid="security01-result"]', "拒否");
        for (const term of ["HTTP側で本人認証が成立", "Cookieチケットが有効", "既存DB台帳に本人が有効登録済み", "既存台帳に対象業務の読み取り権限がある"]) {
            await page.getByLabel(term).check();
        }
        await page.getByRole("button", { name: "架空の条件を判定する" }).click();
        await waitText(page, '[data-testid="security01-result"]', "条件成立");
        await page.getByLabel("既存台帳に対象業務の読み取り権限がある").uncheck();
        await page.getByRole("button", { name: "架空の条件を判定する" }).click();
        await waitText(page, '[data-testid="security01-result"]', "拒否");

        await page.goto(base + "/learn/security-02");
        await page.waitForTimeout(950);
        await page.getByRole("button", { name: "変更要求の模擬判定" }).click();
        await waitText(page, '[data-testid="security02-result"]', "拒否");
        for (const term of ["HTTPS経路で受け取った", "送信元を検証済み", "Antiforgeryトークンを検証済み", "サーバー側で業務認可済み"]) {
            await page.getByLabel(term).check();
        }
        await page.getByRole("button", { name: "変更要求の模擬判定" }).click();
        await waitText(page, '[data-testid="security02-result"]', "4条件成立");

        await page.goto(base + "/learn/security-03");
        await page.waitForTimeout(950);
        await page.getByLabel("学習用文字列（JavaScriptとして実行しません）").fill("<img src=x onerror=alert(1)>");
        await page.getByRole("button", { name: "表示用のHTMLエンコードを確認" }).click();
        await waitText(page, '[data-testid="security03-result"]', "HTMLは実行していません");
        await waitText(page, '[data-testid="security03-encoded"]', "&lt;img");
        if (await page.locator('img[src="x"]').count() !== 0) throw new Error("未信頼文字列がHTMLのimgとして生成されています。");

        await page.goto(base + "/learn/security-04");
        await page.waitForTimeout(950);
        await page.getByRole("button", { name: "模擬構成を点検する" }).click();
        await waitText(page, '[data-testid="security04-result"]', "不足");
        for (const term of ["秘密情報を安全な保存先から注入する", "Data Protection鍵を全Webインスタンス間で共有・保護する", "HTTPS終端とCookie設定を検証する"]) {
            await page.getByLabel(term).check();
        }
        await page.getByRole("button", { name: "模擬構成を点検する" }).click();
        await waitText(page, '[data-testid="security04-result"]', "条件あり");

        await page.goto(base + "/learn/security-05");
        await page.waitForTimeout(950);
        await page.getByRole("button", { name: "再接続後の条件を確認" }).click();
        await waitText(page, '[data-testid="security05-result"]', "条件成立");
        await page.getByLabel("既存台帳の権限版が変更された").check();
        await page.getByRole("button", { name: "再接続後の条件を確認" }).click();
        await waitText(page, '[data-testid="security05-result"]', "拒否");
        await page.goto(base + "/learn/column/security-85");
        await page.getByRole("heading", { name: "第17組の復習：Webの安全境界" }).waitFor();
        await page.screenshot({ path: path.join(evidence, "security-review17.png") });
        console.log("第7段階：模擬認可・XSS否定・失効・5ステップの復習を実ブラウザーで確認");

        // Step66〜70は匿名環境で業務データを返さない。実AD認証の代用ではない。
        for (const [step, label] of [
            [66, "業務サービスに認可を問い合わせる"],
            [67, "認可付き一覧を取得する"],
            [68, "認可付き集計"],
            [69, "認可付き詳細を取得する"],
            [70, "現在の更新権限を確認"],
            [71, "現在の台帳照合を確認"],
            [72, "Cookie認証と台帳照合を確認"],
            [73, "更新可能性を照合する"],
            [74, "監査を伴う一覧取得を確認"],
            [75, "本番運用の認可ゲートを照合"],
            [76, "更新の認可を確認する"],
            [77, "最新の権限とキャッシュを確認"],
            [78, "業務利用の現在の認可を確認"],
            [79, "監査付き一覧のE2E経路を確認"],
            [80, "稼働後の業務認可ゲートを確認"]
        ]) {
            await page.goto(base + "/learn/" + step);
            await page.waitForTimeout(1600);
            await page.getByRole("button", { name: label }).click();
            // Blazorのイベントは非同期で描画される。クリック直後の初期文字列は判定しない。
            await waitText(page, '[data-testid="step' + step + '-state"]', "拒否");
        }
        console.log("ステップ66〜80：匿名の業務アクセスは全画面で拒否");
        await page.goto(base + "/learn/04");
        // 初期のHTMLが表示されていても、対話型回線の準備前にはクリックが処理されない。
        await page.waitForTimeout(1800);
        await page.getByRole("button", { name: "1増やす" }).click();
        await waitText(page, '[data-testid="counter-value"]', "1");
        await page.getByRole("button", { name: "1増やす" }).click();
        await waitText(page, '[data-testid="counter-value"]', "2");
        await page.getByRole("button", { name: "1減らす" }).click();
        await waitText(page, '[data-testid="counter-value"]', "1");
        await page.getByRole("button", { name: "0に戻す" }).click();
        await waitText(page, '[data-testid="counter-value"]', "0");
        if (!(await page.getByRole("button", { name: "1減らす" }).isDisabled())) {
            throw new Error("0のときに減算ボタンが無効化されません。");
        }
        await page.screenshot({ path: path.join(evidence, "step04.png") });
        console.log("ステップ04：クリック・減算・初期化の動作に成功");

        await page.goto(base + "/learn/05");
        await page.waitForTimeout(1200);
        await page.getByRole("textbox", { name: "表示する名前（架空の名前を入力）" }).fill("テスト太郎");
        await waitText(page, '[data-testid="name-preview"]', "こんにちは、テスト太郎 さん。");
        await page.getByRole("textbox", { name: "表示する名前（架空の名前を入力）" })
            .fill("<b>学習用</b>");
        await waitText(page, '[data-testid="name-preview"]', "<b>学習用</b>");
        if (await page.locator('[data-testid="name-preview"] b').count()) {
            throw new Error("入力文字列がHTMLタグとして描画されました。");
        }
        await page.getByRole("button", { name: "入力を消去する" }).click();
        await waitText(page, '[data-testid="name-preview"]', "まだ名前が入力されていません。");
        await page.screenshot({ path: path.join(evidence, "step05.png") });
        console.log("ステップ05：日本語入力・双方向バインディング・HTMLエンコードに成功");

        await page.goto(base + "/learn/06");
        await page.waitForTimeout(1200);
        await waitText(page, '[data-testid="visible-count"]', "4");
        await page.getByRole("checkbox", { name: "未完了の課題だけ表示する" }).check();
        await waitText(page, '[data-testid="visible-count"]', "2");
        await waitText(page, '[data-testid="filter-state"]', "未完了の課題に絞っています。");
        await page.screenshot({ path: path.join(evidence, "step06.png") });
        await page.getByRole("checkbox", { name: "未完了の課題だけ表示する" }).uncheck();
        await waitText(page, '[data-testid="visible-count"]', "4");
        console.log("ステップ06：条件分岐・一覧絞り込みの動作に成功");

        // 新規Step07〜10も、静的HTMLではなく実際の対話処理を検証する。
        await page.goto(base + "/learn/07");
        await page.waitForTimeout(1200);
        if (await page.locator('[data-testid="greeting-card"]').count() !== 2) {
            throw new Error("ステップ07：子コンポーネントを2つ表示できていません。");
        }
        await page.getByRole("textbox", { name: "練習用の表示名" }).fill("練習花子");
        await waitText(page, '[data-testid="greeting-card"]', "練習花子");
        const cards = await page.locator('[data-testid="greeting-card"]').allTextContents();
        if (!cards[1].includes("架空の利用者")) {
            throw new Error("ステップ07：独立した子部品の表示が変わってしまいました。");
        }
        await page.screenshot({ path: path.join(evidence, "step07.png") });
        console.log("ステップ07：Parameterによる親子の値受け渡しに成功");

        await page.goto(base + "/learn/08");
        await page.waitForTimeout(1200);
        await page.getByRole("button", { name: "Razor構文" }).click();
        await waitText(page, '[data-testid="step08-selection"]', "Razor構文");
        await page.getByRole("button", { name: "双方向バインディング" }).click();
        await waitText(page, '[data-testid="step08-selection"]', "双方向バインディング");
        await waitText(page, '[data-testid="step08-count"]', "2");
        await page.screenshot({ path: path.join(evidence, "step08.png") });
        console.log("ステップ08：EventCallbackを受けた親側の状態更新に成功");

        await page.goto(base + "/learn/09");
        await page.waitForTimeout(1200);
        await waitText(page, '[data-testid="step09-topic"]', "初回のテーマ");
        await page.getByRole("button", { name: "テーマを変更する" }).click();
        await waitText(page, '[data-testid="step09-topic"]', "変更後のテーマ 1");
        await page.getByRole("button", { name: "子を非表示にする" }).click();
        await waitText(page, '[data-testid="step09-hidden"]', "破棄しました");
        await page.getByRole("button", { name: "子を再表示する" }).click();
        await waitText(page, '[data-testid="step09-init"]', "1");
        await page.screenshot({ path: path.join(evidence, "step09.png") });
        console.log("ステップ09：更新・破棄・再初期化の操作に成功");

        await page.goto(base + "/learn/10");
        await page.waitForTimeout(1200);
        await page.getByRole("button", { name: "入力内容を確認する" }).click();
        await page.locator('form .form-field').first().getByText("表示名を入力してください。", { exact: true }).waitFor();
        await page.locator('form .form-field').nth(1).getByText("学習メモを入力してください。", { exact: true }).waitFor();
        if (await page.locator('[data-testid="step10-confirmation"]').count() !== 0) {
            throw new Error("ステップ10：必須未入力で確認が成立しました。");
        }
        await page.getByRole("textbox", { name: "練習用の表示名（2〜20文字・必須）" }).fill("あ");
        await page.getByRole("textbox", { name: "学習メモ（3〜80文字・必須）" }).fill("あい");
        await page.getByRole("button", { name: "入力内容を確認する" }).click();
        await page.locator('form .form-field').first().getByText("表示名は2文字以上20文字以内で入力してください。", { exact: true }).waitFor();
        await page.locator('form .form-field').nth(1).getByText("学習メモは3文字以上80文字以内で入力してください。", { exact: true }).waitFor();
        await page.getByRole("textbox", { name: "練習用の表示名（2〜20文字・必須）" }).fill("練習太郎");
        await page.getByRole("textbox", { name: "学習メモ（3〜80文字・必須）" }).fill("フォームの入力検証を復習する");
        await page.getByRole("button", { name: "入力内容を確認する" }).click();
        await waitText(page, '[data-testid="step10-confirmation"]', "入力検証に合格しました");
        await waitText(page, '[data-testid="step10-confirmation"]', "練習太郎");
        await page.screenshot({ path: path.join(evidence, "step10.png") });
        console.log("ステップ10：未入力・文字数不足・正常送信の試験に成功");

        await page.goto(base + "/learn/column/05");
        await page.getByText("画面の表示は、何がきっかけで変わる？").waitFor();
        await page.goto(base + "/learn/column/10");
        await page.getByText("親子コンポーネントと入力検証を整理する").waitFor();
        console.log("学習コラム01・02の日本語表示に成功");

        // Step11〜15：DI、時計の差し替え、ライフタイム、Scoped共有、注入を検証。
        await page.goto(base + "/learn/11");
        await page.waitForTimeout(1100);
        await page.getByRole("button", { name: "時刻を読み直す" }).click();
        if (!(await page.locator('[data-testid="step11-direct"]').textContent())?.trim()) {
            throw new Error("ステップ11：直接生成による挨拶が空です。");
        }
        if (!(await page.locator('[data-testid="step11-injected"]').textContent())?.trim()) {
            throw new Error("ステップ11：DIによる挨拶が空です。");
        }
        await page.screenshot({ path: path.join(evidence, "step11.png") });
        console.log("ステップ11：DIと直接生成の比較に成功");

        await page.goto(base + "/learn/12");
        await page.waitForTimeout(1100);
        await page.getByRole("button", { name: "固定時計（午前8時）" }).click();
        await waitText(page, '[data-testid="step12-kind"]', "固定時計");
        await waitText(page, '[data-testid="step12-time"]', "2026/01/01 08:00");
        await waitText(page, '[data-testid="step12-greeting"]', "おはようございます");
        await page.getByRole("button", { name: "実際の時計" }).click();
        await waitText(page, '[data-testid="step12-kind"]', "実際の時計");
        await page.screenshot({ path: path.join(evidence, "step12.png") });
        console.log("ステップ12：IClockで時計を差し替える操作に成功");

        await page.goto(base + "/learn/13");
        await page.waitForTimeout(1100);
        const transientBefore = (await page.locator('[data-testid="step13-transient-initial"]').textContent()).trim();
        const singletonBefore = (await page.locator('[data-testid="step13-singleton"]').textContent()).trim();
        const scopedBefore = (await page.locator('[data-testid="step13-scoped"]').textContent()).trim();
        await page.getByRole("button", { name: "Transientをもう1つ取得する" }).click();
        // Interactive Serverのイベント後、非同期描画が完了するまで待つ。
        await page.waitForFunction(previous => {
            const value = document.querySelector('[data-testid="step13-transient-new"]')?.textContent?.trim();
            return Boolean(value && value !== previous && value !== "まだ取得していません");
        }, transientBefore, { timeout: 15000 });
        const transientAfter = (await page.locator('[data-testid="step13-transient-new"]').textContent()).trim();
        if (!/^[0-9a-f]{8}$/.test(transientAfter)) {
            throw new Error("ステップ13：Transientの識別子が期待した形式ではありません。");
        }
        if (singletonBefore.length !== 8 || scopedBefore.length !== 8) {
            throw new Error("ステップ13：サービス識別子の形式が不正です。");
        }
        await page.screenshot({ path: path.join(evidence, "step13.png") });
        console.log("ステップ13：3種類の生存期間の識別子を確認");

        await page.goto(base + "/learn/14");
        await page.waitForTimeout(1100);
        await waitText(page, '[data-testid="step14-parent-count"]', "0");
        await page.getByRole("button", { name: "親から1増やす" }).click();
        await waitText(page, '[data-testid="step14-parent-count"]', "1");
        await waitText(page, '[data-testid="step14-child-count"]', "1");
        await page.getByRole("button", { name: "子の表示から1増やす" }).click();
        await waitText(page, '[data-testid="step14-parent-count"]', "2");
        await waitText(page, '[data-testid="step14-child-count"]', "2");
        await page.getByRole("button", { name: "0に戻す" }).click();
        await waitText(page, '[data-testid="step14-parent-count"]', "0");
        await page.screenshot({ path: path.join(evidence, "step14.png") });
        console.log("ステップ14：親子で同一Scoped状態を共有する操作に成功");

        await page.goto(base + "/learn/15");
        await page.waitForTimeout(1100);
        await page.getByRole("textbox", { name: "練習用の名前" }).fill("練習花子");
        await waitText(page, '[data-testid="step15-greeting"]', "練習花子さん");
        await page.screenshot({ path: path.join(evidence, "step15.png") });
        console.log("ステップ15：RazorとC#クラスのDI注入に成功");
        // 第3段階・第5組：学習順021〜025の画面と復習リンクがつながることを確認する。
        await page.goto(base + "/learn/stage/di");
        await page.getByRole("heading", { name: "第3段階の概要：DI・サービスと単体テスト" }).waitFor();
        await page.getByRole("link", { name: "学習順021を始める" }).click();
        await page.locator('[data-testid="step11-direct"]').waitFor();
        await page.goto(base + "/learn/15");
        await page.getByRole("link", { name: "次へ：第5組の復習" }).click();
        await page.getByRole("heading", { name: "学習順021〜025を結び付ける" }).waitFor();
        await page.getByRole("link", { name: "次へ：学習順026" }).click();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順021を開く" }).waitFor();
        await page.getByRole("link", { name: "学習順025を開く" }).waitFor();
        console.log("第3段階概要・第5組復習・学習順021〜026のWeb画面導線を確認");


        await page.goto(base + "/learn/column/15");
        await page.getByText("DIを使うと、画面とサービスの何が変わる？").waitFor();
        console.log("コラム03：日本語画面の表示を確認");


        // Step16〜20：標準DIサービス、テスト容易性、Application分離と課題管理。
        await page.goto(base + "/learn/16");
        await page.waitForTimeout(1100);
        await waitText(page, '[data-testid="step16-title"]', "Blazorステップアップ学習");
        await page.getByRole("button", { name: "設定とDIの関係を確認" }).click();
        await waitText(page, '[data-testid="step16-explain"]', "IOptions");
        await page.getByTestId("di06-compare-16").waitFor();
        console.log("学習順026：Options表示と比較表を確認");

        await page.goto(base + "/learn/17");
        await page.waitForTimeout(1100);
        await page.getByRole("button", { name: "非同期処理を開始する" }).click();
        await waitText(page, '[data-testid="step17-state"]', "処理が完了しました");
        // 中断は処理中に別のボタンを押せるようにして確認する。
        await page.getByRole("button", { name: "非同期処理を開始する" }).click({ noWaitAfter: true });
        await page.getByRole("button", { name: "処理を取り消す" }).click();
        await waitText(page, '[data-testid="step17-state"]', "処理を取り消しました");
        await page.getByTestId("di06-compare-17").waitFor();
        console.log("学習順027：非同期処理の完了・取消と診断表を確認");

        await page.goto(base + "/learn/18");
        await page.waitForTimeout(1100);
        await waitText(page, '[data-testid="step18-morning"]', "おはようございます");
        await waitText(page, '[data-testid="step18-afternoon"]', "こんにちは");
        await page.getByTestId("di06-compare-18").waitFor();
        console.log("学習順028：固定時計の結果と比較表を確認");

        await page.goto(base + "/learn/19");
        await page.waitForTimeout(1100);
        await waitText(page, '[data-testid="step19-summary"]', "残り60ステップ");
        await page.getByRole("button", { name: "5ステップ学習した" }).click();
        await waitText(page, '[data-testid="step19-summary"]', "残り55ステップ");
        for (let i = 0; i < 11; i++) {
            await page.getByRole("button", { name: "5ステップ学習した" }).click();
        }
        await waitText(page, '[data-testid="step19-summary"]', "全60ステップを修了");
        await page.getByRole("button", { name: "5ステップ学習した" }).click();
        await waitText(page, '[data-testid="step19-count"]', "60");
        await page.getByTestId("di06-compare-19").waitFor();
        console.log("学習順029：Applicationの練習用60件上限を確認");

        await page.goto(base + "/learn/20");
        await page.waitForTimeout(1100);
        await waitText(page, '[data-testid="step20-count"]', "2");
        await page.getByRole("button", { name: "課題を追加する" }).click();
        await waitText(page, '[data-testid="step20-message"]', "1〜40文字");
        await page.getByRole("textbox", { name: "練習用の課題名（1〜40文字）" }).fill("ライブラリDIを復習する");
        await page.getByRole("button", { name: "課題を追加する" }).click();
        await waitText(page, '[data-testid="step20-count"]', "3");
        await page.getByRole("checkbox", { name: "ライブラリDIを復習する" }).check();
        await page.getByTestId("di06-compare-20").waitFor();
        for (const title of ["追加課題A", "追加課題B"]) {
            await page.getByRole("textbox", { name: "練習用の課題名（1〜40文字）" }).fill(title);
            await page.getByRole("button", { name: "課題を追加する" }).click();
        }
        await waitText(page, '[data-testid="step20-count"]', "5");
        await page.getByRole("textbox", { name: "練習用の課題名（1〜40文字）" }).fill("上限超過");
        await page.getByRole("button", { name: "課題を追加する" }).click();
        await waitText(page, '[data-testid="step20-message"]', "最大5件");
        await waitText(page, '[data-testid="step20-count"]', "5");
        console.log("学習順030：追加・完了・6件目拒否を確認");

        await page.goto(base + "/learn/column/20");
        await page.getByText("DIを利用して画面の責任を小さくする").waitFor();
        await page.getByTestId("di06-review-table").waitFor();
        await page.goto(base + "/learn/column/library-di");
        await waitText(page, '[data-testid="library-di-title"]', "Blazorステップアップ学習");
        await page.getByRole("button", { name: "ILoggerで学習用ログを書く" }).click();
        await waitText(page, '[data-testid="library-di-message"]', "学習用ログを書き込みました");
        console.log("コラム04・特別コラム05：DIの理解を深める日本語画面を確認");

        // Step21〜25：MudBlazorのイベント・日本語入力・検証・一覧を実際に操作する。
        await page.goto(base + "/learn/stage/browser");
        await page.locator('[data-testid="stage04-compare"]').waitFor();
        await page.goto(base + "/learn/21");
        await page.waitForTimeout(1200);
        await page.getByRole("button", { name: "日本語の通知を表示する" }).click();
        await waitText(page, '[data-testid="step21-state"]', "通知サービスを使いました");
        await page.locator('[data-testid="mud07-compare-21"]').waitFor();
        await page.screenshot({ path: path.join(evidence, "step21.png") });

        await page.goto(base + "/learn/22");
        await page.waitForTimeout(1000);
        await page.getByRole("button", { name: "学習メニューを開閉" }).click();
        await waitText(page, '[data-testid="step22-state"]', "メニューを閉じています");
        await page.getByRole("button", { name: "学習メニューを開閉" }).click();
        await waitText(page, '[data-testid="step22-state"]', "メニューを表示しています");
        await page.locator('[data-testid="mud07-compare-22"]').waitFor();
        await page.screenshot({ path: path.join(evidence, "step22.png") });

        await page.goto(base + "/learn/23");
        await page.waitForTimeout(1000);
        await page.getByRole("textbox", { name: "練習用の氏名" }).fill("練習太郎");
        await waitText(page, '[data-testid="step23-preview"]', "練習太郎");
        await page.locator('[data-testid="mud07-compare-23"]').waitFor();
        await page.screenshot({ path: path.join(evidence, "step23.png") });

        await page.goto(base + "/learn/24");
        await page.waitForTimeout(1000);
        await page.getByRole("button", { name: "入力を検証する" }).click();
        await waitText(page, '[data-testid="step24-result"]', "入力内容に不備があります");
        await page.getByText("課題名を入力してください。").waitFor();
        await page.getByText("学習内容を入力してください。").waitFor();
        await page.getByRole("textbox", { name: "課題名（必須）" }).fill("架空の課題");
        await page.getByRole("textbox", { name: "学習内容（必須）" }).fill("UIを学ぶ");
        await page.getByRole("button", { name: "入力を検証する" }).click();
        await waitText(page, '[data-testid="step24-result"]', "入力検証に合格しました");
        await page.locator('[data-testid="mud07-compare-24"]').waitFor();
        await page.screenshot({ path: path.join(evidence, "step24.png") });

        await page.goto(base + "/learn/25");
        await page.waitForTimeout(1000);
        await waitText(page, '[data-testid="step25-count"]', "4 件");
        await page.locator('[data-testid="mud07-compare-25"]').waitFor();
        await page.getByText("課題名", { exact: true }).last().click();
        await page.screenshot({ path: path.join(evidence, "step25.png") });
        await page.goto(base + "/learn/column/25");
        await page.getByText("公式テンプレートから日本語業務UIへ").waitFor();
        await page.locator('[data-testid="mud07-review-table"]').waitFor();
        console.log("ステップ21〜25とコラム06：MudBlazorの画面と対話を確認");

        // Step26〜30：データベースを用いずUIの対話操作を検証する。
        await page.goto(base + "/learn/26");
        await page.waitForTimeout(1100);
        await waitText(page, '[data-testid="step26-count"]', "6 件");
        await page.getByRole("textbox", { name: "課題名を検索" }).fill("Mud");
        await waitText(page, '[data-testid="step26-count"]', "2 件");
        await page.screenshot({ path: path.join(evidence, "step26.png") });
        console.log("ステップ26：DataGridのQuickFilterを検証");

        await page.goto(base + "/learn/27");
        await page.waitForTimeout(1000);
        await page.getByRole("button", { name: "確認ダイアログを開く" }).click();
        await page.getByRole("button", { name: "中止", exact: true }).click();
        await waitText(page, '[data-testid="step27-result"]', "中止しました");
        await page.getByRole("button", { name: "確認ダイアログを開く" }).click();
        await page.getByRole("button", { name: "実行する", exact: true }).click();
        await waitText(page, '[data-testid="step27-result"]', "実行を選びました");
        await page.screenshot({ path: path.join(evidence, "step27.png") });
        console.log("ステップ27：ダイアログの肯定とキャンセルを検証");

        await page.goto(base + "/learn/28");
        await page.waitForTimeout(1000);
        await page.getByRole("button", { name: "練習用の処理を開始" }).click();
        await waitText(page, '[data-testid="step28-state"]', "完了しました");
        await page.screenshot({ path: path.join(evidence, "step28.png") });
        console.log("ステップ28：Snackbarと完了表示を検証");

        await page.goto(base + "/learn/29");
        await page.waitForTimeout(900);
        await page.getByText("ダークモードを有効にする").click();
        await waitText(page, '[data-testid="step29-theme"]', "ダークモード");
        await page.getByRole("textbox", { name: "操作確認の入力欄" }).fill("練習");
        await page.getByRole("button", { name: "入力状態を確認する" }).click();
        await waitText(page, '[data-testid="step29-state"]', "練習");
        await page.screenshot({ path: path.join(evidence, "step29.png") });
        console.log("ステップ29：テーマ変更と入力を検証");

        await page.goto(base + "/learn/30");
        await page.waitForTimeout(900);
        await waitText(page, '[data-testid="step30-count"]', "3 件");
        await page.getByRole("button", { name: "確認して追加する" }).click();
        await waitText(page, '[data-testid="step30-state"]', "1〜30文字");
        await page.getByRole("textbox", { name: "追加する架空の利用者名" }).fill("テスト 四郎");
        await page.getByRole("button", { name: "確認して追加する" }).click();
        await page.getByRole("button", { name: "中止", exact: true }).click();
        await waitText(page, '[data-testid="step30-state"]', "中止しました");
        await page.getByRole("button", { name: "確認して追加する" }).click();
        await page.getByRole("button", { name: "追加する", exact: true }).click();
        await waitText(page, '[data-testid="step30-count"]', "4 件");
        await page.screenshot({ path: path.join(evidence, "step30.png") });
        await page.goto(base + "/learn/column/30");
        await page.getByText("一覧・確認・通知・テーマを設計に結び付ける").waitFor();
        console.log("ステップ30・コラム07：入力・検索・確認・追加を検証");

        // 第8組の画面・比較表・前後導線を既存の操作試験に重ねて確認する。
        for (const seq of [36, 37, 38, 39, 40]) {
            await page.goto(base + "/learn/" + (seq - 10));
            await page.locator('[data-testid="mud08-compare-' + seq + '"]').waitFor();
        }
        await page.goto(base + "/learn/column/30");
        await page.locator('[data-testid="mud08-review-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順041" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順036〜040の復習" }).waitFor();
        console.log("学習順036〜040・第8組復習・Web目次と前後導線を検証");

        // Step31〜40：DBがなくても学べるEF Core基本と型変換の画面を確認。
        await page.goto(base + "/learn/31");
        await waitText(page, '[data-testid="step31-model"]', "EF Coreのモデルを学ぶ");
        await page.goto(base + "/learn/32");
        await page.getByText("まず開発用のテーブルを作る").waitFor();
        await page.goto(base + "/learn/33");
        await waitText(page, '[data-testid="step33-config"]', "未設定");
        await page.goto(base + "/learn/34");
        await page.getByRole("button", { name: "接続して一覧を取得" }).click();
        await waitText(page, '[data-testid="step34-result"]', "未設定");
        await page.goto(base + "/learn/35");
        await page.getByRole("button", { name: "DBを検索する" }).click();
        await waitText(page, '[data-testid="step35-status"]', "未設定");
        await page.goto(base + "/learn/36");
        await page.getByRole("button", { name: "登録する" }).click();
        await waitText(page, '[data-testid="step36-result"]', "未設定");
        await page.goto(base + "/learn/37");
        await page.getByRole("button", { name: "DB用に変換" }).click();
        await waitText(page, '[data-testid="step37-result"]', "DateOnly");
        await page.goto(base + "/learn/38");
        await page.getByRole("button", { name: "開発用DBに2件登録する" }).click();
        await waitText(page, '[data-testid="step38-result"]', "未設定");
        await page.goto(base + "/learn/39");
        await page.getByRole("button", { name: "DBの状態を読み直す" }).click();
        await waitText(page, '[data-testid="step39-result"]', "未設定");
        await page.goto(base + "/learn/40");
        await page.getByRole("button", { name: "開発用DBへ接続して検索" }).click();
        await waitText(page, '[data-testid="step40-result"]', "未設定");
        await page.goto(base + "/learn/column/35");
        await page.getByText("WebプロジェクトだけでEF Core 10を始める").waitFor();
        await page.goto(base + "/learn/column/40");
        await page.getByText("保存・競合・日付Range・DBテストを結び付ける").waitFor();
        await page.goto(base + "/learn/column/mudblazor-guide");
        await page.getByText("用途からMudBlazorのコンポーネントを選ぶ").waitFor();
        await page.screenshot({ path: path.join(evidence, "mudblazor-guide.png") });
        console.log("ステップ31〜40・コラム08/09・用途別特別コラムを確認");

        // Step41〜45：DB未設定でも日本語のServerData画面を実際に操作する。
        await page.goto(base + "/learn/41");
        await page.waitForTimeout(850);
        await waitText(page, '[data-testid="step41-count"]', "8 件");
        await page.getByText("架空の課題01").waitFor();
        await page.screenshot({ path: path.join(evidence, "step41.png") });
        await page.goto(base + "/learn/42");
        await page.waitForTimeout(850);
        await waitText(page, '[data-testid="step42-state"]', "未設定");
        await page.getByRole("textbox", { name: "課題名を検索" }).fill("研修");
        await page.getByRole("button", { name: "検索する" }).click();
        await waitText(page, '[data-testid="step42-state"]', "未設定");
        await page.goto(base + "/learn/43");
        await page.waitForTimeout(850);
        await waitText(page, '[data-testid="step43-state"]', "未設定");
        await page.getByRole("textbox", { name: "課題名の検索語" }).fill("検索");
        await page.getByRole("button", { name: "DBを検索する" }).click();
        await waitText(page, '[data-testid="step43-state"]', "未設定");
        await page.goto(base + "/learn/44");
        await page.waitForTimeout(850);
        await waitText(page, '[data-testid="step44-state"]', "未設定");
        await page.getByText("複数列ソートを安全にDBへ伝える").waitFor();
        await page.goto(base + "/learn/45");
        await page.waitForTimeout(850);
        await waitText(page, '[data-testid="step45-state"]', "未設定");
        await page.getByRole("textbox", { name: "課題名の部分一致" }).fill("業務");
        await page.getByRole("button", { name: "条件を適用する" }).click();
        await waitText(page, '[data-testid="step45-state"]', "未設定");
        await page.screenshot({ path: path.join(evidence, "step45.png") });
        await page.goto(base + "/learn/column/45");
        await page.getByText("ItemsとServerDataを要件で使い分ける").waitFor();
        console.log("ステップ41〜45の日本語UIとコラム10の画面操作に成功");

        // Step46〜50：DB未設定の明示、仮想化の表示、再試行、コラム11を確認。
        await page.goto(base + "/learn/46");
        await page.waitForTimeout(850);
        await waitText(page, '[data-testid="step46-state"]', "未設定");
        await page.getByRole("button", { name: "関連表を読み込む" }).click();
        await waitText(page, '[data-testid="step46-state"]', "未設定");
        await page.goto(base + "/learn/47");
        await page.waitForTimeout(850);
        await waitText(page, '[data-testid="step47-state"]', "未設定");
        await page.getByRole("button", { name: "先頭から読み直す" }).click();
        await waitText(page, '[data-testid="step47-state"]', "未設定");
        await page.goto(base + "/learn/48");
        await page.waitForTimeout(850);
        await waitText(page, '[data-testid="step48-count"]', "500 件");
        await page.getByText("仮想化の架空課題 001").waitFor();
        await page.screenshot({ path: path.join(evidence, "step48.png") });
        await page.goto(base + "/learn/49");
        await page.waitForTimeout(850);
        await waitText(page, '[data-testid="step49-state"]', "未設定");
        await page.goto(base + "/learn/50");
        await page.waitForTimeout(850);
        await waitText(page, '[data-testid="step50-state"]', "未設定");
        await page.getByRole("textbox", { name: "課題名で検索（400ミリ秒待機）" }).fill("架空");
        await page.waitForTimeout(750);
        await page.getByRole("button", { name: "再読み込み" }).click();
        await waitText(page, '[data-testid="step50-state"]', "未設定");
        await page.screenshot({ path: path.join(evidence, "step50.png") });
        await page.goto(base + "/learn/column/50");
        await page.getByText("JOIN・キーセット・仮想化を要件から選ぶ").waitFor();
        // 第14組の比較表、学習順、旧URL、Web目次と前後導線を検証。
        await page.goto(base + "/learn/46");
        await page.locator('[data-testid="stage06-compare-066"]').waitFor();
        await page.goto(base + "/learn/47");
        await page.locator('[data-testid="stage06-compare-067"]').waitFor();
        await page.goto(base + "/learn/48");
        await page.locator('[data-testid="stage06-compare-068"]').waitFor();
        await page.goto(base + "/learn/49");
        await page.locator('[data-testid="stage06-compare-069"]').waitFor();
        await page.goto(base + "/learn/50");
        await page.locator('[data-testid="stage06-compare-070"]').waitFor();
        await page.goto(base + "/learn/46");
        await page.getByRole("link", { name: "前へ：第13組の復習" }).waitFor();
        await page.goto(base + "/learn/50");
        await page.getByRole("link", { name: "第14組の復習：一覧方式と検索UX" }).waitFor();
        await page.goto(base + "/learn/column/50");
        await page.locator('[data-testid="stage06-review14-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順071" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順066〜070を復習" }).waitFor();
        console.log("第14組の比較表・復習・Web目次をChromeで確認");

        console.log("ステップ46〜50・コラム11：画面と操作の確認に成功");

        // Step51〜55：DBなしでも未設定状態・画面ナビ・コラムの表示を確認。
        for (let step = 51; step <= 55; step++) {
            await page.goto(base + "/learn/" + step);
            await page.waitForTimeout(850);
            await waitText(page, '[data-testid="step' + step + '-state"]', "未設定");
            await page.screenshot({ path: path.join(evidence, "step" + step + ".png") });
        }
        await page.goto(base + "/learn/column/55");
        await page.getByText("保存と表示は、操作ごとに責任を切り分ける").waitFor();
        // 第15組：旧80 URL、各比較表、復習と次の学習順の導線を実Chromeで検証。
        for (const [sequence, oldId] of [[71, 51], [72, 52], [73, 53], [74, 54], [75, 55]]) {
            await page.goto(base + "/learn/" + oldId);
            await page.locator('[data-testid="stage06-compare-' + sequence.toString().padStart(3, "0") + '"]').waitFor();
        }
        await page.goto(base + "/learn/51");
        await page.getByRole("link", { name: "前へ：第14組の復習" }).waitFor();
        await page.goto(base + "/learn/55");
        await page.getByRole("link", { name: "第15組の復習：編集・競合・詳細" }).waitFor();
        await page.goto(base + "/learn/column/55");
        await page.locator('[data-testid="stage06-review15-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順076" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順071〜075を復習" }).waitFor();
        console.log("第15組071〜075の比較表・復習・前後導線を確認");
        console.log("ステップ51〜55・コラム12：日本語画面とDB未設定の案内を確認");

        // Step56〜60：未設定DBの案内、Scopedキャッシュの隔離、総合選定の操作。
        await page.goto(base + "/learn/56");
        await page.waitForTimeout(850);
        await page.getByRole("button", { name: "期間の重なりを検索" }).click();
        await waitText(page, '[data-testid="step56-state"]', "未設定");
        await page.goto(base + "/learn/57");
        await page.waitForTimeout(850);
        await page.getByRole("button", { name: "架空の一覧を表示" }).click();
        await waitText(page, '[data-testid="step57-state"]', "新しい結果");
        await page.getByRole("button", { name: "架空の一覧を表示" }).click();
        await waitText(page, '[data-testid="step57-state"]', "キャッシュを再利用");
        await page.getByRole("button", { name: "権限バージョンを更新" }).click();
        await waitText(page, '[data-testid="step57-state"]', "古い版");
        await page.goto(base + "/learn/58");
        await page.waitForTimeout(850);
        await page.getByRole("button", { name: "完了状態を集計" }).click();
        await waitText(page, '[data-testid="step58-state"]', "未設定");
        await page.goto(base + "/learn/59");
        await page.waitForTimeout(850);
        await page.getByRole("button", { name: "読み取りを20回計測する" }).click();
        await waitText(page, '[data-testid="step59-state"]', "未設定");
        await page.goto(base + "/learn/60");
        await page.waitForTimeout(850);
        await page.getByRole("button", { name: "選定を確認する" }).click();
        await waitText(page, '[data-testid="step60-state"]', "第一候補");
        await page.goto(base + "/learn/column/60");
        await page.getByText("一覧は取得条件・権限・出力・性能まで一体で考える").waitFor();
        await page.screenshot({ path: path.join(evidence, "step60.png") });
        console.log("ステップ56〜60・コラム13：日本語画面と操作を確認");

        // 第16組：旧URL・比較表・復習・ホーム・前後導線をChromeで検査。
        for (const [sequence, oldId] of [[76, 56], [77, 57], [78, 58], [79, 59], [80, 60]]) {
            await page.goto(base + "/learn/" + oldId);
            await page.locator('[data-testid="stage06-compare-' + sequence.toString().padStart(3, "0") + '"]').waitFor();
        }
        await page.goto(base + "/learn/56");
        await page.getByRole("link", { name: "前へ：第15組の復習" }).waitFor();
        await page.goto(base + "/learn/60");
        await page.getByRole("link", { name: "第16組の復習：期間・集計・性能" }).waitFor();
        await page.goto(base + "/learn/column/60");
        await page.locator('[data-testid="stage06-review16-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順081" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順076〜080を復習" }).waitFor();
        console.log("第16組076〜080の比較表・復習と前後導線をChromeで確認");



        // 第17組：改稿画面の比較表・段階概要・前後リンク・復習表をChromeで確認する。
        await page.goto(base + "/learn/stage/security");
        await page.locator('[data-testid="stage07-overview-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順081" }).waitFor();
        for (let n = 81; n <= 85; n++) {
            await page.goto(base + "/learn/security-0" + (n - 80));
            await page.locator('[data-testid="stage07-compare-' + n + '"]').waitFor();
        }
        await page.goto(base + "/learn/security-01");
        await page.getByRole("link", { name: "前へ：第16組の復習" }).waitFor();
        await page.goto(base + "/learn/security-05");
        await page.getByRole("link", { name: "次へ：第17組の復習" }).waitFor();
        await page.goto(base + "/learn/column/security-85");
        await page.locator('[data-testid="stage07-review17-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順086" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順081〜085を復習する" }).waitFor();
        console.log("第17組081〜085の比較表・復習と前後導線をChromeで確認");

        // 第18組：比較表・旧URL・復習導線をChromeで確認する。
        await page.goto(base + "/learn/61");
        await page.getByRole("link", { name: "前へ：第17組の復習" }).waitFor();
        for (let seq = 86; seq <= 90; seq++) {
            await page.goto(base + "/learn/" + (seq - 25));
            await page.locator('[data-testid="stage07-compare-' + seq + '"]').waitFor();
        }
        await page.goto(base + "/learn/column/65");
        await page.locator('[data-testid="stage07-review18-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：1万人規模の認証" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順086〜090を復習する" }).waitFor();
        console.log("第18組086〜090の比較表と前後導線をChromeで確認");

        // Step61〜65：未認証のままDB照合や権限付与を行わず、必ず拒否する。
        for (let step = 61; step <= 65; step++) {
            await page.goto(base + "/learn/" + step);
            await page.waitForTimeout(700);
            await page.getByRole("button", { name: "現在の認証・既存台帳照合状態を確認" }).click();
            await waitText(page, '[data-testid="step' + step + '-state"]', "Windows認証されていません");
            await page.screenshot({ path: path.join(evidence, "step" + step + ".png") });
        }
        await page.goto(base + "/learn/column/65");
        await page.getByText("本人認証と業務権限は別々に確認する").waitFor();
        console.log("ステップ61〜65・コラム14：匿名アクセスの拒否と日本語画面を確認");
        await page.goto(base + "/learn/column/auth-at-scale");
        await page.getByRole("button", { name: "推奨構成を確認" }).click();
        await waitText(page, '[data-testid="auth-scale-choice"]', "IIS Windows認証");
        await page.screenshot({ path: path.join(evidence, "auth-at-scale.png") });
        console.log("1万人規模の認証・既存台帳・ログイン・監査の選定画面を確認");



        // 第19組091〜095：実Chromeで匿名拒否、比較表、前後導線、Web目次を検査。
        const unit19Actions = [
            [66, 91, "業務サービスに認可を問い合わせる"],
            [67, 92, "認可付き一覧を取得する"],
            [68, 93, "認可付き集計"],
            [69, 94, "認可付き詳細を取得する"],
            [70, 95, "現在の更新権限を確認"]
        ];
        for (const [oldId, seq, action] of unit19Actions) {
            await page.goto(base + "/learn/" + oldId);
            await page.locator('[data-testid="stage07-compare-' + seq + '"]').waitFor();
            // Interactive Serverのcircuit初期化前にSSRのボタンを押す競合を避ける。
            await page.waitForTimeout(850);
            await page.getByRole("button", { name: action }).click();
            await waitText(page, '[data-testid="step' + oldId + '-state"]', "拒否");
        }
        await page.goto(base + "/learn/66");
        await page.getByRole("link", { name: "前へ：1万人規模の認証" }).waitFor();
        await page.goto(base + "/learn/70");
        await page.getByRole("link", { name: "次へ：第19組の復習" }).waitFor();
        await page.goto(base + "/learn/column/70");
        await page.locator('[data-testid="stage07-review19-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順096" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順091〜095を復習する" }).waitFor();
        console.log("第19組091〜095：旧URL、匿名拒否、比較表、前後導線、復習、Web目次を確認");

        // 第20組096〜100：旧URL、比較表、サーバー側の匿名拒否、前後リンクを確認。
        await page.goto(base + "/learn/stage/operations");
        await page.locator('[data-testid="stage08-overview-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順096" }).waitFor();
        const unit20Actions = [
            [71, 96, "現在の台帳照合を確認"],
            [72, 97, "Cookie認証と台帳照合を確認"],
            [73, 98, "更新可能性を照合する"],
            [74, 99, "監査を伴う一覧取得を確認"],
            [75, 100, "本番運用の認可ゲートを照合"]
        ];
        for (const [oldId, seq, button] of unit20Actions) {
            await page.goto(base + "/learn/" + oldId);
            await page.waitForTimeout(700);
            await page.locator('[data-testid="stage08-compare-' + String(seq).padStart(3, "0") + '"]').waitFor();
            await page.getByRole("button", { name: button }).click();
            await waitText(page, '[data-testid="step' + oldId + '-state"]', "拒否");
        }
        await page.goto(base + "/learn/71");
        await page.getByRole("link", { name: "前へ" }).waitFor();
        await page.getByRole("link", { name: "次へ", exact: true }).waitFor();
        await page.goto(base + "/learn/75");
        await page.getByRole("link", { name: "次へ", exact: true }).waitFor();
        await page.goto(base + "/learn/column/75");
        await page.locator('[data-testid="stage08-review20-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順101" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順096〜100を復習する" }).waitFor();
        console.log("第20組096〜100：実Chromeの匿名安全拒否、Razor比較表、前後リンク、Web目次を確認");


        // 第21組101〜105：実Chromeで旧URL・比較表・匿名の安全拒否と目次を確認。
        const unit21Actions = [
            [76,101,"更新の認可を確認する"],
            [77,102,"最新の権限とキャッシュを確認"],
            [78,103,"業務利用の現在の認可を確認"],
            [79,104,"監査付き一覧のE2E経路を確認"],
            [80,105,"稼働後の業務認可ゲートを確認"]
        ];
        for (const [oldId,seq,action] of unit21Actions) {
            await page.goto(base + "/learn/" + oldId);
            await page.waitForTimeout(700);
            await page.locator('[data-testid="stage08-compare-' + seq + '"]').waitFor();
            await page.getByRole("button", { name: action }).click();
            await waitText(page, '[data-testid="step' + oldId + '-state"]', "拒否");
        }
        await page.goto(base + "/learn/76");
        await page.getByRole("link", { name: "前へ", exact: true }).waitFor();
        await page.getByRole("link", { name: "次へ", exact: true }).waitFor();
        await page.goto(base + "/learn/80");
        await page.getByRole("link", { name: "次へ：第21組の復習" }).waitFor();
        await page.goto(base + "/learn/column/80");
        await page.locator('[data-testid="stage08-review21-table"]').waitFor();
        await page.getByRole("link", { name: "次へ：学習順106" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順101〜105を復習する" }).waitFor();
        console.log("第21組101〜105：匿名拒否・比較表・旧80URL・目次・前後リンクを確認");

        // 第9段階：実業務アクセスは必ず既存の認可サービスを通し、匿名時の拒否を確認。
        await page.goto(base + "/learn/stage/capstone");
        await page.getByRole("heading", { name: "第9段階の概要：業務アプリの通し演習" }).waitFor();
        await page.goto(base + "/learn/capstone-01");
        await page.waitForTimeout(850);
        await page.getByLabel("要件を選ぶ").selectOption("edit");
        await page.getByRole("button", { name: "設計の確認" }).click();
        await waitText(page, '[data-testid="capstone01-result"]', "xmin");

        await page.goto(base + "/learn/capstone-02");
        await page.waitForTimeout(850);
        await page.getByRole("button", { name: "生成SQLだけを確認する" }).click();
        await waitText(page, '[data-testid="capstone02-sql"]', "SELECT");
        await page.getByRole("button", { name: "権限付き一覧を取得する" }).click();
        await waitText(page, '[data-testid="capstone02-result"]', "拒否");
        await waitText(page, '[data-testid="capstone02-total"]', "総件数：0 件");

        await page.goto(base + "/learn/capstone-03");
        await page.waitForTimeout(850);
        await page.getByRole("button", { name: "認可付き詳細を取得する" }).click();
        await waitText(page, '[data-testid="capstone03-result"]', "拒否");
        if (await page.getByRole("button", { name: "権限と競合を再検査して保存する" }).count() !== 0)
            throw new Error("匿名画面に編集の保存ボタンを表示してはいけません。");

        await page.goto(base + "/learn/capstone-04");
        await page.waitForTimeout(850);
        await page.getByRole("button", { name: "現在の権限と監査の前提を確認" }).click();
        await waitText(page, '[data-testid="capstone04-result"]', "拒否");
        await page.getByRole("button", { name: "認可付き集計を取得する" }).click();
        await waitText(page, '[data-testid="capstone04-summary"]', "拒否");

        await page.goto(base + "/learn/capstone-05");
        await page.waitForTimeout(850);
        await page.getByRole("button", { name: "受入の不足を確認する" }).click();
        await waitText(page, '[data-testid="capstone05-result"]', "受入に不足");
        for (const label of ["コード・日本語教材・Web目次・PDFの内容が一致する",
                             "Windows/Linux・PostgreSQL 18・ChromeのCI証跡を確認した",
                             "未認証・権限失効・競合・監査失敗時に拒否する",
                             "実IIS/AD/Kerberos・証明書・負荷・復旧の実機受入証跡がある"]) {
            await page.getByLabel(label, { exact: true }).check();
        }
        await page.getByRole("button", { name: "受入の不足を確認する" }).click();
        await waitText(page, '[data-testid="capstone05-result"]', "本番受入を宣言できません");
        await page.goto(base + "/learn/column/capstone-110");
        await page.getByRole("heading", { name: "第22組の復習：業務アプリの通し演習" }).waitFor();
        await page.screenshot({ path: path.join(evidence, "capstone-review22.png") });
        console.log("総合演習5画面・認可付き一覧/詳細/集計の匿名拒否・IIS受入区分を確認");

        // 第22組の長文改稿と画面側比較表、旧80URL、前後導線を同期検証。
        await page.goto(base + "/learn/stage/capstone");
        await page.locator('[data-testid="capstone-stage-overview-table"]').waitFor();
        await page.getByRole("link", { name: "前へ", exact: true }).waitFor();
        await page.getByRole("link", { name: "次へ", exact: true }).waitFor();
        await page.goto(base + "/learn/capstone-01");
        await page.locator('[data-testid="capstone-compare-106"]').waitFor();
        await page.getByRole("link", { name: "前へ", exact: true }).waitFor();
        await page.getByRole("link", { name: "次へ", exact: true }).waitFor();
        await page.goto(base + "/learn/capstone-02");
        await page.locator('[data-testid="capstone-compare-107"]').waitFor();
        await page.getByRole("link", { name: "前へ", exact: true }).waitFor();
        await page.getByRole("link", { name: "次へ", exact: true }).waitFor();
        await page.goto(base + "/learn/capstone-03");
        await page.locator('[data-testid="capstone-compare-108"]').waitFor();
        await page.getByRole("link", { name: "前へ", exact: true }).waitFor();
        await page.getByRole("link", { name: "次へ", exact: true }).waitFor();
        await page.goto(base + "/learn/capstone-04");
        await page.locator('[data-testid="capstone-compare-109"]').waitFor();
        await page.getByRole("link", { name: "前へ", exact: true }).waitFor();
        await page.getByRole("link", { name: "次へ", exact: true }).waitFor();
        await page.goto(base + "/learn/capstone-05");
        await page.locator('[data-testid="capstone-compare-110"]').waitFor();
        await page.getByRole("link", { name: "前へ", exact: true }).waitFor();
        await page.getByRole("link", { name: "次へ", exact: true }).waitFor();
        await page.goto(base + "/learn/column/capstone-110");
        await page.locator('[data-testid="capstone-review22-table"]').waitFor();
        await page.getByRole("link", { name: "前へ", exact: true }).waitFor();
        await page.getByRole("link", { name: "次へ", exact: true }).waitFor();
        await page.goto(base + "/learn/80");
        await page.getByRole("link", { name: "次へ：第21組の復習" }).waitFor();
        await page.goto(base + "/");
        await page.getByRole("link", { name: "学習順106を開く" }).waitFor();
        console.log("第22組106〜110：画面比較表・概要・復習・旧80URL・前後導線を確認");


        // 遷移直後のFocusOnNavigateによる自動フォーカスが終わってから検証する。
        await page.goto(base + "/learn/04");
        await page.waitForTimeout(850);
        const keyboardStart = page.getByRole("button", { name: "1増やす" });
        await keyboardStart.waitFor();
        await keyboardStart.focus();
        if (!(await keyboardStart.evaluate(element => document.activeElement === element))) {
            throw new Error("キーボード試験の起点となるボタンにフォーカスできません。");
        }
        await page.keyboard.press("Tab");
        const tabMovedToControl = await page.evaluate(() => {
            const target = document.activeElement;
            return target !== null
                && target !== document.body
                && target !== document.documentElement
                && target.matches('button, a[href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
        });
        if (!tabMovedToControl || await keyboardStart.evaluate(element => document.activeElement === element)) {
            throw new Error("Tabキーで次の操作要素にフォーカスを移動できません。");
        }
        console.log("日本語Blazor画面のキーボード移動とブラウザー操作試験が完了しました。");
    } catch (error) {
        console.error("ブラウザー操作試験が失敗しました：", error);
        // 失敗時に実際のページ状態を記録し、アプリ不具合とテスト誤判定を区別する。
        if (browser) {
            try {
                const pages = browser.contexts().flatMap(context => context.pages());
                const active = pages.at(-1);
                if (active) {
                    console.error("現在表示中の画面：", active.url());
                    console.error("カウンターの表示：", await active.locator('[data-testid="counter-value"]').allTextContents());
                }
            } catch (inspectionError) { console.error("画面取得失敗：", inspectionError); }
        }
        console.error(serverLog.slice(-12000));
        process.exitCode = 1;
    } finally {
        if (browser) await browser.close();
        server.kill();
    }
})();
