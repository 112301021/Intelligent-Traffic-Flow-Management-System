using System.Collections;
using UnityEngine;

/// <summary>
/// Legacy traffic checker for the Right lane.
/// NOTE: This component was an early prototype for stop-line logic.
/// The main stop-line handling is now implemented in Paths.cs (CheckForRed).
/// This script can be removed if scene setup uses Paths.cs signal checking exclusively.
/// </summary>
public class Right_TF : MonoBehaviour
{
    private float timer;
    private GameObject temp;

    [SerializeField] private float redLightTime = 4f;
    [SerializeField] private float defaultSpeed = 0.4f;

    private int phase = 0; // 0 = blocking, 1 = releasing

    private void Awake()
    {
        temp = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car") && phase == 0)
        {
            other.GetComponent<Paths>().speed = 0;
            temp = other.gameObject;
            StartCoroutine(WaitForRed());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Car") && phase == 0)
        {
            other.GetComponent<Paths>().speed = 0;
        }
    }

    private IEnumerator WaitForRed()
    {
        yield return new WaitForSeconds(redLightTime);
        phase = 1;
    }

    void Update()
    {
        if (phase == 1)
        {
            timer += Time.deltaTime;

            if (temp != null)
                temp.GetComponent<Paths>().speed = defaultSpeed;
        }

        if (timer > redLightTime)
        {
            phase = 0;
            timer = 0;
        }
    }
}
