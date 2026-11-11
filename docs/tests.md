# テスト

- UnitTests / IntegrationTests: IdP不要(統合テストはテスト用署名鍵でトークンを検証)
- E2ETests: 認証ガードのテストはIdP不要。ログイン〜CRUD一巡(`DataCrudTests`)は上記Keycloakを起動し、環境変数 `E2E_OIDC=1` を設定したときのみ実行される

E2E(`tests/Template.BlazorWasm.E2ETests`)はPlaywrightで動かす。日常の実行には含めず、必要なときとリリース前に流す。コマンドはリポジトリ直下のPowerShellで実行する。

```powershell
# ブラウザの入手(初回。ビルドで出力されるスクリプトを使う)
dotnet build tests/Template.BlazorWasm.E2ETests
pwsh tests/Template.BlazorWasm.E2ETests/bin/Debug/net10.0/playwright.ps1 install

# 実行(既定は Chromium・画面なし)
dotnet run --project tests/Template.BlazorWasm.E2ETests

# 画面を出して実行
$env:HEADED = "1"; dotnet run --project tests/Template.BlazorWasm.E2ETests; Remove-Item Env:HEADED

# ブラウザを切り替える(chromium / firefox / webkit)
$env:BROWSER = "firefox"; dotnet run --project tests/Template.BlazorWasm.E2ETests; Remove-Item Env:BROWSER

# Playwright Inspector で 1 手ずつ実行
$env:PWDEBUG = "1"; dotnet run --project tests/Template.BlazorWasm.E2ETests; Remove-Item Env:PWDEBUG
```

- 失敗したテストだけ、操作ごとの画面・DOM・通信・コンソールを記録したトレースを `tests/Template.BlazorWasm.E2ETests/bin/Debug/net10.0/playwright-traces/<テストの表示名>.zip` に残す。`pwsh tests/Template.BlazorWasm.E2ETests/bin/Debug/net10.0/playwright.ps1 show-trace <zip>` か https://trace.playwright.dev で開く
