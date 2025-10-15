using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera cam;
    void Start() => cam = Camera.main;
    void LateUpdate()
    {
        if (cam) transform.LookAt(transform.position + cam.transform.forward);
    }
}
