using UnityEngine;
using System.Collections.Generic;
using Kalkatos.DottedArrow;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public GameManager GameManager;
    public EnergySystem EnergySystem;
    public HealthSystem HealthSystem;
    public ScoringSystem ScoringSystem;
    public SlowMotionManager SlowMotionManager;
    public List<Vector2> segmentStartPositions = new List<Vector2>();
    public List<Vector2> segmentEndPositions = new List<Vector2>();
    public float polygonEnergy;  // Energy used to form the polygon (calculated dynamically)
    public GameObject normalBallPrefab;  // Reference to the normal ball prefab
    public int initialSpawnCount = 10;  // Number of normal balls to spawn at the start
    public ObjectPool normalBallPool;  // Reference to the Object Pool for normal balls
    public ObjectPool evilBallPool;  // Reference to the Object Pool for evil balls
    public GameObject polygonVisualizerPrefab;  // Reference to the Polygon Visualizer prefab
    public GameObject Marker;
    public GameObject notificationDisplayPrefab;  // Reference to the notification display prefab
    public Arrow Arrow;
    public BallSpawner BallSpawner;

    
    private float energyConsumedForCurrentTrait = 0f;  // Track energy consumed for current trait
    private Vector2 lastPosition;
    private bool isMoving = false;
    private float shootForce = 7f;  // Adjust as necessary
    private float bounceForce = 5f;
    private Rigidbody2D rb;
    private TrailRenderer _trailRenderer;
    private Vector2 currentDirection;
    private Dictionary<Vector2, (float, int)> intersectionResults = new ();
    private bool isNewPoly;
    
    private List<GameObject> normalBalls = new List<GameObject>();  // List to manage normal balls
    private List<GameObject> evilBalls = new List<GameObject>();  // List to manage evil balls

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _trailRenderer = GetComponent<TrailRenderer>();
    }

    void Start()
    {
        lastPosition = transform.position;
        isNewPoly = true;
    }

    void Update()
    {
        Vector2 currentPosition = transform.position;

        if (Input.GetMouseButtonDown(0) && GameManager.gameUI.activeSelf)
        {
            // Slow down time for aiming
            SlowMotionManager.EnterSlowMotion();
            Arrow.SetupAndActivate(transform);
        }

        if (Input.GetMouseButtonUp(0) && GameManager.gameUI.activeSelf)
        {
            Arrow.Deactivate();
            if (!EnergySystem.IsEnergyEmpty)
            {
                if (isNewPoly)
                {
                    lastPosition = transform.position;
                    isNewPoly = false;
                }
                _trailRenderer.enabled = true;
            }
            
            // When mouse button is released, Bob changes direction
            SlowMotionManager.ExitSlowMotion();  // Return to normal time

            // Clear existing force
            rb.velocity = Vector2.zero;

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentDirection = (mousePosition - currentPosition).normalized;
            rb.velocity = currentDirection * shootForce;

            if (!EnergySystem.IsEnergyEmpty)
            {
                if (lastPosition != currentPosition)
                {
                    float distance = Vector2.Distance(lastPosition, currentPosition);
                    energyConsumedForCurrentTrait += distance * EnergySystem.energyConsumptionRate;
                    AddSegment(lastPosition, currentPosition);
                }
                lastPosition = currentPosition;
                CalculatePotentialIntersections(lastPosition, currentDirection);
                isMoving = true;
            }
        }

        if (isMoving)
        {
            float currentLength = Vector2.Distance(lastPosition, currentPosition);
            foreach (var keyValuePair in intersectionResults)
            {
                if (currentLength >= keyValuePair.Value.Item1)
                {
                    int polygonType = keyValuePair.Value.Item2;
                    Debug.Log($"{GetPolygonType(polygonType)} formed");
                    polygonEnergy = energyConsumedForCurrentTrait;  // Set the energy used for the polygon
                    ShowPolygon(keyValuePair.Key, polygonType);  // Show the polygon visual
                    if (polygonType == 4)  // changed: Check if quadrilateral
                    {
                        BreakEvilBallsInsidePolygon();  // changed: Handle breaking shields for quadrilaterals
                    }
                    else if (polygonType == 5)  // changed: Check if pentagon
                    {
                        HealPlayer();  // changed: Handle healing Bob
                    }
                    else if (polygonType == -1)  // Check for invalid polygon
                    {
                        TransformTraitIntoNormalBalls();  // Transform trait into normal balls
                    }
                    else
                    {
                        KillBallsInsidePolygon();  // changed: Handle damage for other polygons
                    }
                    if(polygonType != -1) 
                        TransformExtraTraitIntoNormalBalls(keyValuePair.Key, polygonType);  // Handle extra trait
                    ClearData();
                    _trailRenderer.Clear();
                    _trailRenderer.enabled = false;
                    isMoving = false;
                    isNewPoly = true;
                    break;
                }
            }
            float energyCost = EnergySystem.energyConsumptionRate * currentLength * Time.deltaTime;
            energyConsumedForCurrentTrait += energyCost;
            if (!EnergySystem.ConsumeEnergy(energyCost))
            {
                // Stop moving and disable the trail renderer when out of energy
                ClearData();
                isMoving = false;
                isNewPoly = true;
            }
        }
    }
    
    private void ResetObjectPools()
    {
        GameObject[] normalBalls = GameObject.FindGameObjectsWithTag("NormalBall");
        foreach (GameObject ball in normalBalls)
        {
            normalBallPool.ReturnObject(ball);
        }

        GameObject[] evilBalls = GameObject.FindGameObjectsWithTag("EvilBall");
        foreach (GameObject ball in evilBalls)
        {
            evilBallPool.ReturnObject(ball);
        }
    }

    void ClearData()
    {
        segmentStartPositions.Clear();
        segmentEndPositions.Clear();
        energyConsumedForCurrentTrait = 0f;  // Reset the energy consumed for the current trait
    }

    void CalculatePotentialIntersections(Vector2 start, Vector2 direction)
    {
        intersectionResults.Clear();

        for (int i = 0; i < segmentStartPositions.Count; i++)
        {
            if (LineSegmentsIntersect(start, direction, segmentStartPositions[i], segmentEndPositions[i], out Vector2 intersection))
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
        int edgeCount = segmentStartPositions.Count - segmentIndex;
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
        return -1;  // Undefined or invalid polygon
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

    public void AddSegment(Vector2 start, Vector2 end)
    {
        segmentStartPositions.Add(start);
        segmentEndPositions.Add(end);

        // Remove the oldest segment if the number of segments exceeds 5
        // if (segmentStartPositions.Count > 5)
        // {
        //     segmentStartPositions.RemoveAt(0);
        //     segmentEndPositions.RemoveAt(0);
        // }
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
                var scale = transform.localScale - new Vector3(0.1f, 0.1f, 0f);
                if (scale.x >= 0.5f)
                {
                    transform.localScale = scale;  // Bob shrinks in size
                }
                else
                {
                    transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                }
                _trailRenderer.startWidth = transform.localScale.x;

                // DisplayNotification("Size Down!", Color.yellow);
            }
            else if (normalBall.powerUp == NormalBall.PowerUpType.Speed)
            {
                // rb.velocity *= 1.5f;  // Bob gains speed
                shootForce *= 1.1f;
                // DisplayNotification("Speed Up!", Color.blue);
            }
            
            normalBallPool.ReturnObject(col.gameObject);// Remove the normal ball
            EnergySystem.GainEnergy(EnergySystem.energyGainAmount);
            ScoringSystem.AddScore(1);  // Add score for killing normal ball
            // DisplayNotification("+1", Color.blue);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (isMoving)
        {
            Vector2 currentPosition = transform.position;
            if (lastPosition != currentPosition)
            {
                float distance = Vector2.Distance(lastPosition, currentPosition);
                energyConsumedForCurrentTrait += distance * EnergySystem.energyConsumptionRate;  // Track energy consumed for the current trait
                AddSegment(lastPosition, currentPosition);
                lastPosition = currentPosition;
                CalculatePotentialIntersections(lastPosition, currentDirection);
            }
        }
        
        if (col.gameObject.CompareTag("EvilBall"))
        {
            Vector2 bounceDirection = (transform.position - col.transform.position).normalized;
            rb.velocity = bounceDirection * bounceForce;
            HealthSystem.TakeDamage(10f);  // Adjust damage value as necessary
            // DisplayNotification("-10", Color.red);
        }
    }
    
    void ShowPolygon(Vector2 intersectionPoint, int polygonEdgesCount) // New method to show polygon visual
    {
        if(polygonEdgesCount < 3) return;
        
        List<Vector2> polygonPoints = new List<Vector2>();
        
        polygonPoints.Add(intersectionPoint);

        for (int i = segmentEndPositions.Count - 1; i >= segmentEndPositions.Count - polygonEdgesCount + 1 ; i--)
        {
            polygonPoints.Add(segmentEndPositions[i]);
        }
        
        Color polygonColor = GetPolygonColor(polygonPoints.Count);

        GameObject polygonVisualizer = Instantiate(polygonVisualizerPrefab, Vector3.zero, Quaternion.identity);
        polygonVisualizer.GetComponent<PolygonVisualizer>().SetPoints(polygonPoints, polygonColor);
    }
    
    Color GetPolygonColor(int sides)
    {
        switch (sides)
        {
            case 3: return Color.red;         // Triangle
            case 4: return Color.blue;       // Quadrilateral
            case 5: return Color.green;        // Pentagon
            default: return Color.white;      // Default
        }
    }
    
    void TransformExtraTraitIntoNormalBalls(Vector2 intersectionPoint, int polygonEdgesCount)  // New method to handle extra trait
    {
        List<Vector2> extraSegmentStartPositions = new List<Vector2>();
        List<Vector2> extraSegmentEndPositions = new List<Vector2>();
        
        var extraSegmentsCount = segmentStartPositions.Count - polygonEdgesCount;
        extraSegmentsCount += 1; // the moving line should count as an edge but wasn't included in the segmentStartPositions.Count
        
        for (int i = 0; i < extraSegmentsCount ; i++)
        {
            extraSegmentStartPositions.Add(segmentStartPositions[i]);
            extraSegmentEndPositions.Add(segmentEndPositions[i]);
        }
        
        extraSegmentStartPositions.Add(segmentStartPositions[extraSegmentsCount]);
        extraSegmentEndPositions.Add(intersectionPoint); // The intersection segment split into two parts.
        
     
        
        float extraLength = CalculateTotalLength(extraSegmentStartPositions, extraSegmentEndPositions);
        int numberOfBalls = Mathf.CeilToInt(extraLength * EnergySystem.energyConsumptionRate / 10f);  // Adjust energy-to-balls ratio as needed

        for (int i = 0; i < numberOfBalls; i++)
        {
            int index = i % extraSegmentStartPositions.Count;
            Vector2 start = extraSegmentStartPositions[index];
            Vector2 end = extraSegmentEndPositions[index];
            Vector2 spawnPosition = Vector2.Lerp(start, end, Random.Range(0f, 1f));
            StartCoroutine(BallSpawner.SpawnNormalBallWithAnimation(spawnPosition));
        }
    }
    
    float CalculateTotalLength(List<Vector2> startPositions, List<Vector2> endPositions)  // New method to calculate total length of extra segments
    {
        float totalLength = 0f;
        for (int i = 0; i < startPositions.Count; i++)
        {
            totalLength += Vector2.Distance(startPositions[i], endPositions[i]);
        }
        return totalLength;
    }
    
    void TransformTraitIntoNormalBalls()  // New method to transform trait into normal balls
    {
        int numberOfBalls = Mathf.CeilToInt(polygonEnergy / 10f);  // Adjust energy-to-balls ratio as needed
        for (int i = 0; i < numberOfBalls; i++)
        {
            Vector2 spawnPosition = Vector2.Lerp(segmentStartPositions[i % segmentStartPositions.Count], segmentEndPositions[i % segmentEndPositions.Count], Random.Range(0f, 1f));
            StartCoroutine(BallSpawner.SpawnNormalBallWithAnimation(spawnPosition));
        }
    }
    
    void HealPlayer()  // changed: New method to heal Bob
    {
        HealthSystem.Heal(polygonEnergy);  // Heal Bob based on the energy used to form the polygon
    }
    
    void BreakEvilBallsInsidePolygon()  // changed: New method to break evil ball shields
    {
        GameObject[] evilBalls = GameObject.FindGameObjectsWithTag("EvilBall");
        foreach (GameObject ball in evilBalls)
        {
            if (IsPointInPolygon(ball.transform.position))
            {
                ball.GetComponent<EvilBall>().BreakShield();
            }
        }
    }

    void KillBallsInsidePolygon()  // changed: updated method to handle killing balls inside polygon
    {
        GameObject[] normalBalls = GameObject.FindGameObjectsWithTag("NormalBall");
        foreach (GameObject ball in normalBalls)
        {
            if (IsPointInPolygon(ball.transform.position))
            {
                normalBallPool.ReturnObject(ball);
                EnergySystem.GainEnergy(EnergySystem.energyGainAmount);
                ScoringSystem.AddScore(1);  // Add score for killing normal ball
            }
        }

        GameObject[] evilBalls = GameObject.FindGameObjectsWithTag("EvilBall");
        foreach (GameObject ball in evilBalls)
        {
            if (IsPointInPolygon(ball.transform.position))
            {
                ball.GetComponent<EvilBall>().TakeDamage((int)polygonEnergy);
            }
        }

        energyConsumedForCurrentTrait = 0f;  // Reset energy consumed for the current trait
    }

    bool IsPointInPolygon(Vector2 point)
    {
        int intersectCount = 0;
        for (int i = 0; i < segmentStartPositions.Count; i++)
        {
            Vector2 v1 = segmentStartPositions[i];
            Vector2 v2 = segmentEndPositions[i];

            if ((v1.y > point.y) != (v2.y > point.y) &&
                (point.x < (v2.x - v1.x) * (point.y - v1.y) / (v2.y - v1.y) + v1.x))
            {
                intersectCount++;
            }
        }

        // var isIn = (intersectCount % 2) == 1;
        // if (isIn) Instantiate(Marker, new Vector3(point.x, point.y, 0), quaternion.identity);
        
        return (intersectCount % 2) == 1;
    }
    
    // Add this field to reference the amount of energy to form the polygon

    // void DisplayNotification(string message, Color color)
    // {
    //     Vector3 spawnPosition = transform.position + Vector3.up * 2;  // Adjust spawn position as needed
    //     GameObject notification = Instantiate(notificationDisplayPrefab, spawnPosition, Quaternion.identity);
    //     notification.GetComponent<NotificationDisplay>().Initialize(message, color);
    // }
    
    public void ResetPlayer()
    {
        // Reset bob
        transform.position = Vector2.zero;
        rb.velocity = Vector2.zero;
        transform.localScale = new Vector3(0.6f, 0.6f, 1);

        // Reset trail renderer
        _trailRenderer.Clear();
        _trailRenderer.enabled = false;
        _trailRenderer.startWidth = 0.6f;

        // Reset internal state
        isMoving = false;
        isNewPoly = true;
        energyConsumedForCurrentTrait = 0f;
        lastPosition = transform.position;
        segmentStartPositions.Clear();
        segmentEndPositions.Clear();

        // Reset health and energy
        HealthSystem.ResetHealth();
        EnergySystem.ResetEnergy();
        
        // Reset object pools
        ResetObjectPools();
    }
}