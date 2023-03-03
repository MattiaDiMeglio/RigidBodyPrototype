using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;
using System.Runtime.CompilerServices;

enum States { grounded, jumping, doubleJumping, falling, gliding, onWall}

public class TestRbMovement : MonoBehaviour
{
    #region inspector
    [SerializeField] private Text _grounded;
    [SerializeField] private Text _jumpPressed;
    [SerializeField] private Text _jumping;
    [SerializeField] private Text _velocity;
    [SerializeField] private Text _movementVec;
    [SerializeField] private Text _peak;
    [SerializeField] private Text _state;
    [SerializeField] private Text _coyoteTime;
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
    [SerializeField] private float DoubleJumpTime = 0.5f;
    [SerializeField] private float DoubleJumpApex = 2f;
    [SerializeField, Range(0.1f, 10f)] private float DoubleJumpGravityMultiplier = 2f;
    [Header("run")]
    [SerializeField] private float _maxRunSpeed = 20f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 20f;
    [SerializeField, Range(0f, 3f)] private float _airAccelerationMultiplier = 1.2f;
    [SerializeField, Range(0f, 1f)] private float _airDecelerationMultiplier = 0.4f;
    [SerializeField, Range(0f, 1f)] private float _coyoteTimeBuffer = 0.2f;
    [Header("glide")]
    [SerializeField, Range(0.1f, 1f)] private float _glideGravityMultiplier = 0.4f;
    [SerializeField, Range(0.1f, 5f)] private float _glideHorizontalVelocityMultiplier = 1.2f;
    [Header("walljump")]
    [SerializeField] private float _wallJumpTimer = 1.2f;
    [SerializeField, Range(0f, 1f)] private float _onWallGravityMultiplier = 0.2f;
    [SerializeField, Range(0f, 1f)] private float _wallJumpLerpAmount = 0.4f;
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
    #endregion
    #region bools
    private bool _onEnter = true;
    private bool _isGrounded = false;
    private bool _isJumping = false;
    private bool _isFalling = false;
    private bool _isInPeak = false;
    private bool _canDjump = false;
    private bool _isDJumping = false;
    private bool _canStillJump = false;
    private bool _coyoteTimerUsed = false;
    private bool _isGliding = false;
    public bool IsGliding => _isGliding;
    private bool _touchingWall = false;
    private bool _isWallJumping = false;
    #endregion
    #region jumpvars
    private float _jumpGravity;
    private float _initialJumpVel;
    private float _dJumpGravity;
    private float _initialDJumpVel;
    private float _wallJumpLerp = 0f;
    private Vector2 _rayCastPosition;
    #endregion
    private Vector2 _finalForce = Vector2.zero;
    private States currentState = States.falling;
    private float coyoteTimeTimer = 0f;
    private float wallDirection = 0f;


    private void Awake()
    {
        if (_controls == null)
            _controls = new InputActions();
    }

