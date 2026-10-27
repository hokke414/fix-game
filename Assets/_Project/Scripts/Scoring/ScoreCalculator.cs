using UnityEngine;

namespace MRRobotRepair.Scoring
{
    /// <summary>工程精度60点、時間30点、安全確認10点の最終スコアを計算する。</summary>
    public static class ScoreCalculator
    {
        /// <summary>100点満点のスコアを計算する。</summary>
        public static ScoreResult Calculate(
            ScoreInput input,
            int minimumTimeScore = 6,
            float overtimeUntilMinimumSeconds = 300f)
        {
            int accuracy = Mathf.RoundToInt(Mathf.Clamp01(input.AccuracyRatio) * 60f);
            int safety = input.SafetyConfirmed ? 10 : 0;
            float overtime = Mathf.Max(0f, input.ElapsedSeconds - Mathf.Max(0f, input.TargetSeconds));
            float reductionRatio = overtimeUntilMinimumSeconds <= 0f
                ? 1f
                : Mathf.Clamp01(overtime / overtimeUntilMinimumSeconds);
            int time = Mathf.RoundToInt(Mathf.Lerp(30f, Mathf.Clamp(minimumTimeScore, 1, 30), reductionRatio));
            return new ScoreResult(accuracy, time, safety);
        }
    }
}
