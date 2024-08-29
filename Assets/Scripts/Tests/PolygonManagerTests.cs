using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

public class PolygonManagerTests
{
    private PolygonManager polygonManager;

    [SetUp]
    public void Setup()
    {
        polygonManager = new PolygonManager();
    }

    [Test]
    public void AddSegment_ShouldAddToLists()
    {
        Vector2 start = new Vector2(0, 0);
        Vector2 end = new Vector2(1, 1);
        polygonManager.AddSegment(start, end);

        Assert.That(polygonManager.SegmentStartPositions.Count, Is.EqualTo(1));
        Assert.That(polygonManager.SegmentEndPositions.Count, Is.EqualTo(1));
        Assert.That(polygonManager.SegmentStartPositions[0], Is.EqualTo(start));
        Assert.That(polygonManager.SegmentEndPositions[0], Is.EqualTo(end));
    }

    [Test]
    public void ClearSegments_ShouldEmptyLists()
    {
        polygonManager.AddSegment(Vector2.zero, Vector2.one);
        polygonManager.ClearSegments();

        Assert.That(polygonManager.SegmentStartPositions.Count, Is.EqualTo(0));
        Assert.That(polygonManager.SegmentEndPositions.Count, Is.EqualTo(0));
    }

    [Test]
    public void SetPolygonEnergy_ShouldSetEnergy()
    {
        float energy = 100f;
        polygonManager.SetPolygonEnergy(energy);

        Assert.That(polygonManager.PolygonEnergy, Is.EqualTo(energy));
    }

    [Test]
    public void GetPolygonPoints_ShouldReturnCorrectPoints()
    {
        Vector2 intersection = new Vector2(1.5f, 1.5f);
        polygonManager.AddSegment(new Vector2(1, 1), new Vector2(2, 2));
        polygonManager.AddSegment(new Vector2(2, 2), new Vector2(4, 4));
        polygonManager.AddSegment(new Vector2(4, 4), new Vector2(6, 6));

        List<Vector2> points = polygonManager.GetPolygonPoints(intersection, 4);

        Assert.That(points.Count, Is.EqualTo(4));
        Assert.That(points[0], Is.EqualTo(intersection));
        Assert.That(points[1], Is.EqualTo(new Vector2(6, 6)));
        Assert.That(points[2], Is.EqualTo(new Vector2(4, 4)));
        Assert.That(points[3], Is.EqualTo(new Vector2(2, 2)));
    }
}