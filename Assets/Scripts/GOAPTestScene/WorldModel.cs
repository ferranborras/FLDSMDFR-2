using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorldModel
{
    public VertexModel[,] map;
    public float discontentment = Mathf.Infinity;
    public int money = 0;
    public Queue<Action> actions = new Queue<Action>();

    public Goal[] goals = null;

    public virtual void GetGoals() {
        List<Goal> goalsList = new List<Goal> {
            new ExpandGoal(),
            new BuildGoal(),
            new DestroyGoal(),
            new AttackGoal(),
            new GenerateGoal(),
            new GenerateSoldierGoal(),
            new GenerateArcherGoal(),
            new GenerateTankGoal()
        };
        goals = goalsList.ToArray();
    }

    public virtual void GetActions() {
        actions = new Queue<Action>();
        if (ThereIsEmptyVertex()) {
            if (money >= Edificio.cost) actions.Enqueue(new BuildAction());
            int randomTropa = UnityEngine.Random.Range(0, 3);
            if (randomTropa == 0) {
                if (money >= Tanque.cost)
                    actions.Enqueue(new GenerateTankAction());
                else if (money >= Soldado.cost)
                    actions.Enqueue(new GenerateSoldierAction());
                else if (money >= Arquero.cost)
                    actions.Enqueue(new GenerateArcherAction());
            }
            else if (randomTropa == 1) {
                if (money >= Soldado.cost)
                    actions.Enqueue(new GenerateSoldierAction());
                else if (money >= Arquero.cost)
                    actions.Enqueue(new GenerateArcherAction());
            }
            else {
                if (money >= Arquero.cost)
                actions.Enqueue(new GenerateArcherAction());
            }
        }
        actions.Enqueue(new NothingAction());
    }

    public bool ThereIsEmptyVertex() {
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (map[c, r] != null) {
                    if (map[c, r].unidad == null && !map[c, r].isGhost && map[c, r].suelo == 2) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public virtual void GenerateFromMap(Vertex[,] vertexMap, int currentMoney = 0) {
        map = new VertexModel[Globals.size, Globals.size];

        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                // ---------------- CREATE MODEL OF VERTEX ---------------- //
                if (vertexMap[c, r] != null) {
                    VertexModel newVertex = new VertexModel();
                    newVertex.GenerateFromVertex(vertexMap[c, r]);
                    map[c, r] = newVertex;
                }
            }
        }

        discontentment = Mathf.Infinity;
        money = currentMoney;
        GetGoals();
        GetActions();
    }

    public virtual WorldModel Clone() {
        WorldModel newModel = new WorldModel();

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
        newModel.GetGoals();
        newModel.GetActions();

        return newModel;
    }

    public virtual float CalculateDiscontentment() {
        if (actions.Count <= 0)
            return discontentment;

        discontentment = 0;
        
        foreach (Goal goal in goals) {
            float newValue = goal.GetValue() + actions.Peek().GetGoalChange(goal);
            //newValue += action.GetDuration() * goal.GetChange();
            discontentment += goal.GetDiscontentment(newValue);
        }

        return discontentment;
    }

    public virtual Action NextAction() {
        if (actions.Count <= 0)
            return null;
        return actions.Dequeue();
    }

    public virtual WorldModel ApplyAction(Action action) {
        return action.ApplyAction(this);
    }
}

public class VertexModel
{
    public Vector2Int coord;
    public int suelo = 0;
    public UnidadModel unidad = null;
    public bool isGhost = false;

    public void GenerateFromVertex(Vertex v) {
        coord = v.coord;
        suelo = v.suelo;
        if (v.unidad != null) {
            unidad = new UnidadModel();
            unidad.GenerateFromUnidad(v.unidad.GetComponent<Unidad>());
        }
        else
            unidad = null;
        isGhost = false;
    }

    public VertexModel Clone() {
        VertexModel newModel = new VertexModel();

        newModel.coord = coord;
        newModel.suelo = suelo;
        if (unidad != null)
            newModel.unidad = unidad.Clone();
        else
            newModel.unidad = null;
        
        newModel.isGhost = isGhost;

        return newModel;
    }
}

