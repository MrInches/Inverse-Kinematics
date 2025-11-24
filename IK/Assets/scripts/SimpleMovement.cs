using UnityEngine;

// Controla o movimento do corpo principal com WASD para você testar
public class SimpleMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotateSpeed = 90f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");   // A/D ou left/right
        float v = Input.GetAxis("Vertical");     // W/S ou up/down

        Vector3 forward = transform.forward * v * moveSpeed * Time.deltaTime;
        transform.position += forward;
        transform.Rotate(0f, h * rotateSpeed * Time.deltaTime, 0f);
    }
}