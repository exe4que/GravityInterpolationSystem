using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManagerCS : MonoBehaviour
{

    #region  Singleton
    private static GravityManagerCS _instance;
    public static GravityManagerCS Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GravityManagerCS>();
            }
            return _instance;
        }
    }
    #endregion

    public SDFVIS SDFVISModule;

    private List<Rigidbody> _bodies = new List<Rigidbody>();

    public void RegisterBody(Rigidbody body)
    {
        _bodies.Add(body);
    }

    private void Start()
    {
        SDFVISModule.Initialize();
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < _bodies.Count; i++)
        {
            var body = _bodies[i];
            SDFVISModule.BodyPositions[i] = body.transform.position;
        }

        SDFVISModule.PhysicsUpdate();
        
        for (int i = 0; i < _bodies.Count; i++)
        {
            var body = _bodies[i];
            var gravity = SDFVISModule.BodyDirections[i] * 9.81f;
            body.AddForce(gravity, ForceMode.Acceleration);
        }
    }

    private void OnDrawGizmos()
    {
        SDFVISModule.DrawDebugData();
    }
}
