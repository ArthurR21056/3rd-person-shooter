using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : MonoBehaviour
{
    Animator anim;
    CharacterController characterCon;

    [System.Serializable]
    public class AnimationSetting
    {
        public string verticalVelocity = "Forward";
        public string horizantalVelocity = "Strafe";
        public string goundedbool = "isGrounded";
        public string jumpbool = "Airborne";
    }
    [SerializeField]
    public AnimationSetting animations;
    
    [System.Serializable]
    public class PhysicsSettings
    {
        public float gravityModifier = 9.81f;
        public float baseGravity = 50.0f;
        public float resetGravity = 1.2f;
    }
    [SerializeField]
    public PhysicsSettings physics;

    [System.Serializable]
    public class MovementSetting
    {
        public float jumpspeed = 6;
        public float jumptime = 0.25f;
    }
    [SerializeField]
    public MovementSetting movement;

    bool isJumping;
    bool resetGravity;
    float gravity;
    bool isGrounded;

    // Use this for initialization
    void Start()
    {
        anim = GetComponent<Animator>();
        characterCon = GetComponent<CharacterController>();
        SetupAnimator();
       
    }

    

    // Update is called once per frame
    void Update()
    {
        ApplyGravity();
        isGrounded = characterCon.isGrounded;
        
    }

    void ApplyGravity()
    {
        if (!characterCon.isGrounded)
        {
            if (!resetGravity)
            {
                gravity = physics.resetGravity;
                resetGravity = true;
            }
            gravity += Time.deltaTime * physics.gravityModifier;

        }
        else
        {
            gravity = physics.baseGravity;
            resetGravity = false;
        }
        Vector3 gravityVector = new Vector3();
        if (!isJumping)
        {
            gravityVector.y -= gravity;
        }
        else
        {
            gravityVector.y = movement.jumpspeed;
        }

        characterCon.Move(gravityVector * Time.deltaTime);
    }

    //Animates the character and root motion handles the movement
    public void Animate(float foward,float strafe)
    {
        anim.SetFloat(animations.verticalVelocity, foward);
        anim.SetFloat(animations.horizantalVelocity, strafe);
        anim.SetBool(animations.goundedbool, isGrounded);
        anim.SetBool(animations.jumpbool, isJumping);

    }

    public void Jump()
    {
        if (isJumping)
        {
            return;
        }
        if (isGrounded)
        {
            isJumping = true;
            StartCoroutine(StopJump());
            
        }

    }

    IEnumerator StopJump()
    {
        yield return new WaitForSeconds(movement.jumptime);
            isJumping = false;
       
    }

    //Set up animator with the child avatar
    void SetupAnimator()
    {
        Animator[] animators = GetComponentsInChildren<Animator>();
        
        if(animators.Length > 0)
        {
            for(int i = 0; i < animators.Length; i++)
            {
                Animator animator = animators[i];
                Avatar av = animator.avatar;
                
                if (animator != anim)
                {
                    anim.avatar = av;
                    Destroy(animator);
                }
            }
        }
    }
}
