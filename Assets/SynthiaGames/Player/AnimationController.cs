using System;
using UnityEngine;

public enum AnimType
{
    Idle,
    Move,
}

public class AnimationController : MonoBehaviour
{
    public Animator controller;
    public AnimType startanimation;


    void Start()
    {
        if (controller == null)
        {
            controller = GetComponent<Animator>();
        }

        controller.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        PlayAnimation(startanimation);
    }

    public void PlayAnimation(AnimType anim, float switchTime = .1f)
    {
        startanimation = anim;
        if (controller != null)
        {
            //controller.Play(anim.ToString());
            controller.CrossFade(startanimation.ToString(), switchTime);
            controller.Update(Time.deltaTime);
        }
    }
    
}