using System;
using System.Collections;
using Activators;
using UnityEngine;

public class AutomaticDoor : MonoBehaviour
{
    [SerializeField] private ActivatorType activatorType;
    [SerializeField] private int id;
    [SerializeField] private float doActionFor;
    [SerializeField] private float keepActionFor;
    [SerializeField] private Vector2 positionDelta;
    private bool isMoving;
    private float time;
    private Vector2 startPos;
    private Vector2 startLerp;
    private Vector2 endLerp;
    private AudioSource audioSource;

    private void Awake()
    {
        isMoving = false;
        startPos = transform.position;

        audioSource = GetComponent<AudioSource>();
        
        switch (activatorType)
        {
            case ActivatorType.Lever:
                Lever.Activate += Open;
                Lever.Deactivate += Close;
                break;
            case ActivatorType.PressurePlate:
                PressurePlate.Activate += Open;
                PressurePlate.Deactivate += Close;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Open(int activatorId)
    {
        if (activatorId != id) return;
        StartCoroutine(WaitAndSetFor(true, 0f));
    }

    private void Close(int activatorId)
    {
        if (activatorId != id) return;
        StartCoroutine(WaitAndSetFor(false, keepActionFor));
    }

    private IEnumerator WaitAndSetFor(bool up, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isMoving && delay > 0.1f) yield break;
        isMoving = true;
        time = 0;

        startLerp = startPos + positionDelta * (up ? 0 : 1);
        endLerp = startLerp + positionDelta * (up ? 1 : -1);

        var progress = ((Vector2) transform.position - endLerp).magnitude / positionDelta.magnitude;
        time = doActionFor - doActionFor * progress;
        
        audioSource.Play();
    }
    
    private void Update()
    {
        if(!isMoving) return;
        
        time += Time.deltaTime;

        transform.position = Vector2.Lerp(startLerp, endLerp, time / doActionFor);

        if (!(time >= doActionFor)) return;
        isMoving = false;
    }
}
