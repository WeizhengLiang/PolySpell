using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PolygonHandler
{
    private readonly PolygonManager polygonManager;
    private readonly EnergySystem energySystem;
    private readonly HealthSystem healthSystem;
    private readonly ScoringSystem scoringSystem;
    private readonly BallSpawner ballSpawner;
    private readonly GameObject polygonVisualizerPrefab;

    public PolygonHandler(PolygonManager polygonManager, EnergySystem energySystem, HealthSystem healthSystem, ScoringSystem scoringSystem, BallSpawner ballSpawner, GameObject polygonVisualizerPrefab)
    {
        this.polygonManager = polygonManager;
        this.energySystem = energySystem;
        this.healthSystem = healthSystem;
        this.scoringSystem = scoringSystem;
        this.ballSpawner = ballSpawner;
        this.polygonVisualizerPrefab = polygonVisualizerPrefab;
    }

    public void HandlePolygonFormation(Vector2 intersectionPoint, int polygonType)
    {
        Debug.Log($"{GetPolygonType(polygonType)} formed");
        polygonManager.SetPolygonEnergy(energySystem.GetConsumedEnergy());
        ShowPolygon(intersectionPoint, polygonType);
        
        switch (polygonType)
        {
            case 4:
                BreakEvilBallsInsidePolygon(intersectionPoint);
                break;
            case 5:
                HealPlayer();
                break;
            case > 5:
                TransformTraitIntoNormalBalls();
                break;
            default:
                KillBallsInsidePolygon(intersectionPoint);
                break;
        }
        
        if (polygonType != -1) 
        {
            TransformExtraTraitIntoNormalBalls(intersectionPoint, polygonType);
        }
    }

    private void ShowPolygon(Vector2 intersectionPoint, int polygonType)
    {
        if (polygonVisualizerPrefab == null)
        {
            Debug.LogError("PolygonVisualizerPrefab is not set. Please assign it in the PlayerController inspector.");
            return;
        }

        List<Vector2> polygonPoints = polygonManager.GetPolygonPoints(intersectionPoint, polygonType);
        Color polygonColor = GetPolygonColor(polygonType);
        GameObject polygonVisualizer = Object.Instantiate(polygonVisualizerPrefab, Vector3.zero, Quaternion.identity);
        polygonVisualizer.GetComponent<PolygonVisualizer>().SetPoints(polygonPoints, polygonColor);
    }

    private void BreakEvilBallsInsidePolygon(Vector2 intersectionPoint)
    {
        List<Vector2> polygonPoints = polygonManager.GetPolygonPoints(intersectionPoint, 4);
        List<EvilBall> evilBallsInside = GetEvilBallsInsidePolygon(polygonPoints);
        foreach (var evilBall in evilBallsInside)
        {
            evilBall.BreakShield();
        }
    }

    private void HealPlayer()
    {
        float healAmount = polygonManager.PolygonEnergy;
        healthSystem.Heal(healAmount);
    }

    private void TransformTraitIntoNormalBalls()
    {
        int normalBallsToSpawn = Mathf.FloorToInt(polygonManager.PolygonEnergy / 10f);
        for (int i = 0; i < normalBallsToSpawn; i++)
        {
            Vector2 spawnPosition = GetRandomPositionInsidePolygon();
            ballSpawner.SpawnNormalBallWithAnimation(spawnPosition, true);
        }
    }

    private void KillBallsInsidePolygon(Vector2 intersectionPoint)
    {
        List<Vector2> polygonPoints = polygonManager.GetPolygonPoints(intersectionPoint, 3);
        List<GameObject> ballsInside = GetBallsInsidePolygon(polygonPoints);
        foreach (var ball in ballsInside)
        {
            if (ball.CompareTag("NormalBall"))
            {
                ballSpawner.normalBallPool.ReturnObject(ball);
                energySystem.GainEnergy(energySystem.EnergyGainAmount);
                scoringSystem.AddScore(1);
            }
            else if (ball.CompareTag("EvilBall"))
            {
                ball.GetComponent<EvilBall>().TakeDamage((int)polygonManager.PolygonEnergy);
            }
        }
    }

    private void TransformExtraTraitIntoNormalBalls(Vector2 intersectionPoint, int polygonType)
    {
        List<Vector2> extraSegmentStartPositions = new List<Vector2>();
        List<Vector2> extraSegmentEndPositions = new List<Vector2>();
        
        int extraSegmentsCount = polygonManager.SegmentStartPositions.Count - polygonType + 1;
        
        for (int i = 0; i < extraSegmentsCount; i++)
        {
            extraSegmentStartPositions.Add(polygonManager.SegmentStartPositions[i]);
            extraSegmentEndPositions.Add(polygonManager.SegmentEndPositions[i]);
        }
        
        extraSegmentStartPositions.Add(polygonManager.SegmentStartPositions[extraSegmentsCount]);
        extraSegmentEndPositions.Add(intersectionPoint);
        
        float extraLength = CalculateTotalLength(extraSegmentStartPositions, extraSegmentEndPositions);
        int numberOfBalls = Mathf.CeilToInt(extraLength * energySystem.EnergyConsumptionRate / 10f);

        for (int i = 0; i < numberOfBalls; i++)
        {
            int index = i % extraSegmentStartPositions.Count;
            Vector2 start = extraSegmentStartPositions[index];
            Vector2 end = extraSegmentEndPositions[index];
            Vector2 spawnPosition = Vector2.Lerp(start, end, Random.Range(0f, 1f));
            ballSpawner.SpawnNormalBallWithAnimation(spawnPosition, true);
        }
    }

    private List<EvilBall> GetEvilBallsInsidePolygon(List<Vector2> polygonPoints)
    {
        return ballSpawner.evilBallPool.activeObjList
            .Select(ball => ball.GetComponent<EvilBall>())
            .Where(evilBall => IsPointInPolygon(evilBall.transform.position, polygonPoints))
            .ToList();
    }

    private List<GameObject> GetBallsInsidePolygon(List<Vector2> polygonPoints)
    {
        List<GameObject> ballsInside = new List<GameObject>();
        ballsInside.AddRange(ballSpawner.normalBallPool.activeObjList.Where(ball => IsPointInPolygon(ball.transform.position, polygonPoints)));
        ballsInside.AddRange(ballSpawner.evilBallPool.activeObjList.Where(ball => IsPointInPolygon(ball.transform.position, polygonPoints)));
        return ballsInside;
    }

    private Vector2 GetRandomPositionInsidePolygon()
    {
        List<Vector2> polygonPoints = polygonManager.GetPolygonPoints(Vector2.zero, polygonManager.SegmentStartPositions.Count);
        Bounds bounds = GetPolygonBounds(polygonPoints);
        Vector2 randomPoint;
        do
        {
            randomPoint = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );
        } while (!IsPointInPolygon(randomPoint, polygonPoints));
        return randomPoint;
    }

    private bool IsPointInPolygon(Vector2 point, List<Vector2> polygonPoints)
    {
        int intersectCount = 0;
        for (int i = 0; i < polygonPoints.Count; i++)
        {
            Vector2 vert1 = polygonPoints[i];
            Vector2 vert2 = polygonPoints[(i + 1) % polygonPoints.Count];
            if (((vert1.y > point.y) != (vert2.y > point.y)) &&
                (point.x < (vert2.x - vert1.x) * (point.y - vert1.y) / (vert2.y - vert1.y) + vert1.x))
            {
                intersectCount++;
            }
        }
        return (intersectCount % 2) == 1;
    }

    private Bounds GetPolygonBounds(List<Vector2> polygonPoints)
    {
        if (polygonPoints.Count == 0)
            return new Bounds();

        Vector2 min = polygonPoints[0];
        Vector2 max = polygonPoints[0];
        for (int i = 1; i < polygonPoints.Count; i++)
        {
            min = Vector2.Min(min, polygonPoints[i]);
            max = Vector2.Max(max, polygonPoints[i]);
        }
        return new Bounds((min + max) / 2, max - min);
    }

    private float CalculateTotalLength(List<Vector2> startPositions, List<Vector2> endPositions)
    {
        float totalLength = 0f;
        for (int i = 0; i < startPositions.Count; i++)
        {
            totalLength += Vector2.Distance(startPositions[i], endPositions[i]);
        }
        return totalLength;
    }

    private string GetPolygonType(int polygonType)
    {
        return polygonType switch
        {
            3 => "Triangle",
            4 => "Square",
            5 => "Pentagon",
            _ => $"{polygonType}-gon"
        };
    }

    private Color GetPolygonColor(int polygonType)
    {
        return polygonType switch
        {
            3 => Color.red,
            4 => Color.blue,
            5 => Color.green,
            _ => Color.gray
        };
    }
}