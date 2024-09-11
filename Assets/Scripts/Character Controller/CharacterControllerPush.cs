using UnityEngine;

public class CharacterControllerPush : MonoBehaviour
{
    public float pushForce = 2.0f;
    public float pushRadius = 0.5f;

    void FixedUpdate()
    {
        PushNearbyRigidbodies();
    }

    void PushNearbyRigidbodies()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, pushRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = nearbyObject.transform.position - transform.position;
                direction.y = 0f; // Keep the push horizontal
                Vector3 push = direction.normalized * pushForce;
                rb.AddForce(push, ForceMode.Impulse);
            }
        }
    }
}