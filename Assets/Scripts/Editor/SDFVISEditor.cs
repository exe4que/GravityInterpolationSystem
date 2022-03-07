using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SDFVIS))]
public class SDFVISEditor : Editor
{
    SerializedProperty m_BodyGroupsCount;
    SerializedProperty m_Arrows;
    SerializedProperty m_SDFComputeShader;
    SerializedProperty m_DirectionsComputeShader;
    SerializedProperty m_BodyDirections;
    private bool _editMode = false;
    private static bool _directionsFoldout = false;
    private static int _selectedPage = 0;
    private static int _arrowsCount = 16;

    private static Transform _referenceTransform;
    private static int fromArrowIndex;
    private static int toArrowIndex;

    void OnEnable()
    {
        m_BodyGroupsCount = serializedObject.FindProperty("BodyGroupsCount");
        m_Arrows = serializedObject.FindProperty("Arrows");
        m_SDFComputeShader = serializedObject.FindProperty("SDFGenerationComputeShader");
        m_DirectionsComputeShader = serializedObject.FindProperty("DirectionsResolvingComputeShader");
        m_BodyDirections = serializedObject.FindProperty("BodyDirections");
        _arrowsCount = m_Arrows.arraySize;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_BodyGroupsCount);
        EditorGUILayout.PropertyField(m_SDFComputeShader);
        EditorGUILayout.PropertyField(m_DirectionsComputeShader);
        _directionsFoldout = EditorGUILayout.Foldout(_directionsFoldout, "Result Directions");
        if (_directionsFoldout)
        {
            for (int i = 0; i < m_BodyDirections.arraySize; i++)
            {
                GUILayout.Label($"[{i}]: " + m_BodyDirections.GetArrayElementAtIndex(i).vector3Value.ToString());
            }
            
        }
        _referenceTransform = (Transform) EditorGUILayout.ObjectField("Reference transform", _referenceTransform, typeof(Transform), true);
        EditorGUI.BeginChangeCheck();
        _editMode = EditorGUILayout.Toggle("Edit mode", _editMode);
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }

        if (_editMode)
        {
            EditorGUILayout.BeginHorizontal();
            fromArrowIndex = EditorGUILayout.IntField("From", fromArrowIndex);
            toArrowIndex = EditorGUILayout.IntField("To", toArrowIndex);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Point to point"))
            {
                if (_referenceTransform != null && fromArrowIndex < toArrowIndex)
                {
                    SDFVIS t = target as SDFVIS;
                    int to = Mathf.Min(toArrowIndex, t.Arrows.Length);
                    for (int i = fromArrowIndex; i < to; i++)
                    {
                        t.Arrows[i].direction = (_referenceTransform.position - t.Arrows[i].position).normalized;
                    }
                    SceneView.RepaintAll();
                }
            }
            if (GUILayout.Button("Point away from point"))
            {
                if (_referenceTransform != null && fromArrowIndex < toArrowIndex)
                {
                    SDFVIS t = target as SDFVIS;
                    int to = Mathf.Min(toArrowIndex, t.Arrows.Length);
                    for (int i = fromArrowIndex; i < to; i++)
                    {
                        t.Arrows[i].direction = (t.Arrows[i].position - _referenceTransform.position).normalized;
                    }
                    SceneView.RepaintAll();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Separator();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = false;
        _arrowsCount = EditorGUILayout.IntField("Arrows count", _arrowsCount, GUILayout.Width(Screen.width * 0.8f - 90));
        GUI.enabled = _editMode;
        EditorGUILayout.BeginVertical();
        if(GUILayout.Button("^", GUILayout.Width(50), GUILayout.Height(8)))
        {
            _arrowsCount++;
        }
        if (GUILayout.Button("v", GUILayout.Width(50), GUILayout.Height(8)))
        {
            _arrowsCount = Mathf.Max(0, --_arrowsCount);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            SDFVIS t = (SDFVIS)target;
            Helper.Resize(ref t.Arrows, _arrowsCount);
        }

        int pageCount = this.m_Arrows.arraySize / 10;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Page: {_selectedPage}");
        GUI.enabled = _selectedPage > 0;
        if (GUILayout.Button("<"))
        {
            _selectedPage = Mathf.Max(0, _selectedPage - 1);
        }
        GUI.enabled = _selectedPage < pageCount;
        if (GUILayout.Button(">"))
        {
            _selectedPage = Mathf.Min(_selectedPage + 1, pageCount);
        }
        EditorGUILayout.EndHorizontal();
        GUI.enabled = _editMode;
        int count = Mathf.Min((_selectedPage + 1) * 10, this.m_Arrows.arraySize);
        for (int i = _selectedPage * 10; i < count; i++)
        {
            var arrow = this.m_Arrows.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(arrow.FindPropertyRelative("position"), new GUIContent($"[{i}] Position"));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(arrow.FindPropertyRelative("direction"), new GUIContent($"      Direction"));
            if (GUILayout.Button("N", GUILayout.Width(20)))
            {
                SerializedProperty m_dir = arrow.FindPropertyRelative("direction");
                m_dir.vector3Value = m_dir.vector3Value.normalized;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        serializedObject.ApplyModifiedProperties();
    }


    public void OnSceneGUI()
    {
        SDFVIS t = target as SDFVIS;
        if (t.Arrows == null) return;

        for (int i = 0; i < t.Arrows.Length; i++)
        {
            var arrow = t.Arrows[i];
            Vector3 gizmoPos = arrow.position - arrow.direction * 5f;
            Vector3 direction = arrow.direction != Vector3.zero ? arrow.direction : Vector3.forward;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.color = Color.yellow;
            Handles.ArrowHandleCap(0, gizmoPos, Quaternion.LookRotation(direction), 5f, EventType.Repaint);
            Handles.Label(arrow.position, i.ToString());

            //Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            if (!_editMode) continue;
            EditorGUI.BeginChangeCheck();
            if (Tools.current == Tool.Rotate)
            {
                Quaternion rot = Quaternion.LookRotation(direction);
                rot = Handles.DoRotationHandle(rot, arrow.position);
                arrow.direction = rot * Vector3.forward;
            }
            else
            {
                Quaternion rotation = UnityEditor.Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : Quaternion.LookRotation(direction);
                t.Arrows[i].position = Handles.DoPositionHandle(arrow.position, rotation);
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
            }
        }
    }
}
