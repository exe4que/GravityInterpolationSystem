using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    /* #region  Singleton */
    private static GravityManager _instance;
    public static GravityManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GravityManager>();
            }
            return _instance;
        }
    }
    /* #endregion */


    public bool debugMode;

    [Space]
    public float gravityAcceleration = 9.8f;

    private List<ArrowData> arrows;

    void Awake()
    {
        arrows = new List<ArrowData>();
        GameObject[] _arrows = GameObject.FindGameObjectsWithTag("GravityIndicator");

        foreach (var arrow in _arrows)
        {
            ArrowData newArrowData = new ArrowData(arrow);
            arrows.Add(newArrowData);
        }
    }

    public Vector3 GetGravity(Vector3 position, bool normalized = true)
    {
        Vector3 directionSum = Vector3.zero;
        float wheightSum = 0f;
        foreach (var arrow in arrows)
        {
            float distance = Vector3.Distance(position, arrow.position);
            if (distance > 0f)
            {
                float wheight = (1f / Mathf.Pow(distance, 2f));
                directionSum += arrow.direction * wheight;
                wheightSum += wheight;
            }
            else
            {
                return arrow.direction;
            }
        }
        Vector3 gravity = (directionSum / wheightSum);
        if (normalized)
        {
            gravity = Vector3.Normalize(gravity);
        }
        gravity *= gravityAcceleration;
        // Debug.Log(gravity);
        return gravity;
    }


    private class ArrowData
    {
        public GameObject arrow;
        public Vector3 position;
        public Vector3 direction;

        public ArrowData(GameObject arrow)
        {
            this.arrow = arrow;
            this.position = arrow.transform.position;
            this.direction = arrow.transform.rotation * Vector3.down;
        }
    }


#if UNITY_EDITOR
    private Transform editorCamera;
    private Vector3 originOffset;
    private int depth = 20;
    private bool debugInitialized = false;
    void OnDrawGizmos()
    {
        if (debugMode)
        {
            UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            if (!debugInitialized)
            {
                originOffset = new Vector3(-8, -5, 10);
                editorCamera = UnityEditor.SceneView.lastActiveSceneView.camera.transform;
                if (editorCamera == null)
                {
                    return;
                }
                Awake();
                debugInitialized = true;
            }
            Vector3 gridEditorCameraPosition = new Vector3((int)editorCamera.position.x, (int)editorCamera.position.y, (int)editorCamera.position.z);

            for (int x = 0; x < Mathf.Abs(originOffset.x) * 2; x++)
            {
                for (int y = 0; y < Mathf.Abs(originOffset.y) * 2; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        Vector3 arrowPosition = gridEditorCameraPosition + originOffset + new Vector3(x, y, z);
                        Vector3 arrowRotation = GetGravity(arrowPosition, false).normalized;
                        UnityEditor.Handles.color = new Color(Mathf.Abs(arrowRotation.x), Mathf.Abs(arrowRotation.y), Mathf.Abs(arrowRotation.z));
                        UnityEditor.Handles.ArrowHandleCap(0, arrowPosition, Quaternion.LookRotation(arrowRotation), 1f, EventType.Repaint);
                    }
                }
            }
        }
        else
        {
            debugInitialized = false;
        }
    }
#endif
}
