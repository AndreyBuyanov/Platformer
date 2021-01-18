using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSensor : MonoBehaviour
{
    [SerializeField]
    private LayerMask LayerToSense;
    [SerializeField]
    private SpriteRenderer Cross;

    private const float MAX_DIST = 10f;
    private const float MIN_DIST = 0.05f;

    public bool Grounded
    {
        get;
        private set;
    }

    void Start()
    {
        Cross.gameObject.SetActive(true);
    }

    void FixedUpdate()
    {
        Vector3 direction = Cross.transform.position - transform.position;
        direction.Normalize();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, MAX_DIST, LayerToSense);
        if (hit.collider == null)
        {
            hit.distance = MAX_DIST;
        }
        else if (hit.distance < MIN_DIST)
        {
            hit.distance = MIN_DIST;
        }
        Grounded = hit.distance == MIN_DIST;
        Cross.transform.position = transform.position + direction * hit.distance;
    }

    public void Hide()
    {
        Cross.gameObject.SetActive(false);
    }

    public void Show()
    {
        Cross.gameObject.SetActive(true);
    }
}
