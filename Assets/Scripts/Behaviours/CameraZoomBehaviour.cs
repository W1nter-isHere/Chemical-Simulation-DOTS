using UnityEngine;

namespace Behaviours
{
    public class CameraZoomBehaviour : MonoBehaviour
    {
        private Camera _mainCamera;
        
        private void Start()
        {
            _mainCamera = GetComponent<Camera>();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Minus))
            {
                _mainCamera.orthographicSize += 0.5f;
            }
        
            if (Input.GetKey(KeyCode.Equals))
            {
                _mainCamera.orthographicSize -= 0.5f;
            }
            
            _mainCamera.orthographicSize = Mathf.Clamp(_mainCamera.orthographicSize, 1f, 50f);
        }
    }
}