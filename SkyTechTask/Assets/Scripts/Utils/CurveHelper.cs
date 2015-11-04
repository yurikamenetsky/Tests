using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace Utils
{
    public static class CurveHelper
    {
        public static Vector3 InterpBezierCube(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float u = 1.0f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
 
            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;
 
            return p;
        }

        public static Vector3 InterpBezierQuad(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1 - t;
            return (u * u * p0) + (2 * u * t * p1) + (t * t * p2);
        }

        public static float GetPathLength(Vector3[] path) 
        {
            float length = 0f;
            for (var i = 1; i < path.Length; i++) 
            {
                length += Vector3.Distance(path[i-1], path[i]);
            }
            return length;
        }
    }
}
