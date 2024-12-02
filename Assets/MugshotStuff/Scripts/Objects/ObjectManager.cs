using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using FMODUnity;
using JetBrains.Annotations;
using Unity.VisualScripting;
//using UnityEditor.SceneManagement;
using UnityEngine;

public abstract class ObjectManager : MonoBehaviour
{
    [Header ("PHYSICS")]
    //[SerializeField] public float Mass = 1;//Object mass (connected to rigidbody mass)
    [SerializeField] public bool Fragile = false;//Is object fragile
    [SerializeField] bool Split = true;//Does object split into fragments
    [SerializeField] bool Shatter = false;//Does object break into pieces (alternative to Split with more pieces)
    [SerializeField] public Vector3 FragmentScale = Vector3.one;//Size of fragments
    [SerializeField] public float BreakForce;//Force required to break object
    [SerializeField] private bool ThrowRotation = true;//Rotate around X and Y when thrown or striking
    [SerializeField] private bool freeze = false;//Is object frozen

    [SerializeField] private Vector3 ExtraForce = Vector3.zero;//Force to add on top of throw force
    [SerializeField] private Vector3 ThrowTorque = Vector3.zero;//Position to apply throw force at

    public float Force;//Force on object

    private float LifeTime = 0.0f;

    [Header ("ACTION")]
    [SerializeField] public bool PlayerCanGrab = true;//Can object be picked up
    public bool DefaultGrab;//Default PlayerCanGrab value
    [SerializeField] public bool Heavy = false;//Can player store in inventory (not used in throw manager)
    
    [SerializeField] GameObject SwitchObject = null;//Object this can replace itself with

    //[SerializeField] bool HoldInput = false;
    [SerializeField] float UseDelay = 0.0f;//Minimum time between uses
    float UseTimer = 0.0f;//Time when object can be used again

    private bool isHeld = false;//Is the player holding the object
    private ThrowManager holder = null;//Player holding the object (reference to the player)

    Rigidbody rb;//RigidBody reference
    public bool isThrown = false;//Is object thrown
    bool Striking = false;//Is object a projectile
    //public static Action<string> GrabbedEvent;

    [Header ("ANIMATION")]
    [SerializeField] public int ObjectID = 0;//Corresponding object ID in animator
    [SerializeField] GameObject HandObject;//Corresponding GameObject in character hand

    private Vector3 StartPos;//Starting position
    private Quaternion StartRot;//Starting rotation
    private Vector3 StartScale;//Not sure, could be anything
    private bool StartGrav;//Starting gravity
    private bool IsThrown2_2Is2Thrown = false;//Was object thrown, only resets when picked up (Sharpshooter only)
    private float ResetTimer = 0;//Timer to reset object

    [Header ("SOUND")]
    private StudioEventEmitter Player;
    [SerializeField] protected EventReference GrabSound;
    [SerializeField] protected EventReference HitSound;
    [SerializeField] protected EventReference BreakSound;

    [Header("OUTLINES")]
    private List<Outline> outlines = new List<Outline>();

    void Start()
    {
        
        rb = gameObject.GetComponent<Rigidbody>();
        if (Player == null) Player = gameObject.GetComponent<StudioEventEmitter>();
        UseTimer = Time.deltaTime;
        
        //Setup starting values
        DefaultGrab = PlayerCanGrab;
        StartPos = transform.position;
        StartRot = transform.rotation;
        StartScale = transform.localScale;
        StartGrav = rb.useGravity;

        Begin();

        //Setup outline
        if(gameObject.TryGetComponent(out MeshRenderer component))
        {
            outlines.Add(gameObject.AddComponent<Outline>());
        }
        else
        {
            for(int i = 0;  i < transform.childCount; i++)
            {
                if(transform.GetChild(i).TryGetComponent(out MeshRenderer rend))
                {
                    outlines.Add(transform.GetChild(i).AddComponent<Outline>());
                }
            }
        }

        foreach(Outline i in outlines)
        {
            i.OutlineWidth = 10;
            i.OutlineMode = Outline.Mode.OutlineVisible;
            i.enabled = false;
        }
    }


    
    void FixedUpdate() {

        LifeTime += Time.deltaTime;

        DefaultGrab = true;
        
        if (transform.position.y < -20) Reset();//Reset object if it leaves the level

        //Disable/Enable collision if held
        if (isHeld) {
            rb.Sleep();
        }
        GetComponent<Collider>().enabled = !isHeld;
        
        if (!ThrowRotation) {//Constrain rotation if ThrowRotation is disabled
            if (isThrown || Striking) {
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            } else {
                rb.constraints = RigidbodyConstraints.None;
            }
        }

        Force = rb.mass * rb.velocity.magnitude;//Calculate force

        if (rb.velocity.magnitude == 0) {//If object is not moving
            Striking = false;//Disable striking
            PlayerCanGrab = DefaultGrab;//Allow grabbing if not moving
            if (IsThrown2_2Is2Thrown && transform.position.z >= 4.8) ResetTimer += Time.deltaTime;//Increase reset timer if thrown and not moving within the range
        }
        else if (!IsThrown2_2Is2Thrown) {//If player has not thrown object, reset ResetTimer
            ResetTimer = 0;
        }
        
        if (ResetTimer > 8) Reset();//Reset object when reset timer reaches 8 seconds
        
    }

