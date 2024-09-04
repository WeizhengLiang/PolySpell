using System.Collections.Generic;
using UnityEngine;

public class PolygonManager
{
    public List<Vector2> SegmentStartPositions { get; private set; } = new List<Vector2>();
    public List<Vector2> SegmentEndPositions { get; private set; } = new List<Vector2>();
    public float PolygonEnergy { get; private set; }

    public List<GameObject> PolygonVisualizerList { get; private set; } = new ();

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

    public List<Vector2> GetPolygonPoints(Vector2 intersectionPoint, int polygonType)
    {
        List<Vector2> points = new List<Vector2> { intersectionPoint };
        int startIndex = SegmentStartPositions.Count - polygonType + 1;
        for (int i = startIndex; i < SegmentStartPositions.Count; i++)
        {
            points.Add(SegmentEndPositions[i]);
        }
        return points;
    }

    public void ClearPolygonVisualizerList()
    {
        foreach (var visualizer in PolygonVisualizerList)
        {
            if (visualizer != null)
            {
                GameObject.Destroy(visualizer);
            }
        }
        PolygonVisualizerList.Clear();
    }

    public void AddPolygonVisualizer(GameObject visualizer)
    {
        PolygonVisualizerList.Add(visualizer);
    }

    public void RemovePolygonVisualizer(GameObject visualizer)
    {
        PolygonVisualizerList.Remove(visualizer);
    }
}