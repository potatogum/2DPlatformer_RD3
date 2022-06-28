using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalScroll : MonoBehaviour
{
    [SerializeField] private float scrollRate = 1f;

    void FixedUpdate()
    {
        transform.Translate(0, scrollRate*Time.fixedDeltaTime, 0);
    }
}
