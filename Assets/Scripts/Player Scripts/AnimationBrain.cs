using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationBrain : MonoBehaviour
{
    private readonly static int[] animations =
    {
        Animator.StringToHash("Idle 1"),
        Animator.StringToHash("Idle 2"),
        Animator.StringToHash("Idle 3"),
        Animator.StringToHash("Walk"),
        Animator.StringToHash("Run"),
        Animator.StringToHash("Jump"),
        Animator.StringToHash("Fight"),
        Animator.StringToHash("Dance 1"),
        Animator.StringToHash("Death"),
        Animator.StringToHash("StandRifleAim"),
        Animator.StringToHash("WalkRifleAim"),
        Animator.StringToHash("StandRifleFire"),
        Animator.StringToHash("WalkRifleFire"),
        Animator.StringToHash("HitReaction"),
        Animator.StringToHash("HitReaction2"),
        Animator.StringToHash("Reload")
    };

    private Animator animator;
    private Animations[] currentAnimation;
    private bool[] layerLocked;
    private Action<int> DefaultAnimation;

    protected void Initialize (int layers , Animations startingAnimation, Animator animator, Action<int> DefaultAnimation)
    {
        layerLocked = new bool[layers];
        currentAnimation = new Animations[layers];
        this.animator = animator;
        this.DefaultAnimation = DefaultAnimation;

        for (int i = 0; i < layers; i++)
        {
            layerLocked[i] = false;
            currentAnimation[i] = startingAnimation;
        }
    }

    public Animations GetCurrentAnimatio(int layer)
    {
        return currentAnimation[layer];
    }

    public void SetLocked(bool locklayer, int layer)
    {
        layerLocked[layer] = locklayer;
    }

    //public void Play(Animations animation, int layer , bool lockLayer, bool bypassLock, float crossfade = 0.1f)
    //{

    //    if (animation == Animations.None)
    //    {
    //        DefaultAnimation(layer);
    //        return;
    //    }

    //    if(layerLocked[layer] && !bypassLock) return;

    //    layerLocked[layer] = lockLayer;

    //    if (currentAnimation[layer] == animation) return;

    //    currentAnimation[layer] = animation;
    //    animator.CrossFade(animations[(int)currentAnimation[layer]], crossfade, layer);
    //}

    public float GetAnimationLength(Animations animation)
    {
        // Get the animation clip length from the animator
        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        for (int i = 0; i < ac.animationClips.Length; i++)
        {
            if (ac.animationClips[i].name == animation.ToString()) //When do this, you must change the motion name to "Reload"(Ex: "mixamo.com" to "Reload")
            {
                return ac.animationClips[i].length;
            }
        }
        return 1.3f; // Default fallback value if animation not found
    }

    public void Play(Animations animation, int layer, bool lockLayer, bool bypassLock, float crossfade = 0.1f)
    {
        if (animation == Animations.None)
        {
            DefaultAnimation(layer);
            return;
        }

        if (layerLocked[layer] && !bypassLock) return;

        layerLocked[layer] = lockLayer;

        if (currentAnimation[layer] == animation) return;

        // If current animation is an Idle and we're switching to Walk, Run, or Jump, shorten the crossfade
        bool isIdle = currentAnimation[layer] == Animations.Idle1 || currentAnimation[layer] == Animations.Idle2 || currentAnimation[layer] == Animations.Idle3;
        //bool isQuickTransition = animation == Animations.Walk || animation == Animations.Run || animation == Animations.Jump;

        float actualCrossfade = (isIdle) ? 0.01f : crossfade;

        currentAnimation[layer] = animation;
        animator.CrossFade(animations[(int)currentAnimation[layer]], actualCrossfade, layer);
    }
}

public enum Animations
{
    Idle1,
    Idle2,
    Idle3,
    Walk,
    Run,
    Jump,
    Fight,
    Dance1,
    Death,
    StandRifleAim,
    WalkRifleAim,
    StandRifleFire,
    WalkRifleFire,
    HitReaction,
    HitReaction2,
    Reload,
    None
}