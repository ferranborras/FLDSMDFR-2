using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action
{
    public abstract float GetGoalChange(Goal goal);

    public abstract float GetDuration();

    public abstract WorldModel ApplyAction(WorldModel worldModel);

    public abstract void CalculateValues(WorldModel worldModel, UnidadModel unidad);

    public abstract void ApplyInGame(GameManager gameManager);
}

public class NothingAction : Action
{
    public override float GetGoalChange(Goal goal) {
        return 0;
    }

    public override float GetDuration() {
        return 0;
    }

    public override WorldModel ApplyAction(WorldModel worldModel) {
        return worldModel.Clone();
    }

    public override void CalculateValues(WorldModel worldModel, UnidadModel unidad) {
        // Nothing to add
    }

    public override void ApplyInGame(GameManager gameManager = null) {
        // Nothing to add
    }
}

public class DestroyAction : Action
{
    private UnidadModel target;
    private UnidadModel actor;

    public override float GetGoalChange(Goal goal) {
        if (goal is DestroyGoal) return -DestroyGoal.value;
        return 0;
    }

    public override float GetDuration() {
        return 0;
    }

    public override WorldModel ApplyAction(WorldModel worldModel) {
        WorldModel newModel = worldModel.Clone();
        
        if (target != null) {
            ((TropaWorldModel) newModel).hasTarget = true;
            bool isDead = newModel.map[target.coord.x, target.coord.y].unidad.GetDamage(actor.damage);

            if (isDead) {
                newModel.map[target.coord.x, target.coord.y].unidad = null;
            }
        }

        return (WorldModel) newModel;
    }

    public override void CalculateValues(WorldModel worldModel, UnidadModel unidad) {
        actor = unidad;
        Vector2Int[] rangeCoords = unidad.GetRangeCoords(unidad.coord);
        UnidadModel bestTarget = null;

        foreach (Vector2Int coord in rangeCoords) {
            VertexModel current = worldModel.map[coord.x, coord.y];
            if (current != null) {
                if (current.unidad != null) {
                    if (current.unidad.tipo == UnidadModel.UnidadType.edificio
                    && current.unidad.isP1) {
                        if (bestTarget == null)
                            bestTarget = current.unidad;
                        else if (current.unidad.currentHealth < bestTarget.currentHealth)
                            bestTarget = current.unidad;
                    }
                }
            }
        }

        target = bestTarget;
    }

    public override void ApplyInGame(GameManager gameManager) {
        gameManager.vertexSelected = gameManager.mapGenerator.map[actor.coord.x, actor.coord.y];
        gameManager.AttackUnidad(target.coord);
    }
}

public class AttackAction : Action 
{
    private UnidadModel target;
    private UnidadModel actor;

    public override float GetGoalChange(Goal goal) {
        if (goal is AttackGoal) return -AttackGoal.value;
        return 0;
    }

    public override float GetDuration() {
        return 0;
    }

    public override WorldModel ApplyAction(WorldModel worldModel) {
        WorldModel newModel = worldModel.Clone();
        
        if (target != null) {
            ((TropaWorldModel) newModel).hasTarget = true;
            bool isDead = newModel.map[target.coord.x, target.coord.y].unidad.GetDamage(actor.damage);

            if (isDead) {
                newModel.map[target.coord.x, target.coord.y].unidad = null;
            }
        }

        return (WorldModel) newModel;
    }

    public override void CalculateValues(WorldModel worldModel, UnidadModel unidad) {
        actor = unidad;
        Vector2Int[] rangeCoords = unidad.GetRangeCoords(unidad.coord);
        UnidadModel bestTarget = null;

        foreach (Vector2Int coord in rangeCoords) {
            VertexModel current = worldModel.map[coord.x, coord.y];
            if (current != null) {
                if (current.unidad != null) {
                    if (current.unidad.tipo != UnidadModel.UnidadType.edificio
                    && current.unidad.isP1) {
                        if (bestTarget == null)
                            bestTarget = current.unidad;
                        else if (current.unidad.currentHealth < bestTarget.currentHealth)
                            bestTarget = current.unidad;
                    }
                }
            }
        }

        target = bestTarget;
    }

    public override void ApplyInGame(GameManager gameManager) {
        gameManager.vertexSelected = gameManager.mapGenerator.map[actor.coord.x, actor.coord.y];
        Debug.Log("11vertexSelected.unidad == null: " + (gameManager.vertexSelected.unidad == null));
        gameManager.AttackUnidad(target.coord);
        Debug.Log("11vertexSelected.unidad == null: " + (gameManager.vertexSelected.unidad == null));
        Debug.Log("11vertexSelected: " + gameManager.vertexSelected.coord);
    }
}

public class BuildAction : Action
{
    private int iterations = 3;
    private Vector2Int target;

