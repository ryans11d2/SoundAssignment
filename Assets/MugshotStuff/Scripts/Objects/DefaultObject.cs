using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DefaultObject : ObjectManager
{
    
    protected override void Begin()
    {

    }
    protected override void LandAction(bool broken)
    {
        //throw new System.NotImplementedException();
    }

    protected override void ObjectGrabbed()
    {
        //throw new System.NotImplementedException();
    }

    protected override void ObjectAction()
    {
        //throw new System.NotImplementedException();
    }

    protected override void ObjectUse()
    {
        //throw new System.NotImplementedException();
        Debug.Log("Used Object: " + gameObject.name);
    }

    protected override void Collided(Collision collision)
    {


       /* if(collision.gameObject.layer == 6)
        {
            source.Play();
            Debug.Log("hit");
            StartCoroutine(DespawnTimer());
        }*/
    }

    protected override void OnReset()
    {
        
    }

    /*private IEnumerator DespawnTimer()
    {

        yield return new WaitForSeconds(1f);
        gameObject.SetActive(false);
        gameObject.transform.position = startPos;
        gameObject.transform.rotation = startRot;
        gameObject.SetActive(true);

    }*/
}
