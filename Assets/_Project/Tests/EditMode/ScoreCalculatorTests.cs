using MRRobotRepair.Scoring; using NUnit.Framework;
namespace MRRobotRepair.Tests { public sealed class ScoreCalculatorTests {
[Test]public void PerfectIs100()=>Assert.AreEqual(100,ScoreCalculator.Calculate(new(1,100,100,true)).TotalScore);
[Test]public void HalfAccuracyGets30()=>Assert.AreEqual(30,ScoreCalculator.Calculate(new(.5f,100,100,false)).AccuracyScore);
[Test]public void OvertimeKeepsMinimum()=>Assert.AreEqual(6,ScoreCalculator.Calculate(new(1,500,100,true)).TimeScore);
[Test]public void MissingSafetyGetsZero()=>Assert.AreEqual(0,ScoreCalculator.Calculate(new(1,100,100,false)).SafetyScore);
}}
