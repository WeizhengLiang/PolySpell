using System.Collections.Generic;
using UnityEngine;

public class PolygonManager
{
    public List<Vector2> SegmentStartPositions { get; private set; } = new List<Vector2>();
    public List<Vector2> SegmentEndPositions { get; private set; } = new List<Vector2>();
    public float PolygonEnergy { get; private set; }

    public void AddSegment(Vector2 start, Vector2 end)
    {
        SegmentStartPositions.Add(start);
        SegmentEndPositions.Add(end);
    }

    public void ClearSegments()
    {
        SegmentStartPositions.Clear();
        SegmentEndPositions.Clear();
    }

    public void SetPolygonEnergy(float energy)
    {
        PolygonEnergy = energy;
    }

    public List<Vector2> GetPolygonPoints(Vector2 intersectionPoint, int polygonEdgesCount)
    {
        List<Vector2> polygonPoints = new List<Vector2> { intersectionPoint };
        for (int i = SegmentEndPositions.Count - 1; i >= SegmentEndPositions.Count - polygonEdgesCount + 1; i--)
        {
            polygonPoints.Add(SegmentEndPositions[i]);
        }
        return polygonPoints;
    }
}