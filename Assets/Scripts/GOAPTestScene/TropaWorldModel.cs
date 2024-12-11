using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TropaWorldModel : WorldModel
{
    public UnidadModel unidad = null;
    public bool hasTarget = false;
    public bool hasMoved = false;

    public TropaWorldModel(Unidad u = null) {
        if (u != null) {
            this.unidad = new UnidadModel();
            this.unidad.GenerateFromUnidad(u);
        }
    }

    public override void GetGoals() {
        //Debug.Log("GetGoals executed2");
        List<Goal> goalsList = new List<Goal> {
            new DestroyGoal(),
            new AttackGoal(),
            new FollowTargetGoal()
        };
        goals = goalsList.ToArray();
    }

    public override void GetActions() {
        actions = new Queue<Action>();
        if (EdificioInRange(unidad) && !hasTarget) actions.Enqueue(new DestroyAction());
        if (TropaInRange(unidad) && !hasTarget) actions.Enqueue(new AttackAction());
        if (!hasTarget && !hasMoved) actions.Enqueue(new MoveAction());
        actions.Enqueue(new NothingAction());
    }

    public void GenerateFromMap(Vertex[,] vertexMap, int currentMoney = 0) {
        base.GenerateFromMap(vertexMap);

        hasTarget = false;
        hasMoved = false;
        GetGoals();
        GetActions();
    }

    public override WorldModel Clone() {
        TropaWorldModel newModel = new TropaWorldModel();

        newModel.map = new VertexModel[Globals.size, Globals.size];

        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                // ---------------- CREATE MODEL OF VERTEX ---------------- //
                if (map[c, r] != null) {
                    newModel.map[c, r] = map[c, r].Clone();
                }
            }
        }

        newModel.discontentment = discontentment;
        newModel.money = money;

        newModel.unidad = unidad.Clone();
        newModel.hasTarget = hasTarget;
        newModel.hasMoved = hasMoved;
        newModel.GetGoals();
        newModel.GetActions();

        return (WorldModel) newModel;
    }

    public override float CalculateDiscontentment() {
        return base.CalculateDiscontentment();
    }

    public override Action NextAction() {
        return base.NextAction();
    }

    public override WorldModel ApplyAction(Action action) {
        return base.ApplyAction(action);
    }

    public bool TropaInRange(UnidadModel unidad) {
        Vector2Int[] rangeCoords = unidad.GetRangeCoords(unidad.coord);

        foreach (Vector2Int coord in rangeCoords) {
            if (map[coord.x, coord.y] != null) {
                if (map[coord.x, coord.y].unidad != null) {
                    if (map[coord.x, coord.y].unidad.tipo != UnidadModel.UnidadType.edificio
                    && map[coord.x, coord.y].unidad.isP1) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public bool EdificioInRange(UnidadModel unidad) {
        if (unidad == null)
            Debug.Log("Unidad == null");
        Vector2Int[] rangeCoords = unidad.GetRangeCoords(unidad.coord);

        foreach (Vector2Int coord in rangeCoords) {
            if (map[coord.x, coord.y] != null) {
                if (map[coord.x, coord.y].unidad != null) {
                    if (map[coord.x, coord.y].unidad.tipo == UnidadModel.UnidadType.edificio
                    && map[coord.x, coord.y].unidad.isP1) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
