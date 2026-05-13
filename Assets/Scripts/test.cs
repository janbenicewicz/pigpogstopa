using UnityEngine;

public class test : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 100f;

    void Update()
    {
        float move = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        transform.Translate(Vector3.forward * move * speed * Time.deltaTime);

        transform.Rotate(Vector3.up * turn * rotationSpeed * Time.deltaTime);
    }
}