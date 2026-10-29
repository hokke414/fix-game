# Meta Quest 3 / 3S 実機確認用

## ファイル

- `MRRobotRepair-Quest3-Development.apk`
- 種別: Android開発ビルド（ARM64 / IL2CPP / Vulkan / OpenXR）
- 対象: Meta Quest 3 / 3S
- アプリ名: `MR Robot Repair`
- パッケージID: `com.hokke414.mrrobotrepair`
- SHA-256: `D4E3B07C0670875DE4CFFEA341A4B4E78F89803E22E9C7C13671E6419542C50E`

## インストール

Questを開発者モードにし、SideQuestからAPKを選ぶか、ADBで次を実行します。

```powershell
adb install -r .\MRRobotRepair-Quest3-Development.apk
```

## 現在の確認範囲

- UnityのAndroidビルド: 成功
- EditModeテスト: 76件成功 / 失敗0件
- ゲーム側PlayModeテスト: 2件成功 / 失敗0件
- Quest 3へのADBインストール: 成功
- Quest 3でのOpenXR起動・プロセス継続: 成功
- 起動直後の致命的例外: なし
- ヘッドセット内での全8工程完走、見え方、72 FPS: 要手動確認

このAPKは低ポリゴンの最小プレイ版です。電源を切ってから、コードや部品を直し、最後に動きを確かめる8工程を含みます。小学校高学年向けに難しい言葉を避け、赤・紫・ピンクなどの色と形の両方で操作場所を見分けられるようにしています。指示文の下にはライブタイマーを表示し、スコアはクリアタイムだけから算出します。工具位置は開始ごとに入れ替わります。
