using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 0.0f;

    private void FixedUpdate()
    {
        var horizontal = Input.GetAxis("Horizontal");

        transform.Rotate(0.0f, 0.0f, -horizontal * rotationSpeed);
    }
}
