using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonVisualizer : MonoBehaviour
{
    private PolygonCollider2D polygonCollider;
    private LineRenderer lineRenderer;

    void Awake()
    {
        polygonCollider = GetComponent<PolygonCollider2D>();
        lineRenderer = gameObject.GetComponent<LineRenderer>();

        // Configure the LineRenderer
        lineRenderer.positionCount = 0;
        lineRenderer.loop = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    public void SetPoints(List<Vector2> points, Color color)
    {
        polygonCollider.SetPath(0, points.ToArray());

        lineRenderer.positionCount = points.Count;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i]);
        }

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float duration = 1.5f;
        float elapsedTime = 0f;
        Color startColor = lineRenderer.startColor;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            Color currentColor = Color.Lerp(startColor, endColor, t);
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;
            yield return null;
        }

        Destroy(gameObject);
    }
}