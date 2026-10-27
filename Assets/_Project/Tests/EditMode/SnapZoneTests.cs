using MRRobotRepair.Interaction; using NUnit.Framework; using UnityEngine;
namespace MRRobotRepair.Tests { public sealed class SnapZoneTests {
GameObject root,part,target; SnapZone zone; [SetUp]public void SetUp(){root=new("zone");part=new("part");target=new("target");zone=root.AddComponent<SnapZone>();zone.Configure("p",target.transform,.1f,10);}[TearDown]public void Down(){Object.DestroyImmediate(root);Object.DestroyImmediate(part);Object.DestroyImmediate(target);}
[Test]public void AcceptsMatchingPart(){Assert.IsTrue(zone.CanSnap("p",part.transform));}
[Test]public void RejectsWrongId(){Assert.IsFalse(zone.CanSnap("x",part.transform));}
[Test]public void RejectsFarPart(){part.transform.position=Vector3.one;Assert.IsFalse(zone.CanSnap("p",part.transform));}
[Test]public void SnapMovesPart(){target.transform.position=Vector3.right;part.transform.position=new Vector3(.95f,0,0);Assert.IsTrue(zone.TrySnap("p",part.transform));Assert.AreEqual(Vector3.right,part.transform.position);}
}}
