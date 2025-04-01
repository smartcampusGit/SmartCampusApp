using UnityEngine;

public class SpinnerRotator : MonoBehaviour
{
    public float rotationSpeed = 200f;

    void Update()
    {
        transform.Rotate(Vector3.forward, -rotationSpeed * Time.deltaTime);
    }
}
