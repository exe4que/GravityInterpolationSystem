using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    Rigidbody rigid;

    void Awake()
    {
        this.rigid = this.GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        Vector3 gravity = GravityManager.Instance.GetGravity(this.transform.position);
        this.rigid.AddForce(gravity, ForceMode.Acceleration);
    }
}
