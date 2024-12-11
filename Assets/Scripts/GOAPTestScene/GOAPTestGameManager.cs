using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPTestGameManager : MonoBehaviour
{
    [SerializeField] private GeneratorAI generatorAi;

    [SerializeField] private bool canBuild;
    [SerializeField] private bool canAttack;
    [SerializeField] private bool canGenerateSoldier;
    [SerializeField] private bool canGenerateArcher;
    [SerializeField] private bool canGenerateTank;

    [SerializeField] private int buildingsDifference;
    [SerializeField] private int tropasDifference;
    [SerializeField] private UnidadMinima unidadMinima;

    [SerializeField] private GameObject tropaPrefab;
    private List<GameObject> tropas = new List<GameObject>();

    enum UnidadMinima {
        soldado,
        arquero,
        tanque,
        edificio,
        none
    }

    // Start is called before the first frame update
    /*void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextAction() {
        ResetActions();

        Action action = generatorAi.ChooseAction();
        print("GeneratorAI: " + action);
        print("Ready");

        if (action is GenerateSoldierAction || action is GenerateArcherAction || action is GenerateTankAction) {
            GameObject tropa = Instantiate(tropaPrefab, new Vector3(Random.Range(-5f, 5f), Random.Range(-4f, 4f), 0), Quaternion.identity) as GameObject;
            tropas.Add(tropa);
        }

        GOAPTestTropaController tropaController;
        foreach (GameObject tropa in tropas) {
            tropaController = tropa.GetComponent<GOAPTestTropaController>();
            tropaController.ResetActions();
            tropaController.MakeAction();
        }
    }

    private void ResetActions() {
        List<Goal> goalsList = new List<Goal>();
        List<Action> actionsList = new List<Action>();

        //goalsList.Add(new NothingGoal());
        goalsList.Add(new GenerateGoal());
        if (tropasDifference < 0) goalsList.Add(new AttackGoal());
        if (buildingsDifference < 0) goalsList.Add(new DestroyGoal());
        if (buildingsDifference < 0 || unidadMinima == UnidadMinima.edificio) goalsList.Add(new BuildGoal());
        if (unidadMinima == UnidadMinima.soldado) goalsList.Add(new GenerateSoldierGoal());
        else if (unidadMinima == UnidadMinima.arquero) goalsList.Add(new GenerateArcherGoal());
        else if (unidadMinima == UnidadMinima.tanque) goalsList.Add(new GenerateTankGoal());
        //goalsList.Add(new ExpandGoal());

        actionsList.Add(new NothingAction());
        if (canGenerateSoldier) actionsList.Add(new GenerateSoldierAction());
        if (canGenerateArcher) actionsList.Add(new GenerateArcherAction());
        if (canGenerateTank) actionsList.Add(new GenerateTankAction());
        if (canBuild) actionsList.Add(new BuildAction());

        generatorAi.goals = goalsList.ToArray();
        generatorAi.actions = actionsList.ToArray();
    }*/
}
