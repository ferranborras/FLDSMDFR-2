using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class AI : MonoBehaviour
{
    public virtual Action ChooseAction(Action[] actions, Goal[] goals) {
        Action bestAction = actions[0];
        float bestValue = CalculateDiscontentment(actions[0], goals); 
        foreach (Action action in actions) {
            float thisValue = CalculateDiscontentment(action, goals);
            if (thisValue < bestValue) {
                bestValue = thisValue;
                bestAction = action;
            }
        }
        return bestAction;
    }

    public virtual float CalculateDiscontentment(Action action, Goal[] goals) {
        float discontentment = 0;
        foreach (Goal goal in goals) {
            float newValue = goal.GetValue() + action.GetGoalChange(goal);
            //newValue += action.GetDuration() * goal.GetChange();
            discontentment += goal.GetDiscontentment(newValue);
        }
        print("Action: " + action + ".\nDiscontentment: " + discontentment);
        return discontentment;
    }

    public virtual Action[] PlanActions(WorldModel worldModel, int maxDepth, UnidadModel unidad = null) {
        WorldModel[] models = new WorldModel[maxDepth+1];

        models[0] = worldModel.Clone();
        //models[0].GetActions();
        int currentDepth = 0;

        Stack<Action> bestActionSequence = new Stack<Action>();
        float bestValue = Mathf.Infinity;

        Stack<Action> currentActionSequence = new Stack<Action>();
        float[] depthCurrentValues = new float[maxDepth+1];

        while (currentDepth >= 0) {
            
            depthCurrentValues[currentDepth] = models[currentDepth].CalculateDiscontentment();
            depthCurrentValues[maxDepth] = 0;
            
            if (currentDepth >= maxDepth) {
                float totalValue = 0;
                foreach (float value in depthCurrentValues)
                    totalValue += value;
                
                if (totalValue < bestValue) {
                    bestValue = totalValue;

                    bestActionSequence = new Stack<Action>(currentActionSequence);
                    PrintActionsStack(bestActionSequence);
                }
                
                currentDepth --;
                if (currentActionSequence.Count > 0)
                    currentActionSequence.Pop();
                continue;
            }

            Action nextAction = models[currentDepth].NextAction();
            if (nextAction != null) {
                nextAction.CalculateValues(models[currentDepth], unidad);

                models[currentDepth+1] = models[currentDepth].ApplyAction(nextAction);
            
                models[currentDepth+1].GetActions();
                currentActionSequence.Push(nextAction);

                currentDepth++;
            }
            else {
                currentDepth--;
                if (currentActionSequence.Count > 0)
                    currentActionSequence.Pop();
            }
        }
        Action[] bestActionSequenceArray = bestActionSequence.ToArray();
        return bestActionSequenceArray;
    }

    public void PrintActionsStack(Stack<Action> array) {
        string r = "";
        Action[] actionsArray = array.ToArray();
        //Array.Reverse(actionsArray);

        foreach (Action action in actionsArray)
            r += action + ", ";

        //Debug.Log(r);
    }
}
