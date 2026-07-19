# AGENTS.md

## 基本方針

- ユーザーへの応答は日本語で行い、一般的でない略語や、対象と責務を省いた短縮表現を避ける。
- 作業前に `git status --short --branch` と対象差分を確認し、既存の未コミット変更や未追跡ファイルを他者の作業として保持する。
- `AGENTS.md` は作業者向けの安定した判断規則を持つ。`README.md` は、インストール方法、CLI の使用例、公開パッケージに含まれるスキルの案内を利用者へ示す反映先とし、その内容をここへ複製しない。スキルカタログの正本は `skills/bundle.json` と `skills/definitions/**` とする。

## プロジェクト概要

SkillsPack は、再利用可能な Agent Skills を配布する .NET global tool である。NuGet パッケージ `MackySoft.SkillsPack` は、`skills-pack` CLI とホスト別に生成されたスキル群を一つの成果物として配布する。

ホストに依存しないスキルの列挙、選択、書き出し、導入、更新、診断などの中核処理は `MackySoft.AgentSkills` が所有する。このリポジトリは、SkillsPack のスキルカタログ、生成済みスキルの同梱、SkillsPack 固有の JSON 出力契約、エラーと終了コード、NuGet パッケージおよびリリース処理を所有する。汎用処理を CLI 層へ重複実装しない。

## プロジェクト構造

| パス | 責務 |
| --- | --- |
| `skills/bundle.json` | カタログ ID とスキルバンドルのバージョンを定める正本。 |
| `skills/definitions/<category>/<skill>/` | スキルの正本。`skill.json` が表示情報と依存関係、`SKILL.md.template` が本文、`references/*.template` が参照資料を持つ。 |
| `skills/generated/` | 正本から生成され、コミットと NuGet パッケージへ含める派生物。手作業では編集しない。 |
| `src/SkillsPack/` | .NET CLI 本体とパッケージ設定。 |
| `src/SkillsPack/Hosting/Cli/Common/` | JSON プロトコル、コマンド名、終了コード、実行、失敗変換、出力を含む CLI 共通境界。 |
| `src/SkillsPack/Hosting/Cli/Skills/` | `MackySoft.AgentSkills` の引数と実行結果を SkillsPack の公開契約へ変換するアダプター。 |
| `src/SkillsPack/Hosting/Composition/` | DI と Agent Skills ランタイムを構成する場所。 |
| `tests/SkillsPack.Tests/` | `src/SkillsPack/` の構造に対応する xUnit テスト。 |
| `scripts/` | スキル生成、整形、テスト、パッケージ検証、リリースでローカルと CI が共有する入口。 |
| `.github/workflows/` | スキル同期、マルチプラットフォーム検証、パッケージ公開の自動化。 |

`SkillsPack.slnx` は本体とテストのプロジェクト構成、`src/SkillsPack/SkillsPack.csproj` は global tool と生成済みスキルの同梱、`Directory.Build.props` は共通の NuGet メタデータ、`.config/dotnet-tools.json` はスキル生成ツールの固定バージョンを所有する。

## スキルの変更

- スキルの追加、本文、description、依存関係、参照資料は `skills/definitions/**` で変更する。
- `skills/generated/**`、digest、manifest、`agent-skill.json`、`agents/openai.yaml` は直接編集しない。
- 正本を変更したら `bash scripts/generate-skills.sh` を実行し、対応する生成差分を正本と同じ変更へ含める。
- 公開パッケージの利用方法、カテゴリ、同梱スキルが変わる場合は、正本の内容を利用者向けに `README.md` へ反映する。
- カテゴリ、スキル数、依存閉包、同梱物が変わる場合は、`tests/SkillsPack.Tests/**` と `scripts/verify-cli-package.sh` にあるカタログ、選択、`export`、`install` の期待値も確認する。
- `.github/workflows/skills-sync.yaml` による push 後の同期を、ローカルでの生成と確認の代わりにしない。

## CLI と C# の変更

- SkillsPack 固有の公開 JSON、エラー、終了コードは `Hosting/Cli/Common/Contracts/`、共通実行は `Hosting/Cli/Common/Execution/`、スキル操作の変換は `Hosting/Cli/Skills/`、DI は `Hosting/Composition/` に置く。
- 公開 `payload`、エラー、終了コード、互換引数を変更する場合は、対応する契約テストを同じ作業で更新する。
- テストは本体コードとの対応が追える `tests/SkillsPack.Tests/` 配下へ置く。
- C# の書式と命名は `.editorconfig`、標準の整形入口は `bash scripts/code-quality.sh format` とする。
- `--no-restore` は、同じ作業で restore が成功している場合だけ使用する。

## セットアップと検証

初回または依存関係の変更後は、リポジトリルートで次を実行する。

```bash
dotnet tool restore
dotnet restore SkillsPack.slnx
```

スキルの正本を変更した場合は、生成と生成物の検査を行う。

```bash
bash scripts/generate-skills.sh
bash scripts/verify-skills.sh
```

C# の変更中は、必要な範囲で整形とテストを実行する。

```bash
bash scripts/code-quality.sh format
bash scripts/test-dotnet.sh
```

完了前の標準検証は次のコマンドとする。これは生成物の整合、C# の書式、Release build、xUnit テストを確認する。

```bash
bash scripts/verify.sh
```

NuGet パッケージ、同梱物、カタログ選択、CLI の配布時契約へ影響する場合は、`verify.sh` に加えてパッケージのスモークテストも行う。通常開発時のバージョンは `Directory.Build.props` を確認する。

```bash
dotnet pack src/SkillsPack/SkillsPack.csproj \
  --configuration Release \
  --no-restore \
  -p:Version=<package-version> \
  -p:PackageVersion=<package-version> \
  --output artifacts/packages
bash scripts/verify-cli-package.sh artifacts/packages <package-version>
```

## リリース

リリース処理の正本は `.github/workflows/package-publish.yaml` と、そこから呼び出す `scripts/` のリリース用スクリプトである。タグは接頭辞なしの `<major>.<minor>.<patch>` 形式を使用する。ワークフローは、リリース元の検証、バージョン反映、`pack`、スモークテスト、NuGet.org への公開、公開確認、GitHub Release への反映を所有する。明示的なリリース依頼がない通常作業では、タグ作成、パッケージ公開、グローバル環境の更新を行わない。

## リリース後のグローバル更新

NuGet.org へのパッケージ公開が成功したら、リリース後の作業として、グローバルにインストールされている CLI と OpenAI のユーザースコープに配置されたスキルを、公開した同じバージョンへ更新する。

1. 公開したバージョンを指定して CLI を更新する。

   ```bash
   dotnet tool update --global MackySoft.SkillsPack --version <release-version>
   ```

2. CLI のバージョンが `<release-version>` と一致することを確認する。

   ```bash
   skills-pack --version
   ```

3. 更新した CLI が提供するカテゴリを確認する。

   ```bash
   skills-pack skills list
   ```

4. 一覧に含まれるすべてのカテゴリを指定し、同梱されている全スキルを OpenAI のユーザースコープへ反映する。

   ```bash
   skills-pack skills update --host openai --scope user --category <comma-separated-categories>
   ```

5. 同じカテゴリを指定して、ユーザースコープのスキルに問題がないことを確認する。

   ```bash
   skills-pack skills doctor --host openai --scope user --category <comma-separated-categories>
   ```

6. 更新したスキルを読み込ませるため、Codex アプリを再起動するか、新しいセッションを開始する。

これらの更新、確認、再読み込みが完了するまで、リリース後のローカル反映を完了扱いにしない。失敗した場合は、失敗したコマンドと原因を報告する。
