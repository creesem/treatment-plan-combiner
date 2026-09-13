namespace TreatmentPlanCombiner.Models;

public enum IssueType
{
    None = 0,
    MissingTreatmentPlanId,
    MissingPatientId,
    TreatmentPlanHasNoProblems,
    MissingProblemId,
    OrphanProblem,
    MissingProblemNarrative,
    ProblemHasNoGoals,
    MissingGoalId,
    MissingProblemIdOnGoal,
    OrphanGoal,
    MissingGoalNarrative,
    GoalHasNoObjectives,
    MissingObjectiveId,
    MissingGoalIdOnObjective,
    OrphanObjective,
    MissingObjectiveNarrative,
    ObjectiveHasNoInterventions,
    MissingInterventionId,
    MissingObjectiveIdOnIntervention,
    OrphanIntervention,
    MissingInterventionNarrative,
    DuplicateTreatmentPlanId,
    DuplicateProblemId,
    DuplicateGoalId,
    DuplicateObjectiveId,
    DuplicateInterventionId,
    MalformedCsvRow
}