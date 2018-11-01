using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Part3Boid : MonoBehaviour
{
    [SerializeField]
    private float maxAcceleration = 10; // this depends on the boid
    [SerializeField]
    private float maxSpeed = 10; // this also depends on the boid

    [SerializeField]
    private Transform targetTransform; // the thing to avoid or go to or whatever...
    [SerializeField]
    private Behavior brain;

    private Behavior.SteeringOutput prevSteeringOutput = new Behavior.SteeringOutput();
    private Color color; // this is set by script, you shouldn't have to do it
    private GameObject temporaryTransform; // if we set the transform with a vector3 create a transform and use that instead. Then delete it afterwards

    private Vector3 startingPos;
    private Quaternion startingRot;

    public float pathProgress = 0; // start from the 0 section of the path, then move forwards from there
    public int pathTargetIndex = 0;

    [Space]
    [SerializeField]
    private SpriteRenderer circleSprite;
    [SerializeField]
    private SpriteRenderer pointerSprite;

    private Rigidbody2D rb;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startingPos = transform.position;
        startingRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.rotation = startingRot;
            transform.position = startingPos;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0;
            pathProgress = 0;
            pathTargetIndex = 0;
        }
        if (brain != null)
        {
            // poll the brain for what to do!
            Behavior.SteeringOutput steeringOutput = brain.Action(transform, targetTransform, prevSteeringOutput, maxAcceleration);
            Vector2 dpos = steeringOutput.GetDPos();
            rb.AddForce(dpos);
            // transform.position += (Vector3)dpos; // kinematic
            if (brain.faceTarget == Behavior.Facing.DirectionOfMovement)
            {
                transform.rotation = Quaternion.Euler(0, 0, Behavior.VectorAngle(rb.velocity) * Mathf.Rad2Deg);
            }
            else
            {
                // face the target
                Vector2 targetDirection = targetTransform.position - transform.position;
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(targetDirection.y, targetDirection.x));
            }
            /*else
            {
                transform.rotation = steeringOutput.GetFacing();
            }*/
            if (brain.useMaxSpeed)
            {
                if (rb.velocity.magnitude > maxSpeed)
                {
                    rb.velocity = rb.velocity.normalized * maxSpeed; // limit it to max speed.
                }
            }
            prevSteeringOutput = steeringOutput;
        }
    }

    public Color GetColor()
    {
        return color;
    }

    // Initialization functions called by the narrator:
    public void SetTarget(Transform t)
    {
        if (temporaryTransform != null)
        {
            Destroy(temporaryTransform);
            temporaryTransform = null;
        }
        targetTransform = t;
    }

    public void SetTarget(Vector2 v)
    {
        // create a temporary transform and use that
        GameObject go = new GameObject();
        go.transform.position = v;
        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;
        // go.name = "Part 3 Boid";
        SetTarget(go.transform);
        temporaryTransform = go;
    }

    public void SetBehavior(Behavior b)
    {
        brain = b;
    }

    public void ClearTarget()
    {
        SetTarget(null);
    }

    public void SetAngle(float a)
    {
        // pass in degrees, because setting angles is just easier with degrees.
        prevSteeringOutput = new Behavior.SteeringOutput(a * Mathf.Deg2Rad, prevSteeringOutput.GetSpeed());
        transform.rotation = Quaternion.Euler(0, 0, a);
    }

    public void SetPosition(Vector2 pos)
    {
        transform.position = pos;
    }

    public void SetColor(Color c)
    {
        color = c;
        circleSprite.color = c;
        pointerSprite.color = c;
    }

    public void SetMaxSpeed(float s)
    {
        maxSpeed = s;
    }

    public void SetMaxAcceleration(float a)
    {
        maxAcceleration = a;
    }
}
