using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Unity.VisualScripting;
//using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ThrowManager : MonoBehaviour
{

    /*
    * Target object nearby player opposite to camera position
    *
    * E - Grab selected object, currently uses player interact input
    * LMB - Press to charge throw, release to launch object in look direction
    *
    *
    */

    [Header ("REFERENCES")]
    [SerializeField] Camera cam;//Reference to main camera
    private Animator animator;//Reference to animator
    [SerializeField] Image FillBar;//Reference to throw charge bar

    [Header ("THROWING")]
    [SerializeField] float Reach = 200.5f;//Maximum distance from player to target an object
    [SerializeField] float ThrowForce;//Throw charge speed
    [SerializeField] float MaxThrowForce;//Maximum throw force
    public float ThrowPercent = 0.0f;//Throw charge as a percentage
    bool chargeThrow = false;//Is player throwing
    float ThrowCharge;//Current throw force
    
    Vector3 LookPosition;//Location to search from
    ObjectManager LookObject = null;//Targeted object
    ObjectManager Holding;//Player's held objects
    private Vector3 aimPoint;//Point where player is aiming

    [Header ("GRABBING")]
    [SerializeField] Transform HoldPosition;//Position of held objects
    [SerializeField] float GrabDelay;//Cooldown for grabing objects
    float GrabTimer;//Cooldown timer for grabbing objects
    [SerializeField] private LayerMask layer;//Layer to search for objects in
   
    [SerializeField] float GrabRadius = 0.25f;//Radius around LookPosition to search got objects
    [SerializeField] StudioEventEmitter ThrowSound;
    [SerializeField] StudioEventEmitter ThrowChargeSound;

    private ObjectManager lastLookedAt;
    

    void Start()
    {
        HoldPosition.transform.parent = transform;//Setup hold position
        animator = gameObject.GetComponent<Animator>();//Setup animator
    }

    void Update()
    {
        //Set look position at a point opposite to the camera
        //LookPosition = gameObject.transform.position - (cam.gameObject.transform.position - gameObject.transform.position) / 2.6f;

        //Debug.DrawRay(transform.position, transform.forward * Reach, Color.green, 1);
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, Reach))//Find where camera is pointing
        {
            LookPosition = new Vector3(hit.point.x, hit.point.y, hit.point.z);
        }
        

        //Increase throw force if charging throw
        if (chargeThrow && Holding != null) {
            //Holding.transform.position = HoldPosition.transform.position;  

            ThrowCharge += ThrowForce * Time.deltaTime;//Increase throw charge
            ThrowCharge = Mathf.Clamp(ThrowCharge, 0, MaxThrowForce);

            ThrowPercent = ThrowCharge / MaxThrowForce;//Calculate throw percent
            FillBar.fillAmount = ThrowPercent;//Update throw bar
            FillBar.enabled = true;

        } else FillBar.enabled = false;       

        //Release object after throw animation
        if (animator.GetCurrentAnimatorStateInfo(2).IsName("OverHandEnd") && chargeThrow) FinishThrow();
        if (animator.GetCurrentAnimatorStateInfo(2).IsName("UnderhandEnd1") && chargeThrow) FinishThrow();
        if (animator.GetCurrentAnimatorStateInfo(2).IsName("UnderHandEnd2") && chargeThrow) FinishThrow();

        animator.SetFloat("ArmHeight", Mathf.Round(cam.transform.forward.y * 100 + 71) / 142);//Set Arm Height
        
        //Find the object which the player is looking at
        LookObject = null;
        LookObject = GetLookObject();
        if (LookObject != null) {
            
            if(LookObject != lastLookedAt)
            {
                if(lastLookedAt != null)
                {
                    lastLookedAt.ChangeOutlineState(false);
                }
                
                LookObject.ChangeOutlineState(true);
                lastLookedAt = LookObject;
            }
        }
        else if (lastLookedAt != null)
        {
            lastLookedAt.ChangeOutlineState(false);
            lastLookedAt = null;
        }

    }

    public ObjectManager GetLookObject() {//Find the object which the player is looking at
        
        Collider[] hit = Physics.OverlapSphere(LookPosition, GrabRadius, layer);//Objects within radius of LookPosition
        ObjectManager nearest = null;//The object nearest to LookPosition
        float dist = 0.0f;//Distance to object nearest to LookPosition

        //Find which overlapping object is nearest to LookPosition
        foreach (Collider i in hit)
        {        
            if (i.TryGetComponent<ObjectManager>(out ObjectManager component)) {//Check that object is ObjectManager (or interactable eventualy)
                
                if (component.PlayerCanGrab && component.gameObject.GetComponent<Rigidbody>().velocity.magnitude == 0) {
                    if (nearest == null) {//If there is no nearest object, make this the nearest object
                        nearest = component;
                        dist = Vector3.Distance(LookPosition, nearest.transform.position);
                    }
                    else {//If this is closer to LookPoint than nearest object, make this nearest
                        float new_dist = Vector3.Distance(LookPosition, i.transform.position);
                        if (new_dist < dist) {
                            nearest = component;
                            dist = new_dist;
                        }
                    }
                }
            }

        }

        return nearest;
    }
    public void GrabObject()//Pickup LookObject
    {
        
        if (Holding != null) return;//Make sure player is not already holding something
        
        chargeThrow = false;//Reset throwing
        ThrowCharge = 0;

        LookObject = GetLookObject();//Find LookObject
        
        //Pickup targeted object
        if (LookObject != null && GrabTimer < Time.time) {
            GrabTimer = Time.time + GrabDelay;//Reset grab cooldown

            Holding = LookObject;//Set held object to LookObject
            LookObject = null;

            Holding.Pickup(gameObject.GetComponent<ThrowManager>());//Trigger held object being picked up

            animator.SetInteger("objectID", Holding.ObjectID);
        }
    }

    public void StartThrow() {//Begin charging throw
        if (Holding != null) {
            chargeThrow = true;
            animator.SetBool("objectThrown", true);//Setup animator for held object
            ThrowChargeSound.Stop();
            ThrowChargeSound.Play();
        }
    }
    public void ThrowObject()//Release throw
    {
        if (Holding != null && chargeThrow) animator.SetBool("objectThrown", false);//Start throwing animation
    }

    void FinishThrow() {//Release held object
        Holding.Throw(cam.GetComponentInParent<Transform>().forward.normalized, ThrowCharge);//Throw held object
        Holding = null;//Reset held object
        chargeThrow = false;//Reset throwing
        ThrowCharge = 0;
        animator.SetInteger("objectID", 0);//Reset animator
        ThrowChargeSound.Stop();
        ThrowSound.Play();
    }

    public void UseObject() {//Trigger action of held object or look object
        Debug.Log(Holding);
        if (Holding == null) {
            if (LookObject != null) LookObject.Activate(false);
        }
        else Holding.Activate(true);
    }


    //This will not be triggered on this script
    public void SetHolding(ObjectManager newHold) {//Manualy set the held object to something else in the inventory
        Holding = newHold;
    }

}
