using System.Runtime.CompilerServices;
using UnityEngine;


public static class VectorExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]public static Vector2 Rotate(this Vector2 vector, float angle) => new Vector2((vector.x*Mathf.Cos(angle * Mathf.Deg2Rad)) - (vector.y*Mathf.Sin(angle * Mathf.Deg2Rad)), (vector.x * Mathf.Sin(angle * Mathf.Deg2Rad)) + (vector.y * Mathf.Cos(angle * Mathf.Deg2Rad)));
}
