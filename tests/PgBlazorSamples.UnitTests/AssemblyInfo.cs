using Xunit;

// 開発用PostgreSQL 18を共有する各結合試験が互いの件数・行状態に干渉しないよう、
// テストメソッドの同時実行を禁止する。実運用性能試験とは別の目的。
[assembly: CollectionBehavior(DisableTestParallelization = true)]
