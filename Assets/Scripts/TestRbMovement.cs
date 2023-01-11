using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class TestRbMovement : MonoBehaviour
{
    #region inspector
    [SerializeField] private Text _grounded;
    [SerializeField] private Text _jumpPressed;
    [SerializeField] private Text _jumping;
    [SerializeField] private Text _velocity;
    [SerializeField] private Text _movementVec;
    [SerializeField] private Text _peak;
    [SerializeField] private float _verticalJumpBufferLength = 1f;
    [SerializeField, Range(-100f, 0f)] private float _baseGravity = -20f;
    [SerializeField, Range(-200, 0f)] private float _maxFallingSpeed = -50f;
    [Header("jump")]
    [SerializeField] private float JumpTime = 0.5f;
    [SerializeField] private float JumpApex = 2f;
    [SerializeField, Range(0.1f, 10f)] private float JumpGravityMultiplier = 2f;
    [SerializeField, Range(0.01f, 1f)] private float peakGravityMultiplier = 0.5f;
    [SerializeField, Range(0f, 2f)] private float peakRange = 0.15f;
    [SerializeField] private float peakHorizontalMovementMultiplier = 1.3f;
    [Header("run")]
    [SerializeField] private float _maxRunSpeed = 20f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 20f;
    [SerializeField, Range(0f, 3f)] private float _airAccelerationMultiplier = 1.2f;
    [SerializeField, Range(0f, 1f)] private float _airDecelerationMultiplier = 0.4f;
    #endregion
    #region Component
    private Rigidbody rb;
    private static InputActions _controls;
    private Vector2 _movementDirection;
    private bool _isJumpPressed;
    private bool _isHookPressed;
    private bool _isBallPressed;
    private bool _isGlidePressed;
    private bool _isStompPressed;
    private bool _isDashPressed;
    private List<Collider> _collidersList;
    private BoxCollider _feetBoxCollider;
    #endregion
    #region bools
    private bool _isGrounded = false;
    private bool _isJumping = false;
    private bool _isFalling = false;
    #endregion
    #region jumpvars
    private float _jumpGravity;
    private float _initialJumpVel;
    private Vector2 _rayCastPosition;
    #endregion
    private Vector2 _finalForce = Vector2.zero;
    private void Awake()
    {
        if (_controls == null)
            _controls = new InputActions();
    }

    private void OnEnable()
    {
        if (_controls != null)
        {
            _controls.PlayerControllerScheme.Enable();
            //movement
            _controls.PlayerControllerScheme.Movement.performed += context => _movementDirection =  context.ReadValue<Vector2>();
            _controls.PlayerControllerScheme.Movement.canceled += _ => _movementDirection = Vector2.zero;
            //jump
            _controls.PlayerControllerScheme.Jump.started += _ => _isJumpPressed = true;
            _controls.PlayerControllerScheme.Jump.canceled += _ => _isJumpPressed = false;
            //Hook
            _controls.PlayerControllerScheme.Hook.started += _ => _isHookPressed = true;
            _controls.PlayerControllerScheme.Hook.canceled += _ => _isHookPressed = false;
            //Ball
            _controls.PlayerControllerScheme.Ball.started += _ => _isBallPressed = true;
            _controls.PlayerControllerScheme.Ball.canceled += _ => _isBallPressed = false;
            //Glide
            _controls.PlayerControllerScheme.Glide.started += _ => _isGlidePressed = true;
            _controls.PlayerControllerScheme.Glide.canceled += _ => _isGlidePressed = false;
            //Stomp
            _controls.PlayerControllerScheme.Stomp.started += _ => _isStompPressed = true;
            _controls.PlayerControllerScheme.Stomp.canceled += _ => _isStompPressed = false;
            //Dash
            _controls.PlayerControllerScheme.Dash.started += _ => _isDashPressed = true;
            _controls.PlayerControllerScheme.Dash.canceled += _ => _isDashPressed = false;
        }
        float timeToApex = JumpTime * 0.5f;
        _jumpGravity = (-2 * JumpApex) / Mathf.Pow(timeToApex, 2);
        _initialJumpVel = (2 * JumpApex) / timeToApex;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float timeToApex = JumpTime * 0.5f;
        _jumpGravity = (-2 * JumpApex) / Mathf.Pow(timeToApex, 2);
        _initialJumpVel = (2 * JumpApex) / timeToApex;
        CheckIfGrounded();
        HandleHorizontalMovement();
        HandleVerticalMovement();
        bool _isInPeak = !_isGrounded && rb.velocity.y.Between(-peakRange, peakRange);
        _grounded.text = ("grounded: " + _isGrounded);
        _jumpPressed.text = ("jumpPressed: " + _isJumpPressed);
        _jumping.text = ("jumping: " + _isJumping);
        _velocity.text = ("velocity: " + rb.velocity);
        _movementVec.text = ("movementVec " + _movementDirection);
        _peak.text = ("isInPeak " + _isInPeak);
    }

    private void CheckIfGrounded()
    {
        _rayCastPosition = transform.position;
        RaycastHit hit;
        _isGrounded = Physics.Raycast(_rayCastPosition, Vector2.down, out hit, _verticalJumpBufferLength + GetComponent<CapsuleCollider>().bounds.size.y / 2)
            && rb.velocity.y <= 0f;
        if(_isGrounded && _isJumping)
        {
            _isJumping = false;
        }
    }

    private void HandleHorizontalMovement()
    {
        bool _isInPeak = !_isGrounded && rb.velocity.y.Between(-peakRange, peakRange);
        float targetSpeed = _movementDirection.x * _maxRunSpeed;
        float accelRate = (!_movementDirection.x.Between(-0.2f, 0.2f)) ?  _acceleration : _deceleration;
        if (!_isGrounded)
        {
            accelRate = (!_movementDirection.x.Between(-0.2f, 0.2f)) ? _acceleration * _airAccelerationMultiplier 
                : _deceleration * _airDecelerationMultiplier; 
        }
        if(_isJumping && _isInPeak)
        {
            accelRate *= peakHorizontalMovementMultiplier;
            targetSpeed *= peakHorizontalMovementMultiplier;
        }
        float speedDif = targetSpeed - rb.velocity.x;
        float resultingForce = PhysicsExtension.Vertlet(rb.velocity.x, speedDif * accelRate);
        //rb.AddForce(Vector2.right * resultingForce, ForceMode.Force);
        _finalForce.x = resultingForce;
        //_finalForce.x = Mathf.Clamp(_finalForce.x, -_maxRunSpeed, _maxRunSpeed);
    }

    private void HandleVerticalMovement()
    {
        if (_isGrounded)
        {
            if (_isJumpPressed && !_isJumping)
            {
                _finalForce.y = _initialJumpVel;
                _isJumping = true;
            }
        }
        else
        {
            if (_isJumping)
            {
                _isFalling = rb.velocity.y < -peakRange || _isJumpPressed == false;
                bool _isInPeak = !_isGrounded && rb.velocity.y.Between(-peakRange, peakRange);
                if (_isFalling)
                {
                    _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, _jumpGravity * JumpGravityMultiplier);
                    _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
                } else if (_isInPeak)
                {
                    _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, _jumpGravity * peakGravityMultiplier);
                    _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
                    if(PhysicsExtension.Vertlet(_finalForce.y, _jumpGravity * peakGravityMultiplier) < -peakRange)
                    {
                        _isJumpPressed = false;
                    }
                } else 
                {
                    _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, _jumpGravity);
                    _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
                }
            }
            else
            {
                _finalForce.y = _baseGravity;
            }
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = _finalForce;
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Handles.DrawLine(_rayCastPosition, _rayCastPosition + (Vector2.down * (_verticalJumpBufferLength + GetComponent<CapsuleCollider>().bounds.size.y / 2)));
    }


}
