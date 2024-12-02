using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //COMPONENTS
    private Rigidbody rb;
    

    private ThrowManager inventory;
    private Animator playerAnimator;


    [Header("MOVEMENT")]
    
    [SerializeField] private float baseSpeed;
   

    public float velocity;
 
    private Vector3 moveDir, adjustedDir;


    [Header("CAMERA")]
    [SerializeField] private float minYAngle;
    [SerializeField] private float maxYAngle;
    [SerializeField] private GameObject shoulderLookAt;

    [Header("Test")]
    //[SerializeField] private GameObject objec;
    [SerializeField] StudioEventEmitter Step;
    private float stepTimer = 0;
    private float stepDelay = 0;
    
    
    //ENUM for camera modes
    [SerializeField] private GameManager manager;

    private void Awake()
    {
        PlayerInput.Init(this);
        
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        inventory = GetComponent<ThrowManager>();
        playerAnimator = GetComponent<Animator>();
        
        
        Cursor.visible = false;
    }
    // Update is called once per frame
    void Update()
    {
        velocity = rb.velocity.magnitude;
        playerAnimator.SetFloat("VelocityX", adjustedDir.x);
        playerAnimator.SetFloat("VelocityZ", adjustedDir.z);

        stepTimer += Time.deltaTime;
        if(velocity > 0.3f && stepTimer >= stepDelay)
        {
            Step.Play();
            stepDelay = 0.35f;
            stepTimer = 0;

        }

    }

    private void FixedUpdate()
    {

        rb.velocity = transform.rotation * moveDir * baseSpeed ;

    }
    
    //movement controls for the free aspect camera
    

    private float LockRotation(float Angle, float min, float max)
    {
        if(Angle < 90 || Angle > 270)
        {
            if (Angle > 180) Angle -= 360;
            if (max > 180) max -= 360;
            if (min > 180) min -= 360;
        }
        Angle = Mathf.Clamp(Angle, min, max);
        if (Angle < 0) Angle += 360;
        return Angle;
    }
   

    public void Move(Vector3 dir)
    {
        moveDir = dir;//sets teh movement direction
    }

    public void Look(Vector2 mouseDir)
    {
        if (manager.paused) return;

        mouseDir *= manager.sensitivity;

        Quaternion yMove = Quaternion.Euler(-mouseDir.y, 0, 0);
        Quaternion zMove = Quaternion.Euler(0, mouseDir.x, 0);


        shoulderLookAt.transform.rotation = shoulderLookAt.transform.rotation * yMove;//rotates the look at target

        float Angle = shoulderLookAt.transform.eulerAngles.x;
        Angle = LockRotation(Angle, minYAngle, maxYAngle);
      //  print("clamped angle is: " + Angle);
        shoulderLookAt.transform.rotation = transform.rotation * Quaternion.Euler(Angle, 0, 0);





        transform.rotation = transform.rotation * zMove;//rotates the full body
    }

   
    public void GrabObject() 
    {
        if (!manager.paused) inventory.GrabObject();
       // Instantiate(objec, Camera.main.transform.position, Quaternion.identity).GetComponent<Rigidbody>().velocity =  Camera.main.transform.forward* 10;
    }

    public void StartThrow() 
    {
        if (!manager.paused) inventory.StartThrow();
    }
    public void ThrowObject() 
    {
        if (!manager.paused) inventory.ThrowObject();
    }

    public void UseObject() {
        if (!manager.paused) inventory.UseObject();
    }

}
