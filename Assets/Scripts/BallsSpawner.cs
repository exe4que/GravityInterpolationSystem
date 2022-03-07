using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallsSpawner : MonoBehaviour
{
    public int Count = 1000;
    public Vector3 Extents = new Vector3(90, 90,90);
    public GameObject BallPrefab;

    private void Start()
    {
        for (int i = 0; i < Count; i++)
        {
            Vector3 ballPos = new Vector3();
            ballPos.x = this.transform.position.x + Random.Range(-Extents.x * 0.5f, Extents.x * 0.5f);
            ballPos.y = this.transform.position.y + Random.Range(-Extents.y * 0.5f, Extents.y * 0.5f);
            ballPos.z = this.transform.position.z + Random.Range(-Extents.z * 0.5f, Extents.z * 0.5f);
            var ball = Instantiate(BallPrefab, ballPos, Quaternion.identity);
            GravityManagerCS.Instance.RegisterBody(ball.GetComponent<Rigidbody>());
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(this.transform.position, new Vector3(Extents.x, Extents.y, Extents.z));
    }
}
