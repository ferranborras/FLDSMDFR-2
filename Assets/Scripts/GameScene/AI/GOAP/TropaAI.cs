using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TropaAI : AI
{
    /*public override Action ChooseAction(Action[] a = null, Goal[] g = null) {
        return base.ChooseAction(actions, goals);
    }

    public override float CalculateDiscontentment(Action action, Goal[] goals) {
        return base.CalculateDiscontentment(action, goals);
    }*/

    public override Action[] PlanActions(WorldModel worldModel, int maxDepth, UnidadModel unidad = null) {
        UnidadModel unidadModel = new UnidadModel();
        unidadModel.GenerateFromUnidad(gameObject.GetComponent<Unidad>());

        ((TropaWorldModel) worldModel).unidad = unidadModel;
        
        return base.PlanActions((WorldModel) worldModel, maxDepth, unidadModel);
    }
}
