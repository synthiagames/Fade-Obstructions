using UnityEngine;
using System.Collections.Generic;

public class ObstacleFadeManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Raycast Settings")]
    public LayerMask obstacleLayer;
    public float castRadius = 0.5f;
    public int raycastBufferSize = 20;

    [Header("Fade Settings")]
    public float fadeSpeed = 4f;
    public float transparentAlpha = 0.3f;

    private RaycastHit[] hitsBuffer;
    private HashSet<FadeObstacle> fadedObstacles = new HashSet<FadeObstacle>();

    void Awake()
    {
        hitsBuffer = new RaycastHit[raycastBufferSize];
    }

    void Update()
    {
        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;
        Vector3 normalizedDirection = direction.normalized;

        int hitCount = Physics.SphereCastNonAlloc(transform.position, castRadius, normalizedDirection, hitsBuffer, distance, obstacleLayer);
        HashSet<FadeObstacle> hitObstacles = new HashSet<FadeObstacle>();

        for (int i = 0; i < hitCount; i++)
        {
            FadeObstacle fade = hitsBuffer[i].collider.GetComponent<FadeObstacle>();
            if (fade != null)
            {
                hitObstacles.Add(fade);
                if (!fadedObstacles.Contains(fade))
                {
                    fade.FadeOut();
                    fadedObstacles.Add(fade);
                }
            }
        }

        var toRemove = new List<FadeObstacle>();
        foreach (var fade in fadedObstacles)
        {
            if (!hitObstacles.Contains(fade))
            {
                fade.FadeIn();
                toRemove.Add(fade);
            }
        }

        foreach (var fade in toRemove)
        {
            fadedObstacles.Remove(fade);
        }
    }
}