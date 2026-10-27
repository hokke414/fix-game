namespace MRRobotRepair.Repair
{
    /// <summary>修理工程を完了とみなす操作種別。</summary>
    public enum RepairCompletionCondition
    {
        Interaction,
        ToolUse,
        PartInstalled,
        StateConfirmed,
    }
}
