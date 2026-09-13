using TreatmentPlanCombiner.Models;
using TreatmentPlanCombiner.Services;

namespace TreatmentPlanCombiner.Tests;

public class ValidationServiceTests
{
    private readonly ValidationService _validationService = new();

    [Fact]
    public void ValidData_ProducesNoIssues()
    {
        var data = CreateValidData();

        var issues = Validate(data);

        Assert.Empty(issues);
    }

    [Fact]
    public void MissingTreatmentPlanId_IsBlocking()
    {
        var data = CreateValidData();
        data.Plans[0].Id = string.Empty;

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.MissingTreatmentPlanId);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void MissingPatientId_IsBlocking()
    {
        var data = CreateValidData();
        data.Plans[0].PatientId = string.Empty;

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.MissingPatientId);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void MissingProblemId_IsBlocking()
    {
        var data = CreateValidData();
        data.Problems[0].ProblemId = string.Empty;

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.MissingProblemId);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void MissingInterventionId_IsBlocking()
    {
        var data = CreateValidData();
        data.Interventions[0].InterventionId = string.Empty;

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.MissingInterventionId);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void OrphanProblem_IsBlocking()
    {
        var data = CreateValidData();
        data.Problems[0].Id = "TP-999";

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.OrphanProblem);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void OrphanGoal_IsBlocking()
    {
        var data = CreateValidData();
        data.Goals[0].ProblemId = "PRB-999";

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.OrphanGoal);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void OrphanObjective_IsBlocking()
    {
        var data = CreateValidData();
        data.Objectives[0].GoalId = "G-999";

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.OrphanObjective);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void OrphanIntervention_IsBlocking()
    {
        var data = CreateValidData();
        data.Interventions[0].ObjectiveId = "OBJ-999";

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.OrphanIntervention);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void DuplicateTreatmentPlanId_IsBlocking()
    {
        var data = CreateValidData();
        data.Plans.Add(new TreatmentPlanRecord { Id = "TP-1", PatientId = "PAT-9" });

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.DuplicateTreatmentPlanId);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void DuplicateProblemId_IsBlocking()
    {
        var data = CreateValidData();
        data.Problems.Add(new ProblemRecord { Id = "TP-1", ProblemId = "PRB-1", PlanFieldProblemNarrative = "Narrative" });

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.DuplicateProblemId);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void DuplicateGoalId_IsBlocking()
    {
        var data = CreateValidData();
        data.Goals.Add(new GoalRecord { Id = "TP-1", ProblemId = "PRB-1", GoalId = "G-1", PlanFieldGoalNarrative = "Narrative" });

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.DuplicateGoalId);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void DuplicateObjectiveId_IsBlocking()
    {
        var data = CreateValidData();
        data.Objectives.Add(new ObjectiveRecord { Id = "TP-1", GoalId = "G-1", ObjectiveId = "OBJ-1", PlanFieldObjectiveNarrative = "Narrative" });

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.DuplicateObjectiveId);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void DuplicateInterventionId_IsBlocking()
    {
        var data = CreateValidData();
        data.Interventions.Add(new InterventionRecord { Id = "TP-1", ObjectiveId = "OBJ-1", InterventionId = "INT-1", PlanFieldIntvNarrative = "Narrative" });

        var issues = Validate(data);

        var issue = Assert.Single(
            issues,
            x => x.IssueType == IssueType.DuplicateInterventionId);
        Assert.Equal(IssueSeverity.Error, issue.Severity);
    }

    [Fact]
    public void WarningsOnly_AreNotBlocking()
    {
        var data = CreateValidData();
        data.Problems[0].PlanFieldProblemNarrative = string.Empty;
        data.Goals[0].PlanFieldGoalNarrative = string.Empty;

        var issues = Validate(data);

        Assert.Contains(issues, x => x.IssueType == IssueType.MissingProblemNarrative);
        Assert.Contains(issues, x => x.IssueType == IssueType.MissingGoalNarrative);
        Assert.All(issues, x => Assert.Equal(IssueSeverity.Warning, x.Severity));
        Assert.False(_validationService.HasBlockingIssues(issues));
    }

    [Fact]
    public void TreatmentPlanWithoutProblems_ProducesWarning()
    {
        var data = CreateValidData();
        data.Plans[0].Id = "TP-9";

        var issues = Validate(data);

        Assert.Contains(
            issues,
            x => x.IssueType == IssueType.TreatmentPlanHasNoProblems
                && x.Severity == IssueSeverity.Warning);
    }

    [Fact]
    public void HasBlockingIssues_True_WhenErrorsPresent()
    {
        var data = CreateValidData();
        data.Plans[0].Id = string.Empty;

        var issues = Validate(data);

        Assert.True(_validationService.HasBlockingIssues(issues));
    }

    private static List<ValidationIssue> Validate(TestData data)
    {
        return new ValidationService().Validate(
            data.Plans,
            data.Problems,
            data.Goals,
            data.Objectives,
            data.Interventions);
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
                    PlanFieldProblemNarrative = "Problem narrative"
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
                    PlanFieldGoalNarrative = "Goal narrative"
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
                    PlanFieldObjectiveNarrative = "Objective narrative"
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
                    PlanFieldIntvNarrative = "Intervention narrative"
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