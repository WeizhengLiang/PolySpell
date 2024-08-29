using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Kalkatos.DottedArrow;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public GameManager GameManager;
    public EnergySystem EnergySystem;
    public HealthSystem HealthSystem;
    public ScoringSystem ScoringSystem;
    public SlowMotionManager SlowMotionManager;
    public GameObject polygonVisualizerPrefab;
    public GameObject Marker;
    public GameObject Marker2;
    public GameObject notificationDisplayPrefab;
    public Arrow Arrow;
    public BallSpawner BallSpawner;

    private MovementController movementController;
    private PolygonManager polygonManager;
    private float energyConsumedForCurrentTrait = 0f;
    private Vector2 lastPosition;
    private bool isMoving = false;
    private Rigidbody2D rb;
    private TrailRenderer _trailRenderer;
    private Dictionary<Vector2, (float, int)> intersectionResults = new ();
    private bool isNewPolygon;
    private float timeEnergyRunOut = -1f;
    private bool afterEnergyDrained;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _trailRenderer = GetComponent<TrailRenderer>();
        movementController = new MovementController(rb);
        polygonManager = new PolygonManager();
    }

    void Start()
    {
        lastPosition = transform.position;
        isNewPolygon = true;
    }

    void Update()
    {
        Vector2 currentPosition = transform.position;

        HandleMouseInput(currentPosition);
        HandleMovement(currentPosition);
    }

    private void HandleMouseInput(Vector2 currentPosition)
    {
        if (Input.GetMouseButtonDown(0) && GameManager.gameUI.activeSelf && !SlowMotionManager.inSlowMotion && !GameManager.IsPaused)
        {
            SlowMotionManager.EnterSlowMotion();
            Arrow.SetupAndActivate(transform);
        }
        
        if (Input.GetMouseButtonUp(0) && GameManager.gameUI.activeSelf && SlowMotionManager.inSlowMotion && !GameManager.IsPaused)
        {
            Arrow.Deactivate();
            if (!EnergySystem.IsEnergyEmpty)
            {
                if (isNewPolygon)
                {
                    lastPosition = transform.position;
                    isNewPolygon = false;
                }
                _trailRenderer.enabled = true;
            }
            
            SlowMotionManager.ExitSlowMotion();

            rb.velocity = Vector2.zero;

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            movementController.SetDirection(mousePosition - currentPosition);
            movementController.Shoot();

            if (!EnergySystem.IsEnergyEmpty)
            {
                if (lastPosition != currentPosition)
                {
                    float distance = Vector2.Distance(lastPosition, currentPosition);
                    energyConsumedForCurrentTrait += distance * EnergySystem.energyConsumptionRate;
                    polygonManager.AddSegment(lastPosition, currentPosition);
                }
                lastPosition = currentPosition;
                CalculatePotentialIntersections(lastPosition, movementController.CurrentDirection);
                isMoving = true;
            }
        }
    }

    private void HandleMovement(Vector2 currentPosition)
    {
        if (isMoving)
        {
            float currentLength = Vector2.Distance(lastPosition, currentPosition);
            foreach (var keyValuePair in intersectionResults)
            {
                if (currentLength >= keyValuePair.Value.Item1)
                {
                    int polygonType = keyValuePair.Value.Item2;
                    var startEndTrailPoint = keyValuePair.Key;
                    Debug.Log($"{GetPolygonType(polygonType)} formed");
                    polygonManager.SetPolygonEnergy(energyConsumedForCurrentTrait);
                    ShowPolygon(keyValuePair.Key, polygonType);
                    if (polygonType == 4)
                    {
                        BreakEvilBallsInsidePolygon(startEndTrailPoint);
                    }
                    else if (polygonType == 5)
                    {
                        HealPlayer();
                    }
                    else if (polygonType > 5)
                    {
                        TransformTraitIntoNormalBalls();
                    }
                    else
                    {
                        KillBallsInsidePolygon(startEndTrailPoint);
                    }
                    if(polygonType != -1) 
                        TransformExtraTraitIntoNormalBalls(startEndTrailPoint, polygonType);
                    ClearData();
                    _trailRenderer.Clear();
                    _trailRenderer.enabled = false;
                    isMoving = false;
                    isNewPolygon = true;
                    break;
                }
            }
            float energyCost = EnergySystem.energyConsumptionRate * currentLength * Time.deltaTime;
            energyConsumedForCurrentTrait += energyCost;
            if (!EnergySystem.ConsumeEnergy(energyCost))
            {
                if (_trailRenderer.positionCount > 0)
                {
                    ClearTrailAndRestoreEnergy();
                }
                ClearData();
                isMoving = false;
                isNewPolygon = true;
            }
            else
            {
                _trailRenderer.time = 50f;
            }
        }
    }

    public bool IsPointInTrail(TrailRenderer trailRenderer, Vector3 point, float tolerance = 0.1f)
    {
        int positionCount = trailRenderer.positionCount;
        Vector3[] positions = new Vector3[positionCount];
        trailRenderer.GetPositions(positions);

        for (int i = 0; i < positionCount; i++)
        {
            if (Vector3.Distance(positions[i], point) <= tolerance)
            {
                return true;
            }
        }
        return false;
    }

    void ClearData()
    {
        polygonManager.ClearSegments();
        energyConsumedForCurrentTrait = 0f;
    }

    void CalculatePotentialIntersections(Vector2 start, Vector2 direction)
    {
        intersectionResults.Clear();

        for (int i = 0; i < polygonManager.SegmentStartPositions.Count; i++)
        {
            if (LineSegmentsIntersect(start, direction, polygonManager.SegmentStartPositions[i], polygonManager.SegmentEndPositions[i], out Vector2 intersection))
            {
                float distance = Vector2.Distance(start, intersection);
                int polygonType = DeterminePolygonType(i);
                intersectionResults[intersection] = (distance, polygonType);
                Debug.Log("Potential polygon: " + GetPolygonType(polygonType));
            }
        }
    }
    
    int DeterminePolygonType(int segmentIndex)
    {
        int edgeCount = polygonManager.SegmentStartPositions.Count - segmentIndex;
        if (edgeCount == 2)
        {
            return 3;  // Triangle
        }
        else if (edgeCount == 3)
        {
            return 4;  // Quadrilateral
        }
        else if (edgeCount == 4)
        {
            return 5;  // Pentagon
        }
        return edgeCount + 1;  // Undefined or invalid polygon
    }

    string GetPolygonType(int sides)
    {
        switch (sides)
        {
            case 3: return "Triangle";
            case 4: return "Quadrilateral";
            case 5: return "Pentagon";
            default: return "Invalid polygon";
        }
    }

    bool LineSegmentsIntersect(Vector2 p1, Vector2 dir, Vector2 p2, Vector2 q2, out Vector2 intersection)
    {
        intersection = Vector2.zero;

        float a1 = dir.y;
        float b1 = -dir.x;
        float c1 = a1 * p1.x + b1 * p1.y;

        float a2 = q2.y - p2.y;
        float b2 = p2.x - q2.x;
        float c2 = a2 * p2.x + b2 * p2.y;

        float determinant = a1 * b2 - a2 * b1;

        if (determinant == 0)
        {
            return false; // Parallel lines
        }
        else
        {
            float x = (b2 * c1 - b1 * c2) / determinant;
            float y = (a1 * c2 - a2 * c1) / determinant;
            intersection = new Vector2(x, y);

            var OnSegment2 = IsPointOnLineSegment(p2, q2, intersection);
            var OnSegment1 = IsPointOnLineSegment(p1, intersection, intersection);
            var dotProduct = Vector2.Dot(intersection - p1, dir);
            
            if (OnSegment2 && OnSegment1 && intersection != p1 && dotProduct > 0)
            {
                return true;
            }
        }

        return false;
    }

    bool IsPointOnLineSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        return (r.x <= Mathf.Max(p.x, q.x) && r.x >= Mathf.Min(p.x, q.x) && r.y <= Mathf.Max(p.y, q.y) && r.y >= Mathf.Min(p.y, q.y));
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("NormalBall"))
        {
            NormalBall normalBall = col.gameObject.GetComponent<NormalBall>();

            if (normalBall.powerUp == NormalBall.PowerUpType.Size)
            {
                HandleSizePowerUp();
            }
            else if (normalBall.powerUp == NormalBall.PowerUpType.Speed)
            {
                HandleSpeedPowerUp();
            }
            else if(normalBall.powerUp == NormalBall.PowerUpType.None)
            {
                StartCoroutine(VFXManager.Instance.SpawnTextVFX($"+{1}", ColorCombo.Hex2Color(ColorCombo.AddNormalScore_3), normalBall.transform.position));
            }
            
            normalBall.TriggerDieVFX(col.transform.position);
            
            BallSpawner.normalBallPool.ReturnObject(col.gameObject);
            EnergySystem.GainEnergy(EnergySystem.energyGainAmount);
            ScoringSystem.AddScore(1);
        }
    }

    private void HandleSizePowerUp()
    {
        var scale = transform.localScale - new Vector3(0.1f, 0.1f, 0f);
        if (scale.x >= 0.5f)
        {
            transform.localScale = scale;
            StartCoroutine(VFXManager.Instance.SpawnTextVFX("Size Down!", ColorCombo.Hex2Color(ColorCombo.RegularText_3),transform.position));
        }
        else
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            StartCoroutine(VFXManager.Instance.SpawnTextVFX("Min Size!", ColorCombo.Hex2Color(ColorCombo.RegularText_3),transform.position));
        }
        _trailRenderer.startWidth = transform.localScale.x;
    }

    private void HandleSpeedPowerUp()
    {
        StartCoroutine(VFXManager.Instance.SpawnTextVFX("Speed Up!", ColorCombo.Hex2Color(ColorCombo.RegularText_3),transform.position));
        movementController.SetDirection(movementController.CurrentDirection * 1.15f);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        var normal = col.contacts[0].normal;
        movementController.Bounce(normal);
        
        if (isMoving)
        {
            Vector2 currentPosition = transform.position;
            if (lastPosition != currentPosition)
            {
                float distance = Vector2.Distance(lastPosition, currentPosition);
                energyConsumedForCurrentTrait += distance * EnergySystem.energyConsumptionRate;
                polygonManager.AddSegment(lastPosition, currentPosition);
                lastPosition = currentPosition;
                CalculatePotentialIntersections(lastPosition, movementController.CurrentDirection);
            }
        }
        
        if (col.gameObject.CompareTag("EvilBall"))
        {
            Vector2 contactPoint = col.contacts[0].point;
            StartCoroutine(VFXManager.Instance.SpawnTextVFX($"-{10}", ColorCombo.Hex2Color(ColorCombo.DamageText_3), transform.position));
            VFXManager.Instance.SpawnVFX(VFXType.hitEffect ,VFXManager.Instance.hitEffectPrefab, contactPoint);
            HealthSystem.TakeDamage(10f);
        }
    }
    
    void ShowPolygon(Vector2 intersectionPoint, int polygonEdgesCount)
    {
        if(polygonEdgesCount < 3) return;
        
        List<Vector2> polygonPoints = polygonManager.GetPolygonPoints(intersectionPoint, polygonEdgesCount);
        Color polygonColor = GetPolygonColor(polygonPoints.Count);

        GameObject polygonVisualizer = Instantiate(polygonVisualizerPrefab, Vector3.zero, Quaternion.identity);
        polygonVisualizer.GetComponent<PolygonVisualizer>().SetPoints(polygonPoints, polygonColor);
    }
    
    Color GetPolygonColor(int sides)
    {
        switch (sides)
        {
            case 3: return Color.red;
            case 4: return Color.blue;
            case 5: return Color.green;
            default: return Color.gray;
        }
    }
    
    void TransformExtraTraitIntoNormalBalls(Vector2 intersectionPoint, int polygonEdgesCount)
    {
        List<Vector2> extraSegmentStartPositions = new List<Vector2>();
        List<Vector2> extraSegmentEndPositions = new List<Vector2>();
        
        var extraSegmentsCount = polygonManager.SegmentStartPositions.Count - polygonEdgesCount + 1;
        
        for (int i = 0; i < extraSegmentsCount; i++)
        {
            extraSegmentStartPositions.Add(polygonManager.SegmentStartPositions[i]);
            extraSegmentEndPositions.Add(polygonManager.SegmentEndPositions[i]);
        }
        
        extraSegmentStartPositions.Add(polygonManager.SegmentStartPositions[extraSegmentsCount]);
        extraSegmentEndPositions.Add(intersectionPoint);
        
        float extraLength = CalculateTotalLength(extraSegmentStartPositions, extraSegmentEndPositions);
        int numberOfBalls = Mathf.CeilToInt(extraLength * EnergySystem.energyConsumptionRate / 10f);

        for (int i = 0; i < numberOfBalls; i++)
        {
            int index = i % extraSegmentStartPositions.Count;
            Vector2 start = extraSegmentStartPositions[index];
            Vector2 end = extraSegmentEndPositions[index];
            Vector2 spawnPosition = Vector2.Lerp(start, end, Random.Range(0f, 1f));
            StartCoroutine(BallSpawner.SpawnNormalBallWithAnimation(spawnPosition, true));
        }
    }
    
    float CalculateTotalLength(List<Vector2> startPositions, List<Vector2> endPositions)
    {
        float totalLength = 0f;
        for (int i = 0; i < startPositions.Count; i++)
        {
            totalLength += Vector2.Distance(startPositions[i], endPositions[i]);
        }
        return totalLength;
    }
    
    void TransformTraitIntoNormalBalls()
    {
        int numberOfBalls = Mathf.CeilToInt(polygonManager.PolygonEnergy / 10f);
        for (int i = 0; i < numberOfBalls; i++)
        {
            Vector2 spawnPosition = Vector2.Lerp(polygonManager.SegmentStartPositions[i % polygonManager.SegmentStartPositions.Count], 
                                                 polygonManager.SegmentEndPositions[i % polygonManager.SegmentEndPositions.Count], 
                                                 Random.Range(0f, 1f));
            StartCoroutine(BallSpawner.SpawnNormalBallWithAnimation(spawnPosition, true));
        }
    }
    
    void HealPlayer()
    {
        HealthSystem.Heal(polygonManager.PolygonEnergy);
    }
    
    void BreakEvilBallsInsidePolygon(Vector2 startEndTrailPoint)
    {
        List<Vector2> polygonPoints = new List<Vector2> { startEndTrailPoint };
        polygonPoints.AddRange(polygonManager.SegmentEndPositions);
        polygonPoints.Add(startEndTrailPoint);

        foreach (GameObject ball in BallSpawner.evilBallPool.activeObjList)
        {
            if (IsPointInPolygon(ball.transform.position, polygonPoints))
            {
                ball.GetComponent<EvilBall>().BreakShield();
            }
        }
    }

    void KillBallsInsidePolygon(Vector2 startEndTrailPoint)
    {
        List<Vector2> polygonPoints = new List<Vector2> { startEndTrailPoint };
        polygonPoints.AddRange(polygonManager.SegmentEndPositions);
        polygonPoints.Add(startEndTrailPoint);

        for (int i = BallSpawner.normalBallPool.activeObjList.Count - 1; i >= 0; i--)
        {
            var ball = BallSpawner.normalBallPool.activeObjList[i];
            if (IsPointInPolygon(ball.transform.position, polygonPoints))
            {
                BallSpawner.normalBallPool.ReturnObject(ball);
                EnergySystem.GainEnergy(EnergySystem.energyGainAmount);
                ScoringSystem.AddScore(1);
            }
        }

        for (int i = BallSpawner.evilBallPool.activeObjList.Count - 1; i >= 0; i--)
        {
            var ball = BallSpawner.evilBallPool.activeObjList[i];
            if (IsPointInPolygon(ball.transform.position, polygonPoints))
            {
                ball.GetComponent<EvilBall>().TakeDamage((int)polygonManager.PolygonEnergy);
            }
        }

        energyConsumedForCurrentTrait = 0f;
    }

    bool IsPointInPolygon(Vector2 point, List<Vector2> polygonPoints)
    {
        int intersectCount = 0;
        for (int i = 1; i < polygonPoints.Count; i++)
        {
            Vector2 v1 = polygonPoints[i];
            Vector2 v2 = polygonPoints[i - 1];

            if ((v1.y > point.y) != (v2.y > point.y) &&
                (point.x < (v2.x - v1.x) * (point.y - v1.y) / (v2.y - v1.y) + v1.x))
            {
                intersectCount++;
            }
        }
        return (intersectCount % 2) == 1;
    }
    
    private void ClearTrailAndRestoreEnergy()
    {
        float trailLength = CalculateTrailLength();
        float energyRestored = trailLength * EnergySystem.energyConsumptionRate;
        EnergySystem.GainEnergy(energyRestored);
        _trailRenderer.Clear();
        _trailRenderer.enabled = false;
    }

    private float CalculateTrailLength()
    {
        float length = 0f;
        for (int i = 0; i < _trailRenderer.positionCount - 1; i++)
        {
            length += Vector3.Distance(_trailRenderer.GetPosition(i), _trailRenderer.GetPosition(i + 1));
        }
        return length;
    }
    
    public void ResetPlayer()
    {
        StopAllCoroutines();
        
        transform.position = Vector2.zero;
        rb.velocity = Vector2.zero;
        transform.localScale = new Vector3(0.6f, 0.6f, 1);
        movementController = new MovementController(rb);

        _trailRenderer.Clear();
        _trailRenderer.enabled = false;
        _trailRenderer.startWidth = 0.6f;

        isMoving = false;
        isNewPolygon = true;
        energyConsumedForCurrentTrait = 0f;
        lastPosition = transform.position;
        polygonManager.ClearSegments();

        HealthSystem.ResetHealth();
        EnergySystem.ResetEnergy();
    }
}