using TreatmentPlanCombiner.Models;

namespace TreatmentPlanCombiner.Services;

public class ValidationService
{
    private const IssueSeverity BlockingSeverity = IssueSeverity.Error;

    public List<ValidationIssue> Validate(
        List<TreatmentPlanRecord> treatmentPlans,
        List<ProblemRecord> problems,
        List<GoalRecord> goals,
        List<ObjectiveRecord> objectives,
        List<InterventionRecord> interventions)
    {
        var issues = new List<ValidationIssue>();

        var treatmentPlanIds = treatmentPlans
            .Select(x => x.Id)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(IdNormalizer.Normalize)
            .ToHashSet();

        var problemIds = problems
            .Select(x => x.ProblemId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(IdNormalizer.Normalize)
            .ToHashSet();

        var goalIds = goals
            .Select(x => x.GoalId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(IdNormalizer.Normalize)
            .ToHashSet();

        var objectiveIds = objectives
            .Select(x => x.ObjectiveId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(IdNormalizer.Normalize)
            .ToHashSet();

        ValidateTreatmentPlans(treatmentPlans, problems, issues);
        ValidateProblems(problems, goals, treatmentPlanIds, issues);
        ValidateGoals(goals, objectives, problemIds, issues);
        ValidateObjectives(objectives, interventions, goalIds, issues);
        ValidateInterventions(interventions, objectiveIds, issues);

        ValidateDuplicates(
            treatmentPlans.Select(x => x.Id),
            IssueType.DuplicateTreatmentPlanId,
            "Duplicate treatment plan Id found.",
            issues);

        ValidateDuplicates(
            problems.Select(x => x.ProblemId),
            IssueType.DuplicateProblemId,
            "Duplicate ProblemId found.",
            issues);

        ValidateDuplicates(
            goals.Select(x => x.GoalId),
            IssueType.DuplicateGoalId,
            "Duplicate GoalId found.",
            issues);

        ValidateDuplicates(
            objectives.Select(x => x.ObjectiveId),
            IssueType.DuplicateObjectiveId,
            "Duplicate ObjectiveId found.",
            issues);

        ValidateDuplicates(
            interventions.Select(x => x.InterventionId),
            IssueType.DuplicateInterventionId,
            "Duplicate InterventionId found.",
            issues);

        return issues;
    }

    public bool HasBlockingIssues(List<ValidationIssue> issues)
    {
        return issues.Any(x => x.Severity == BlockingSeverity);
    }

    private void ValidateTreatmentPlans(
        List<TreatmentPlanRecord> treatmentPlans,
        List<ProblemRecord> problems,
        List<ValidationIssue> issues)
    {
        foreach (var plan in treatmentPlans)
        {
            if (string.IsNullOrWhiteSpace(plan.Id))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.MissingTreatmentPlanId,
                    plan.Id,
                    string.Empty,
                    "Treatment plan is missing Id.");
            }

            if (string.IsNullOrWhiteSpace(plan.PatientId))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.MissingPatientId,
                    plan.Id,
                    string.Empty,
                    "Treatment plan is missing PatientId.");
            }

            var hasProblems = problems.Any(x => IdsMatch(x.Id, plan.Id));

            if (!hasProblems)
            {
                AddIssue(
                    issues,
                    IssueSeverity.Warning,
                    IssueType.TreatmentPlanHasNoProblems,
                    plan.Id,
                    string.Empty,
                    "Treatment plan has no related problems.");
            }
        }
    }

    private static void ValidateProblems(
        List<ProblemRecord> problems,
        List<GoalRecord> goals,
        HashSet<string> treatmentPlanIds,
        List<ValidationIssue> issues)
    {
        foreach (var problem in problems)
        {
            if (string.IsNullOrWhiteSpace(problem.ProblemId))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.MissingProblemId,
                    problem.Id,
                    problem.ProblemId,
                    "Problem is missing ProblemId.");
            }

            if (!treatmentPlanIds.Contains(IdNormalizer.Normalize(problem.Id)))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.OrphanProblem,
                    problem.Id,
                    problem.ProblemId,
                    $"Problem references missing treatment plan Id '{problem.Id}'.");
            }

            if (string.IsNullOrWhiteSpace(problem.PlanFieldProblemNarrative))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Warning,
                    IssueType.MissingProblemNarrative,
                    problem.Id,
                    problem.ProblemId,
                    "Problem narrative is blank.");
            }

            var hasGoals = goals.Any(x => IdsMatch(x.ProblemId, problem.ProblemId));

            if (!hasGoals)
            {
                AddIssue(
                    issues,
                    IssueSeverity.Warning,
                    IssueType.ProblemHasNoGoals,
                    problem.Id,
                    problem.ProblemId,
                    "Problem has no related goals.");
            }
        }
    }

    private static void ValidateGoals(
        List<GoalRecord> goals,
        List<ObjectiveRecord> objectives,
        HashSet<string> problemIds,
        List<ValidationIssue> issues)
    {
        foreach (var goal in goals)
        {
            if (string.IsNullOrWhiteSpace(goal.GoalId))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.MissingGoalId,
                    goal.Id,
                    goal.GoalId,
                    "Goal is missing GoalId.");
            }

            if (string.IsNullOrWhiteSpace(goal.ProblemId))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.MissingProblemIdOnGoal,
                    goal.Id,
                    goal.GoalId,
                    "Goal is missing ProblemId.");
            }
            else if (!problemIds.Contains(IdNormalizer.Normalize(goal.ProblemId)))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.OrphanGoal,
                    goal.Id,
                    goal.GoalId,
                    $"Goal references missing ProblemId '{goal.ProblemId}'.");
            }

            if (string.IsNullOrWhiteSpace(goal.PlanFieldGoalNarrative))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Warning,
                    IssueType.MissingGoalNarrative,
                    goal.Id,
                    goal.GoalId,
                    "Goal narrative is blank.");
            }

            var hasObjectives = objectives.Any(x => IdsMatch(x.GoalId, goal.GoalId));

            if (!hasObjectives)
            {
                AddIssue(
                    issues,
                    IssueSeverity.Warning,
                    IssueType.GoalHasNoObjectives,
                    goal.Id,
                    goal.GoalId,
                    "Goal has no related objectives.");
            }
        }
    }

    private static void ValidateObjectives(
        List<ObjectiveRecord> objectives,
        List<InterventionRecord> interventions,
        HashSet<string> goalIds,
        List<ValidationIssue> issues)
    {
        foreach (var objective in objectives)
        {
            if (string.IsNullOrWhiteSpace(objective.ObjectiveId))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.MissingObjectiveId,
                    objective.Id,
                    objective.ObjectiveId,
                    "Objective is missing ObjectiveId.");
            }

            if (string.IsNullOrWhiteSpace(objective.GoalId))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.MissingGoalIdOnObjective,
                    objective.Id,
                    objective.ObjectiveId,
                    "Objective is missing GoalId.");
            }
            else if (!goalIds.Contains(IdNormalizer.Normalize(objective.GoalId)))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.OrphanObjective,
                    objective.Id,
                    objective.ObjectiveId,
                    $"Objective references missing GoalId '{objective.GoalId}'.");
            }

            if (string.IsNullOrWhiteSpace(objective.PlanFieldObjectiveNarrative))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Warning,
                    IssueType.MissingObjectiveNarrative,
                    objective.Id,
                    objective.ObjectiveId,
                    "Objective narrative is blank.");
            }

            var hasInterventions = interventions.Any(x => IdsMatch(x.ObjectiveId, objective.ObjectiveId));

            if (!hasInterventions)
            {
                AddIssue(
                    issues,
                    IssueSeverity.Warning,
                    IssueType.ObjectiveHasNoInterventions,
                    objective.Id,
                    objective.ObjectiveId,
                    "Objective has no related interventions.");
            }
        }
    }

    private static void ValidateInterventions(
        List<InterventionRecord> interventions,
        HashSet<string> objectiveIds,
        List<ValidationIssue> issues)
    {
        foreach (var intervention in interventions)
        {
            if (string.IsNullOrWhiteSpace(intervention.InterventionId))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.MissingInterventionId,
                    intervention.Id,
                    intervention.InterventionId,
                    "Intervention is missing InterventionId.");
            }

            if (string.IsNullOrWhiteSpace(intervention.ObjectiveId))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.MissingObjectiveIdOnIntervention,
                    intervention.Id,
                    intervention.InterventionId,
                    "Intervention is missing ObjectiveId.");
            }
            else if (!objectiveIds.Contains(IdNormalizer.Normalize(intervention.ObjectiveId)))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Error,
                    IssueType.OrphanIntervention,
                    intervention.Id,
                    intervention.InterventionId,
                    $"Intervention references missing ObjectiveId '{intervention.ObjectiveId}'.");
            }

            if (string.IsNullOrWhiteSpace(intervention.PlanFieldIntvNarrative))
            {
                AddIssue(
                    issues,
                    IssueSeverity.Warning,
                    IssueType.MissingInterventionNarrative,
                    intervention.Id,
                    intervention.InterventionId,
                    "Intervention narrative is blank.");
            }
        }
    }

    private static void ValidateDuplicates(
        IEnumerable<string> ids,
        IssueType issueType,
        string message,
        List<ValidationIssue> issues)
    {
        var duplicateIds = ids
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .GroupBy(IdNormalizer.Normalize)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicateId in duplicateIds)
        {
            AddIssue(
                issues,
                IssueSeverity.Error,
                issueType,
                string.Empty,
                duplicateId,
                $"{message} Duplicate value: '{duplicateId}'.");
        }
    }

    private static bool IdsMatch(string left, string right)
    {
        return IdNormalizer.Normalize(left) == IdNormalizer.Normalize(right);
    }

    private static void AddIssue(
        List<ValidationIssue> issues,
        IssueSeverity severity,
        IssueType issueType,
        string treatmentPlanId,
        string recordId,
        string message)
    {
        issues.Add(new ValidationIssue
        {
            Severity = severity,
            IssueType = issueType,
            TreatmentPlanId = treatmentPlanId,
            RecordId = recordId,
            Message = message
        });
    }
}