using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.CompilerServices;
using System.Net.NetworkInformation;

public static class AnimatorExtension 
{
    /// <summary>
    /// Ritorna la durata in secondo della clip clipName
    /// </summary>
    /// <param name="animator">l'animator contenente la clip</param>
    /// <param name="clipName">il nome delle clip di cui si vuole la durata</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetLength(this Animator animator, string clipName)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        Debug.LogWarning("Animator Extension: " + clipName + " clipNotFound");
        return 0f;
    }

    public static float GetLength(this Animator animator, int clipNumber)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clipNumber == Animator.StringToHash(clip.name))
                return clip.length;
        }
        Debug.LogWarning("Animator Extension: " + clipNumber + " clipNotFound");
        return 0f;
    }
}
