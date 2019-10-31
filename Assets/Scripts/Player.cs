using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody rigid;
    private Vector3 gravity;

    void Awake()
    {
        rigid = this.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        gravity = GravityManager.Instance.GetGravity(this.transform.position);
        rigid.AddForce(gravity, ForceMode.Acceleration);
    }


    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawLine(this.transform.position, this.transform.position + this.gravity);
        }
    }
}
