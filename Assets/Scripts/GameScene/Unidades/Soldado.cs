using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldado : Unidad
{
    [HideInInspector] public static int range { get; set; } = 3;
    [HideInInspector] public static int cost { get; set; } = 3;
    [HideInInspector] public static float maxHealth { get; set; } = 50;
    [HideInInspector] public static int deathEarnings { get; set; } = 2;
    [HideInInspector] public static float damage { get; set; } = 20;

    private Animator animator;

    public override void Place(Vector2Int cell) {
        base.Place(cell);
    }

    public override void Initialize(Vector2Int cell, bool p1Turn) {
        this.health         = Soldado.maxHealth;
        base.Initialize(cell, p1Turn);
    }

    public override void Start() {
        animator = GetComponent<Animator>();
        base.Start();
    }

    public override int Cost() {
        return Soldado.cost;
    }

    public override float Damage() {
        return Soldado.damage;
    }

    public override int DeathEarnings() {
        return Soldado.deathEarnings;
    }

    public override float MaxHealth() {
        return Soldado.maxHealth;
    }

    public override int Range() {
        return Soldado.range;
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

    public override void PlayAttack() {
        animator.SetTrigger("Attack");
    }

    public override void PlayHit() {
        animator.SetTrigger("Hit");
    }
}
