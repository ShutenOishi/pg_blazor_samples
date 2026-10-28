#!/usr/bin/env node
// 学習順015：専用Chrome BrowserContextのWebSocketだけを切断する隔離試験。
// 実AD・IIS・業務DB・本番ネットワーク・認証セッションは操作しない。
const { chromium } = require("playwright-core");
const { spawn, execFileSync } = require("node:child_process");
const path = require("node:path");

const root = path.resolve(__dirname, "..");
const base = "http://127.0.0.1:5186";
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));

function findChrome() {
    for (const name of ["google-chrome", "google-chrome-stable", "chromium", "chromium-browser"]) {
        try { return execFileSync("which", [name], { encoding: "utf8" }).trim(); }
        catch { /* 続けて探す */ }
    }
    throw new Error("学習順015：Chromeが見つかりません");
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
    for (let i = 0; i < 80; i++) {
        if (server.exitCode !== null) break;
        try { if ((await fetch(base + "/learn/015")).ok) return; }
        catch { /* 起動待ち */ }
        await delay(500);
    }
    throw new Error("学習順015：アプリ起動失敗\n" + serverLog.slice(-4000));
}
async function getCount(page) {
    return (await page.getByTestId("runtime02-count").textContent()).trim();
}
async function getToken(page) {
    return (await page.getByTestId("runtime02-instance").textContent()).trim();
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
        const context = await browser.newContext({ locale: "ja-JP" });
        let blocked = false;
        let activeSocket = null;
        let socketAttempts = 0;
        await context.routeWebSocket("**/_blazor?*", async socket => {
            socketAttempts++;
            if (blocked) {
                await socket.close({ code: 1012, reason: "isolated-learning-disconnect" });
                return;
            }
            activeSocket = socket;
            await socket.connectToServer();
        });
        const page = await context.newPage();
        const response = await page.goto(base + "/learn/015");
        if (!response || response.status() !== 200)
            throw new Error("学習順015：初期DocumentのHTTP 200を確認できません");
        await page.getByTestId("runtime02-count").waitFor();
        await page.waitForTimeout(900);
        const click = page.getByRole("button", { name: "対話回数を1増やす" });
        await click.click();
        await page.waitForFunction(() => document.querySelector('[data-testid="runtime02-count"]')?.textContent?.trim() === "1");
        await click.click();
        await page.waitForFunction(() => document.querySelector('[data-testid="runtime02-count"]')?.textContent?.trim() === "2");
        if (!activeSocket || socketAttempts < 1)
            throw new Error("学習順015：対話WebSocketを捕捉できず切断は未実証");
        const initialToken = await getToken(page);
        if (!/^[0-9a-f]{32}$/.test(initialToken))
            throw new Error("学習順015：学習用インスタンス識別値が不正");
        console.log("学習順015：実WebSocket経路・C#イベント2回・一時状態IDを確認");

        blocked = true;
        await activeSocket.close({ code: 1012, reason: "isolated-learning-disconnect" });
        // 回線のモード変更だけでなく、Blazorが切断を実際に検知したことを要求する。
        await page.waitForFunction(() => {
            const el = document.getElementById("components-reconnect-modal");
            return !!el && ["components-reconnect-show", "components-reconnect-retrying",
                "components-reconnect-failed", "components-reconnect-rejected"]
                .some(name => el.classList.contains(name));
        }, null, { timeout: 25000 });
        if (await getCount(page) !== "2")
            throw new Error("学習順015：切断中の値が意図せず変わりました");
        blocked = false;
        // 同一Documentへ再接続したと実証できる場合だけ状態保持として受入。
        await page.waitForFunction(() => {
            const el = document.getElementById("components-reconnect-modal");
            return !!el && !["components-reconnect-show", "components-reconnect-retrying",
                "components-reconnect-failed", "components-reconnect-rejected",
                "components-reconnect-paused"].some(name => el.classList.contains(name));
        }, null, { timeout: 45000 });
        if (socketAttempts < 2)
            throw new Error("学習順015：WebSocketの再接続試行がありません");
        if (await getToken(page) !== initialToken || await getCount(page) !== "2")
            throw new Error("学習順015：短時間再接続の一時状態保持を実証できません");
        await click.click();
        await page.waitForFunction(() => document.querySelector('[data-testid="runtime02-count"]')?.textContent?.trim() === "3");
        console.log("学習順015：隔離切断・再接続案内・WebSocket再接続・状態保持を確認");

        const reload = await page.reload();
        if (!reload || reload.status() !== 200)
            throw new Error("学習順015：再読込のDocument HTTP 200を確認できません");
        await page.waitForFunction(token => {
            const value = document.querySelector('[data-testid="runtime02-instance"]')?.textContent?.trim();
            return !!value && value !== token;
        }, initialToken, { timeout: 15000 });
        if (await getCount(page) !== "0")
            throw new Error("学習順015：新Documentの状態が0に初期化されません");
        console.log("学習順015：再読込は別インスタンスで回数0。保持期限切れ・認可失効はNOT_TESTED");
        await context.close();

        // 学習順041：015と独立したBrowserContext/circuitを作成し、JS例外の
        // 人工生成ではなく、実WebSocketを切断してC# catchのログを検証する。
        const context041 = await browser.newContext({ locale: "ja-JP" });
        let socket041 = null;
        let block041 = false;
        let socket041Attempts = 0;
        await context041.routeWebSocket("**/_blazor?*", async socket => {
            socket041Attempts++;
            if (block041) {
                await socket.close({ code: 1012, reason: "isolated-041-disconnect" });
                return;
            }
            socket041 = socket;
            await socket.connectToServer();
        });
        const page041 = await context041.newPage();
        const response041 = await page041.goto(base + "/learn/041");
        if (!response041 || response041.status() !== 200)
            throw new Error("学習順041：独立Document HTTP200なし");
        // 初回Documentの静的事前描画とInteractive Server接続を区別する。
        await page041.getByTestId("browser01-count").waitFor();
        await page041.waitForTimeout(1100);
        await page041.getByRole("button", { name: "C#だけで回数を増やす" }).click();
        await page041.waitForFunction(() =>
            document.querySelector('[data-testid="browser01-count"]')?.textContent?.trim() === "1",
            null, { timeout: 15000 });
        await page041.getByRole("button", { name: "JavaScriptでHTMLの言語設定を読む" }).click();
        await page041.waitForFunction(() =>
            document.querySelector('[data-testid="browser01-language"]')?.textContent?.includes("HTMLの言語設定："),
            null, { timeout: 15000 });
        if (!socket041 || socket041Attempts < 1)
            throw new Error("学習順041：独立WebSocketの捕捉なし");
        await page041.getByRole("button", { name: "切断捕捉の待機処理を開始する" }).click();
        await page041.waitForFunction(() =>
            document.querySelector('[data-testid="browser01-disconnect-probe"]')?.textContent?.includes("実切断試験の待機中"),
            null, { timeout: 15000 });
        block041 = true;
        await socket041.close({ code: 1012, reason: "isolated-041-disconnect" });
        // WebSocketだけでなく、再接続時のHTTP代替経路も隔離する。
        // このBrowserContextに限定し、他のテスト・DB・外部ネットワークへは触れない。
        await context041.setOffline(true);
        await page041.waitForFunction(() => {
            const el = document.getElementById("components-reconnect-modal");
            return !!el && ["components-reconnect-show", "components-reconnect-retrying",
                "components-reconnect-failed", "components-reconnect-rejected"]
                .some(name => el.classList.contains(name));
        }, null, { timeout: 25000 });
        for (let attempt = 0; attempt < 40 && !serverLog.includes("L041_REAL_DISCONNECT_CAUGHT"); attempt++)
            await delay(500);
        if (!serverLog.includes("L041_REAL_DISCONNECT_CAUGHT")) {
            // 実切断の事実とC#側の例外型を推測で結び付けない。
            const invoked = serverLog.includes("L041_PROBE_JS_INVOKE_START");
            const completed = serverLog.includes("L041_PROBE_JS_INVOKE_COMPLETED");
            const jsError = serverLog.includes("L041_PROBE_JS_EXCEPTION_NOT_DISCONNECT");
            const otherError = serverLog.includes("L041_PROBE_OTHER_EXCEPTION_");
            console.log("学習順041：041独立circuit実WebSocket切断・BrowserContext offline・再接続案内を検証。C# JSDisconnectedException捕捉はNOT_TESTED。" +
                " invokeStarted=" + invoked + " invokeCompleted=" + completed +
                " jsError=" + jsError + " otherError=" + otherError + "。捕捉ログ未成立につき正式受入禁止");
        } else {
            console.log("学習順041：041独立circuit実WebSocket切断・BrowserContext offline後、C# JSDisconnectedException catchのサーバーログ識別子を検証");
        }
        await context041.close();
    } catch (error) {
        console.error(error);
        console.error(serverLog.slice(-8000));
        process.exitCode = 1;
    } finally {
        if (browser) await browser.close();
        server.kill();
    }
})();