    public void Pickup(ThrowManager NewHolder) {//Setup from being grabbed

        //Setup variables
        isHeld = true;
        holder = NewHolder;
        IsThrown2_2Is2Thrown = false;
        ResetTimer = 0;

        MakeSound(GrabSound);

        gameObject.SetActive(false);//Hide self
        HandObject.SetActive(true);//Show animator object

        ObjectGrabbed();//Trigger special object action

    }

    public void Throw(Vector3 Dir, float ThrowForce) {//Apply force and release from player
        
        ExtraForce *= holder.ThrowPercent;//Reduce extra force based on throw charge
        rb.useGravity = true;//Enable gravity
        rb.constraints = RigidbodyConstraints.None;//Disable constraints
        HandObject.SetActive(false);//Hide animator object
        gameObject.SetActive(true);//Activate self
        PlayerCanGrab = false;//Disable player grabbing
        isHeld = false;//Disable isHeld
        holder = null;//Reset holder
        IsThrown2_2Is2Thrown = true;//Player has thrown object

        //Match position and rotation with animato object
        transform.position = HandObject.transform.position;
        transform.rotation = HandObject.transform.rotation;
        
        //Apply force to object
        //rb.AddForce(Dir * ThrowForce, ForceMode.Impulse);
        rb.AddForceAtPosition(Dir * ThrowForce + ExtraForce, transform.position + ThrowTorque, ForceMode.Impulse);

        //ObjectThrown();//Special object action, doesn't exist yet
        
    }

    void Break() {//Destroy Self
        if (Split) SplitBreak();//Divide into fragments
        else if (Shatter) SectionBreak();//Shatter

        MakeSound(BreakSound);

        //Destroy(gameObject);//Replaced by Reset
        Reset();//Reset Object
    }
    void SplitBreak() {//Devide mesh into different sections for fragments

        int Fragments = UnityEngine.Random.Range(3, 7);//Determine number of pieces to split

        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;//Mesh of this object
        Vector3[] oldMesh = new Vector3 [mesh.vertices.Length];//Copy of mesh
        Vector3[][] newMesh = new Vector3[Fragments][];//New meshes for each fragment
        
        int increment = mesh.vertices.Length / Fragments;//Number of vertices per fragment

        for (int i = 0; i < oldMesh.Length; i++) {//Copy mesh vertices into oldMesh
            oldMesh[i] = mesh.vertices[i];
        }

        for (int h = 0; h < oldMesh.Length - 1; h++) {//Sort oldMesh vertices based on y position for better fragment shapes
            for (int i = 1; i < oldMesh.Length; i++) {
                if (oldMesh[i - 1].y > oldMesh[i].y) {
                    Vector3 swap = oldMesh[i];
                    oldMesh[i] = oldMesh[i - 1];
                    oldMesh[i - 1] = swap;
                }
            }
        }

        //Setup new fragment meshes

        for (int i = 0; i < newMesh.Length; i++) {//Set new mesh lengths
            newMesh[i] = new Vector3 [oldMesh.Length];
        }

        for (int i = 0; i < Fragments; i++) {//Fill new fragment mesh with unique section of oldMesh
            int k = 0;
            for (int j = 0; j < newMesh[i].Length; j++) {
                newMesh[i][j] = oldMesh[k + (increment * i)];//increment * i = start of unique section, k = index in unique section
                k++;
                if (k >= increment) k = 0;
            }
            
        }
        
        for(int i = 0; i < Fragments; i++) {//Instantiate each fragment
            AddFragment(newMesh[i]);
        }
        
    }

