using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsController : MonoBehaviour
{
    public float shakeForceMultiflier;
    public Rigidbody2D[] shakingRigids = new Rigidbody2D[50];

    public void ShakeRigidbodies(Vector3 deviceAcceleration)
    {
        foreach(var rigidbody in shakingRigids)
        {
            rigidbody.AddForce(deviceAcceleration * shakeForceMultiflier, ForceMode2D.Impulse);
        }
    }
    
    public void ClearRigidbodies()
    {
        foreach(var rigid in shakingRigids)
        {
            rigid.velocity = Vector2.zero;
        }
        shakingRigids.Initialize();
    }

    public void AddRigidbody(List<DV> dv)
    {
        int i = 0;
        foreach(DV d in dv)
        {
            shakingRigids[i++] = d.GetComponent<Rigidbody2D>();
        }
    }
}
