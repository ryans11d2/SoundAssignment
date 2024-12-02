using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpawningObject : ObjectManager
{

    /*
    * When an object action is called, a gameobject will spawn
    * Object is set in SpawnObjects[]
    * Multiple objects can be put in the SpawnObjects array, and the object will cycle to the next one each action and repeat
    * If an element (excluding the first one) is left as null, the object will spawn the last object it used
    */

    [SerializeField] Rigidbody[] SpawnObjects = null;//Objects(s) created by object
    [SerializeField] Vector3 SpawnForce;//Force applied to new objects
    [SerializeField] int AmmoCount;//Max number of uses, set tp -1 fpr infinite
    
    private int SpawnIndex = 0;//Index of current object to shoot
    private Rigidbody CurrentObject = null;//Current object to shoot
    private float Ammo = 0;//Remaining ammo
    private bool CanSpawn = true;//Can object spawn

    void OnEnable() {
        Ammo = AmmoCount;
        CurrentObject = SpawnObjects[0];
    }

    private void Spawn() {
        if (!CanSpawn || (Ammo <= 0 && AmmoCount >= 0)) return;//Make sure spawning is valid

        Rigidbody newObject = Instantiate(SpawnObjects[SpawnIndex]);//Setup new object
        newObject.transform.position = transform.position;
        newObject.AddForce(Vector3.forward + SpawnForce, ForceMode.Impulse);

        SpawnIndex++;//Prepare next projectile
        if (SpawnIndex >= SpawnObjects.Length) SpawnIndex = 0;
        if (SpawnObjects[SpawnIndex] != null) CurrentObject = SpawnObjects[SpawnIndex];


    }

    protected override void ObjectUse()//Spawn object
    {
        Spawn();
        ObjectSpawn();
    }

    protected abstract void ObjectSpawn();
    protected abstract override void ObjectAction();
    protected abstract override void ObjectGrabbed();
    protected abstract override void LandAction(bool Broken);

}
