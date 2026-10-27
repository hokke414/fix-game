using System.Collections.Generic;

namespace MRRobotRepair.Scoring
{
    /// <summary>同じ誤操作の短時間連打を重複計上しない。</summary>
    public sealed class MistakeTracker
    {
        private readonly float suppressionSeconds;
        private readonly Dictionary<string, float> lastRecordedTimes = new();

        public MistakeTracker(float suppressionSeconds = 1.5f)
        {
            this.suppressionSeconds = suppressionSeconds < 0f ? 0f : suppressionSeconds;
        }

        public int Count { get; private set; }

        /// <summary>抑制期間外なら誤操作を記録する。</summary>
        public bool TryRecord(string mistakeId, float timestampSeconds)
        {
            if (string.IsNullOrWhiteSpace(mistakeId))
            {
                return false;
            }

            if (lastRecordedTimes.TryGetValue(mistakeId, out float previous)
                && timestampSeconds - previous < suppressionSeconds)
            {
                return false;
            }

            lastRecordedTimes[mistakeId] = timestampSeconds;
            Count++;
            return true;
        }

        /// <summary>誤操作記録を初期化する。</summary>
        public void Reset()
        {
            lastRecordedTimes.Clear();
            Count = 0;
        }
    }
}
