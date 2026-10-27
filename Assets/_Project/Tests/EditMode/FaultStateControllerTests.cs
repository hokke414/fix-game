using MRRobotRepair.Repair; using NUnit.Framework; using UnityEngine;
namespace MRRobotRepair.Tests { public sealed class FaultStateControllerTests {
GameObject go; FaultStateController f; [SetUp]public void SetUp(){go=new("fault");f=go.AddComponent<FaultStateController>();f.Configure("wire");}[TearDown]public void Down()=>Object.DestroyImmediate(go);
[Test]public void CorrectRepairSucceeds(){Assert.IsTrue(f.TryRepair("wire"));Assert.IsTrue(f.IsRepaired);}
[Test]public void WrongRepairFails(){Assert.IsFalse(f.TryRepair("fuse"));Assert.IsFalse(f.IsRepaired);}
[Test]public void ResetClearsRepair(){f.TryRepair("wire");f.ResetFault();Assert.IsFalse(f.IsRepaired);}
}}
