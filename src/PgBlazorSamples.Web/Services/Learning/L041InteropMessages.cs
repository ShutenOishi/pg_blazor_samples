using Microsoft.JSInterop;

namespace PgBlazorSamples.Web.Services.Learning;

/// <summary>学習順041。実際のエラー詳細を利用者画面へ表示しない。</summary>
public static class L041InteropMessages
{
    public static string Describe(Exception exception) => exception switch
    {
        JSDisconnectedException => "サーバーとの接続が切れました。再接続してください。",
        JSException => "ブラウザーの処理に失敗しました。再度お試しください。",
        _ => "処理に失敗しました。管理者に連絡してください。"
    };
}