    public override float GetGoalChange(Goal goal) {
        if (goal is BuildGoal) return -BuildGoal.value;
        if (goal is GenerateGoal) return -GenerateGoal.value;
        if (goal is ExpandGoal) return -ExpandGoal.value;
        return 0;
    }

    public override float GetDuration() {
        return 0;
    }

    public override WorldModel ApplyAction(WorldModel worldModel) {
        WorldModel newModel = worldModel.Clone();

        newModel.money -= Edificio.cost;

        UnidadModel newUnidad = new UnidadModel();
        newUnidad.GenerateFromType(typeof(Edificio), target, false);

        newModel.map[target.x, target.y].unidad = newUnidad;

        Vector2Int[] rangeCoords = newUnidad.GetExtremmeCoords(target);

        foreach (Vector2Int coord in rangeCoords) {
            if (newModel.map[coord.x, coord.y] != null) {
                newModel.map[coord.x, coord.y].suelo = newModel.map[coord.x, coord.y].suelo != 1 ? 2 : 1;
            }
        }

        newModel.GetActions();

        return newModel;
    }

    public override void CalculateValues(WorldModel worldModel, UnidadModel unidad) {
        // --------------------- BUILD MAP --------------------- //
        InfluenceMap buildMap = new InfluenceMap();
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (worldModel.map[c, r] != null) {
                    if (worldModel.map[c, r].unidad != null) {
                        if (worldModel.map[c, r].unidad.tipo == UnidadModel.UnidadType.edificio && !worldModel.map[c, r].unidad.isP1)
                            buildMap.SetInfluence(new Vector2Int(c, r), -1);
                    }
                    else if (worldModel.map[c, r].suelo != 2) {
                        buildMap.SetInfluence(new Vector2Int(c, r), 1);
                    }
                }
            }
        }

        for (int i = 0; i < iterations; i++) {
            buildMap.PropagateInfluence(true);
        }

        float maxValue = -2;
        Vector2Int bestCoord = Vector2Int.zero;
        
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (worldModel.map[c, r] != null) {
                    if (worldModel.map[c, r].unidad == null
                    && !worldModel.map[c, r].isGhost
                    && worldModel.map[c, r].suelo == 2
                    && buildMap.map[c, r] > maxValue) {
                        maxValue = buildMap.map[c, r];
                        bestCoord = new Vector2Int(c, r);
                    }
                }
            }
        }
        target = bestCoord;
    }

    public override void ApplyInGame(GameManager gameManager) {
        gameManager.unidadSelected = Globals.p1IsCarbonara ?
                                    gameManager.supremeTeam.structurePrefab :
                                    gameManager.carbonaraTeam.structurePrefab;
        gameManager.ActionBuy(target);
    }
}

public class MoveAction : Action
{
    private Vector2Int target;
    private int iterations = 3;
    private UnidadModel actor;

    public override float GetGoalChange(Goal goal) {
        if (goal is FollowTargetGoal) return -FollowTargetGoal.value;
        return 0;
    }

    public override float GetDuration() {
        return 0;
    }

    public Vector2Int GetTarget() {
        return target;
    }

