using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealthControls : MonoBehaviour
{
    //COMPONENT
    private Rigidbody rb;

    [Header("STEALTH BASE")]
    [SerializeField] SphereCollider auditoryCollider;
 
    [SerializeField] private float baseAuditoryRadius;
   
    [SerializeField] private float modifiedAuditory;// for testing purposes only, do not touch

    [SerializeField] private float velocityBonus;// added to increase the magnitude of the velocity

    [SerializeField] private float velocityDenominator;//devide the velocity by this amount

     private bool IsCloaked;

    [SerializeField] private float cloakActivationDelay;
    private float timer;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
       
        auditoryCollider.radius = baseAuditoryRadius;

        modifiedAuditory = baseAuditoryRadius;
       
        
    }

    // Update is called once per frame
    void Update()
    {
       
        if (rb.velocity.magnitude > 0.2)
        {


            /* modifiedAuditory = baseAuditoryRadius + rb.velocity.magnitude;*/

            modifiedAuditory = Mathf.Pow(baseAuditoryRadius, (rb.velocity.magnitude+velocityBonus) / velocityDenominator);
            if (IsCloaked)
            {
                IsCloaked = false;
            }
        }
        else
        {
            modifiedAuditory = 0f;
            if (!IsCloaked)
            {
                timer += Time.deltaTime;
                if (timer >= cloakActivationDelay)
                {
                    IsCloaked = true;
                    timer = 0;
                }
            }
        }
        auditoryCollider.radius = modifiedAuditory;
    }

    

    private void OnDrawGizmos()
    {
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(auditoryCollider.transform.position, modifiedAuditory);
    }
}
