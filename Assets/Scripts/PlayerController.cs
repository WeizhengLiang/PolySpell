using UnityEngine;
using System;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public event Action<PlayerController> OnDeath;

    private GameManager gameManager;
    private EnergySystem energySystem;
    private HealthSystem healthSystem;
    private ScoringSystem scoringSystem;
    private SlowMotionManager slowMotionManager;
    private GameObject polygonVisualizerPrefab;
    private BallSpawner ballSpawner;
    private PlayerUI playerUI;

    private MovementController movementController;
    private PolygonManager polygonManager;
    private PolygonHandler polygonHandler;
    private Rigidbody2D rb;
    private TrailRenderer trailRenderer;

    private float energyConsumedForCurrentTrait = 0f;
    private Vector2 lastPosition;
    private Dictionary<Vector2, (float, int)> intersectionResults = new();
    private bool isNewPolygon = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
        movementController = new MovementController(rb);
        polygonManager = new PolygonManager();
    }

    private void Start()
    {
        lastPosition = transform.position;
        ResetPlayer();
    }

    private void Update()
    {
        if (gameManager == null || !gameManager.IsGameRunning) return;

        Vector2 currentPosition = transform.position;
        HandleMouseInput(currentPosition);
        HandleMovement(currentPosition);
    }

    public void Initialize(GameManager gameManager, EnergySystem energySystem, HealthSystem healthSystem, ScoringSystem scoringSystem, SlowMotionManager slowMotionManager, GameObject polygonVisualizerPrefab, BallSpawner ballSpawner, PlayerUI playerUI)
    {
        this.gameManager = gameManager;
        this.energySystem = energySystem;
        this.healthSystem = healthSystem;
        this.scoringSystem = scoringSystem;
        this.slowMotionManager = slowMotionManager;
        this.polygonVisualizerPrefab = polygonVisualizerPrefab;
        this.ballSpawner = ballSpawner;
        this.playerUI = playerUI;

        energySystem.Initialize(playerUI);
        healthSystem.Initialize(playerUI);
        polygonHandler = new PolygonHandler(polygonManager, energySystem, healthSystem, scoringSystem, ballSpawner, polygonVisualizerPrefab);
    }

    private void HandleMouseInput(Vector2 currentPosition)
    {
        if (Input.GetMouseButtonDown(0) && gameManager.IsGameRunning && !slowMotionManager.inSlowMotion && !gameManager.IsPaused)
        {
            slowMotionManager.EnterSlowMotion();
            playerUI.SetupAndActivateArrow(transform);
        }

        if (Input.GetMouseButtonUp(0) && gameManager.IsGameRunning && slowMotionManager.inSlowMotion && !gameManager.IsPaused)
        {
            playerUI.DeactivateArrow();
            slowMotionManager.ExitSlowMotion();

            rb.velocity = Vector2.zero;

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            movementController.SetDirection(mousePosition - currentPosition);
            movementController.Shoot();

            if (!energySystem.IsEnergyEmpty)
            {
                if (isNewPolygon)
                {
                    lastPosition = transform.position;
                    isNewPolygon = false;
                }
                trailRenderer.enabled = true;

                if (lastPosition != currentPosition)
                {
                    float distance = Vector2.Distance(lastPosition, currentPosition);
                    energyConsumedForCurrentTrait += distance * energySystem.EnergyConsumptionRate;
                    polygonManager.AddSegment(lastPosition, currentPosition);
                }
                lastPosition = currentPosition;
                CalculatePotentialIntersections(lastPosition, movementController.CurrentDirection);
            }
        }
    }

    private void HandleMovement(Vector2 currentPosition)
    {
        if (trailRenderer.enabled)
        {
            float currentLength = Vector2.Distance(lastPosition, currentPosition);
            foreach (var keyValuePair in intersectionResults)
            {
                if (currentLength >= keyValuePair.Value.Item1)
                {
                    int polygonType = keyValuePair.Value.Item2;
                    var startEndTrailPoint = keyValuePair.Key;
                    polygonHandler.HandlePolygonFormation(startEndTrailPoint, polygonType);
                    ClearData();
                    trailRenderer.Clear();
                    trailRenderer.enabled = false;
                    isNewPolygon = true;
                    break;
                }
            }
            float energyCost = energySystem.EnergyConsumptionRate * currentLength * Time.deltaTime;
            energyConsumedForCurrentTrait += energyCost;
            if (!energySystem.ConsumeEnergy(energyCost))
            {
                if (trailRenderer.positionCount > 0)
                {
                    ClearTrailAndRestoreEnergy();
                }
                ClearData();
                trailRenderer.enabled = false;
                isNewPolygon = true;
            }
            else
            {
                trailRenderer.time = 50f;
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
        intersectionResults.Clear();
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
            else if (normalBall.powerUp == NormalBall.PowerUpType.None)
            {
                StartCoroutine(VFXManager.Instance.SpawnTextVFX($"+{1}", ColorCombo.Hex2Color(ColorCombo.AddNormalScore_3), normalBall.transform.position));
            }

            normalBall.TriggerDieVFX(col.transform.position);

            ballSpawner.ballTypes.Find(info => info.type == BallSpawner.BallType.Normal).pool.ReturnObject(col.gameObject);
            energySystem.GainEnergy(energySystem.EnergyGainAmount);
            scoringSystem.AddScore(1);
        }
    }

    private void HandleSizePowerUp()
    {
        var scale = transform.localScale - new Vector3(0.1f, 0.1f, 0f);
        if (scale.x >= 0.5f)
        {
            transform.localScale = scale;
            StartCoroutine(VFXManager.Instance.SpawnTextVFX("Size Down!", ColorCombo.Hex2Color(ColorCombo.RegularText_3), transform.position));
        }
        else
        {
            transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            StartCoroutine(VFXManager.Instance.SpawnTextVFX("Min Size!", ColorCombo.Hex2Color(ColorCombo.RegularText_3), transform.position));
        }
        trailRenderer.startWidth = transform.localScale.x;
    }

    private void HandleSpeedPowerUp()
    {
        StartCoroutine(VFXManager.Instance.SpawnTextVFX("Speed Up!", ColorCombo.Hex2Color(ColorCombo.RegularText_3), transform.position));
        movementController.SetDirection(movementController.CurrentDirection * 1.15f);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        var normal = col.contacts[0].normal;
        movementController.Bounce(normal);

        if (trailRenderer.enabled)
        {
            Vector2 currentPosition = transform.position;
            if (lastPosition != currentPosition)
            {
                float distance = Vector2.Distance(lastPosition, currentPosition);
                energyConsumedForCurrentTrait += distance * energySystem.EnergyConsumptionRate;
                polygonManager.AddSegment(lastPosition, currentPosition);
                lastPosition = currentPosition;
                CalculatePotentialIntersections(lastPosition, movementController.CurrentDirection);
            }
        }

        if (col.gameObject.CompareTag("EvilBall"))
        {
            Vector2 contactPoint = col.contacts[0].point;
            StartCoroutine(VFXManager.Instance.SpawnTextVFX($"-{10}", ColorCombo.Hex2Color(ColorCombo.DamageText_3), transform.position));
            VFXManager.Instance.SpawnVFX(VFXType.hitEffect, contactPoint);
            healthSystem.TakeDamage(10f);

            if (healthSystem.CurrentHealth <= 0)
            {
                Die();
            }
        }
    }

    private void ClearTrailAndRestoreEnergy()
    {
        float trailLength = CalculateTrailLength();
        float energyRestored = trailLength * energySystem.EnergyConsumptionRate;
        energySystem.GainEnergy(energyRestored);
        trailRenderer.Clear();
        trailRenderer.enabled = false;
    }

    private float CalculateTrailLength()
    {
        float length = 0f;
        for (int i = 0; i < trailRenderer.positionCount - 1; i++)
        {
            length += Vector3.Distance(trailRenderer.GetPosition(i), trailRenderer.GetPosition(i + 1));
        }
        return length;
    }

    public void ResetPlayer()
    {
        StopAllCoroutines();
        polygonManager.ClearPolygonVisualizerList();

        transform.position = Vector2.zero;
        rb.velocity = Vector2.zero;
        transform.localScale = new Vector3(0.6f, 0.6f, 1);
        movementController = new MovementController(rb);

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.enabled = false;
        }
        trailRenderer.startWidth = 0.6f;

        isNewPolygon = true;
        energyConsumedForCurrentTrait = 0f;
        lastPosition = transform.position;
        polygonManager.ClearSegments();

        healthSystem.ResetHealth();
        energySystem.ResetEnergy();
    }

    public void HandleExternalInput(Vector2 inputDirection, bool isMoving)
    {
        if (isMoving)
        {
            movementController.SetDirection(inputDirection);
            movementController.Shoot();
        }
    }

    private void Die()
    {
        // 死亡逻辑
        OnDeath?.Invoke(this);
    }
}