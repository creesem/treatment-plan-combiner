using TreatmentPlanCombiner.Models;

namespace TreatmentPlanCombiner.Services;

public class HierarchyBuilder
{
    public List<TreatmentPlanHierarchy> Build(
        List<TreatmentPlanRecord>? treatmentPlans,
        List<ProblemRecord>? problems,
        List<GoalRecord>? goals,
        List<ObjectiveRecord>? objectives,
        List<InterventionRecord>? interventions)
    {
        var plans = treatmentPlans ?? new List<TreatmentPlanRecord>();
        var problemRecords = problems ?? new List<ProblemRecord>();
        var goalRecords = goals ?? new List<GoalRecord>();
        var objectiveRecords = objectives ?? new List<ObjectiveRecord>();
        var interventionRecords = interventions ?? new List<InterventionRecord>();

        var problemsByPlanId = problemRecords
            .GroupBy(x => IdNormalizer.Normalize(x.Id))
            .ToDictionary(g => g.Key, g => g.ToList());

        var goalsByKey = goalRecords
            .GroupBy(x => BuildKey(IdNormalizer.Normalize(x.Id), IdNormalizer.Normalize(x.ProblemId)))
            .ToDictionary(g => g.Key, g => g.ToList());

        var objectivesByKey = objectiveRecords
            .GroupBy(x => BuildKey(IdNormalizer.Normalize(x.Id), IdNormalizer.Normalize(x.GoalId)))
            .ToDictionary(g => g.Key, g => g.ToList());

        var interventionsByKey = interventionRecords
            .GroupBy(x => BuildKey(IdNormalizer.Normalize(x.Id), IdNormalizer.Normalize(x.ObjectiveId)))
            .ToDictionary(g => g.Key, g => g.ToList());

        var hierarchies = new List<TreatmentPlanHierarchy>();

        foreach (var plan in plans)
        {
            var planKey = IdNormalizer.Normalize(plan.Id);

            var hierarchy = new TreatmentPlanHierarchy
            {
                TreatmentPlanId = plan.Id,
                PatientId = plan.PatientId
            };

            if (problemsByPlanId.TryGetValue(planKey, out var planProblems))
            {
                foreach (var problemRecord in planProblems)
                {
                    hierarchy.Problems.Add(BuildProblem(
                        problemRecord,
                        planKey,
                        goalsByKey,
                        objectivesByKey,
                        interventionsByKey));
                }
            }

            hierarchies.Add(hierarchy);
        }

        return hierarchies;
    }

    private static Problem BuildProblem(
        ProblemRecord problemRecord,
        string planKey,
        Dictionary<string, List<GoalRecord>> goalsByKey,
        Dictionary<string, List<ObjectiveRecord>> objectivesByKey,
        Dictionary<string, List<InterventionRecord>> interventionsByKey)
    {
        var problem = new Problem
        {
            ProblemId = problemRecord.ProblemId,
            Narrative = problemRecord.PlanFieldProblemNarrative,
            Status = problemRecord.PlanFieldProblemStatus
        };

        var problemKey = IdNormalizer.Normalize(problemRecord.ProblemId);

        if (goalsByKey.TryGetValue(BuildKey(planKey, problemKey), out var problemGoals))
        {
            foreach (var goalRecord in problemGoals)
            {
                problem.Goals.Add(BuildGoal(
                    goalRecord,
                    planKey,
                    objectivesByKey,
                    interventionsByKey));
            }
        }

        return problem;
    }

    private static Goal BuildGoal(
        GoalRecord goalRecord,
        string planKey,
        Dictionary<string, List<ObjectiveRecord>> objectivesByKey,
        Dictionary<string, List<InterventionRecord>> interventionsByKey)
    {
        var goal = new Goal
        {
            GoalId = goalRecord.GoalId,
            Narrative = goalRecord.PlanFieldGoalNarrative,
            Status = goalRecord.PlanFieldGoalStatus
        };

        var goalKey = IdNormalizer.Normalize(goalRecord.GoalId);

        if (objectivesByKey.TryGetValue(BuildKey(planKey, goalKey), out var goalObjectives))
        {
            foreach (var objectiveRecord in goalObjectives)
            {
                goal.Objectives.Add(BuildObjective(
                    objectiveRecord,
                    planKey,
                    interventionsByKey));
            }
        }

        return goal;
    }

    private static Objective BuildObjective(
        ObjectiveRecord objectiveRecord,
        string planKey,
        Dictionary<string, List<InterventionRecord>> interventionsByKey)
    {
        var objective = new Objective
        {
            ObjectiveId = objectiveRecord.ObjectiveId,
            Narrative = objectiveRecord.PlanFieldObjectiveNarrative,
            Status = objectiveRecord.PlanFieldObjectiveStatus
        };

        var objectiveKey = IdNormalizer.Normalize(objectiveRecord.ObjectiveId);

        if (interventionsByKey.TryGetValue(BuildKey(planKey, objectiveKey), out var objectiveInterventions))
        {
            foreach (var interventionRecord in objectiveInterventions)
            {
                objective.Interventions.Add(new Intervention
                {
                    InterventionId = interventionRecord.InterventionId,
                    Narrative = interventionRecord.PlanFieldIntvNarrative,
                    Status = interventionRecord.PlanFieldIntvStatus,
                    Priority = interventionRecord.PlanFieldIntvPriority
                });
            }
        }

        return objective;
    }

    private static string BuildKey(string planId, string childId)
    {
        return $"{planId}|{childId}";
    }
}