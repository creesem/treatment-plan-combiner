using TreatmentPlanCombiner.Models;
using TreatmentPlanCombiner.Services;

namespace TreatmentPlanCombiner.Tests;

public class HierarchyBuilderTests
{
    private readonly HierarchyBuilder _builder = new();

    [Fact]
    public void BuildsCompleteHierarchy()
    {
        var data = CreateValidData();

        var hierarchies = _builder.Build(
            data.Plans,
            data.Problems,
            data.Goals,
            data.Objectives,
            data.Interventions);

        var plan = Assert.Single(hierarchies);
        Assert.Equal("TP-1", plan.TreatmentPlanId);
        Assert.Equal("PAT-1", plan.PatientId);

        var problem = Assert.Single(plan.Problems);
        Assert.Equal("PRB-1", problem.ProblemId);

        var goal = Assert.Single(problem.Goals);
        Assert.Equal("G-1", goal.GoalId);

        var objective = Assert.Single(goal.Objectives);
        Assert.Equal("OBJ-1", objective.ObjectiveId);

        var intervention = Assert.Single(objective.Interventions);
        Assert.Equal("INT-1", intervention.InterventionId);
        Assert.Equal("Intervention narrative", intervention.Narrative);
    }

    [Fact]
    public void KeepsRecordsFromSeparatePlansSeparated()
    {
        var data = CreateValidData();
        data.Plans.Add(new TreatmentPlanRecord { Id = "TP-2", PatientId = "PAT-2" });
        data.Problems.Add(new ProblemRecord
        {
            Id = "TP-2",
            PatientId = "PAT-2",
            ProblemId = "PRB-2",
            PlanFieldProblemNarrative = "Second narrative"
        });

        var hierarchies = _builder.Build(
            data.Plans,
            data.Problems,
            data.Goals,
            data.Objectives,
            data.Interventions);

        Assert.Equal(2, hierarchies.Count);

        var secondPlan = hierarchies[1];
        Assert.Equal("TP-2", secondPlan.TreatmentPlanId);

        var problem = Assert.Single(secondPlan.Problems);
        Assert.Equal("PRB-2", problem.ProblemId);
        Assert.Empty(problem.Goals);
    }

    [Fact]
    public void HandlesPlansWithoutChildren()
    {
        var data = CreateValidData();
        data.Plans.Add(new TreatmentPlanRecord { Id = "TP-EMPTY", PatientId = "PAT-9" });

        var hierarchies = _builder.Build(
            data.Plans,
            data.Problems,
            data.Goals,
            data.Objectives,
            data.Interventions);

        Assert.Equal(2, hierarchies.Count);

        var emptyPlan = hierarchies[1];
        Assert.Equal("TP-EMPTY", emptyPlan.TreatmentPlanId);
        Assert.Empty(emptyPlan.Problems);
    }

    [Fact]
    public void HandlesEmptyInputCollections()
    {
        var hierarchies = _builder.Build(
            new List<TreatmentPlanRecord>(),
            new List<ProblemRecord>(),
            new List<GoalRecord>(),
            new List<ObjectiveRecord>(),
            new List<InterventionRecord>());

        Assert.Empty(hierarchies);
    }

    [Fact]
    public void HandlesNullInputCollections()
    {
        var hierarchies = _builder.Build(null, null, null, null, null);

        Assert.Empty(hierarchies);
    }

    [Fact]
    public void AttachesChildrenToCorrectParent()
    {
        var data = CreateValidData();
        data.Goals.Add(new GoalRecord
        {
            Id = "TP-1",
            PatientId = "PAT-1",
            GoalId = "G-2",
            ProblemId = "PRB-1",
            PlanFieldGoalNarrative = "Second goal for same problem"
        });

        var hierarchies = _builder.Build(
            data.Plans,
            data.Problems,
            data.Goals,
            data.Objectives,
            data.Interventions);

        var plan = Assert.Single(hierarchies);
        var problem = Assert.Single(plan.Problems);

        Assert.Equal(2, problem.Goals.Count);
        Assert.Equal(new[] { "G-1", "G-2" }, problem.Goals.Select(g => g.GoalId));
    }

    private static TestData CreateValidData()
    {
        return new TestData
        {
            Plans = new List<TreatmentPlanRecord>
            {
                new() { Id = "TP-1", PatientId = "PAT-1" }
            },
            Problems = new List<ProblemRecord>
            {
                new()
                {
                    Id = "TP-1",
                    PatientId = "PAT-1",
                    ProblemId = "PRB-1",
                    PlanFieldProblemNarrative = "Problem narrative",
                    PlanFieldProblemStatus = "Active"
                }
            },
            Goals = new List<GoalRecord>
            {
                new()
                {
                    Id = "TP-1",
                    PatientId = "PAT-1",
                    GoalId = "G-1",
                    ProblemId = "PRB-1",
                    PlanFieldGoalNarrative = "Goal narrative",
                    PlanFieldGoalStatus = "In Progress"
                }
            },
            Objectives = new List<ObjectiveRecord>
            {
                new()
                {
                    Id = "TP-1",
                    PatientId = "PAT-1",
                    ObjectiveId = "OBJ-1",
                    GoalId = "G-1",
                    PlanFieldObjectiveNarrative = "Objective narrative",
                    PlanFieldObjectiveStatus = "In Progress"
                }
            },
            Interventions = new List<InterventionRecord>
            {
                new()
                {
                    Id = "TP-1",
                    PatientId = "PAT-1",
                    InterventionId = "INT-1",
                    ObjectiveId = "OBJ-1",
                    PlanFieldIntvNarrative = "Intervention narrative",
                    PlanFieldIntvPriority = "High",
                    PlanFieldIntvStatus = "Active"
                }
            }
        };
    }

    private sealed class TestData
    {
        public List<TreatmentPlanRecord> Plans { get; set; } = new();
        public List<ProblemRecord> Problems { get; set; } = new();
        public List<GoalRecord> Goals { get; set; } = new();
        public List<ObjectiveRecord> Objectives { get; set; } = new();
        public List<InterventionRecord> Interventions { get; set; } = new();
    }
}