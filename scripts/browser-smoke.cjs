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
        await page.getByText("表示名を入力してください。").waitFor();
        await page.getByText("学習メモを入力してください。").waitFor();
        if (await page.locator('[data-testid="step10-confirmation"]').count() !== 0) {
            throw new Error("ステップ10：必須未入力で確認が成立しました。");
        }
        await page.getByRole("textbox", { name: "練習用の表示名（2〜20文字・必須）" }).fill("あ");
        await page.getByRole("textbox", { name: "学習メモ（3〜80文字・必須）" }).fill("あい");
        await page.getByRole("button", { name: "入力内容を確認する" }).click();
        await page.getByText("表示名は2文字以上20文字以内で入力してください。").waitFor();
        await page.getByText("学習メモは3文字以上80文字以内で入力してください。").waitFor();
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

        await page.goto(base + "/learn/column/15");
        await page.getByText("DIを使うと、画面とサービスの何が変わる？").waitFor();
        console.log("コラム03：日本語画面の表示を確認");


        // Step16〜20：標準DIサービス、テスト容易性、Application分離と課題管理。
        await page.goto(base + "/learn/16");
        await page.waitForTimeout(1100);
        await waitText(page, '[data-testid="step16-title"]', "Blazorステップアップ学習");
        await page.getByRole("button", { name: "設定とDIの関係を確認" }).click();
        await waitText(page, '[data-testid="step16-explain"]', "IOptions");
        console.log("ステップ16：Optionsで設定を読み取れました。");

        await page.goto(base + "/learn/17");
        await page.waitForTimeout(1100);
        await page.getByRole("button", { name: "非同期処理を開始する" }).click();
        await waitText(page, '[data-testid="step17-state"]', "処理が完了しました");
        // 中断は処理中に別のボタンを押せるようにして確認する。
        await page.getByRole("button", { name: "非同期処理を開始する" }).click({ noWaitAfter: true });
        await page.getByRole("button", { name: "処理を取り消す" }).click();
        await waitText(page, '[data-testid="step17-state"]', "処理を取り消しました");
        console.log("ステップ17：非同期処理の完了と取り消しに成功");

        await page.goto(base + "/learn/18");
        await page.waitForTimeout(1100);
        await waitText(page, '[data-testid="step18-morning"]', "おはようございます");
        await waitText(page, '[data-testid="step18-afternoon"]', "こんにちは");
        console.log("ステップ18：固定時計の結果を確認");

        await page.goto(base + "/learn/19");
        await page.waitForTimeout(1100);
        await waitText(page, '[data-testid="step19-summary"]', "残り60ステップ");
        await page.getByRole("button", { name: "5ステップ学習した" }).click();
        await waitText(page, '[data-testid="step19-summary"]', "残り55ステップ");
        console.log("ステップ19：ApplicationサービスをDI経由で利用");

        await page.goto(base + "/learn/20");
        await page.waitForTimeout(1100);
        await waitText(page, '[data-testid="step20-count"]', "2");
        await page.getByRole("button", { name: "課題を追加する" }).click();
        await waitText(page, '[data-testid="step20-message"]', "1〜40文字");
        await page.getByRole("textbox", { name: "練習用の課題名（1〜40文字）" }).fill("ライブラリDIを復習する");
        await page.getByRole("button", { name: "課題を追加する" }).click();
        await waitText(page, '[data-testid="step20-count"]', "3");
        await page.getByRole("checkbox", { name: "ライブラリDIを復習する" }).check();
        console.log("ステップ20：空欄拒否、追加、完了切替を確認");

        await page.goto(base + "/learn/column/20");
        await page.getByText("DIを利用して画面の責任を小さくする").waitFor();
        await page.goto(base + "/learn/column/library-di");
        await waitText(page, '[data-testid="library-di-title"]', "Blazorステップアップ学習");
        await page.getByRole("button", { name: "ILoggerで学習用ログを書く" }).click();
        await waitText(page, '[data-testid="library-di-message"]', "学習用ログを書き込みました");
        console.log("コラム04・特別コラム05：DIの理解を深める日本語画面を確認");

        // Step21〜25：MudBlazorのイベント・日本語入力・検証・一覧を実際に操作する。
        await page.goto(base + "/learn/21");
        await page.waitForTimeout(1200);
        await page.getByRole("button", { name: "日本語の通知を表示する" }).click();
        await waitText(page, '[data-testid="step21-state"]', "通知サービスを使いました");
        await page.screenshot({ path: path.join(evidence, "step21.png") });

        await page.goto(base + "/learn/22");
        await page.waitForTimeout(1000);
        await page.getByRole("button", { name: "学習メニューを開閉" }).click();
        await waitText(page, '[data-testid="step22-state"]', "メニューを閉じています");
        await page.getByRole("button", { name: "学習メニューを開閉" }).click();
        await waitText(page, '[data-testid="step22-state"]', "メニューを表示しています");
        await page.screenshot({ path: path.join(evidence, "step22.png") });

        await page.goto(base + "/learn/23");
        await page.waitForTimeout(1000);
        await page.getByRole("textbox", { name: "練習用の氏名" }).fill("練習太郎");
        await waitText(page, '[data-testid="step23-preview"]', "練習太郎");
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
        await page.screenshot({ path: path.join(evidence, "step24.png") });

        await page.goto(base + "/learn/25");
        await page.waitForTimeout(1000);
        await waitText(page, '[data-testid="step25-count"]', "4 件");
        await page.getByText("課題名", { exact: true }).last().click();
        await page.screenshot({ path: path.join(evidence, "step25.png") });
        await page.goto(base + "/learn/column/25");
        await page.getByText("公式テンプレートから日本語業務UIへ").waitFor();
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
