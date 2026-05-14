using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIEI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public Tween currentTween;
    public void Enable()
    {
        Animation();
    }
    public void Disable()
    {
        Animation(false);
    }

    public void Animation(bool forward = true)
    {
        StartTween(canvasGroup.DOFade(forward ? 1 : 0, 1f));
    }


    public Tween StartTween(Tween tween)
    {
        if(currentTween != null)
        {
            Debug.Log("Tween killed");
            currentTween.Kill();
        }
        currentTween = tween;

        return currentTween;               
    }

    //test
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Enable();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Disable();
        }
    }











}
