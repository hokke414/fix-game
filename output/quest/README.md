# Meta Quest 3 / 3S 実機確認用

## ファイル

- `MRRobotRepair-Quest3-Development.apk`
- 種別: Android開発ビルド（ARM64 / IL2CPP / Vulkan / OpenXR）
- 対象: Meta Quest 3 / 3S
- アプリ名: `MR Robot Repair`
- パッケージID: `com.hokke414.mrrobotrepair`
- SHA-256: `55D458CB6CCDE42D27685494E888CD8E219FE0AB00A48471BFD0D0C06F11A4C8`

## インストール

Questを開発者モードにし、SideQuestからAPKを選ぶか、ADBで次を実行します。

```powershell
adb install -r .\MRRobotRepair-Quest3-Development.apk
```

## 現在の確認範囲

- UnityのAndroidビルド: 成功
- EditModeテスト: 72件成功 / 失敗0件
- PlayModeテスト: 928件成功 / 3件スキップ / 失敗0件
- Quest 3へのADBインストール: 成功
- Quest 3でのOpenXR起動・プロセス継続: 成功
- 起動直後の致命的例外: なし
- ヘッドセット内での全8工程完走、見え方、72 FPS: 要手動確認

このAPKは低ポリゴンの最小プレイ版です。電源OFF、断線、ヒューズ、基板、最終テストの8工程とスコア表示を含みます。指示文とミス数は同じ画面の上下にまとめ、工具はテスター・ケーブル・ヒューズ・ドライバー・基板を形でも見分けられるようにしています。
