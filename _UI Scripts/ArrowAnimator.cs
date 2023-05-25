using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ArrowAnimator : MonoBehaviour
{
    [SerializeField] SpriteRenderer ArrowObject;
    [SerializeField] private Vector3 endPoint;
    //Up = -0.1f, Down = 0.1f
    //Left = 0.1f, Right = -0.1f

    void Start()
    {
        if(ArrowObject == null) ArrowObject = GetComponent<SpriteRenderer>();
        //Use endPoint as an offset
        if(transform.position != Vector3.zero) 
            endPoint = new Vector3(transform.position.x + endPoint.x, 
            transform.position.y + endPoint.y, transform.position.z);
        
        Active();
    }

    void Active()
    {
        transform.DOMove(endPoint, .5f).SetLoops(-1, LoopType.Yoyo);
    }
}
