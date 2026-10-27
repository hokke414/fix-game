using MRRobotRepair.MR; using NUnit.Framework; using UnityEngine;
namespace MRRobotRepair.Tests { public sealed class WorkspaceReachabilityCheckerTests {
GameObject root,left,right; BoxCollider bounds; WorkspaceReachabilityChecker c; [SetUp]public void SetUp(){root=new("reach");bounds=root.AddComponent<BoxCollider>();bounds.size=new(.8f,.8f,.5f);left=new("left");right=new("right");c=root.AddComponent<WorkspaceReachabilityChecker>();c.Configure(bounds,left.transform,right.transform);}[TearDown]public void Down(){Object.DestroyImmediate(root);Object.DestroyImmediate(left);Object.DestroyImmediate(right);}
[Test]public void TouchingIsReachable(){left.transform.position=new(.4f,0,0);right.transform.position=Vector3.one*5;Assert.IsTrue(c.IsWithinReach());}
[Test]public void FarIsNotReachable(){left.transform.position=Vector3.one*2;right.transform.position=Vector3.one*-2;Assert.IsFalse(c.IsWithinReach());}
[Test]public void DisabledBoundsIsNotReachable(){bounds.enabled=false;Assert.IsFalse(c.IsWithinReach());}
}}
