using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeInfo
{
    public enum SwipeDirection
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    public SwipeDirection Direction {get;set;}
    public float Magnitude {
        get {
            return GetVelocity().magnitude;
        }
    }

    public Vector3 StartPosition {
        get {
            return fingerDown.position;
        }
    }
    public Vector3 EndPosition {
        get {
            return fingerUp.position;
        }
    }
    private Touch fingerDown;
    private Touch fingerUp;

    private float timeDelta;
    
    public SwipeInfo(Touch fingerDown, Touch fingerUp, float timeDelta) {
        this.fingerDown = fingerDown;
        this.fingerUp = fingerUp;
        this.timeDelta = timeDelta;
        this.Direction = DetermineDirection();
    }


    public Vector3 GetVelocity() {
        return EndPosition - StartPosition;
    }

    public Vector3 GetWorldVelocity(Camera camera, bool scaled) {
        Vector3 rawVelocity = camera.ScreenToWorldPoint(EndPosition) - camera.ScreenToWorldPoint(StartPosition);
        if(scaled) {
            return rawVelocity / timeDelta;
        }
        return rawVelocity;
    }

    private SwipeDirection DetermineDirection() {
        Vector3 velocity = GetVelocity();
        float angle = (Vector3.SignedAngle(Vector3.right, velocity, Vector3.forward) + 360) % 360;
        if(angle > 315 || angle <= 45) {
            return SwipeDirection.RIGHT;
        } else if (angle > 45 && angle <= 135) {
            return SwipeDirection.UP;
        } else if(angle > 135 && angle <= 225) {
            return SwipeDirection.LEFT;
        } else if(angle > 225 && angle <= 315) {
            return SwipeDirection.DOWN;
        }
        Debug.LogError("Unknown swipe detected");
        return SwipeDirection.RIGHT;
    }

}