public class UnidadModel
{
    public float maxHealth = 0;
    public float currentHealth = 0;
    public int range;
    public Vector2Int coord;
    public UnidadType tipo;
    public bool isP1;
    public float damage = 0;
    
    public enum UnidadType {
        soldado,
        arquero,
        tanque,
        edificio
    }

    public void GenerateFromType(Type unidad, Vector2Int target, bool p1) {
        if (unidad == typeof(Soldado)) {
            tipo = UnidadType.soldado;
            maxHealth = Soldado.maxHealth;
            currentHealth = maxHealth;
            range = Soldado.range;
            damage = Soldado.damage;
        }
        else if (unidad == typeof(Arquero)) {
            tipo = UnidadType.arquero;
            maxHealth = Arquero.maxHealth;
            currentHealth = maxHealth;
            range = Arquero.range;
            damage = Arquero.damage;
        }
        else if (unidad == typeof(Tanque)) {
            tipo = UnidadType.tanque;
            maxHealth = Tanque.maxHealth;
            currentHealth = maxHealth;
            range = Tanque.range;
            damage = Tanque.damage;
        }
        else if (unidad == typeof(Edificio)) {
            tipo = UnidadType.edificio;
            maxHealth = Edificio.maxHealth;
            currentHealth = maxHealth;
            range = Edificio.range;
            damage = 0;
        }
        coord = target;
        isP1 = p1;
    }

    public void GenerateFromUnidad(Unidad unidad) {
        switch (unidad) {
            case Soldado: tipo = UnidadType.soldado;
                break;
            case Arquero: tipo = UnidadType.arquero;
                break;
            case Tanque: tipo = UnidadType.tanque;
                break;
        }

        damage = unidad.Damage();
        maxHealth = unidad.MaxHealth();
        currentHealth = unidad.health;
        coord = unidad.coord;
        range = unidad.Range();
        isP1 = unidad.isP1;
    }

    public UnidadModel Clone() {
        UnidadModel newModel = new UnidadModel();
        newModel.tipo = tipo;
        newModel.maxHealth = maxHealth;
        newModel.currentHealth = currentHealth;
        newModel.coord = coord;
        newModel.range = range;
        newModel.isP1 = isP1;

        return newModel;
    }

    public bool GetDamage(float damage) {
        currentHealth -= damage;
        return currentHealth <= 0;
    }

    public bool InRange(Vector2Int target) {
        Vector2Int[] coords = GetRangeCoords(coord);

        foreach (Vector2Int c in coords) {
            if (target == c)
                return true;
        }

        return false;
    }

    public Vector2Int[] GetRangeCoords(Vector2Int center) {
        List<Vector2Int> coords = new List<Vector2Int>();

        for (int r = -range; r <= range; r++) {
            int maxColumnOffset = range - Mathf.Abs(r);
            for (int c = -maxColumnOffset; c <= maxColumnOffset; c++) {
                int newX = center.x + c;
                int newY = center.y + r;

                // Verificar limites del tablero
                if (newX >= 0 && newX < Globals.size && newY >= 0 && newY < Globals.size) {
                    coords.Add(new Vector2Int(newX, newY));
                }
            }
        }

        return coords.ToArray();
    }

    public Vector2Int[] GetExtremmeCoords(Vector2Int center) {
        List<Vector2Int> coords = new List<Vector2Int>();

        for (int r = -range; r <= range; r++) {
            int maxColumnOffset = range - Mathf.Abs(r);
            
            Vector2Int left = new Vector2Int(center.x + maxColumnOffset, center.y + r);
            Vector2Int right = new Vector2Int(center.x - maxColumnOffset, center.y + r);

            // Verificar limites del tablero
            if (left.x >= 0 && left.x < Globals.size && left.y >= 0 && left.y < Globals.size)
                coords.Add(left);

            if (right.x >= 0 && right.x < Globals.size && right.y >= 0 && right.y < Globals.size && Mathf.Abs(r) != range)
                coords.Add(right);
        }

        return coords.ToArray();
    }
}