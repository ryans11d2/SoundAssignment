using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.Splines;

public class Target : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private float speed = 0.1f;
    private bool isHit;
    [SerializeField] private SplineContainer container;

    [SerializeField] EventReference Hit;


    SplinePath path;
    float t = 0f;
    public bool onPath = true;
    Track track;

    private void Update()
    {
        if(path != null)
        {
            if(t <= 1)
            {
                var pos = path.EvaluatePosition(t);
                var direction =  Vector3.right;
                transform.position = pos;
                Vector3 pos2 = pos;
                transform.LookAt( direction);
                t += speed * Time.deltaTime;
               
            }
            else
            {
                t = 0;
                var pos = path.EvaluatePosition(t);
              
                track.ReturnObjectToPool(gameObject);
            }
        }
    }
    public void SetTrack(Track newtrack, SplineContainer spline)
    {
        track = newtrack;
        path = track.GetPath();
        
        //container = spline;
/*
        var localToWorldMatrix = container.transform.localToWorldMatrix;
        path = new SplinePath(new[]
        {
        new SplineSlice<Spline>(container.Splines[0], new SplineRange(0, container.Splines[0].Count), localToWorldMatrix),

        });
        t = 0;
        CarPathCoroutine(path);*/
    }

    // Update is called once per frame
    
    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.TryGetComponent(out ObjectManager comp))
        {
            
            print("collide");
            var score = Vector2.Distance(gameObject.transform.position, collision.GetContact(0).point) * 50;

            score = Mathf.Clamp(score, 0, 20);
            print("before: " + score);
            score = 20 - score;
            print("after: " + score);
            t = 0;
            float p = transform.position.x / 5;
            score += p;
            track.ReturnObjectToPool(gameObject);
            GameManager.Instance.AddScore(Mathf.RoundToInt(score));

            GameObject Sound = Instantiate(new GameObject());
            Sound.transform.position = transform.position;
            //Sound.transform.parent = transform;
            Sound.AddComponent<StudioEventEmitter>();
            Sound.GetComponent<StudioEventEmitter>().EventReference = Hit;
            Sound.GetComponent<StudioEventEmitter>().Play();
            Destroy(Sound, 5);

        }
    }

}
