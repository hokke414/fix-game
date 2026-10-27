using MRRobotRepair.Core; using NUnit.Framework; using UnityEngine;
namespace MRRobotRepair.Tests { public sealed class WorkspacePlacementControllerTests {
GameObject go; WorkspacePlacementController c; [SetUp]public void SetUp(){go=new("workspace");c=go.AddComponent<WorkspacePlacementController>();}[TearDown]public void Down()=>Object.DestroyImmediate(go);
[Test]public void InitialStateIsNotPlaced()=>Assert.IsFalse(c.IsPlaced);
[Test]public void ConfirmSetsPlaced(){Assert.IsTrue(c.ConfirmPlacement(true));Assert.IsTrue(c.IsPlaced);}
[Test]public void SecondConfirmFails(){c.ConfirmPlacement(true);Assert.IsFalse(c.ConfirmPlacement(true));}
[Test]public void ResetClearsPlaced(){c.ConfirmPlacement(true);c.ResetPlacement();Assert.IsFalse(c.IsPlaced);}
[Test]public void SetPoseUpdatesTransform(){c.SetPositionAndRotation(Vector3.one,Quaternion.Euler(0,90,0));Assert.AreEqual(Vector3.one,c.Position);Assert.Less(Quaternion.Angle(Quaternion.Euler(0,90,0),c.Rotation),.001f);}
[Test]public void InitializesInFront(){Assert.IsTrue(c.InitializeInFrontOf(new Vector3(2,1.7f,-3),new Vector3(0,-.5f,1),.8f));Assert.AreEqual(.8f,c.Position.y,.0001f);Assert.AreEqual(-1.75f,c.Position.z,.0001f);}
[Test]public void RejectsVerticalForward(){c.SetPositionAndRotation(Vector3.one,Quaternion.identity);Assert.IsFalse(c.InitializeInFrontOf(Vector3.zero,Vector3.up,.8f));Assert.AreEqual(Vector3.one,c.Position);}
[Test]public void AppliesMovementHeightYaw(){Assert.IsTrue(c.AdjustPlacement(Vector2.right,1,1,Vector3.forward,1));Assert.AreEqual(.5f,c.Position.x,.0001f);Assert.AreEqual(.35f,c.Position.y,.0001f);Assert.AreEqual(60f,c.Rotation.eulerAngles.y,.001f);}
[Test]public void RejectsAdjustAfterConfirm(){c.ConfirmPlacement(true);Assert.IsFalse(c.AdjustPlacement(Vector2.up,1,1,Vector3.forward,1));}
[Test]public void RejectsOutOfReach(){Assert.IsFalse(c.ConfirmPlacement(false));Assert.IsFalse(c.IsPlaced);}
[Test]public void BeginAdjustmentUnlocks(){c.ConfirmPlacement(true);Assert.IsTrue(c.BeginAdjustment());Assert.IsFalse(c.IsPlaced);}
}}
