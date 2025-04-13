using System;
using EasyCharacterMovement;
using UnityEngine;

[System.Serializable]
public class PlayerControllerData
{
    public float rotationRate = 540.0f;
    public float maxSpeed = 5f;
    public float customAngle = 45f;
    public float acceleration = 20.0f;
    public float deceleration = 20.0f;
    public float groundFriction = 8.0f;
    public float airFriction = 0.5f;
    [Range(0.0f, 1.0f)] public float airControl = 0.3f;
    public Vector3 gravity = Vector3.down * 9.81f;
    internal CharacterMovement characterMovement;
}


[RequireComponent(typeof(CharacterMovement)), RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Data Refrence")] public PlayerControllerData playerControllerData;
    public AnimationController animationController;
    private Vector3 movementDirection;
    private Vector3 desiredVelocity;

    private void Awake()
    {
        playerControllerData.characterMovement = GetComponent<CharacterMovement>();
    }

    private InputManager inputManager;

    private void Start()
    {
        inputManager = InputManager.instance;
    }

    private void Update()
    {
        HandleMovement();
        HandleAnimation();
    }

    private float horizontal;
    private float vertical;

    #region Movement

    public void HandleMovement()
    {
        horizontal = inputManager.MoveInput.x;
        vertical = inputManager.MoveInput.y;

        movementDirection = Vector3.zero;
        movementDirection += Vector3.right * horizontal;
        movementDirection += Vector3.forward * vertical;

        movementDirection = Quaternion.AngleAxis(playerControllerData.customAngle, Vector3.up) * movementDirection;
        movementDirection = Vector3.ClampMagnitude(movementDirection, 1.0f);

        playerControllerData.characterMovement.RotateTowards(movementDirection,
            playerControllerData.rotationRate * Time.deltaTime * 2f);

        desiredVelocity = movementDirection * playerControllerData.maxSpeed;

        float actualAcceleration = playerControllerData.characterMovement.isGrounded
            ? playerControllerData.acceleration
            : playerControllerData.acceleration * playerControllerData.airControl;
        float actualDeceleration =
            playerControllerData.characterMovement.isGrounded ? playerControllerData.deceleration : 0.0f;

        float actualFriction = playerControllerData.characterMovement.isGrounded
            ? playerControllerData.groundFriction
            : playerControllerData.airFriction;

        playerControllerData.characterMovement.SimpleMove(desiredVelocity, playerControllerData.maxSpeed,
            actualAcceleration, actualDeceleration, actualFriction, actualFriction, playerControllerData.gravity);
    }

    public bool IsMoving()
    {
        return desiredVelocity.sqrMagnitude > 0.0001f;
    }

    private float velocity;

    public float GetVelocity()
    {
        velocity = new Vector2(horizontal, vertical).magnitude;
        return velocity = Mathf.Clamp01(velocity);
    }

    #endregion

    #region Animation

    public void HandleAnimation()
    {
        if (IsMoving())
        {
            if (animationController.startanimation != AnimType.Move)
            {
                animationController.PlayAnimation(AnimType.Move, 0);
            }

            animationController.controller.SetFloat("Velocity", GetVelocity());
        }
        else
        {
            if (animationController.startanimation != AnimType.Idle)
            {
                animationController.PlayAnimation(AnimType.Idle, 0);
            }
        }
    }

    #endregion
}