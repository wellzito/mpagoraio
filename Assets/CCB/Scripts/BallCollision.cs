using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class BallCollision : NetworkBehaviour
{
    public static BallCollision instance;

    public Rigidbody rb;
    public NetworkRigidbody3D rbNetwork;
    public float gravityMultiply = 1.5f;

    [Header("Angle Settings")]
    public bool useFixedAngle = false;
    public float fixedAngle = 45f;
    public float minAngle = 45f, maxAngle = 85f;

    [Header("Force Settings")]
    public bool useFixedKickForce = false;
    public float kickForce = 5f;
    public float minKickForce = 5f, maxKickForce = 15f;

    [Header("Ground")]
    public LayerMask groundLayer; // Camada do chão
    public float raycastDistance = 0.1f;

    [Header("Sound Kick")]
    public AudioClip kickSound;
    public AudioSource kickAudioSource;
    public float forceDivide = 3f;
    public float pitchRange = 0.35f;
    public float volumeRange = 0.35f;
    public float otherHitVolume = .5f;
    public float groundHitVolume = .5f;
    private void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody>();
        rbNetwork = GetComponent<NetworkRigidbody3D>();
        kickAudioSource = GetComponent<AudioSource>();
    }

    public override void Render()
    {
        if (!HasStateAuthority) return;

        if (Input.GetKeyDown(KeyCode.F3)) gravityMultiply -= .1f;
        if (Input.GetKeyDown(KeyCode.F4)) gravityMultiply += .1f;

        if (Input.GetKeyDown(KeyCode.R))
        {
            RPC_ResetBallPosition();
        }
    }

    private void Update()
    {
        if (CheckGrounded())
        {
            rb.drag = 3;
            rb.angularDrag = 1;
        }
        else
        {
            rb.drag = 0;
            rb.angularDrag = 0.01f;
        }
    }

    private void FixedUpdate()
    {
        if (!HasStateAuthority) return;

        rb.AddForce(Physics.gravity * rb.mass * gravityMultiply); // Dobra a força da gravidade


        if (transform.position.x >= 70 || transform.position.x <= -70 || transform.position.z >= 70 || transform.position.z <= -70)
        {
            rb.velocity = Vector3.zero;
            transform.position = new Vector3(0, 10, 0);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        float force = (collision.impulse.magnitude / Time.fixedDeltaTime)/ (forceDivide*1000);
        if (HasStateAuthority)
            RPC_HitSound(Random.Range(1 - pitchRange, 1 + pitchRange), Random.Range((1*force) - volumeRange, 1*force));
        //Debug.Log(string.Format("colision force: {0}", force));
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ColBall"))
        {
            if(HasStateAuthority)
            RPC_HitSound(Random.Range(1 - pitchRange, 1 + pitchRange), Random.Range(1 - volumeRange, 1));
            // Gera um ângulo aleatório entre 0 e o ângulo máximo
            float angle = Random.Range(minAngle, maxAngle);
            if (useFixedAngle) angle = fixedAngle;

            // Calcula a direção da força
            //Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * other.transform.forward;
            //Vector3 direction = Quaternion.Euler(angle, 0, angle) * other.transform.forward;
            Vector3 direction = Quaternion.AngleAxis(90 - angle, other.transform.right) * Vector3.up;

            float force = Mathf.Lerp(maxKickForce, minKickForce, angle / maxAngle);
            rb.drag = 0;
            rb.angularDrag = 0.01f;
            if (useFixedKickForce) force = kickForce;
            // Aplica a força
            rb.AddForce(direction * force, ForceMode.Impulse);
        }

        else if (other.CompareTag("Ground"))
        {
            rb.drag = 3;
            rb.angularDrag = 1; 

            //if (HasStateAuthority)
                //RPC_HitSound(Random.Range(1 - pitchRange, 1 + pitchRange), Random.Range(groundHitVolume - volumeRange, groundHitVolume));
        }

        else if (other.CompareTag("GoalLeft"))
        {
            CCB.Controller.GameController.instance.Score(1);
        }
        else if (other.CompareTag("GoalRight"))
        {
            CCB.Controller.GameController.instance.Score(2);
        }
        else
        {
            //if (HasStateAuthority)
                //RPC_HitSound(Random.Range(1 - pitchRange, 1 + pitchRange), Random.Range(otherHitVolume - volumeRange, otherHitVolume));
        }
    }

    bool CheckGrounded()
    {
        // Cria um raio direcionado para baixo a partir da posição da bola
        Ray ray = new Ray(transform.position, Vector3.down);

        // Se um raio atingir o chão, a bola está no chão
        if (Physics.Raycast(ray, raycastDistance, groundLayer))
        {
            // Debug.Log("On Ground");
            return true;
        }
        else
        {
            return false;
        }

        Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.blue);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_ResetBallPosition()
    {
        rb.velocity = Vector3.zero;
        Vector3 position = new Vector3(0, 10, 0);
        rbNetwork.RBPosition = position; // (rb, position, Quaternion.identity);
        //transform.position = position;
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_HitSound(float pitch, float volume)
    {
        kickAudioSource.pitch = pitch;
        kickAudioSource.volume = volume;
        kickAudioSource.PlayOneShot(kickSound);
    }
}
