using UnityEngine;

public class Light : MonoBehaviour
{
    public Transform target; // Referensi ke transform Ring

    void Update()
    {
        if (target != null)
        {
            // Ikuti posisi Ring
            transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);
        }
    }
}
