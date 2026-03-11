using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    [Range(0.1f, 10f)]
    [SerializeField] public float sensitivity = 0.5f;
    [SerializeField] float maxLookDelta = 25f;
    [SerializeField] float minPitch = -80f;
    [SerializeField] float maxPitch = 80f;
    [SerializeField] GameControll gameManager;
    [SerializeField] PlayerController player;
    [SerializeField] Transform hand;

    [SerializeField] float acceleration;

    [Header("FlahlighSettings")]
    [SerializeField] float flashlightAcceleration;
    [SerializeField] AudioClip flashlightSound;
    [SerializeField] AudioClip flashlightFlicker;
    [SerializeField] AudioSource flashlightSoundSource;
    [SerializeField] GameObject flashlightLightSource;
    [SerializeField] int minFlickerCount;
    [SerializeField] int maxFlickerCount;
    [SerializeField] float maxFlickerInterval;
    [Range(0f,10f)]
    [SerializeField] int flickerSmoothing;



    public bool accessability;

    InputAction lookAction;
    Vector2 currentVelocity;
    Vector2 flashlightVelocity;
    bool hadLookControl;

    Vector3 rotation = Vector3.zero; // x = pitch, y = yaw
    Vector3 flashlightRotation = Vector2.zero; // x = pitch, y = yaw

    //Flashlight Stuff
    float maxIntensity;
    float minIntensity = 0.0f;
    float flickerTimer;
    bool isFlickering;
    Light flashlight;
    Queue<float> smoothQueue;
    float lastSum;

    float tempAcceleration;
    float tempFlashlightAcceleration;

    float handOffset;
    void Start()
    {
        maxIntensity = flashlightLightSource.gameObject.GetComponent<Light>().intensity;
        smoothQueue = new Queue<float>(flickerSmoothing);

        handOffset = hand.position.y - transform.position.y;

        flashlight = flashlightLightSource.GetComponent<Light>();

        lookAction = InputSystem.actions.FindAction("Look");

        // Initialize rotation from current transform
        rotation.x = transform.eulerAngles.x;
        rotation.y = transform.eulerAngles.y;

        flashlightRotation.x = rotation.x;
        flashlightRotation.y = rotation.y;

        tempAcceleration = acceleration;
        tempFlashlightAcceleration = flashlightAcceleration;
        RestartFlicker();
    }

    void Update()
    {
        if (accessability)
        {
            acceleration = 10;
            flashlightAcceleration = 10;
        }else
        {
            acceleration = tempAcceleration;
            flashlightAcceleration = tempFlashlightAcceleration;
        }
        bool canControlLook = !gameManager.pause && !player.locked && !player.lockedByDying;

        if (!canControlLook)
        {
            hadLookControl = false;
            ResetLookSmoothing();
            return;
        }

        if (!hadLookControl)
        {
            hadLookControl = true;
            ResetLookSmoothing();
            return;
        }

        Vector2 rawLookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
        rawLookInput = Vector2.ClampMagnitude(rawLookInput, maxLookDelta);

        currentVelocity = Vector2.Lerp(currentVelocity, rawLookInput, acceleration * Time.deltaTime);

        rotation.x -= currentVelocity.y * (sensitivity * 0.07f);
        rotation.y += currentVelocity.x * (sensitivity * 0.07f);
        rotation.x = Mathf.Clamp(rotation.x, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(rotation);

        if(flashlightLightSource  != null)
        {
            flashlightVelocity = Vector2.Lerp(flashlightVelocity, rawLookInput, flashlightAcceleration * Time.deltaTime);

            flashlightRotation.x -= flashlightVelocity.y * (sensitivity * 0.07f);
            flashlightRotation.y += flashlightVelocity.x * (sensitivity * 0.07f);
            flashlightRotation.x = Mathf.Clamp(flashlightRotation.x, minPitch, maxPitch);

            if (Input.GetKey(KeyCode.F))
            {
                flashlightRotation.y = Mathf.Lerp(flashlightRotation.y, rotation.y, 0.1f);
                flashlightRotation.x = Mathf.Lerp(flashlightRotation.x, rotation.x, 0.1f);
            }

            hand.rotation = Quaternion.Euler(flashlightRotation);

            if (!isFlickering  && flashlightLightSource.activeSelf)
            {   
                flickerTimer -= Time.deltaTime;
                if(flickerTimer <= 0)
                {
                    StartCoroutine(FlickerLight());
                }
            }
            hand.position = new Vector3(hand.position.x, transform.position.y + handOffset , hand.position.z);
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        ResetLookSmoothing();
        if (!hasFocus)
        {
            hadLookControl = false;
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        ResetLookSmoothing();
        if (pauseStatus)
        {
            hadLookControl = false;
        }
    }

    void ResetLookSmoothing()
    {
        currentVelocity = Vector2.zero;
        flashlightVelocity = Vector2.zero;
    }


    void RestartFlicker()
    {
        flickerTimer = Random.Range(0.1f, maxFlickerInterval);
        smoothQueue.Clear();
        lastSum = 0;
    }

    IEnumerator FlickerLight()
    {
        isFlickering = true;
        flashlightSoundSource.PlayOneShot(flashlightFlicker);

        smoothQueue.Clear();
        lastSum = 0f;

        float elapsed = 0f;
        float duration = flashlightFlicker.length;

        while (elapsed < duration)
        {
            while (smoothQueue.Count >= flickerSmoothing)
                lastSum -= smoothQueue.Dequeue();

            float newVal = Random.Range(minIntensity, maxIntensity);
            smoothQueue.Enqueue(newVal);
            lastSum += newVal;
            flashlight.intensity = lastSum / smoothQueue.Count;

            elapsed += Time.deltaTime;
            yield return null; // wait one frame
        }

        bool staysOff = Random.value > 0.7f;
        if (staysOff) flashlightLightSource.SetActive(false);

        isFlickering = false;
  
        RestartFlicker();
    }
}