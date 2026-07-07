# sync-latest

## 目的
現在ブランチへ取り込むべき最新の基準を決め、安全に同期する。

## フロー

### Phase 1: 作業状態を確認する
- detached HEAD では停止する。
- `origin` が存在することを確認する。
- 現在ブランチ、upstream、open PR、未コミット差分を確認する。
- 同期で上書きされうる未コミット差分がある場合は停止し、先に `$commit` で保護する。

### Phase 2: 最新状態を取得する
`git fetch origin --prune` で remote-tracking branch を更新する。
判断に必要な場合だけ tag も取得する。
前提確認なしに `git pull` を使わない。

### Phase 3: 同期基準を決める

| 状態 | 同期基準 |
| --- | --- |
| ユーザーがブランチを指定している | 指定ブランチ |
| push 前で upstream が behind している | upstream branch |
| 現在ブランチに open PR がある | PR の base branch |
| 現在ブランチが既定ブランチである | `origin/<default>` |
| 作業ブランチで PR がない | `origin/<default>` |

複数該当する場合は、upstream を先に同期し、その後に PR base または既定ブランチを同期する。

### Phase 4: 同期する
- 現在ブランチが同期基準そのものなら fast-forward のみ行う。
- 作業ブランチでは同期基準を現在ブランチへ merge する。
- 既に共有済みのブランチでは履歴を書き換えない。
- `--force` と `--force-with-lease` は使わない。
- 衝突した場合は push、PR 作成、PR マージへ進まない。

### Phase 5: 状態を残す
同期した基準ブランチ、同期前後の ahead / behind、作成された merge commit の有無、衝突または未解決事項、次に進める作業を残す。
