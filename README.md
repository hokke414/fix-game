# MR Robot Repair Game

Meta Quest 3 / 3S向けMRロボット修理ゲームのUnityプロジェクトです。

## 現在の開発環境

- Unity: 6000.3.15f1
- Render Pipeline: Universal Render Pipeline 17.3.0
- Input System: 1.19.0
- Test Framework: 1.6.0
- XR Plug-in Management: 4.7.0
- OpenXR Plugin: 1.18.0
- Unity OpenXR Meta: 2.6.1
- XR Interaction Toolkit: 3.6.0
- Meta XR Core SDK: 205.0.0

XRパッケージはUnity 6対応版へ固定しています。Android Build Support（SDK / NDK / OpenJDK含む）は導入済みです。

## 実装状況

1. ゲーム状態・修理工程・リセット基盤：実装済み
2. スコア、誤操作重複抑制、30秒ヒント判定：実装済み
3. OpenXR / XRI / Meta XR SDKとQuest向け設定：復旧済み
4. 作業台配置コア：実装済み
5. 作業台配置シーン接続：実装・再検証済み
6. 断線、ヒューズ、基板の各工程：最小プレイ版を実装済み

配置確定後は、プリミティブで作ったロボットと6個の工具が表示されます。電源OFF、断線診断・交換、ヒューズ診断・交換、基板開放・交換、最終テストの8手順を順番に進め、完了時にスコアを表示します。高品質な3Dモデルや演出は今後差し替える前提です。

## 作業台配置

初期距離は仕様範囲1〜1.5mの中間である1.25mです。左スティックで前後左右、右スティックで高さと水平回転を調整し、左右どちらかの主ボタンで確定します。確定には、左右いずれかのコントローラーがガイドから0.2m以内にあることが必要です。

半透明ガイドの仮寸法は幅0.8m、奥行き0.5m、高さ0.75mです。左Menuボタンによる再調整は、ポーズメニュー実装までの暫定接続です。

## テスト

Unity Test RunnerでEditModeとPlayModeを実行します。復旧時点でEditModeは72件すべて成功、PlayModeは928件成功・3件スキップ・失敗0件です。

最新のAndroid Development APKはUnityビルドに成功し、Quest 3へのADBインストール・OpenXR起動・プロセス継続・致命的例外なしまで確認済みです。ヘッドセット内での見え方、全8工程の手動完走、72 FPSは実機装着での確認が必要です。

実機確認用ファイルは`output/quest/MRRobotRepair-Quest3-Development.apk`です。開発用署名のため、SideQuestまたはADBでサイドロードしてください。

## 暫定スコア設定

目標時間超過後300秒で時間点が30点から最低6点まで線形に下がります。時間だけでは0点にしない要件を満たす仮値です。
