using System;
using UnityEngine;

public class Rotater : MonoBehaviour
{
    private void Start()
    {
    }

    private void FixedUpdate()
    {
        base.transform.Rotate(0f, this.speed * Time.fixedDeltaTime, 0f);
        if (base.name == "SpinWheel")
        {
            base.transform.Rotate(0f, -this.speed * Time.fixedDeltaTime, 0f);
            base.transform.Rotate(0f, 0f, 4f, Space.Self);
        }
    }

    public float speed;
}
