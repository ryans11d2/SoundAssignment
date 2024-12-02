using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfClub : ObjectManager
{

    [SerializeField] float strikeRange = 8;//Distance an object can be targeted from
    [SerializeField] float strikeForce = 10;//Force applied to struck object

    protected override void Begin()
    {
        throw new System.NotImplementedException();
    }

    protected override void Collided(Collision collision)
    {
        throw new System.NotImplementedException();
    }

    protected override void LandAction(bool broken)
    {
        
    }

    protected override void ObjectAction()
    {
        
    }

    protected override void ObjectGrabbed()
    {
        
    }

    protected override void ObjectUse()
    {
        ObjectManager[] targets = gameObject.transform.parent.GetComponentsInChildren<ObjectManager>();//All objects
        ObjectManager nearest = null;//Object to strike

        foreach (ObjectManager i in targets) {//Find the closest active object other than this one
            if (i.gameObject != gameObject && i.gameObject.activeSelf) {
                if (nearest == null) nearest = i;
                else {
                    if (Vector3.Distance(gameObject.transform.position, i.transform.position) < Vector3.Distance(gameObject.transform.position, nearest.transform.position))
                        nearest = i;
                }
            }
        }

        if (nearest != null) {//Strike the targeted object
            if (Vector3.Distance(gameObject.transform.position, nearest.transform.position) <= strikeRange)
                nearest.Strike((transform.forward * strikeForce) + new Vector3(0, 2, 0));
        }


    }

    protected override void OnReset()
    {
        throw new System.NotImplementedException();
    }
}