    public override void CalculateValues(WorldModel worldModel, UnidadModel unidad) {
        actor = unidad;

        // --------------------- BUILDING MAP --------------------- //
        InfluenceMap buildingMap = new InfluenceMap();
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (worldModel.map[c, r] != null) {
                    if (worldModel.map[c, r].unidad != null) {
                        if (worldModel.map[c, r].unidad.tipo == UnidadModel.UnidadType.edificio && worldModel.map[c, r].unidad.isP1)
                            buildingMap.SetInfluence(new Vector2Int(c, r), 1);
                        else if (worldModel.map[c, r].unidad.isP1)
                            buildingMap.SetInfluence(new Vector2Int(c, r), -1);
                    }
                }
            }
        }

        // --------------------- TROPA MAP --------------------- //
        InfluenceMap tropaMap = new InfluenceMap();
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (worldModel.map[c, r] != null) {
                    if (worldModel.map[c, r].unidad != null) {
                        if (worldModel.map[c, r].unidad.tipo != UnidadModel.UnidadType.edificio && worldModel.map[c, r].unidad.isP1) {
                            // Extremos
                            Vector2Int[] extremmeCoords = unidad.GetExtremmeCoords(new Vector2Int(c, r));
                            foreach (Vector2Int ec in extremmeCoords) {
                                if (worldModel.map[ec.x, ec.y] != null) {
                                    if (worldModel.map[ec.x, ec.y].unidad != null) {
                                        if (worldModel.map[ec.x, ec.y].unidad.tipo == UnidadModel.UnidadType.edificio || !worldModel.map[ec.x, ec.y].unidad.isP1)
                                            buildingMap.SetInfluence(new Vector2Int(ec.x, ec.y), 1);
                                    }
                                    else
                                        buildingMap.SetInfluence(new Vector2Int(ec.x, ec.y), 1);
                                }
                            }

                            // Tropa
                            buildingMap.SetInfluence(new Vector2Int(c, r), -1);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < iterations; i++) {
            buildingMap.PropagateInfluence(true);
            tropaMap.PropagateInfluence(true);
        }

        float maxValue = -2;
        Vector2Int bestCoord = unidad.coord;
        Vector2Int[] rangeCoords = unidad.GetRangeCoords(unidad.coord);
        foreach (Vector2Int coord in rangeCoords) {
            if (worldModel.map[coord.x, coord.y] != null) {
                if (worldModel.map[coord.x, coord.y].unidad == null && !worldModel.map[coord.x, coord.y].isGhost
                && Mathf.Max(buildingMap.map[coord.x, coord.y], tropaMap.map[coord.x, coord.y]) > maxValue) {
                    maxValue = Mathf.Max(buildingMap.map[coord.x, coord.y], tropaMap.map[coord.x, coord.y]);
                    bestCoord = coord;
                }
            }
        }

        target = bestCoord;
    }

    public override WorldModel ApplyAction(WorldModel worldModel) {
        WorldModel newModel = worldModel.Clone();

        newModel.map[actor.coord.x, actor.coord.y].isGhost = true;
        newModel.map[actor.coord.x, actor.coord.y].unidad = null;
        newModel.map[target.x, target.y].unidad = actor;

        ((TropaWorldModel) newModel).hasMoved = true;

        return newModel;
    }

    public override void ApplyInGame(GameManager gameManager) {
        gameManager.vertexSelected = gameManager.mapGenerator.map[actor.coord.x, actor.coord.y];
        gameManager.MoveUnidad(target);
        gameManager.mapGenerator.map[target.x, target.y].unidad = gameManager.vertexSelected.unidad;
    }
}

public class GenerateSoldierAction : Action
{
    private int iterations = 3;
    private Vector2Int target;

    public override float GetGoalChange(Goal goal) {
        if (goal is GenerateGoal) return -GenerateGoal.value;
        if (goal is AttackGoal) return -AttackGoal.value*2/3f;
        if (goal is DestroyGoal) return -DestroyGoal.value*2/3f;
        if (goal is GenerateSoldierGoal) return -GenerateSoldierGoal.value;
        return 0;
    }

    public override float GetDuration() {
        return 0;
    }

    public override WorldModel ApplyAction(WorldModel worldModel) {
        WorldModel newModel = worldModel.Clone();

        newModel.money -= Soldado.cost;

        UnidadModel newUnidad = new UnidadModel();
        newUnidad.GenerateFromType(typeof(Soldado), target, false);

        newModel.map[target.x, target.y].unidad = newUnidad;

        return newModel;
    }

    public override void CalculateValues(WorldModel worldModel, UnidadModel unidad) {
        // --------------------- TROPA MAP --------------------- //
        InfluenceMap tropaMap = new InfluenceMap();
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (worldModel.map[c, r] != null) {
                    if (worldModel.map[c, r].unidad != null) {
                        if (worldModel.map[c, r].unidad.tipo != UnidadModel.UnidadType.edificio && !worldModel.map[c, r].unidad.isP1)
                            tropaMap.SetInfluence(new Vector2Int(c, r), 1);
                        else if (worldModel.map[c, r].unidad.tipo != UnidadModel.UnidadType.edificio)
                            tropaMap.SetInfluence(new Vector2Int(c, r), -1);
                    }
                }
            }
        }

        for (int i = 0; i < iterations; i++) {
            tropaMap.PropagateInfluence(true);
        }

        float maxValue = -2;
        Vector2Int bestCoord = Vector2Int.zero;
        
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (worldModel.map[c, r] != null) {
                    if (worldModel.map[c, r].unidad == null
                    && !worldModel.map[c, r].isGhost
                    && worldModel.map[c, r].suelo == 2
                    && tropaMap.map[c, r] > maxValue) {
                        maxValue = tropaMap.map[c, r];
                        bestCoord = new Vector2Int(c, r);
                    }
                }
            }
        }

        target = bestCoord;
    }

    public override void ApplyInGame(GameManager gameManager) {
        gameManager.unidadSelected = Globals.p1IsCarbonara ?
                                    gameManager.supremeTeam.soldierPrefab :
                                    gameManager.carbonaraTeam.soldierPrefab;
        gameManager.ActionBuy(target);
    }
}

public class GenerateArcherAction : Action
{
    private int iterations = 3;
    private Vector2Int target;

    public override float GetGoalChange(Goal goal) {
        if (goal is GenerateGoal) return -GenerateGoal.value;
        if (goal is AttackGoal) return -AttackGoal.value/3f;
        if (goal is DestroyGoal) return -DestroyGoal.value;
        if (goal is GenerateArcherGoal) return -GenerateArcherGoal.value;
        return 0;
    }

