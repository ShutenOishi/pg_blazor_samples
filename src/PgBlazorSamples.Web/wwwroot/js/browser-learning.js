// ブラウザーのDOMとダウンロードに限定した学習用JS。認証・認可情報は処理しない。
export function readLanguage() {
    return document.documentElement.lang || "未設定";
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
