using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edificio : Unidad
{
    [HideInInspector] public static int range { get; set; } = 2;
    [HideInInspector] public static int cost { get; set; } = 5;
    [HideInInspector] public static float maxHealth { get; set; } = 200;
    [HideInInspector] public static int deathEarnings { get; set; } = 3;

    public override void Place(Vector2Int cell) {
        base.Place(cell);
    }

    public override void Initialize(Vector2Int cell, bool p1Turn) {
        this.health         = Edificio.maxHealth;
        base.Initialize(cell, p1Turn);
    }

    public override void Start() {
        base.Start();
    }

    public override int Cost() {
        return Edificio.cost;
    }

    public override int DeathEarnings() {
        return Edificio.deathEarnings;
    }

    public override float MaxHealth() {
        return Edificio.maxHealth;
    }

    public override int Range() {
        return Edificio.range;
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

    public override float Damage() {
        return base.Damage();
    }
}