    void SectionBreak()//Turn sections of mesh vertices into new mesh for fragments 
    {
        //Creates fragments relative to the number of vertices in mesh

        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;//Gameobject mesh
        Vector3[] vertices = new Vector3[mesh.vertices.Length];//New mesh
        Vector3[][] new_vertices = new Vector3[(int)mesh.vertices.Length / 5][];//Mesh for all fragments

        Vector3 MidPoint = Vector3.zero;//Middle of object mesh

        for (int i = 0; i < mesh.vertices.Length; i++)//Copy mesh vertices into new array
        {
            vertices[i] = mesh.vertices[i];
            MidPoint += vertices[i];//Calculate middle of mesh
        }

        MidPoint = MidPoint / vertices.Length;//Calculate middle of mesh

        int index = 0;//Index of object mesh to copy from
        for (int a = 0; a < new_vertices.Length; a++)//Distribute groups of 3 vertices into each fragment mesh
        {

            new_vertices[a] = new Vector3[mesh.vertices.Length];

            for (int b = 0; b < new_vertices[a].Length; b++)
            {
                if (b < 4)//Set first 3 elements to object mesh vertices
                {
                    new_vertices[a][b] = vertices[index];
                    index += 1;
                }
                else new_vertices[a][b] = Vector3.zero;//Fill remaining elements with zero
                
            }

        }

        /*
        for (int a = 0; a < new_vertices.Length; a++)//Shuffle Vertices (connect faces beter) (doesn't work)
        {
            for (int b = 0; b < new_vertices[a].Length; b++)
            {
                //new_vertices[a][b]
                int r = UnityEngine.Random.Range(0, new_vertices[a].Length);
                Vector3 s1 = new_vertices[a][b];
                Vector3 s2 = new_vertices[a][r];
                new_vertices[a][r] = s1;
                new_vertices[a][b] = s2;

            }
        }
        */

        for (int a = 0; a < new_vertices.Length; a++)//Fill each fragment mesh
        {

            new_vertices[a][4] = MidPoint;//Set fourth vertex to MidPoint

            for (int b = 5; b < new_vertices[a].Length; b++)//Copy first vertices into leftover vertices
            {
                new_vertices[a][b] = new_vertices[a][b - 5];
            }

            AddFragment(new_vertices[a]);//Instantiate fragment

        }

    }
    
    private void AddFragment(Vector3[] vertices) {//Create a fragment object

        GameObject newFragment = Instantiate(gameObject);//Instantiate copy of this object (new fragment)
        Destroy(newFragment.GetComponent<ObjectManager>());//Remove object scripts from copy
        Destroy(newFragment.GetComponent<DefaultObject>());
        Destroy(newFragment.GetComponent<StudioEventEmitter>());
        //Destroy(newFragment.GetComponent<MeshCollider>());
        newFragment.transform.position = transform.position;//Setup fragment position

        Vector3 NewScale = Vector3.one;//Setup scale of fragment
        NewScale.x = transform.localScale.x * FragmentScale.x;
        NewScale.y = transform.localScale.y * FragmentScale.y;
        NewScale.z = transform.localScale.z * FragmentScale.z;
        newFragment.transform.localScale = NewScale;

        Rigidbody new_rb = newFragment.GetComponent<Rigidbody>();//RigidBody component of new fragment
        new_rb.constraints = RigidbodyConstraints.None;//Disable fragment constraints
        new_rb.useGravity = true;//Enable fragment gravity
        new_rb.velocity = Vector2.zero;//Reset fragment velocity
        newFragment.GetComponent<MeshFilter>().mesh.SetVertices(vertices);//Replace fragment mesh with new fragment mesh

        float EForce = UnityEngine.Random.Range(-2.0f, 2.0f);//Explosion force direction

        new_rb.AddExplosionForce(50, transform.position + new Vector3(EForce, EForce, EForce), 20);//Apply explosion force
        newFragment.layer = 8;//Set to fragment layer
        //newFragment.GetComponent<Collider>().enabled = false;

        Destroy(newFragment, 20);

    }

