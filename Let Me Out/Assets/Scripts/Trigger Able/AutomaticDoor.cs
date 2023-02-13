using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticDoor : MonoBehaviour
{
    [SerializeField] private int id;
    [SerializeField] private float doActionFor;
    [SerializeField] private float keepActionFor;
    [SerializeField] private Vector2 positionDelta;
    private bool isMoving;
    private float time;
    private Vector2 startPos;
    private Vector2 endPos;

    private void Awake()
    {
        PressurePlate.Activate += Open;
        PressurePlate.Deactivate += Close;
    }

    private void Open(int activatorId)
    {
        if(activatorId != id) return;

        StartCoroutine(WaitAndSetFor(true, 0f));
    }

    private void Close(int activatorId)
    {
        if(activatorId != id) return;

        StartCoroutine(WaitAndSetFor(false, keepActionFor));
    }

    private IEnumerator WaitAndSetFor(bool up, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        isMoving = true;
        time = 0;
        
        startPos = transform.position;
        endPos = new Vector2(
            startPos.x + positionDelta.x * (up ? 1 : -1), 
            startPos.y + positionDelta.y * (up ? 1 : -1));
    }
    
    private void Update()
    {
        if(!isMoving) return;
        
        time += Time.deltaTime;

        transform.position = Vector2.Lerp(startPos, endPos, time / doActionFor);

        if (time >= doActionFor) isMoving = false;
    }
}
