using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unidad : MonoBehaviour
{
    [HideInInspector] public bool isP1;
    [HideInInspector] public float health;
    [HideInInspector] public Queue<Vector3> path = new Queue<Vector3>();
    [HideInInspector] public Vector2Int coord;
    [HideInInspector] public Vector2Int origin;

    public HealthBar healthBar;
    
    public float speed;

    public virtual void Start() {

    }

    public virtual Vector2Int[] GetRangeCoords(Vector2Int center, int range = 0) {
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
        Vector2Int[] coordsArray = coords.ToArray();
        PrintV2IntArray(coordsArray);
        return coordsArray;
    }

    public void PrintV2IntArray(Vector2Int[] array) {
        if (array.Length == 0) {
            print("[]");
            return;
        }

        string result = "[" + array[0];
        for (int i = 1; i < array.Length; i++) {
            result += ", " + array[i];
        }
        result += "]";
        print(result);
    }

    public virtual int Cost() {
        return 0;
    }

    public virtual float Damage() {
        return 0;
    }

    public virtual int DeathEarnings() {
        return 0;
    }

    public virtual float MaxHealth() {
        return 0;
    }

    public virtual int Range() {
        return 0;
    }

    public virtual void Initialize(Vector2Int cell, bool p1Turn) {
        this.isP1 = p1Turn;
        this.speed = 5;
        this.coord = cell;
        this.origin = cell;
        this.path = new Queue<Vector3>();
        Place(cell);
    }

    public virtual void Place(Vector2Int cell) {
        transform.position = new Vector3(cell.x+.5f, cell.y+.5f, 0);
    }

    public virtual void Update() {
        if (path.Count > 0) {
            transform.position = Vector3.MoveTowards(transform.position, path.Peek(), speed * Time.deltaTime);
            if (transform.position == path.Peek())
                path.Dequeue();
        }
    }

    public virtual bool GetDamage(float healthLeft) {
        print("HEALTH LEFT: " + healthLeft);
        return healthLeft <= 0;
    }

}
