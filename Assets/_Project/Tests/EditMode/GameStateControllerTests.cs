using MRRobotRepair.Core; using NUnit.Framework; using UnityEngine;
namespace MRRobotRepair.Tests { public sealed class GameStateControllerTests {
GameObject go; GameStateController c; [SetUp] public void SetUp(){go=new("state");c=go.AddComponent<GameStateController>();}[TearDown]public void Down()=>Object.DestroyImmediate(go);
[Test]public void StartsAtBoot()=>Assert.AreEqual(GameState.Boot,c.CurrentState);
[Test]public void AdvanceFollowsOrder(){Assert.IsTrue(c.TryAdvance());Assert.AreEqual(GameState.SafetyCheck,c.CurrentState);}
[Test]public void RestartReturnsToSafety(){c.TryAdvance();c.TryAdvance();c.Restart();Assert.AreEqual(GameState.SafetyCheck,c.CurrentState);}
}}
