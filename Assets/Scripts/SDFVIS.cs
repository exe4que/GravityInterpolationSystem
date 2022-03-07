using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;

//Signed distance field vector interpolation system.
public class SDFVIS : MonoBehaviour
{
    public int BodyGroupsCount = 1;
    public ArrowData[] Arrows;
    public Vector3[] BodyPositions;
    public Vector3[] BodyDirections;
    public float[] ArrowWeghtsFlattened;


    public ComputeShader SDFGenerationComputeShader;
    public ComputeShader DirectionsResolvingComputeShader;

    //Buffers
    ComputeBuffer _arrowsBuffer;
    ComputeBuffer _bodiesBuffer;
    ComputeBuffer _weightsBuffer;
    ComputeBuffer _resultDirectionsBuffer;

    [System.Serializable]
    public struct ArrowData
    {
        public Vector3 position;
        public Vector3 direction;
    };


    public void Initialize()
    {
        ArrowWeghtsFlattened = new float[Arrows.Length * BodyGroupsCount * 1024];
        BodyPositions = new Vector3[BodyGroupsCount * 1024];
        BodyDirections = new Vector3[BodyGroupsCount * 1024];
        InitializeBuffers();
    }

    public void PhysicsUpdate()
    {
        GenerateSDF_GPU();
        ResolveDirections();
    }

    private void InitializeBuffers()
    {
        int floatSize = sizeof(float);
        int vector3Size = floatSize * 3;
        _arrowsBuffer = new ComputeBuffer(Arrows.Length, vector3Size * 2);
        SDFGenerationComputeShader.SetBuffer(0, "arrows", _arrowsBuffer);
        _bodiesBuffer = new ComputeBuffer(BodyGroupsCount * 1024, vector3Size);
        SDFGenerationComputeShader.SetBuffer(0, "bodies", _bodiesBuffer);
        _weightsBuffer = new ComputeBuffer(Arrows.Length * BodyGroupsCount * 1024, floatSize);
        SDFGenerationComputeShader.SetBuffer(0, "weights", _weightsBuffer);

        DirectionsResolvingComputeShader.SetBuffer(0, "weights", _weightsBuffer);
        _resultDirectionsBuffer = new ComputeBuffer(BodyGroupsCount * 1024, vector3Size);
        DirectionsResolvingComputeShader.SetBuffer(0, "directions", _resultDirectionsBuffer);
        DirectionsResolvingComputeShader.SetBuffer(0, "arrows", _arrowsBuffer);
    }

    private void ResolveDirections()
    {
        if (!Application.isPlaying) return;

        //Write data
        _resultDirectionsBuffer.SetData(BodyDirections);

        //Process
        DirectionsResolvingComputeShader.Dispatch(0, BodyGroupsCount, 1, 1);

        //Read data
        _resultDirectionsBuffer.GetData(BodyDirections);
    }

    public void GenerateSDF_GPU()
    {
        if (!Application.isPlaying) return;

        //Write data
        _arrowsBuffer.SetData(Arrows);
        _bodiesBuffer.SetData(BodyPositions);
        _weightsBuffer.SetData(ArrowWeghtsFlattened);

        //Process
        SDFGenerationComputeShader.Dispatch(0, Arrows.Length, BodyGroupsCount, 1);

        //Read data
        _weightsBuffer.GetData(ArrowWeghtsFlattened);
    }

    public void DrawDebugData()
    {
        //if (Arrows != null)
        //{
        //    Gizmos.color = Color.white;
        //    for (int i = 0; i < Arrows.Length; i++)
        //    {
        //        var arrow = Arrows[i];
        //        Helper.DrawArrow(arrow.position, arrow.direction);
        //    }
        //}

        if (BodyPositions != null)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < BodyPositions.Length; i++)
            {
                var pos = BodyPositions[i];
                var dir = BodyDirections[i];
                Helper.DrawArrow(pos, dir);
            }
        }
    }
}
