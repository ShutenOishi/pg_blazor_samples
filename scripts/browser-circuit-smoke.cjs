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
    } catch (error) {
        console.error(error);
        console.error(serverLog.slice(-8000));
        process.exitCode = 1;
    } finally {
        if (browser) await browser.close();
        server.kill();
    }
})();
