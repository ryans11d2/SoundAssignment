using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FishObject : ObjectManager
{
    private Vector3 startPos;
    private Quaternion startRot;
    private int bounces = 0;
    protected override void Begin()
    {
        startRot = transform.rotation;
        startPos = transform.position + new Vector3(0, 0.2f, 0);
    }
    protected override void LandAction(bool broken)
    {
        //throw new System.NotImplementedException();

        if (bounces == 0 && Force > 1.5f) bounce();
        else if (bounces > 0 && Force > 0.5f) bounce();
        
        

    }

    void bounce() {
        bounces++;

        transform.localScale *= 1.8f;

        if (bounces > 3) {
            FishBreak();
        }
    }

    void FishBreak()//Turn sections of mesh vertices into new mesh for fragments 
    {

        transform.localScale *= 5f;

        int Fragments = 50;

        Mesh mesh = gameObject.GetComponentInChildren<MeshFilter>().mesh;
        Vector3[] oldMesh = new Vector3 [mesh.vertices.Length];
        Vector3[][] newMesh = new Vector3[Fragments][];
        
        int increment = mesh.vertices.Length / Fragments;

        for (int i = 0; i < oldMesh.Length; i++) {
            oldMesh[i] = mesh.vertices[i];
        }

        for (int h = 0; h < oldMesh.Length - 1; h++) {
            for (int i = 1; i < oldMesh.Length; i++) {
                if (oldMesh[i - 1].y > oldMesh[i].y) {
                    Vector3 swap = oldMesh[i];
                    oldMesh[i] = oldMesh[i - 1];
                    oldMesh[i - 1] = swap;
                }
            }
        }

        for (int i = 0; i < newMesh.Length; i++) {
            newMesh[i] = new Vector3 [oldMesh.Length];
        }

        for (int i = 0; i < Fragments; i++) {
            int k = 0;
            for (int j = 0; j < newMesh[i].Length; j++) {
                newMesh[i][j] = oldMesh[k + (increment * i)];
                k++;
                if (k >= increment) k = 0;
            }
            
        }
        
        MakeSound(BreakSound);

        for(int i = 0; i < Fragments; i++) {
            GameObject newFragment = Instantiate(gameObject);
            Destroy(newFragment.GetComponent<ObjectManager>());
            Destroy(newFragment.GetComponent<DefaultObject>());
            //Destroy(newFragment.GetComponent<MeshCollider>());
            newFragment.transform.position = transform.position;

            Vector3 NewScale = Vector3.one;
            NewScale.x = transform.localScale.x * (FragmentScale.x / 5);
            NewScale.y = transform.localScale.y * (FragmentScale.y / 5);
            NewScale.z = transform.localScale.z * (FragmentScale.z / 5);
            newFragment.transform.localScale = NewScale;

            Rigidbody new_rb = newFragment.GetComponent<Rigidbody>();
            new_rb.constraints = RigidbodyConstraints.None;
            new_rb.useGravity = true;
            new_rb.velocity = Vector2.zero;
            new_rb.AddExplosionForce(50, new Vector3(UnityEngine.Random.Range(-2.0f, 2.0f), UnityEngine.Random.Range(-2.0f, 2.0f), UnityEngine.Random.Range(-2.0f, 2.0f)), 20);
            newFragment.GetComponentInChildren<MeshFilter>().mesh.SetVertices(newMesh[i]);
            newFragment.layer = 8;
            //newFragment.GetComponent<Collider>().enabled = false;

            //newFragment.AddComponent<BoxCollider>();
            //newFragment.GetComponent<BoxCollider>().enabled = true;
            //newFragment.GetComponent<BoxCollider>().size = new Vector3 (0.003f, 0.003f, 0.003f);

            Destroy(newFragment, 3);
        }

        Reset();
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
        //Debug.Log("Used Object: " + gameObject.name);
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
        bounces = 0;
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
