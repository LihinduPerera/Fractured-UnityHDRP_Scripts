using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAnimationBrain : MonoBehaviour
{
    private readonly static int[] animations =
    {
        Animator.StringToHash("Idle"),
        Animator.StringToHash("Agonizing"),
        Animator.StringToHash("Walk"),
        Animator.StringToHash("Run"),
        Animator.StringToHash("HitReact"),
        Animator.StringToHash("HitReact2"),
        Animator.StringToHash("Attack"),
        Animator.StringToHash("Dead1"),
        Animator.StringToHash("Dead2"),
    };

    private Animator animator;
    private ZombieAnimations[] currentAnimation;
    private bool[] layerLocked;
    private Action<int> DefaultAnimation;

    protected void Initialize (int layers , ZombieAnimations startingAnimation, Animator animator, Action<int> DefaultAnimation)
    {
        layerLocked = new bool[layers];
        currentAnimation = new ZombieAnimations[layers];
        this.animator = animator;
        this.DefaultAnimation = DefaultAnimation;

        for (int i = 0; i < layers; i++)
        {
            layerLocked[i] = false;
            currentAnimation[i] = startingAnimation;
        }
    }

    public ZombieAnimations GetCurrentAnimatio(int layer)
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

    public void Play(ZombieAnimations animation, int layer, bool lockLayer, bool bypassLock, float crossfade = 0.2f)
    {
        if (animation == ZombieAnimations.None)
        {
            DefaultAnimation(layer);
            return;
        }

        if (layerLocked[layer] && !bypassLock) return;

        layerLocked[layer] = lockLayer;

        if (currentAnimation[layer] == animation) return;

        // If current animation is an Idle and we're switching to Walk, Run, or Jump, shorten the crossfade
        bool isIdle = currentAnimation[layer] == ZombieAnimations.Idle || currentAnimation[layer] == ZombieAnimations.Agonizing;
        bool isQuickTransition = animation == ZombieAnimations.Walk || animation == ZombieAnimations.Run || animation == ZombieAnimations.Attack;

        float actualCrossfade = (isIdle) ? 0.01f : crossfade;

        currentAnimation[layer] = animation;
        animator.CrossFade(animations[(int)currentAnimation[layer]], actualCrossfade, layer);
    }
}

public enum ZombieAnimations
{
    Idle,
    Agonizing,
    Walk,
    Run,
    HitReact,
    HitReact2,
    Attack,
    Dead1,
    Dead2,
    None
}