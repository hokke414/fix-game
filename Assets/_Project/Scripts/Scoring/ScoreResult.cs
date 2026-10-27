namespace MRRobotRepair.Scoring
{
    /// <summary>カテゴリ別および合計スコア。</summary>
    public readonly struct ScoreResult
    {
        public ScoreResult(int accuracy, int time, int safety)
        {
            AccuracyScore = accuracy;
            TimeScore = time;
            SafetyScore = safety;
        }

        public int AccuracyScore { get; }
        public int TimeScore { get; }
        public int SafetyScore { get; }
        public int TotalScore => AccuracyScore + TimeScore + SafetyScore;
    }
}
