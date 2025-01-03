using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Ball : MonoBehaviour
{
    public float power = 0f;
    public float maxPower = 10f;
    public float powerIncreaseRate = 10f;
    public float upwardForceFactor = 15f;
    public float forwardForce = 10f;
    public float spinForce = 500f; // Besarnya gaya putaran pada bola
    public bool isCharging = false;
    public Slider powerSlider;
    public Image sliderFill;

    public float rotationSpeed = 100f; // Kecepatan rotasi bola ke kiri/kanan

    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool hasScored = false;
    private bool canShoot = true; // Untuk memastikan hanya satu tembakan yang dapat dilakukan hingga bola di-reset
    private bool isIncreasing = true; // Menentukan arah pengisian daya (naik atau turun)

    private Coroutine resetTimerCoroutine; // Coroutine untuk reset bola otomatis

    // Event untuk memberitahu Ring bahwa bola telah mencetak skor
    public delegate void BallScoredAction();
    public static event BallScoredAction OnBallScored;

    // Variabel untuk kamera
    public Transform cameraTransform; // Referensi ke transform kamera
    public Vector3 cameraOffset = new Vector3(0, 7, 13); // Offset posisi kamera
    public float cameraSmoothSpeed = 0.125f; // Kecepatan perpindahan kamera

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (powerSlider != null)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = maxPower;
            powerSlider.gameObject.SetActive(false);
        }

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        ResetBall(); // Inisialisasi posisi awal dan set isKinematic ke true
    }

    void Update()
    {
        // Logika untuk kembali ke main menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Main_menu");
        }

        // Mengubah arah bola ke kiri atau kanan
        if (rb.isKinematic) // Hanya bisa mengubah arah jika bola belum dilempar
        {
            RotateBall();
        }

        // Mulai mengisi daya saat tombol Space atau Mouse ditekan
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && canShoot && !isCharging)
        {
            isCharging = true;
            power = 0f;
            if (powerSlider != null) powerSlider.gameObject.SetActive(true);
        }

        // Melempar bola saat tombol Space atau Mouse dilepas
        if ((Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonUp(0)) && isCharging)
        {
            Shoot();
            isCharging = false;
            if (powerSlider != null) powerSlider.gameObject.SetActive(false);
        }

        // Oscillating power logic
        if (isCharging)
        {
            UpdateSliderColor();
            if (isIncreasing)
            {
                power += powerIncreaseRate * Time.deltaTime;
                if (power >= maxPower)
                {
                    power = maxPower;
                    isIncreasing = false; // Ubah arah pengisian daya
                }
            }
            else
            {
                power -= powerIncreaseRate * Time.deltaTime;
                if (power <= 0f)
                {
                    power = 0f;
                    isIncreasing = true; // Ubah arah pengisian daya
                }
            }

            if (powerSlider != null)
            {
                powerSlider.value = power;
            }
        }
    }

    void LateUpdate()
    {
        if (cameraTransform != null)
        {
            if (rb.isKinematic)
            {
                // Kamera mengikuti pergerakan bola saat diarahkan
                UpdateCameraWhileAiming();
            }
            else
            {
                // Kamera mengikuti bola saat dilempar
                UpdateCameraAfterShoot();
            }
        }
    }

    void RotateBall()
    {
        // Input keyboard untuk rotasi
        float rotationInput = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            rotationInput = -1f; // Rotasi ke kiri
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rotationInput = 1f; // Rotasi ke kanan
        }

        // Terapkan rotasi
        transform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        if (!canShoot) return; // Cegah tembakan tambahan
        canShoot = false; // Blokir input tembakan hingga bola di-reset

        rb.isKinematic = false; // Aktifkan fisika sebelum menembak
        rb.drag = 0.1f; // Tambahkan resistansi udara
        rb.angularDrag = 0.05f; // Hambatan rotasi bola

        Vector3 force = transform.forward * forwardForce + Vector3.up * power * upwardForceFactor;
        Debug.Log($"Force Applied: {force}"); // Debug gaya yang diterapkan
        rb.AddForce(force, ForceMode.Impulse);

        // Tambahkan gaya rotasi (spin) pada bola
        Vector3 spinDirection = transform.right; // Rotasi memutar pada sumbu X
        rb.AddTorque(spinDirection * spinForce, ForceMode.Impulse);

        // Mulai hitungan waktu untuk reset bola otomatis
        if (resetTimerCoroutine != null)
        {
            StopCoroutine(resetTimerCoroutine);
        }
        resetTimerCoroutine = StartCoroutine(ResetBallAfterDelay(5f));
    }

    void UpdateCameraWhileAiming()
    {
        // Kamera mengikuti pergerakan kiri/kanan bola (saat diarahkan)
        Vector3 desiredPosition = transform.position - (transform.forward.normalized * cameraOffset.z) + (Vector3.up * cameraOffset.y);
        Vector3 smoothedPosition = Vector3.Lerp(cameraTransform.position, desiredPosition, cameraSmoothSpeed);
        cameraTransform.position = smoothedPosition;

        // Kamera menghadap bola
        Vector3 lookAtTarget = transform.position + Vector3.up * 1.0f;
        cameraTransform.LookAt(lookAtTarget);
    }

    void UpdateCameraAfterShoot()
    {
        // Kamera mengikuti bola (saat dilempar)
        Vector3 desiredPosition = transform.position - (Vector3.forward * cameraOffset.z) + (Vector3.up * cameraOffset.y);
        Vector3 smoothedPosition = Vector3.Lerp(cameraTransform.position, desiredPosition, cameraSmoothSpeed);
        cameraTransform.position = smoothedPosition;

        // Kamera tetap menghadap bola tanpa rotasi
        Vector3 lookAtTarget = transform.position + Vector3.up * 1.0f;
        cameraTransform.LookAt(lookAtTarget);
    }

    void OnTriggerEnter(Collider other)
    {
        // Cek jika bola menyentuh ring
        if (other.CompareTag("Ring") && !hasScored)
        {
            hasScored = true; // Set status bahwa bola sudah mencetak skor
            Debug.Log("Score!");
            OnBallScored?.Invoke(); // Memanggil event untuk mencetak skor
            StopResetTimer(); // Hentikan reset otomatis
            ResetBall(); // Reset posisi bola
        }
    }

    private IEnumerator ResetBallAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Reset bola jika tidak menyentuh ring dalam waktu tertentu
        if (!hasScored)
        {
            Debug.Log("Reset Ball: Timeout");
            ResetBall();
        }
    }

    private void StopResetTimer()
    {
        if (resetTimerCoroutine != null)
        {
            StopCoroutine(resetTimerCoroutine);
            resetTimerCoroutine = null;
        }
    }

    void UpdateSliderColor()
    {
        if (sliderFill != null)
        {
            // Menghitung lerp warna berdasarkan nilai power
            Color sliderColor = Color.Lerp(Color.yellow, Color.red, power / maxPower);
            sliderFill.color = sliderColor;
        }
    }

    void ResetBall()
    {
        rb.isKinematic = true; // Nonaktifkan fisika bola
        rb.drag = 0f; // Reset drag
        rb.angularDrag = 0.05f; // Reset hambatan rotasi
        transform.position = initialPosition; // Kembali ke posisi awal
        transform.rotation = initialRotation; // Reset rotasi ke awal
        rb.velocity = Vector3.zero; // Hentikan pergerakan
        rb.angularVelocity = Vector3.zero; // Hentikan rotasi
        hasScored = false; // Reset skor status
        power = 0f; // Reset daya
        isIncreasing = true; // Reset arah pengisian daya
        canShoot = true; // Izinkan tembakan lagi
    }
}
