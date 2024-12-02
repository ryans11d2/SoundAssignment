using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
public class Track : MonoBehaviour
{
    // Start is called before the first frame update

    //[SerializeField] private int numTargets;
    
    [SerializeField] private float spawnSpeed;
    [SerializeField] private int numObjectsToSpawn;
    int numSpawned;
    private WaitForSeconds delay;
    public List<GameObject> InactiveObjects = new List<GameObject>();
    public List<GameObject> activeObjects = new List<GameObject>();
    
    public GameObject objectToSpawn;
    private SplineContainer container;

    private void Awake()
    {
        

        delay = new WaitForSeconds(spawnSpeed);
        container = GetComponent<SplineContainer>();
        StartCoroutine(SpawnClock());
        Debug.Log("start" + gameObject.name);
    }

    private void Start()
    {
        
    }
    private void OnDisable()
    {
        /*StopAllCoroutines();
        foreach(GameObject i in activeObjects)
        {
            ReturnObjectToPool(i);
        }*/
    }

    private IEnumerator SpawnClock()
    {
        Debug.Log("spawn");
        while (true)
        {
            yield return delay;
            activeObjects.Add(SpawnObject());
            
        }
    }

    public SplinePath GetPath()
    {
        var localToWorldMatrix = container.transform.localToWorldMatrix;
       var path = new SplinePath(new[]
        {
        new SplineSlice<Spline>(container.Splines[0], new SplineRange(0, container.Splines[0].Count), localToWorldMatrix),

        });
        return path;
        
    }
    public GameObject SpawnObject()
    {
       
        GameObject spawnableObj = null;

        foreach (GameObject obj in InactiveObjects)
        {
            if (obj != null)
            {
                spawnableObj = obj;
                break;
            }
        }
        if (spawnableObj == null)
        {
            spawnableObj = Instantiate(objectToSpawn,gameObject.transform);
            spawnableObj.GetComponent<Target>().SetTrack(this, container );
        }
        else
        {
           
           // spawnableObj.transform.rotation = spawnRot;
            InactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }
        
        return spawnableObj;

    }
    
    public void ReturnObjectToPool(GameObject Obj)
    {
        activeObjects.Remove(Obj);
        Obj.SetActive(false);
        Obj.GetComponent<Target>().StopAllCoroutines();
        InactiveObjects.Add(Obj);
        
        
    } 
}
