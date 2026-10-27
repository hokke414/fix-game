namespace MRRobotRepair.Core
{
    /// <summary>ゲーム全体の進行状態。</summary>
    public enum GameState
    {
        Boot,
        SafetyCheck,
        WorkspacePlacement,
        Tutorial,
        RepairWire,
        RepairFuse,
        RepairBoard,
        FinalTest,
        Result,
    }
}
