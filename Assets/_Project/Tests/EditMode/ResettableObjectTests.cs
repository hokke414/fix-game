using MRRobotRepair.Interaction; using NUnit.Framework; using UnityEngine;
namespace MRRobotRepair.Tests { public sealed class ResettableObjectTests {
GameObject go; ResettableObject r; [SetUp]public void SetUp(){go=new("reset");r=go.AddComponent<ResettableObject>();r.RecordInitialTransform();}[TearDown]public void Down()=>Object.DestroyImmediate(go);
[Test]public void ResetRestoresPosition(){go.transform.position=Vector3.one;Assert.IsTrue(r.ResetObject());Assert.AreEqual(Vector3.zero,go.transform.position);}
[Test]public void ResetRestoresRotation(){go.transform.rotation=Quaternion.Euler(0,90,0);r.ResetObject();Assert.Less(Quaternion.Angle(Quaternion.identity,go.transform.rotation),.001f);}
[Test]public void RecordChangesOrigin(){go.transform.position=Vector3.one;r.RecordInitialTransform();go.transform.position=Vector3.zero;r.ResetObject();Assert.AreEqual(Vector3.one,go.transform.position);}
[Test]public void ResetFiresEvent(){bool fired=false;r.ResetCompleted+=()=>fired=true;r.ResetObject();Assert.IsTrue(fired);}
}}