    public override float GetDuration() {
        return 0;
    }

    public override WorldModel ApplyAction(WorldModel worldModel) {
        WorldModel newModel = worldModel.Clone();

        newModel.money -= Arquero.cost;

        UnidadModel newUnidad = new UnidadModel();
        newUnidad.GenerateFromType(typeof(Arquero), target, false);

        newModel.map[target.x, target.y].unidad = newUnidad;

        return newModel;
    }

    public override void CalculateValues(WorldModel worldModel, UnidadModel unidad) {
        // --------------------- TROPA MAP --------------------- //
        InfluenceMap tropaMap = new InfluenceMap();
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (worldModel.map[c, r] != null) {
                    if (worldModel.map[c, r].unidad != null) {
                        if (worldModel.map[c, r].unidad.tipo != UnidadModel.UnidadType.edificio && !worldModel.map[c, r].unidad.isP1)
                            tropaMap.SetInfluence(new Vector2Int(c, r), 1);
                        else if (worldModel.map[c, r].unidad.tipo != UnidadModel.UnidadType.edificio)
                            tropaMap.SetInfluence(new Vector2Int(c, r), -1);
                    }
                }
            }
        }

        for (int i = 0; i < iterations; i++) {
            tropaMap.PropagateInfluence(true);
        }

        float maxValue = -2;
        Vector2Int bestCoord = Vector2Int.zero;
        
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (worldModel.map[c, r] != null) {
                    if (worldModel.map[c, r].unidad == null
                    && !worldModel.map[c, r].isGhost
                    && worldModel.map[c, r].suelo == 2
                    && tropaMap.map[c, r] > maxValue) {
                        maxValue = tropaMap.map[c, r];
                        bestCoord = new Vector2Int(c, r);
                    }
                }
            }
        }

        target = bestCoord;
    }

    public override void ApplyInGame(GameManager gameManager) {
        gameManager.unidadSelected = Globals.p1IsCarbonara ?
                                    gameManager.supremeTeam.archerPrefab :
                                    gameManager.carbonaraTeam.archerPrefab;
        gameManager.ActionBuy(target);
    }
}

public class GenerateTankAction : Action
{
    private int iterations = 3;
    private Vector2Int target;

    public override float GetGoalChange(Goal goal) {
        if (goal is GenerateGoal) return -GenerateGoal.value;
        if (goal is AttackGoal) return -AttackGoal.value;
        if (goal is DestroyGoal) return -DestroyGoal.value/3f;
        if (goal is GenerateTankGoal) return -GenerateTankGoal.value;
        return 0;
    }

    public override float GetDuration() {
        return 0;
    }

    public override WorldModel ApplyAction(WorldModel worldModel) {
        WorldModel newModel = worldModel.Clone();

        newModel.money -= Tanque.cost;

        UnidadModel newUnidad = new UnidadModel();
        newUnidad.GenerateFromType(typeof(Tanque), target, false);

        newModel.map[target.x, target.y].unidad = newUnidad;

        return newModel;
    }

    public override void CalculateValues(WorldModel worldModel, UnidadModel unidad) {
        // --------------------- TROPA MAP --------------------- //
        InfluenceMap tropaMap = new InfluenceMap();
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (worldModel.map[c, r] != null) {
                    if (worldModel.map[c, r].unidad != null) {
                        if (worldModel.map[c, r].unidad.tipo != UnidadModel.UnidadType.edificio && !worldModel.map[c, r].unidad.isP1)
                            tropaMap.SetInfluence(new Vector2Int(c, r), 1);
                        else if (worldModel.map[c, r].unidad.tipo != UnidadModel.UnidadType.edificio)
                            tropaMap.SetInfluence(new Vector2Int(c, r), -1);
                    }
                }
            }
        }

        for (int i = 0; i < iterations; i++) {
            tropaMap.PropagateInfluence(true);
        }

        float maxValue = -2;
        Vector2Int bestCoord = Vector2Int.zero;
        
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (worldModel.map[c, r] != null) {
                    if (worldModel.map[c, r].unidad == null
                    && !worldModel.map[c, r].isGhost
                    && worldModel.map[c, r].suelo == 2
                    && tropaMap.map[c, r] > maxValue) {
                        maxValue = tropaMap.map[c, r];
                        bestCoord = new Vector2Int(c, r);
                    }
                }
            }
        }

        target = bestCoord;
    }

    public override void ApplyInGame(GameManager gameManager) {
        gameManager.unidadSelected = Globals.p1IsCarbonara ?
                                    gameManager.supremeTeam.tankPrefab :
                                    gameManager.carbonaraTeam.tankPrefab;
        gameManager.ActionBuy(target);
    }
}