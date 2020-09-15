using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TileRotation {

[Serializable]
public enum Orientation {
    Turn0,
    Turn90,
    Turn180,
    Turn270
}

public static class RotationHelper 
{

    public static Quaternion GetRotation(Orientation orientation) {
        switch(orientation) {
            case Orientation.Turn0:
                return Quaternion.identity;
            case Orientation.Turn90:
                return Quaternion.identity * Quaternion.Euler(0,0,90);
            case Orientation.Turn180:
                return Quaternion.identity * Quaternion.Euler(0,0,180);
            case Orientation.Turn270:
                return Quaternion.identity * Quaternion.Euler(0,0,-90);
        }
        return Quaternion.identity;
    }

    public static Orientation RandomOrientation() {
        return (Orientation) UnityEngine.Random.Range(0, OrientationSize());
    }

    private static int OrientationSize() {
        return Enum.GetNames(typeof(Orientation)).Length;
    }

    public static Orientation GetNextOrientation(this Orientation orientation, float degrees) {
        int direction = (int)Mathf.Sign(degrees);
        return (Orientation) (((int)orientation + direction + OrientationSize()) % OrientationSize());
        
    }

}
}
