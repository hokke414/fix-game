using MRRobotRepair.Interaction; using NUnit.Framework; using UnityEngine;
namespace MRRobotRepair.Tests { public sealed class ToolControllerTests {
GameObject go; ToolController t; [SetUp]public void SetUp(){go=new("tool");t=go.AddComponent<ToolController>();t.Configure("driver","screw");}[TearDown]public void Down()=>Object.DestroyImmediate(go);
[Test]public void StoresId()=>Assert.AreEqual("driver",t.ToolId);
[Test]public void CorrectTargetSucceeds()=>Assert.IsTrue(t.TryUse("screw"));
[Test]public void WrongTargetFails()=>Assert.IsFalse(t.TryUse("wire"));
[Test]public void WrongTargetFiresEvent(){bool fired=false;t.InvalidUse+=_=>fired=true;t.TryUse("wire");Assert.IsTrue(fired);}
}}
