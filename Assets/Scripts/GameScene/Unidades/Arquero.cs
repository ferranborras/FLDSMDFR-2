using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arquero : Unidad
{
    [HideInInspector] public static int range { get; set; } = 4;
    [HideInInspector] public static int cost { get; set; } = 2;
    [HideInInspector] public static float maxHealth { get; set; } = 25;
    [HideInInspector] public static int deathEarnings { get; set; } = 1;
    [HideInInspector] public static float damage { get; set; } = 10;

    public override void Place(Vector2Int cell) {
        base.Place(cell);
    }

    public override void Initialize(Vector2Int cell, bool p1Turn) {
        this.health         = Arquero.maxHealth;
        base.Initialize(cell, p1Turn);
    }

    public override void Start() {
        base.Start();
    }

    public override int Cost() {
        return Arquero.cost;
    }

    public override float Damage() {
        return Arquero.damage;
    }

    public override int DeathEarnings() {
        return Arquero.deathEarnings;
    }

    public override float MaxHealth() {
        return Arquero.maxHealth;
    }

    public override int Range() {
        return Arquero.range;
    }

    public override Vector2Int[] GetRangeCoords(Vector2Int center, int r = 0) {
        return base.GetRangeCoords(center, range);
    }

    public override void Update() {
        base.Update();
    }

    public override bool GetDamage(float damaged) {
        health -= damaged;

        if (!healthBar.gameObject.activeSelf)
            healthBar.Initialize();
        healthBar.UpdateHealth(health/maxHealth);
        
        return base.GetDamage(health);
    }
}
