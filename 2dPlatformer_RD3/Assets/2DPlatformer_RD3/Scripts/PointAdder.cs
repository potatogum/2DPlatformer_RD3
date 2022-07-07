using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAdder : MonoBehaviour
{
    [SerializeField] private int pointsToGive = 100;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        FindObjectOfType<GameSession>().AddPointsToScore(pointsToGive);
        //// There are more than 1 collider on the player, we want the bodyCollider (polygon)
        //if (collision is BoxCollider2D)
        //{
        //    FindObjectOfType<GameSession>().AddPointsToScore(pointsToGive);
        //}
    }
}
