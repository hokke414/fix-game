using MRRobotRepair.UI; using NUnit.Framework;
namespace MRRobotRepair.Tests { public sealed class GuidanceHintTimerTests {
[Test]public void TriggersAt30Seconds(){var t=new GuidanceHintTimer();Assert.IsFalse(t.Tick(29));Assert.IsTrue(t.Tick(1));Assert.IsFalse(t.Tick(1));}
[Test]public void ResetAllowsSecondHint(){var t=new GuidanceHintTimer();t.Tick(30);t.Reset();Assert.IsTrue(t.Tick(30));}
}}
