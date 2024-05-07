using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedVector3
{
    public float x;
    public float y;
    public float z;
}

public static class Vector3Extensions
{
    public static Vector3 ToVector3(this SerializedVector3 serializedVector3)
    {
        return new Vector3(serializedVector3.x, serializedVector3.y, serializedVector3.z);
    }

    public static SerializedVector3 FromVector3(this Vector3 vector3)
    {
        SerializedVector3 posSet = new SerializedVector3();
        posSet.x = vector3.x;
        posSet.y = vector3.y;
        posSet.z = vector3.z;

        return posSet;
    }
}