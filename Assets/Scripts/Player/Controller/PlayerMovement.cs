using Fusion;
using Fusion.Addons.KCC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : NetworkBehaviour
{
    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
    private static readonly int JumpHash = Animator.StringToHash("JumpTrigger");
    private static readonly int FreeFallHash = Animator.StringToHash("FreeFall");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    private KCC _cc;
    private Transform _transform;
    [Header("Player Name")]

    public float PlayerSpeed = 15f;
    public float TurboSpeed = 30f;
    public float NormalSpeed = 15f;
    public float WalkSpeed = 5f;
    public float pushForce = 15f; // Força de empurrar a bola de forma passiva
    public float extraPushForce = 15;
    public int _hitMask;

    [Header("Collider Fire")]
    public GameObject _kickCollider;

    public Animator _anim;
    [Networked] private int _fireTick { get; set; }
    [Networked] private Vector3 _firePosition { get; set; }

    [Networked] private Vector3 _fireVelocity { get; set; }

    [Header("Fire Settings")]

    public float timeToFire = .35f;
    private bool isFiring = false;
    [Networked] public NetworkBool _colisionEnabled { get; set; }

    
    public bool smoothRotation = false;
    public float smoothSpeed = 0.125f;

    private float _animationBlend;
    public bool canMove = true;

    public float SpeedChangeRate = 10.0f;


    [Header("KCC Capsule Collider")]
    public float radius = 2, height = 2;
    public Vector3 position = new Vector3(0, 2, 0);
    [Header("KCC Speed Parameter")]
    public EnvironmentProcessor speedProcessor;


    private void Awake()
    {
        _cc = GetComponent<KCC>();
        if (_anim == null) _anim = GetComponentInChildren<Animator>();
        _transform = transform;
    }

    public override void Spawned()
    {

        
    }

    private void Update()
    {
        
        if (CCB.Controller.GameController.instance != null)
        {
            canMove = CCB.Controller.GameController.instance.canMove;
        }
    }

    public override void Render()
    {
        if (HasStateAuthority)
        {
            _cc.Collider.radius = radius;
            _cc.Collider.height = height;
            _cc.Collider.center = position;
        }
        if (HasInputAuthority)
        {

            if (Input.GetKeyDown(KeyCode.F12)) RPC_PushForce(1);
            if (Input.GetKeyDown(KeyCode.F11)) RPC_PushForce(-1);

        }
    }
    public override void FixedUpdateNetwork()
    {


        if (_kickCollider != null) _kickCollider.SetActive(_colisionEnabled);

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(horizontalInput, 0, verticalInput) * Runner.DeltaTime * PlayerSpeed;
        move = Vector3.zero;
        if (GetInput(out NetworkInputData data2) /*&& canMove*/)
        {
            data2.direction.Normalize();

            move = 5 * PlayerSpeed * Runner.DeltaTime * data2.direction;
            if (move != Vector3.zero)
                _animationBlend = Mathf.Lerp(_animationBlend, PlayerSpeed, Runner.DeltaTime * SpeedChangeRate);
            else _animationBlend = Mathf.Lerp(_animationBlend, move.magnitude, Runner.DeltaTime * SpeedChangeRate);

            if (_animationBlend < 0.01f) _animationBlend = 0f;
            // _anim.SetFloat("Speed", _animationBlend);
            //_anim.SetFloat("MotionSpeed", move.magnitude);
            _anim.SetFloat(MoveSpeedHash, _animationBlend);
        }
        if (Runner.TryGetInputForPlayer(Object.InputAuthority, out NetworkInputData data) == true /*&& canMove*/)
        {
            data.direction.Normalize();

            move = 5 * PlayerSpeed * Runner.DeltaTime * data.direction;
            PlayerSpeed = data.isShift ? TurboSpeed : NormalSpeed;
            speedProcessor.KinematicSpeed = PlayerSpeed;
            //_cc.maxSpeed = PlayerSpeed;
            //Debug.Log(data.rawInput.magnitude);
            //_anim.SetFloat("moveX", data.rawInput.x);
            //_anim.SetFloat("moveY", move.magnitude);
            if (move != Vector3.zero)
                _animationBlend = Mathf.Lerp(_animationBlend, PlayerSpeed, Runner.DeltaTime * SpeedChangeRate);
            else _animationBlend = Mathf.Lerp(_animationBlend, move.magnitude, Runner.DeltaTime * SpeedChangeRate);

            if (_animationBlend < 0.01f) _animationBlend = 0f;

            _anim.SetFloat(MoveSpeedHash, _animationBlend);
            _anim.SetBool(IsGroundedHash, true);//SetFloat("MotionSpeed", move.magnitude);
            if (data.magnitude < 0.3f && data.onMobile || data.direction.magnitude > 0.1f && Input.GetKey(KeyCode.JoystickButton4))
            {
                PlayerSpeed = WalkSpeed;
                _anim.SetFloat(MoveSpeedHash, WalkSpeed);
                move = 5 * PlayerSpeed * Runner.DeltaTime * data.direction;

                if(data.magnitude != 0)
                _anim.SetBool(IsGroundedHash, true);
            }


            /* _anim.SetFloat("moveX", data.rawInput.x);
             _anim.SetFloat("moveY", data.rawInput.y);*/



            //_cc.maxSpeed = PlayerSpeed;


            /* _anim.SetFloat("moveX", data.rawInput.x);
             _anim.SetFloat("moveY", data.rawInput.y);*/

            if (!IsProxy)
            {
                if (data.fire && !isFiring) StartCoroutine(DelayedFire());

            }

        }
        else
        {
            //_anim.SetFloat(MoveSpeedHash, 0);
           // _anim.SetBool(IsGroundedHash, true);
        }



        _cc.SetInputDirection(move); // move no servidor e nos proxies, para simular deterministico :D

        if (move != Vector3.zero && !smoothRotation)
        {
            // Mantém o jogador reto nos eixos X e Z
            Quaternion targetRotation = Quaternion.LookRotation(move.normalized, Vector3.up);
            targetRotation.x = 0; // Mantém o jogador reto no eixo X
            targetRotation.z = 0; // Mantém o jogador reto no eixo Z
            _transform.rotation = targetRotation;
            _cc.SetLookRotation(targetRotation);
        }
        else if (move != Vector3.zero && smoothRotation)
        {
            // Mantém o jogador reto nos eixos X e Z
            Quaternion targetRotation = Quaternion.LookRotation(move.normalized, Vector3.up);
            targetRotation.x = 0; // Mantém o jogador reto no eixo X
            targetRotation.z = 0; // Mantém o jogador reto no eixo Z

            // Suaviza a rotação
            //smoothSpeed = 0.125f; // Ajuste este valor para alterar a velocidade de suavização
            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, smoothSpeed);
            _cc.SetLookRotation(Quaternion.Slerp(_transform.rotation, targetRotation, smoothSpeed));
        }


        if (IsProxy == true)
            return;

        //LagCompensation
        var previousPosition = GetMovePosition(Runner.Tick - 1);
        var nextPosition = GetMovePosition(Runner.Tick);
        var direction = nextPosition - previousPosition;
        if (Runner.LagCompensation.Raycast(previousPosition, direction, direction.magnitude, Object.InputAuthority,
             out var hit, _hitMask, HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority))
        {
            if (hit.Collider == null) return;
            // Resolve collision
            Rigidbody rb = hit.Collider.attachedRigidbody;

            // Se não houver Rigidbody ou se o Rigidbody for cinemático, não faz nada
            if (rb == null || rb.isKinematic)
                return;

            // Empurra a bola na direção do movimento do personagem
            float force = 0;
            if (data.isShift) force = pushForce + extraPushForce;
            else force = pushForce;
            rb.velocity = -hit.Normal * force;
        }
    }

    private Vector3 GetMovePosition(float currentTick)
    {
        float time = (currentTick - _fireTick) * Runner.DeltaTime;

        if (time <= 0f)
            return _firePosition;

        return _firePosition + _fireVelocity * time;
    }

    // Método chamado quando o Rigidbody colide com alguma coisa
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Player"))
        {
            //DoPunch(); estava pensando em um sistema de dar soco no player kkkkk
        }
        //if (!HasStateAuthority) return;

        Rigidbody rb = collision.collider.attachedRigidbody;
        if (rb == null) return;

        if (!rb.gameObject.CompareTag("Ball")) return;

        // Empurra a bola na direção do movimento do personagem
        rb.velocity = -collision.contacts[0].normal * pushForce;
    }

    #region Fire
    IEnumerator DelayedFire()
    {
        isFiring = true;
        _anim.SetTrigger("isKick");
        yield return new WaitForSeconds(timeToFire);
        _colisionEnabled = true;
        yield return new WaitForSeconds(timeToFire / 2);
        _colisionEnabled = false;
        yield return new WaitForSeconds(.4f);
        isFiring = false;
    }
    #endregion

    


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_PushForce(int value)
    {
        pushForce += value;
    }
}
