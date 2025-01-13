using UnityEngine;

public class KinematicRBController : MonoBehaviour
{
    // new keyword ekleyerek uyarýyý giderelim
    public new Rigidbody rigidbody;

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += Vector3.up * Time.deltaTime;
        }
    }
}