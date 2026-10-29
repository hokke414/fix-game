# Meta Quest 3 / 3S 実機確認用

## ファイル

- `MRRobotRepair-Quest3-Development.apk`
- 種別: Android開発ビルド（ARM64 / IL2CPP / Vulkan / OpenXR）
- 対象: Meta Quest 3 / 3S
- アプリ名: `MR Robot Repair`
- パッケージID: `com.hokke414.mrrobotrepair`
- SHA-256: `E32224E051C7F3B3D6B44021B0654D413D98F983C9EE6E424C6C29AD8EFA1598`

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

このAPKは低ポリゴンの最小プレイ版です。電源OFF、断線、ヒューズ、基板、最終テストの8工程を含みます。指示文の下にはライブタイマーを表示し、スコアはクリアタイムだけから算出します。工具位置は開始ごとに入れ替わります。工具と機械側の操作箇所は色と形の両方で見分けられます。
