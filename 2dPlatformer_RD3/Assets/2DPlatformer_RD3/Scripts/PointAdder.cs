using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAdder : MonoBehaviour
{
    [SerializeField] private int pointsToGive = 100;
    private Collider2D[] colliders;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // There are more than 1 collider on the player, we want the bodyCollider (polygon)
        if (collision is PolygonCollider2D)
        {
            FindObjectOfType<GameSession>().AddPointsToScore(pointsToGive);
        }
    }
}
