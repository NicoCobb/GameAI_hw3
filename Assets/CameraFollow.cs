using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [SerializeField]
    private Transform toFollow;

    [SerializeField]
    private bool followPosition;
    [SerializeField]
    private Vector3 followOffset;
    [SerializeField]
    private bool followAngle;
    [SerializeField]
    private float zRotationOffset;

    // Update is called once per frame
    void Update () {
		if (followPosition)
        {
            transform.position = toFollow.position + followOffset;
        }
        if (followAngle)
        {
            transform.rotation = toFollow.rotation * Quaternion.Euler(0, 0, zRotationOffset);
        }
	}
}
