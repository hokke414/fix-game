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
3. OpenXR / XRI / Meta XR SDKとQuest向け設定：復旧中
4. 作業台配置コア：実装済み
5. 作業台配置シーン接続：実装済み、再検証中
6. 断線、ヒューズ、基板の各工程：未実装

## 作業台配置

初期距離は仕様範囲1〜1.5mの中間である1.25mです。左スティックで前後左右、右スティックで高さと水平回転を調整し、左右どちらかの主ボタンで確定します。確定には、左右いずれかのコントローラーがガイドから0.2m以内にあることが必要です。

半透明ガイドの仮寸法は幅0.8m、奥行き0.5m、高さ0.75mです。左Menuボタンによる再調整は、ポーズメニュー実装までの暫定接続です。

## テスト

Unity Test RunnerでEditModeとPlayModeを実行します。Quest実機でのパススルー、入力、72 FPSは未検証です。

## 暫定スコア設定

目標時間超過後300秒で時間点が30点から最低6点まで線形に下がります。時間だけでは0点にしない要件を満たす仮値です。
