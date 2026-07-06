# writing

## 目的
文書前提に従って、自然言語の文章を整える。
意味保持と事実性を守り、論理構造、文書構造、文体を調整する。

## 前提
対象、読者、媒体、言語、出力形式は `$writing-context` の判断に従う。
未固定の場合は、本文を作る前に対象、読者、媒体、言語、出力形式を固定する。
意味保持の強さは、元文の有無、ユーザー指定、媒体、出力形式から判断する。

## 参照
必要な参照だけを読む。

| 必要な判断 | 参照 |
| --- | --- |
| 事実性、意味保持、主張、根拠、不確実性 | [references/lens-argument-integrity.md](references/lens-argument-integrity.md) |
| 見出し、段落、順序、例、表、箇条書き | [references/lens-document-structure.md](references/lens-document-structure.md) |
| 冗長表現、反復、文体、過剰な一般論 | [references/lens-style-concision.md](references/lens-style-concision.md) |
| チャット、メール、レビューコメント、Issue/PR コメント | [references/medium-interaction.md](references/medium-interaction.md) |
| README、仕様、設計文書、API 説明、技術記事 | [references/medium-technical-document.md](references/medium-technical-document.md) |
| 日本語の語彙、表記、文末、強調、外来語 | [references/language-japanese.md](references/language-japanese.md) |
| 日本語の技術原稿、長文記事、書籍調の文章 | [references/profile-japanese-technical-manuscript.md](references/profile-japanese-technical-manuscript.md) |

## 優先順位
1. 確認済みの事実、意味保持、責任主体、条件、不確実性を守る。
2. 文書前提を優先する。
3. 媒体に合う構造と粒度にする。
4. 言語ごとの自然な表記、語法、強調へ整える。
5. 簡潔さ、読みやすさ、語感を調整する。

## 基本規則
- 確認していない内容を、確認済みのように書かない。
- 弱い根拠を、強い断定へ変えない。
- 意味保持が必要な作業では、事実、主張、条件、責任主体、時制、確度を変えない。
- 冗長を削る場合も、読者に必要な定義、条件、根拠、制約は残す。
- 文体を整えるためだけに、判断に必要な曖昧さや未確定事項を消さない。
- 既存文書を更新する場合は、既存の見出し階層、用語、表記、粒度に合わせる。
- 出力に、参照選択や推敲過程の説明を混ぜない。
- 参照へ分けた規則を、要約して弱めない。

## 出力
完成文を求められている場合は、完成文を先に返す。
レビューを求められている場合は、問題、理由、修正方向を返す。
推敲前後の比較が必要な場合だけ、変更前後を並べる。
判断が必要な点が残る場合は、確認が必要なことと、今回はどう扱ったかを分けて返す。
