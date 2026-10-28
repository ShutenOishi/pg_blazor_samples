// ブラウザーのDOMとダウンロードに限定した学習用JS。認証・認可情報は処理しない。
export function readLanguage() {
    return document.documentElement.lang || "未設定";
}

// 学習順041：実際にJS例外を投げ、C#側のJSException処理を試験する。
// ブラウザーや利用者の設定を書き換えず、認証情報にも触れない。
export function throwLearningError() {
    throw new Error("L041_JS_ERROR_TEST_ONLY");
}

// .NETストリームをブラウザーのBlobに変換する。URLは利用後に解放する。
export async function downloadStream(fileName, mediaType, streamReference) {
    const bytes = await streamReference.arrayBuffer();
    const objectUrl = URL.createObjectURL(new Blob([bytes], { type: mediaType }));
    const link = document.createElement("a");
    link.href = objectUrl;
    link.download = fileName;
    document.body.appendChild(link);
    try {
        link.click();
    } finally {
        link.remove();
        setTimeout(() => URL.revokeObjectURL(objectUrl), 30000);
    }
}
