using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
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
    [SerializeField] GameObject Objects;//Reference to objects
    [SerializeField] Camera cam;//Reference to main camera

    [Header ("THROWING")]
    [SerializeField] float Reach = 2.5f;//Maximum distance from player to target an object
    [SerializeField] float ThrowForce;//Throw charge speed
    [SerializeField] float MaxThrowForce;//Maximum throw force
    bool chargeThrow = false;//Is player throwing
    float ThrowCharge;//Current throw force
    
    Vector3 LookPosition;//Location to search from
    ObjectManager LookObject = null;//Targeted object
    int HeldObject = 0;//Index of currently held object in Holding
    ObjectManager[] Holding = new ObjectManager[3];//Player's held objects
    private Vector3 aimPoint;//Point where player is aiming
    

    [Header ("GRABBING")]
    [SerializeField] Transform HoldPosition;//Position of held objects
    [SerializeField] float GrabDelay;//Cooldown for grabing objects
    float GrabTimer;//Cooldown timer for grabbing objects

    void Start()
    {
        HoldPosition.transform.parent = transform;
    }

    void Update()
    {
        
        getSwitch();//Temporary Inventory Controls
        
        //Set look position at a point opposite to the camera
        LookPosition = gameObject.transform.position - (cam.gameObject.transform.position - gameObject.transform.position) / 2.6f;

        //Select object within reach that is nearest to the look point
        LookObject = null;
        foreach (ObjectManager i in Objects.GetComponentsInChildren<ObjectManager>()) {
            if (Vector3.Distance(gameObject.transform.position, i.transform.position) < Reach) {
                float dist = Vector3.Distance(LookPosition, i.transform.position);
                if (LookObject == null) LookObject = i;
                else if (dist < Vector3.Distance(LookPosition, LookObject.transform.position)) {
                    LookObject = i;
                }
            }
        }

        //Place HeldObject at HoldPosition
        if (Holding[HeldObject] != null) {
            Holding[HeldObject].transform.position = HoldPosition.position;
            Holding[HeldObject].transform.rotation = transform.rotation;
        }

        //Increase throw force if charging throw
        if (chargeThrow) {
            ThrowCharge += ThrowForce * Time.deltaTime;
            if (ThrowCharge > MaxThrowForce) {
                ThrowCharge = MaxThrowForce;
            }
        }

    }

    public void GrabObject() 
    {
        
        if (LookObject == null) {
            Debug.Log("null");
            return; 
        }
        Debug.Log("not null");
        int newIndex = HeldObject;
        if (Holding[HeldObject] != null) {
            for (int i = 0; i < Holding.Length; i++) {
                if (Holding[i] != null) {
                    newIndex = i;
                    break;
                }
            }
            if (Holding[newIndex] == null) SwitchObject(newIndex);
            else return;
        }

        //Check if target object can be reached
        GameObject pointer = new GameObject();
        pointer.transform.position = HoldPosition.position;
        pointer.transform.rotation = HoldPosition.rotation;
        pointer.transform.LookAt(LookObject.transform.position);
        if (Physics.Raycast(HoldPosition.position, pointer.transform.forward * Reach, out RaycastHit hit, Reach)) {
            if(hit.collider.name != LookObject.name) return;
        }
        Destroy(pointer);
        //Debug.DrawRay(HoldPosition.position, pointer.forward, Color.green, 100);

        //Pickup targeted object
        if (LookObject != null && GrabTimer < Time.time) {
            GrabTimer = Time.time + GrabDelay;

            /*
            if (HeldObject != null) {//Swap held object with new targeted object
                HeldObject.isHeld = false;
                HeldObject.transform.position = LookObject.transform.position;
            }*/

            Holding[HeldObject] = LookObject;
            Holding[HeldObject].Pickup(gameObject.GetComponent<ThrowManager>());
        }
    }

    public void StartThrow() {//Begin charging throw
        chargeThrow = true;
    }
    public void ThrowObject()//Finish throwing object
    {
        if (Holding[HeldObject] == null) return;//Nake sure an object is held
        Holding[HeldObject].Throw(Camera.main.transform.forward, ThrowCharge);
        Holding[HeldObject] = null;
        chargeThrow = false;//Reset throwing
        ThrowCharge = 0;
    }

    public void UseObject() {
        Debug.Log(Holding[HeldObject]);
        if (Holding[HeldObject] == null) {
            if (LookObject != null) LookObject.Activate(false);
        }
        else Holding[HeldObject].Activate(true);
    }

    public void SwitchObject(int index) {
        
        if (Holding[HeldObject] != null) {//Make sure switch is valid
            if (Holding[HeldObject].Heavy || HeldObject == index) return;

            Holding[HeldObject].gameObject.SetActive(false);//Disable current held object
        }

        HeldObject = index;//Switch held object

        if (Holding[HeldObject] != null) Holding[HeldObject].gameObject.SetActive(true);//Enable new held object

    }

    public void getSwitch() {//Temporty inventory controls

        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchObject(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchObject(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchObject(2);

    } 

    public void SetHolding(ObjectManager newHold) {//Manualy set the held object to something else
        Holding[HeldObject] = newHold;
    }

}
