namespace MRRobotRepair.Scoring
{
    /// <summary>最終スコア計算へ渡す入力。</summary>
    public readonly struct ScoreInput
    {
        public ScoreInput(float accuracyRatio, float elapsedSeconds, float targetSeconds, bool safetyConfirmed)
        {
            AccuracyRatio = accuracyRatio;
            ElapsedSeconds = elapsedSeconds;
            TargetSeconds = targetSeconds;
            SafetyConfirmed = safetyConfirmed;
        }

        public float AccuracyRatio { get; }
        public float ElapsedSeconds { get; }
        public float TargetSeconds { get; }
        public bool SafetyConfirmed { get; }
    }
}
