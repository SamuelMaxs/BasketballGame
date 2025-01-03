using UnityEngine;

public class Ring : MonoBehaviour
{
    public Vector3 spawnAreaMin = new Vector3(-100f, 1f, -100f); // Batas minimum area spawn
    public Vector3 spawnAreaMax = new Vector3(100f, 1f, 100f);   // Batas maksimum area spawn
    public Transform spotlight; // Referensi ke Spotlight
    public Transform ballTransform; // Referensi ke transform bola
    private Vector3 ballInitialPosition; // Posisi awal bola

    private void Start()
    {
        if (ballTransform != null)
        {
            ballInitialPosition = ballTransform.position; // Simpan posisi awal bola
        }
    }

    private void OnEnable()
    {
        Ball.OnBallScored += MoveRing; // Subscribe ke event BallScored
    }

    private void OnDisable()
    {
        Ball.OnBallScored -= MoveRing; // Unsubscribe dari event BallScored
    }

    void MoveRing()
    {
        // Hitung posisi baru secara acak dalam area spawn
        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomZ = Random.Range(spawnAreaMin.z, spawnAreaMax.z);
        float fixedY = spawnAreaMin.y; // Tinggi Y tetap (1)

        Vector3 newPosition = new Vector3(randomX, fixedY, randomZ);

        // Pindahkan ring ke posisi baru
        transform.position = newPosition;

        // Pindahkan spotlight ke posisi baru
        if (spotlight != null)
        {
            spotlight.position = new Vector3(randomX, spotlight.position.y, randomZ);
        }

        // Atur rotasi ring agar menghadap posisi awal bola hanya pada sumbu Y
        if (ballTransform != null)
        {
            Vector3 directionToBall = ballInitialPosition - transform.position;
            directionToBall.y = 0; // Abaikan sumbu Y untuk hanya menghitung arah horizontal
            Quaternion targetRotation = Quaternion.LookRotation(directionToBall);
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0); // Hanya gunakan rotasi Y
        }
    }
}
