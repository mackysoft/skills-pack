# pr-submit

## 目的
現在作業を PR として作成または既存 open PR へ更新する。

PR の base を決め、必要なら `$sync-latest` で最新化し、検証、`$push`、本文作成、PR 作成または既存 open PR の更新まで進める。

## フロー

### Phase 1: 作業状態を確認する
- `gh auth status` を確認する。
- 現在ブランチ、未コミット差分、既存 open PR の有無を確認する。
- detached HEAD で PR 対象を確定できない場合は停止する。

### Phase 2: Issue と base を決める
Issue は明示入力、ブランチ名の `issue-<N>`、既存 PR の関連情報の順に解決する。
Issue が解決できた場合は、PR 本文末尾に必ず `Closes #<N>` を入れて Development へ紐付ける。
Issue が解決できない場合は、Issue なしで進める。

base branch は明示入力、既存 PR の base、Issue 側の指定、リポジトリの既定ブランチの順に決める。

### Phase 3: PR 対象を最新化する
検証前に `$sync-latest` で base branch を現在ブランチへ同期する。
同期で衝突した場合は検証や PR 作成へ進まない。

base との差分がなく、未コミット差分もない場合は PR 対象なしとして停止する。

### Phase 4: 検証する
`$verification-gate` を PR 前用途で実行する。
失敗した場合は PR を作成または更新しない。

### Phase 5: push する
`$push` を使い、未コミット差分のコミット、必要な最新化、push を行う。

### Phase 6: PR タイトルと本文を書く
差分、コミット履歴、検証結果、Issue 本文から PR タイトルと本文を作る。

PR タイトルと本文の文章前提は `$writing` に従う。
PR タイトルと本文は同じ言語で書く。
本文は `##` 見出しと短い箇条書きを基本にする。
本文の基本順は `Summary`、`Scope`、`Verification` にする。
任意セクションは、必要な場合だけ次の表の順に差し込む。
本文の見出しは本文の言語に合わせて翻訳しない。
`Closes #<N>` のような GitHub の構文は原文のまま置く。

| 見出し | 必須 | 内容 |
| --- | --- | --- |
| `Summary` | 必須 | 変更の要点。レビュー前に把握すべき結論を短く書く。 |
| `Changelog` | 任意 | 利用判断に使う変更説明が必要な場合に置く。項目は `$changelog` で作り、見出しはこのスキルで置く。 |
| `Scope` | 必須 | 変更した範囲、生成物、影響する機能や文書を列挙する。 |
| `Verification` | 必須 | 実行した検証、確認できた結果、実行できなかった検証を書く。 |
| `References` | 任意 | レビュー時に確認すべき資料、仕様、外部URL、リポジトリ内の相対パスを書く。ローカル環境の絶対パスは書かない。 |
| `Risks / Rollback` | 任意 | レビュー判断に必要な具体的なリスク、未確認事項、通常のrevertだけでは足りない戻し方を書く。 |

`Changelog` にカテゴリ見出しが必要な場合は、`## Changelog` の下に `### Added` のような1段下の見出しを置く。
空の任意セクションは置かない。
長い補足やログを本文に含める必要がある場合だけ `<details>` を使い、通常の説明は箇条書きまたは短い段落で書く。

本文の形は次を基準にし、任意セクションは必要なものだけ残す。

```md
## Summary
- <summary item>

## Changelog
- <user-facing change item>

## Scope
- <scope item>

## Verification
- `<command or check>`

## References
- <repository-relative path or URL>

## Risks / Rollback
- <risk or rollback item>
```

### Phase 7: PR を作成または更新する
同じブランチの open PR がある場合は、その PR を更新する。
open PR がない場合は `gh pr create` で作成する。

Issue の受け入れ条件が未完了または未確認なら Draft PR にする。
完了している場合は通常 PR にする。

### Phase 8: 状態を残す
PR URL、base branch、検証結果、push 結果、Draft 状態、Issue 連携、停止理由を残す。
