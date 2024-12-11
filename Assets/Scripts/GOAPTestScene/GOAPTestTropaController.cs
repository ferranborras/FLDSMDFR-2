using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPTestTropaController : MonoBehaviour
{
    [SerializeField] private TropaAI tropaAi;

    //[SerializeField] private bool canAttack;

    [SerializeField] private bool edificioInRange;
    [SerializeField] private bool tropaInRange;

    // Start is called before the first frame update
    /*void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MakeAction() {
        Action action = tropaAi.ChooseAction();
        print(gameObject.name + ": " + action);
        print("Ready");
    }

    public void ResetActions() {
        List<Goal> goalsList = new List<Goal>();
        List<Action> actionsList = new List<Action>();

        goalsList.Add(new AttackGoal());
        goalsList.Add(new DestroyGoal());
        goalsList.Add(new FollowTargetGoal());

        actionsList.Add(new NothingAction());
        if (tropaInRange) actionsList.Add(new AttackAction());
        if (edificioInRange) actionsList.Add(new DestroyAction());
        actionsList.Add(new MoveAction());

        tropaAi.goals = goalsList.ToArray();
        tropaAi.actions = actionsList.ToArray();
    }*/
}