    private void OnEnable()//abilitiamo e settiamo i callback degli input
    {
        if (_controls != null)
        {
            _controls.PlayerControllerScheme.Enable();
            //movement
            _controls.PlayerControllerScheme.Movement.performed += context => _movementDirection = context.ReadValue<Vector2>();
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
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>(); //otteniamo l'rb
    }

    private void UpdateUI()
    {
        _grounded.text = ("grounded: " + _isGrounded);
        _jumpPressed.text = ("jumpPressed: " + _isJumpPressed);
        _jumping.text = ("jumping: " + _isJumping);
        _velocity.text = ("velocity: " + rb.velocity);
        _movementVec.text = ("movementVec " + _movementDirection);
        _peak.text = ("isInPeak " + _isInPeak);
        _state.text = ("currentState: " + currentState);
        _coyoteTime.text = ("CoyoteTimeActive: " + _canStillJump);
    }

    private void CheckIfGrounded()
    {
        _rayCastPosition = transform.position;
        RaycastHit hit;
        _isGrounded = Physics.Raycast(_rayCastPosition, Vector2.down, out hit, _verticalJumpBufferLength + GetComponent<CapsuleCollider>().bounds.size.y / 2)
            && rb.velocity.y <= 0f;
        if (_isGrounded)
        {
            _canStillJump = false;
            coyoteTimeTimer = 0f;
            _coyoteTimerUsed = false;
            _isJumping = false;
            _isDJumping = false;
            coyoteTimeTimer = Time.time;
        } else
        {
            if (Time.time > coyoteTimeTimer + _coyoteTimeBuffer)
            {
                _canStillJump = false;
            } else
            {
                _canStillJump = true;
            }

        }
    }

    private void CheckState()
    {
        _isInPeak = !_isGrounded && rb.velocity.y.Between(-peakRange, peakRange);
        _isFalling = rb.velocity.y < -peakRange || _isJumpPressed == false;

        if (_touchingWall && currentState!=States.onWall)
        {
            currentState = States.onWall;
            _isJumping = false;
            _isJumpPressed = false;
            _isGliding = false;
            _onEnter = true;
        }
        switch (currentState)
        {
            case States.falling:
                if (_onEnter)
                {
                    Gravity(1);
                    _onEnter = false;
                    return;
                }
                 if (_isGrounded)
                {
                    currentState = States.grounded;
                    _onEnter = true;
                }
                if (_isJumpPressed && _canStillJump)
                {
                    currentState = States.jumping;
                    _onEnter = true;
                    return;
                }
                if (_isGlidePressed)
                {
                    currentState = States.gliding;
                    _onEnter = true;
                    return;
                }
                Run(true, _airAccelerationMultiplier, _airDecelerationMultiplier, 1f, 1f);
                Gravity(1);
                break;
            case States.grounded:
                if (_onEnter)
                {
                    _canDjump = false;
                    _onEnter = false;
                }
                if (_isJumpPressed)
                {
                    currentState = States.jumping;
                    _onEnter = true;
                    return;
                }
                if (!_isGrounded && !_isJumping)
                {
                    currentState = States.falling;
                    _onEnter = true;
                    return;
                }
                Run(true, 1f, 1f, 1f, 1f);
                break;
            case States.jumping:
                if (_onEnter)
                {
                    if (_isWallJumping)
                    {
                        _finalForce = new Vector2(-1 * wallDirection * _maxRunSpeed, _initialJumpVel);
                        rb.AddForce(_finalForce, ForceMode.Impulse);
                    } else
                    {
                        _finalForce = new Vector2(rb.velocity.x, _initialJumpVel);
                        rb.AddForce(_initialJumpVel * Vector2.up, ForceMode.Impulse);
                    }
                    //rb.AddForce((_initialJumpVel / (JumpTime * .5f)) * Vector2.up, ForceMode.Force);
                    //rb.AddForce(((_initialJumpVel - rb.velocity.y)/Time.fixedDeltaTime) * Vector2.up , ForceMode.Force);
                    //rb.velocity = _finalForce;
                    _isJumping = true;
                    _onEnter = false;
                    return;
                }
                if (_isGrounded && _isFalling)
                {
                    currentState = States.grounded;
                    _isFalling = false;
                    _isWallJumping = false;
                    _isJumping = false;
                    _onEnter = true;
                }
                if (_isGlidePressed)
                {
                    currentState = States.gliding;
                    _isWallJumping = false;
                    _isJumping = false;
                    _onEnter = true;
                    return;
                }
                if (_isFalling && !_canDjump)
                {
                    _isJumpPressed = false;
                    _canDjump = true;
                }
                if (_isJumpPressed && _canDjump) {
                    _isFalling = false;
                    currentState = States.doubleJumping;
                    _onEnter = true;
                    return;
                }
                if (_isFalling)
                {
                    _isJumpPressed = false;
                }
                if(_isWallJumping && _wallJumpLerp >= 1f)
                {
                    _isWallJumping = false;
                }
                Jump(_jumpGravity, JumpGravityMultiplier);
                Run(true, _airAccelerationMultiplier, _airDecelerationMultiplier, _isInPeak ? peakHorizontalMovementMultiplier : 1, _isWallJumping ? (_wallJumpLerp + _wallJumpLerpAmount) : 1f);
                break;
            case States.doubleJumping:
                if (_onEnter)
                {
                    _finalForce = new Vector2(rb.velocity.x, _initialDJumpVel/Time.deltaTime);
                    rb.AddForce(((_initialDJumpVel - rb.velocity.y)) * Vector2.up, ForceMode.Impulse);
                    //rb.AddForce(((_initialDJumpVel - rb.velocity.y ) * (_initialDJumpVel / (DoubleJumpTime *.5f))) * Vector2.up, ForceMode.Force);
                    //rb.AddForce(_initialDJumpVel * Vector2.up, ForceMode.Impulse);
                    _canDjump = false;
                    _onEnter = false;
                    return;
                }
                if (_isGrounded)
                {
                    currentState = States.grounded;
                    _isJumping = false;
                    _isWallJumping = false;
                    _onEnter = true;
                    return;
                }
                if (_isGlidePressed)
                {
                    currentState = States.gliding;
                    _onEnter = true;
                    _isWallJumping = false;
                    return;
                }
                if (_isFalling)
                {
                    _isJumpPressed = false;
                }
                Jump(_dJumpGravity, DoubleJumpGravityMultiplier);
                Run(true, _airAccelerationMultiplier, _airDecelerationMultiplier, _isInPeak ? peakHorizontalMovementMultiplier : 1, 1f);
                break;
            case States.gliding:
                if (_onEnter)
                {
                    _finalForce = -rb.velocity;
                    rb.AddForce(_finalForce, ForceMode.Impulse);
                    _onEnter = false;
                    _isGliding = true;
                    return;
                }
                if (_isGrounded)
                {
                    currentState = States.grounded;
                    _isGliding = false;
                    _isGlidePressed = false;
                    _onEnter = true;
                    return;
                }
                if (!_isGlidePressed)
                {
                    currentState = States.falling;
                    _canStillJump = false;
                    coyoteTimeTimer = 0f;
                    _coyoteTimerUsed = false;
                    _isJumping = false;
                    _isDJumping = false;
                    _isGliding = false;
                    _onEnter = true;
                    return;
                }
                Gravity(_glideGravityMultiplier);
                Run(true, _airAccelerationMultiplier, _airDecelerationMultiplier, _glideHorizontalVelocityMultiplier, 1f);
                break;
            case States.onWall:
                if (_onEnter)
                {
                    _finalForce = new Vector2(rb.velocity.x, -rb.velocity.y);
                    rb.AddForce(_finalForce, ForceMode.Impulse);
                    _canDjump = false;
                    _onEnter = false;
                    return;
                }
                if (_isGrounded)
                {
                    currentState = States.grounded;
                    _touchingWall = false;
                    _onEnter = true;
                    return;
                }
                if (_isJumpPressed)
                {
                    _isWallJumping = true;
                    _touchingWall = false;
                    _wallJumpLerp = 0f;
                    currentState = States.jumping;
                    _onEnter = true;
                    return;
                }
                if (!_touchingWall)
                {
                    _isWallJumping = true;
                    _touchingWall = false;
                    _wallJumpLerp = 0f;
                    currentState = States.falling;
                    _onEnter = true;
                }
                Gravity(_onWallGravityMultiplier);
                Run(false, 1f, 1f, 1f, 1f);
                break;
        }
    }

    private void HandleHorizontalMovement()
    {
        if (_onEnter)
            return;
        if (currentState == States.onWall)
        {
            _finalForce.x = 0f;
            return;
        }
        float targetSpeed = _movementDirection.x * _maxRunSpeed;
        float accelRate = (!_movementDirection.x.Between(-0.2f, 0.2f)) ? _acceleration : _deceleration;
        if (!_isGrounded)
        {
            accelRate = (!_movementDirection.x.Between(-0.2f, 0.2f)) ? _acceleration * _airAccelerationMultiplier
                : _deceleration * _airDecelerationMultiplier;
        }
        if (_isJumping && _isInPeak)
        {
            accelRate *= peakHorizontalMovementMultiplier;
            targetSpeed *= peakHorizontalMovementMultiplier;
        }
        if (currentState == States.gliding)
        {
            accelRate *= _glideHorizontalVelocityMultiplier;
            targetSpeed *= _glideHorizontalVelocityMultiplier;
        }
        float speedDif = targetSpeed - rb.velocity.x;
        float resultingForce = PhysicsExtension.Vertlet(rb.velocity.x, speedDif * accelRate);
        _finalForce.x = resultingForce;
    }

    private void HandleVerticalMovement()
    {
        if (_onEnter)
            return;
        switch (currentState)
        {
            case States.falling:
                _finalForce.y = _baseGravity;
                break;
            case States.grounded:
                break;
            case States.jumping:
                if (_isFalling)
                {
                    _isJumpPressed = false;
                    _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, _jumpGravity * JumpGravityMultiplier);
                    _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
                }
                else if (_isInPeak)
                {
                    _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, _jumpGravity * peakGravityMultiplier);
                    _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
                }
                else
                {
                    _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, _jumpGravity);
                    _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
                }
                break;
            case States.doubleJumping:
                if (_isFalling)
                {
                    _isJumpPressed = false;
                    _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, _dJumpGravity * JumpGravityMultiplier);
                    _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
                }
                else if (_isInPeak)
                {
                    _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, _dJumpGravity * peakGravityMultiplier);
                    _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
                }
                else
                {
                    _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, _dJumpGravity);
                    _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
                }
                break;
            case States.gliding:
                _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, _baseGravity * _glideGravityMultiplier);
                _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
                break;
            case States.onWall:
                _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, _baseGravity * _onWallGravityMultiplier);
                _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
                break;
        }
    }

    private void FixedUpdate()
    {
        //calcoliamo le variabili per il salto ed il doppio salto.
        //(momentaneamente nell'update in modo che se modifichiamo l'inspector le modifiche sono riflesse
        float timeToApex = JumpTime * 0.5f;
        _jumpGravity = (-2 * JumpApex) / Mathf.Pow(timeToApex, 2);
        _initialJumpVel = (2 * JumpApex) / timeToApex;
        float timeToApexDjump = DoubleJumpTime * 0.5f;
        _dJumpGravity = (-2 * DoubleJumpApex) / Mathf.Pow(timeToApexDjump, 2);
        _initialDJumpVel = (2 * DoubleJumpApex) / timeToApexDjump;


        CheckIfGrounded();//controllo grounded
        CheckState();//simil macchina a stati banale
        //HandleHorizontalMovement();//movimento orizzontale
        //HandleVerticalMovement();//movimento verticale
        UpdateUI();//update dell'ui, giusto per controllo
        if(rb.velocity.y < _maxFallingSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, _maxFallingSpeed);
        }
        Debug.Log("vel: " + Time.fixedDeltaTime);
        //rb.AddForce(_finalForce, ForceMode.Force);
        //rb.velocity = _finalForce;
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Handles.DrawLine(_rayCastPosition, _rayCastPosition + (Vector2.down * (_verticalJumpBufferLength + GetComponent<CapsuleCollider>().bounds.size.y / 2)));
    }