    public void Activate(bool held) {//Trigget object action if it is valid
        
        if (UseTimer < Time.time) {//If UseTimer has been surpassed
            if (held) ObjectUse();//If player is holding, trigger action
            else ObjectAction();//If object is not held, trigger action
            UseTimer = Time.time + UseDelay;//Reset use timer
        }
        
    }

    public void Switch(GameObject forceObject = null) {//Replace this object with a new object
        GameObject newObject = null;//New object
        if (forceObject == null) {//If an object is not inputed, switch to default object
            if (SwitchObject == null) return;//If no onject is set for this to replace, end the function
            newObject = Instantiate(SwitchObject);//Replace this object with a set object
        } else {
            newObject = Instantiate(forceObject);//Replace this object with an inputed object
        }

        //Set new object data to this data
        newObject.transform.parent = transform.parent;
        newObject.transform.position = transform.position;
        newObject.transform.rotation = transform.rotation;

        if (isHeld && isActiveAndEnabled) {//If this object is held, update player inventory
            holder.SetHolding(newObject.GetComponent<ObjectManager>());
            newObject.GetComponent<ObjectManager>().isHeld = true;
            newObject.GetComponent<ObjectManager>().holder = holder;
        }

        Destroy(gameObject);//Destroy this object
    }

    public void Strike(Vector3 force, bool strike = false) {//Make object a projectile (this is still experimental)
        Striking = strike;//Is object striking (it will not break while striking)
        rb.AddForce(force, ForceMode.Impulse);//Apply inputed force
    }

    void OnCollisionEnter(Collision other) {
        
        GameObject collision = other.gameObject;//Get colliding object
        
        //Calculate collision force with other objects
        float HitForce = Force;//Force of collision
        if (collision.GetComponent<ObjectManager>() != null) {//If colliding with another ObjectManager, add forces
            HitForce += collision.GetComponent<ObjectManager>().Force;
        }
        else if (collision.GetComponent<Rigidbody>() != null) {//Elif colliding wilh another RigidBody calculate and add forces
            HitForce += collision.GetComponent<Rigidbody>().mass + collision.GetComponent<Rigidbody>().velocity.magnitude;
        }
        else {//Otherwise end throwing
            isThrown = false;
            Striking = false;
        }

        //Debug.Log(HitForce);

        if (collision.TryGetComponent(out IDamageable damageable)) {//If collision can be damaged, damage it
            damageable.TakeDamage(Force);
        }

        //Run land functions
        if (Fragile && HitForce >= BreakForce && !Striking) {//If break force exceeded, trigger break functions
            LandAction(true);//Trigger special function for landing and breaking
            Break();//Break
        } else {
            //if (!Player.isPlaying) Player.clip = HitSound;
            
            MakeSound(HitSound);

            LandAction(false);//Trigger special function for landing
        }
        Collided(other);//Trigger special object funtion
    }

    public void Reset() {//Reset variables to starting values
        IsThrown2_2Is2Thrown = false;
        ResetTimer = 0;
        transform.position = StartPos;
        transform.rotation = StartRot;
        transform.localScale = StartScale;
        rb.useGravity = StartGrav;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        if (freeze) rb.constraints = RigidbodyConstraints.FreezePosition;
        Force = 0;
        LifeTime = 0.0f;
        OnReset();
    }

    protected void MakeSound(EventReference SoundEvent) {
        if (LifeTime < 0.5f) return;
        GameObject Sound = Instantiate(new GameObject());
        Sound.transform.position = transform.position;
        //Sound.transform.parent = transform;
        Sound.AddComponent<StudioEventEmitter>();
        Sound.GetComponent<StudioEventEmitter>().EventReference = SoundEvent;
        Sound.GetComponent<StudioEventEmitter>().Play();
        Destroy(Sound, 5);
    }

    public void ChangeOutlineState(bool state)
    {
        foreach(Outline i in outlines)
        {
            i.enabled = state;
        }
    }
    
    protected abstract void Collided(Collision collision);//Executed at the end of OnCollisionEnter()
    protected abstract void Begin();//Executed at the end of Start()
    protected abstract void ObjectAction();//Execute function on player input while targeted but not held
    protected abstract void ObjectGrabbed();//Execute function when picked up
    protected abstract void ObjectUse();//Execute function on player input while held
    protected abstract void LandAction(bool broken);//Execute function when hitting something
    protected abstract void OnReset();//Execute Function after reseting

}
