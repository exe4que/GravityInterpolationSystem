using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public static class Helper
{

    /// <summary>
    /// Re-scales a value between a new float range.
    /// </summary>
    public static float Remap(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue, bool clamp = true)
    {
        if (clamp)
            OldValue = Mathf.Clamp(OldValue, OldMin, OldMax);
        return (((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin)) + NewMin;
    }

    /// <summary>
    /// Returns a random element within an Array.
    /// </summary>
    public static T GetRandomElement<T>(this T[] array)
    {
        return array[UnityEngine.Random.Range(0, array.Length)];
    }

    /// <summary>
    /// Returns a random element within a List.
    /// </summary>
    public static T GetRandomElement<T>(this List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    /// <summary>
    /// Returns whether or not this Rectransform contains another completely or in certain axis.
    /// </summary>
    public static bool ContainsRectTransform(this RectTransform rt, RectTransform otherRectTransform, Axis axis = Axis.None)
    {
        Vector3[] r = new Vector3[4];
        rt.GetWorldCorners(r);

        Vector3[] a = new Vector3[4];
        otherRectTransform.GetWorldCorners(a);

        switch (axis)
        {
            case Axis.X:
                return r[0].x <= a[0].x && r[2].x >= a[2].x;
            case Axis.Y:
                return r[0].y <= a[0].y && r[2].y >= a[2].y;
            default:
                return r[0].x <= a[0].x && r[0].y <= a[0].y && r[2].x >= a[2].x && r[2].y >= a[2].y;
        }
    }

    /// <summary>
    /// Returns whether or not this Rectransform overlaps another in one or both axis.
    /// </summary>
    public static bool OverlapsRectTransform(this RectTransform rt, RectTransform otherRectTransform, Axis axis = Axis.None)
    {
        Vector3[] r = new Vector3[4];
        rt.GetWorldCorners(r);

        Vector3[] a = new Vector3[4];
        otherRectTransform.GetWorldCorners(a);

        switch (axis)
        {
            case Axis.X:
                return r[0].x.InRange(a[0].x, a[2].x) || a[0].x.InRange(r[0].x, r[2].x);
            case Axis.Y:
                return r[0].y.InRange(a[0].y, a[2].y) || a[0].y.InRange(r[0].y, r[2].y);
            default:
                return rt.OverlapsRectTransform(otherRectTransform, Axis.X) && rt.OverlapsRectTransform(otherRectTransform, Axis.Y);
        }
    }

    /// <summary>
    /// Returns whether or not this float is in a certain float range.
    /// </summary>
    public static bool InRange(this float a, float min, float max)
    {
        return a >= min && a <= max;
    }

    /// <summary>
    /// 3D Parabola normalized movement for use with DoTween
    /// </summary>
    /// <param name="start">Starting point</param>
    /// <param name="end">End point</param>
    /// <param name="height">Parabola height at its midpoint</param>
    /// <param name="t">Normalized point to sample</param>
    /// <returns></returns>
    public static Vector3 Parabola(Vector3 start, Vector3 end, float height, float t)
    {
        float Func(float x) => 4 * (-height * x * x + height * x);

        var mid = Vector3.Lerp(start, end, t);

        return new Vector3(mid.x, Func(t) + Mathf.Lerp(start.y, end.y, t), mid.z);
    }


    /// <summary>
    /// 2D Parabola normalized movement for use with DoTween
    /// </summary>
    /// <param name="start">Starting point</param>
    /// <param name="end">End point</param>
    /// <param name="height">Parabola height at its midpoint</param>
    /// <param name="t">Normalized point to sample</param>
    public static Vector2 Parabola(Vector2 start, Vector2 end, float height, float t)
    {
        float Func(float x) => 4 * (-height * x * x + height * x);

        var mid = Vector2.Lerp(start, end, t);

        return new Vector2(mid.x, Func(t) + Mathf.Lerp(start.y, end.y, t));
    }

    /// <summary>
    /// Calculates the minimum distance between a ray (i.e. the camera direction), and a point.
    /// Useful for clicking stuff on screen without using an actual raycast.
    /// </summary>
    /// <param name="ray">Input ray.</param>
    /// <param name="point">Input point</param>
    /// <returns></returns>
    public static float DistanceToRay(Ray ray, Vector3 point)
    {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    /// <summary>
    /// Calls a method delayed by a certain number of frames
    /// </summary>
    /// <param name="caller"></param>
    /// <param name="method"></param>
    /// <param name="frames"></param>
    public static void DelayedCallInFrames(this MonoBehaviour caller, Action method, int frames)
    {
        caller.StartCoroutine(DelayedCallInFramesCo(method, frames));
    }

    public static void DelayedCallInSeconds(this MonoBehaviour caller, Action method, int seconds)
    {
        caller.StartCoroutine(DelayedCallInSecondsCo(method, seconds));
    }

    private static IEnumerator DelayedCallInFramesCo(Action method, int frames)
    {
        int frameCount = 0;
        while (frameCount < frames)
        {
            yield return null;
            frameCount++;
        }
        method.Invoke();
    }

    private static IEnumerator DelayedCallInSecondsCo(Action method, int seconds)
    {
        float startTime = Time.time;
        while (Time.time < startTime + seconds)
        {
            yield return null;
        }
        method.Invoke();
    }


    public static string Color(this string str, Color c)
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(c);
        return $"<color=#{hexColor}>{str}</color>";

    }

    public static void Resize<T>(ref T[] array, int count)
    {
        T[] tempArray = new T[count];
        for (int i = 0; i < Mathf.Min(count, array.Length); i++)
        {
            tempArray[i] = array[i];
        }
        array = tempArray;
    }

#if UNITY_EDITOR
    public static void DrawWireCapsule(Vector3 p1, Vector3 p2, float radius)
    {
        // Special case when both points are in the same position
        if (p1 == p2)
        {
            Gizmos.DrawWireSphere(p1, radius);
            return;
        }
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Gizmos.matrix))
        {
            Quaternion p1Rotation = Quaternion.LookRotation(p1 - p2);
            Quaternion p2Rotation = Quaternion.LookRotation(p2 - p1);
            // Check if capsule direction is collinear to Vector.up
            float c = Vector3.Dot((p1 - p2).normalized, Vector3.up);
            if (c == 1f || c == -1f)
            {
                // Fix rotation
                p2Rotation = Quaternion.Euler(p2Rotation.eulerAngles.x, p2Rotation.eulerAngles.y + 180f, p2Rotation.eulerAngles.z);
            }
            // First side
            UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.left, p1Rotation * Vector3.down, 180f, radius);
            UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.up, p1Rotation * Vector3.left, 180f, radius);
            UnityEditor.Handles.DrawWireDisc(p1, (p2 - p1).normalized, radius);
            // Second side
            UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.left, p2Rotation * Vector3.down, 180f, radius);
            UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.up, p2Rotation * Vector3.left, 180f, radius);
            UnityEditor.Handles.DrawWireDisc(p2, (p1 - p2).normalized, radius);
            // Lines
            UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.down * radius, p2 + p2Rotation * Vector3.down * radius);
            UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.left * radius, p2 + p2Rotation * Vector3.right * radius);
            UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.up * radius, p2 + p2Rotation * Vector3.up * radius);
            UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.right * radius, p2 + p2Rotation * Vector3.left * radius);
        }
    }

    public static void DrawWireCylinder(Vector3 p1, Vector3 p2, float radius)
    {
        // Special case when both points are in the same position
        if (p1 == p2)
        {
            Gizmos.DrawWireSphere(p1, radius);
            return;
        }
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Gizmos.matrix))
        {
            Quaternion p1Rotation = Quaternion.LookRotation(p1 - p2);
            Quaternion p2Rotation = Quaternion.LookRotation(p2 - p1);
            // Check if capsule direction is collinear to Vector.up
            float c = Vector3.Dot((p1 - p2).normalized, Vector3.up);
            if (c == 1f || c == -1f)
            {
                // Fix rotation
                p2Rotation = Quaternion.Euler(p2Rotation.eulerAngles.x, p2Rotation.eulerAngles.y + 180f, p2Rotation.eulerAngles.z);
            }
            UnityEditor.Handles.DrawWireDisc(p1, (p2 - p1).normalized, radius);
            UnityEditor.Handles.DrawWireDisc(p2, (p1 - p2).normalized, radius);
            // Lines
            UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.down * radius, p2 + p2Rotation * Vector3.down * radius);
            UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.left * radius, p2 + p2Rotation * Vector3.right * radius);
            UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.up * radius, p2 + p2Rotation * Vector3.up * radius);
            UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.right * radius, p2 + p2Rotation * Vector3.left * radius);
        }
    }

    public static void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }
#endif
}