#endif

    private void Jump(float gravity, float gMultiplier)
    {
        if (_isFalling)
        {
            _isJumpPressed = false;
            _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, gravity * gMultiplier);
            _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
            rb.AddForce(Vector2.up * gravity * gMultiplier * .5f, ForceMode.Acceleration);
        }
        else if (_isInPeak)
        {
            _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, gravity * peakGravityMultiplier);
            _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
            rb.AddForce(Vector2.up * gravity * peakGravityMultiplier * .5f, ForceMode.Acceleration);
        }
        else
        {
            _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, gravity);
            _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
            rb.AddForce(Vector2.up * gravity * .5f, ForceMode.Acceleration);
        }
        
    }

    private void Gravity(float gravityMultiplier)
    {
        _finalForce.y = PhysicsExtension.Vertlet(rb.velocity.y, _baseGravity * gravityMultiplier);
        _finalForce.y = Mathf.Max(_finalForce.y, _maxFallingSpeed);
        rb.AddForce(Vector2.up * _baseGravity * gravityMultiplier * .5f, ForceMode.Acceleration);
    }

    private void Run(bool canMove, float accelerationMultiplier, float decelerationMultiplier, float velocityMultiplier, float lerpRate)
    {
        if (!canMove)
        {
            rb.AddForce(Vector2.right * -rb.velocity.x, ForceMode.Force);
            return;
        }
        float targetSpeed = _movementDirection.x * _maxRunSpeed;
        float accelRate = (!_movementDirection.x.Between(-0.2f, 0.2f)) ? _acceleration * accelerationMultiplier 
            : _deceleration * decelerationMultiplier;
        accelRate *= velocityMultiplier;
        targetSpeed *= velocityMultiplier;
        targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, lerpRate);
        float speedDif = targetSpeed - rb.velocity.x;
        _finalForce.x = speedDif * accelRate;// resultingForce;
        rb.AddForce(Vector2.right * _finalForce, ForceMode.Force);
    }

    public void InWallJump(float posX)
    {
        if(wallDirection != posX - transform.position.x)
        {
            wallDirection = Mathf.Sign(posX - transform.position.x);
            _touchingWall = true;
            _wallJumpLerp = 0f;
        }
    }

    public void OutWallJump()
    {
        _touchingWall = false;
    }
}
