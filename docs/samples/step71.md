# ステップ71：既存台帳のキー正規化・重複拒否と索引

## 到達目標

ADが返す `DOMAIN\user` を、既存台帳・外部テーブルのログイン名と正しく比較し、表記揺れ・重複・改名時の拒否を実装します。対象は1万人以上の既存利用者ですが、同時接続者数とは分けて考えます。

## 操作して確認する

1. 学習画面の「現在の台帳照合を確認」を押します。認証のない環境では拒否されます。
2. 開発用PostgreSQL 18の架空台帳に大小の違う同名を2行用意し、`ambiguous` となることを試験します。
3. 古い名前を改名した後、旧名が `unknown`、新名が `allowed` になることを試験します。
4. 正本が無効・閲覧不可・DB停止のとき、画面が業務データを返さないことを確認します。

## コードとSQLのしくみ

WindowsUserAccessServiceは、Windows認証が発行したドメイン付き名前だけを取得して大文字正規化します。EF Core 10の読み取り専用ビューから `Where(x => x.LoginName == login).Take(2)` で検索し、0件は未登録、2件は重複として拒否します。リスト全体を読み込みません。

開発専用ビューの要点は次のSQLです。

```sql
CREATE INDEX IF NOT EXISTS learning_existing_user_ci_login_upper_idx
    ON learning_existing_user_ci (upper(login_name));
CREATE OR REPLACE VIEW learning_user_access_v AS
SELECT upper(login_name) AS login_name,
       display_name, is_active, can_read, can_edit, unit_code, authorization_version
FROM learning_existing_user_ci;
```

式索引は**この架空のローカルテーブルだけ**に対応します。本番ではデータ正本の一意制約、照合規則、FDWでの述語pushdown、`EXPLAIN (ANALYZE, BUFFERS)` の計画と実測をDB管理者が検証します。索引の存在だけで高速性や外部DBへの条件pushdownは保証されません。

## 実運用の重要な境界

- ログイン名が再利用される場合、**ADのユーザー名だけでは別人物を区別できません**。正本での名前再利用禁止、または不変IDの別契約が必須です。
- ADからグループや属性を取得しない方針は維持します。権限と部署範囲は既存DBが正本です。
- CIの仮Principalは実AD/IIS/Kerberosの受入ではありません。外部表の遅延・切断時はフェイルクローズを確認します。
- 利用者の登録、所属変更、ロール付与UIは実装しません。

## 確認問題と解答

**Q1. 台帳のログイン名をすべてメモリに取り込み検索する？** いいえ。WHERE条件と最大2件の結果に絞ります。

**Q2. 元表に文字列のPRIMARY KEYがあれば大小文字違いの重複は防げる？** その制約だけでは不十分です。正規化後のキーでも重複を拒否します。

**Q3. ADの `DOMAIN\USER` を再利用して別の職員へ割り当てても判別できる？** いいえ。ユーザー名以外の不変IDまたは非再利用の正本契約が必要です。

## 次へ

Step72では、Windows認証を用いた専用HTTPログイン経路と短命Cookieの設計・安全試験へ進みます。
