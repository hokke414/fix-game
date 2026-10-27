namespace MRRobotRepair.UI
{
    /// <summary>工程進行が30秒止まったとき、一度だけ追加ヒントを要求する。</summary>
    public sealed class GuidanceHintTimer
    {
        private readonly float delaySeconds;
        private float elapsedSeconds;
        private bool triggered;

        public GuidanceHintTimer(float delaySeconds = 30f)
        {
            this.delaySeconds = delaySeconds < 0f ? 0f : delaySeconds;
        }

        /// <summary>経過時間を進め、今回ヒントを表示すべき場合だけtrueを返す。</summary>
        public bool Tick(float deltaSeconds)
        {
            if (triggered || deltaSeconds <= 0f)
            {
                return false;
            }

            elapsedSeconds += deltaSeconds;
            if (elapsedSeconds < delaySeconds)
            {
                return false;
            }

            triggered = true;
            return true;
        }

        /// <summary>工程進行時に待ち時間と表示済み状態を初期化する。</summary>
        public void Reset()
        {
            elapsedSeconds = 0f;
            triggered = false;
        }
    }
}
