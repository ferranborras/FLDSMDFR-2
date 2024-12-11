using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Goal
{
    public static float value { get; set; }

    public virtual float GetDiscontentment(float newValue) {
        return newValue * newValue;
    }

    public virtual float GetChange() {
        return 0;
    }

    public abstract float GetValue();
}

public class NothingGoal : Goal
{
    public static float value = 0;

    public override float GetDiscontentment(float newValue) {
        return base.GetDiscontentment(newValue);
    }

    public override float GetChange() {
        return base.GetChange();
    }

    public override float GetValue() {
        return value;
    }
}

public class DestroyGoal : Goal
{
    public static float value = 5;

    public override float GetDiscontentment(float newValue) {
        return base.GetDiscontentment(newValue);
    }

    public override float GetChange() {
        return base.GetChange();
    }

    public override float GetValue() {
        return value;
    }
}

public class AttackGoal : Goal
{
    public static float value = 4;

    public override float GetDiscontentment(float newValue) {
        return base.GetDiscontentment(newValue);
    }

    public override float GetChange() {
        return base.GetChange();
    }

    public override float GetValue() {
        return value;
    }
}

public class BuildGoal : Goal
{
    public static float value = 6;

    public override float GetDiscontentment(float newValue) {
        return base.GetDiscontentment(newValue);
    }

    public override float GetChange() {
        return base.GetChange();
    }

    public override float GetValue() {
        return value;
    }
}

public class GenerateGoal : Goal
{
    public static float value = 3;

    public override float GetDiscontentment(float newValue) {
        return base.GetDiscontentment(newValue);
    }

    public override float GetChange() {
        return base.GetChange();
    }

    public override float GetValue() {
        return value;
    }
}

public class GenerateSoldierGoal : Goal
{
    public static float value = 3;

    public override float GetDiscontentment(float newValue) {
        return base.GetDiscontentment(newValue);
    }

    public override float GetChange() {
        return base.GetChange();
    }

    public override float GetValue() {
        return value;
    }
}

public class GenerateArcherGoal : Goal
{
    public static float value = 3;

    public override float GetDiscontentment(float newValue) {
        return base.GetDiscontentment(newValue);
    }

    public override float GetChange() {
        return base.GetChange();
    }

    public override float GetValue() {
        return value;
    }
}

public class GenerateTankGoal : Goal
{
    public static float value = 3;

    public override float GetDiscontentment(float newValue) {
        return base.GetDiscontentment(newValue);
    }

    public override float GetChange() {
        return base.GetChange();
    }

    public override float GetValue() {
        return value;
    }
}

public class ExpandGoal : Goal
{
    public static float value = 7;

    public override float GetDiscontentment(float newValue) {
        return base.GetDiscontentment(newValue);
    }

    public override float GetChange() {
        return base.GetChange();
    }

    public override float GetValue() {
        return value;
    }
}

public class FollowTargetGoal : Goal
{
    public static float value = 3;

    public override float GetDiscontentment(float newValue) {
        return base.GetDiscontentment(newValue);
    }

    public override float GetChange() {
        return base.GetChange();
    }

    public override float GetValue() {
        return value;
    }
}