using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Empty Behavior", menuName = "Behaviors/Empty", order = 0)]
public class Behavior : ScriptableObject
{
    public bool useMaxSpeed = true;
    public Facing faceTarget = Facing.DirectionOfMovement; // if this is false it faces its velocity direction instead

    public virtual SteeringOutput Action(Transform position, Transform targetPos, SteeringOutput prev, float maxAcceleration)
    {
        // for this default behavior, don't do anything.
        Debug.Log("Base action called");
        return new SteeringOutput(prev.GetAngle(), 0);
    }

    public virtual string GetName()
    {
        return "No Behavior";
    }

    public static float VectorAngle(Vector2 v)
    {
        return Mathf.Atan2(v.y, v.x);
    }

    public static float RandomBinomial()
    {
        return Random.value - Random.value;
    }

    public class SteeringOutput
    {
        private float dir;
        private float speed;

        public SteeringOutput()
        {
            dir = 0;
            speed = 0;
        }

        public SteeringOutput(float dir, float speed)
        {
            this.dir = dir;
            this.speed = speed;
        }

        public float GetAngle()
        {
            return dir; // in radians
        }

        public float GetDir()
        {
            // returns in radians
            return dir;
        }

        public float GetSpeed()
        {
            return speed;
        }

        public Vector2 GetDPos()
        {
            return new Vector2(Mathf.Cos(dir), Mathf.Sin(dir)) * speed;
        }

        public Quaternion GetFacing()
        {
            return Quaternion.Euler(0, 0, dir * Mathf.Rad2Deg);
        }
    }

    public enum Facing
    {
        DirectionOfMovement, Target
    }
}
