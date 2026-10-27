using MRRobotRepair.Repair; using NUnit.Framework; using UnityEngine;
namespace MRRobotRepair.Tests { public sealed class RepairFlowControllerTests {
GameObject go; RepairFlowController c; RepairStepDefinition a,b; [SetUp]public void SetUp(){go=new("flow");c=go.AddComponent<RepairFlowController>();a=ScriptableObject.CreateInstance<RepairStepDefinition>();b=ScriptableObject.CreateInstance<RepairStepDefinition>();a.Initialize("a","A");b.Initialize("b","B",new[]{"a"});}[TearDown]public void Down(){Object.DestroyImmediate(a);Object.DestroyImmediate(b);Object.DestroyImmediate(go);}
[Test]public void CompletesInOrder(){Assert.IsTrue(c.Configure(new[]{a,b}));Assert.IsTrue(c.TryCompleteStep("a",RepairCompletionCondition.Interaction));Assert.AreEqual("b",c.CurrentStep.Id);}
[Test]public void RejectsOutOfOrder(){c.Configure(new[]{a,b});Assert.IsFalse(c.TryCompleteStep("b",RepairCompletionCondition.Interaction));}
[Test]public void RejectsInvalidPrerequisite(){a.Initialize("a","A",new[]{"missing"});Assert.IsFalse(c.Configure(new[]{a}));}
}}
