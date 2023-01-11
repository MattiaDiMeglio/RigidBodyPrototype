using UnityEngine;
using System.Runtime.CompilerServices;


public static class PhysicsExtension
{
    /// <summary>
    /// Metodo per il calcolo del verlet su un singolo asse
    /// </summary>
    /// <param name="oldVelocity">current body velocity</param>
    /// <param name="acceleration">acceleration applied to the body</param>
    /// <returns>'the body velocity at the next step</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float Vertlet(float oldVelocity, float acceleration) =>(oldVelocity + (oldVelocity + (acceleration * Time.deltaTime))) * 0.5f;
}
