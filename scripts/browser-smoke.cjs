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
        browser = await chromium.launch({
            executablePath: findChrome(),
            headless: true,
            args: ["--no-sandbox", "--disable-dev-shm-usage"]
        });
        const page = await browser.newPage({ locale: "ja-JP", viewport: { width: 1280, height: 850 } });
        const evidence = path.resolve(root, "dist", "browser-evidence");
        mkdirSync(evidence, { recursive: true });

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
        const transientAfter = (await page.locator('[data-testid="step13-transient-new"]').textContent()).trim();
        if (transientAfter === transientBefore || transientAfter === "まだ取得していません") {
            throw new Error("ステップ13：新しいTransientを取得できませんでした。");
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

        await page.keyboard.press("Tab");
        if ((await page.locator(":focus").count()) === 0) {
            throw new Error("キーボードでフォーカスを移動できません。");
        }
        console.log("日本語Blazor画面の実ブラウザー操作試験が完了しました。");
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
