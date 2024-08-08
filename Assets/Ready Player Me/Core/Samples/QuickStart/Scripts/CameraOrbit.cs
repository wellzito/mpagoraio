using UnityEngine;

namespace ReadyPlayerMe.Samples.QuickStart
{
    public class CameraOrbit : MonoBehaviour
    {
        public CameraFollow cameraFollow; 
        private const float SMOOTH_TIME = 0.1f;
        
        [Tooltip("PlayerInput component is required to listen for input")]
        public PlayerInput playerInput;
        [SerializeField][Tooltip("Used to set lower limit of camera rotation clamping")]
        private float minRotationX = -60f;
        [SerializeField][Tooltip("Used to set upper limit of camera rotation clamping")]
        private float maxRotationX = 50f;

        [SerializeField][Tooltip("Useful to apply smoothing to mouse input")]
        private bool smoothDamp = false;
        
        private Vector3 rotation;
        private Vector3 currentVelocity;

        private float pitch;
        private float yaw;


        public float distance = 10.0f; // Distância da câmera ao objeto
        public float xSpeed = 120.0f; // Velocidade de rotação no eixo X
        public float ySpeed = 120.0f; // Velocidade de rotação no eixo Y

        public float yMinLimit = -20f; // Limite mínimo do ângulo Y
        public float yMaxLimit = 80f; // Limite máximo do ângulo Y

        private float x = 0.0f; // Ângulo atual no eixo X
        private float y = 0.0f; // Ângulo atual no eixo Y

        private void Start()
        {
            Vector3 angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;
        }

        private void Update()
        {
            if (!cameraFollow.enabled) cameraFollow.enabled = true;
        }

        private void LateUpdate()
        {
            if (cameraFollow.target)
            {
                x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                y = ClampAngle(y, yMinLimit, yMaxLimit);

                Quaternion rotation = Quaternion.Euler(y, x, 0);
                Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + cameraFollow.target.transform.position;

                transform.rotation = rotation;
                transform.position = position;
            }
        }

        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
    }
}
