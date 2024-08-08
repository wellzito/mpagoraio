using UnityEngine;

namespace ReadyPlayerMe.Samples.QuickStart
{
    public class LookAtCamera : MonoBehaviour
    {
        [SerializeField] private GameObject cam;

        private void Update()
        {
            cam = Camera.main.gameObject;
            transform.LookAt(cam.transform);
        }
    }
}
