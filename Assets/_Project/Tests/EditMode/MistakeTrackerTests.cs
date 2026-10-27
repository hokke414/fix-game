using MRRobotRepair.Scoring; using NUnit.Framework;
namespace MRRobotRepair.Tests { public sealed class MistakeTrackerTests {
[Test]public void SuppressesRapidDuplicate(){var t=new MistakeTracker();Assert.IsTrue(t.TryRecord("x",0));Assert.IsFalse(t.TryRecord("x",1));Assert.AreEqual(1,t.Count);}
[Test]public void ResetClearsCount(){var t=new MistakeTracker();t.TryRecord("x",0);t.Reset();Assert.AreEqual(0,t.Count);}
}}
