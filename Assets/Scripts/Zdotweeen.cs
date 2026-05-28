using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Zdotweeen : MonoBehaviour
{

    private void Start()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMoveX(10, 1).SetEase(Ease.OutBounce));
        seq.Join(transform.DOScale(new Vector3(2, 2, 2), 1).SetEase(Ease.InOutBack));

        seq.Append(transform.DORotate(new Vector3(0, 180, 0), 1).SetEase(Ease.InOutBack));

        seq.Append(transform.DOMoveY(5, 1).SetEase(Ease.OutBounce));

        seq.Append(transform.DOMoveX(-10, 1).SetEase(Ease.OutElastic));

        seq.Append(transform.DOMoveY(-2, 1).SetEase(Ease.InFlash));
        seq.Join(transform.DOScale(new Vector3(-1, -1, -1), 1).SetEase(Ease.InExpo));
        seq.Join(transform.DORotate(new Vector3(90, 90, 0), 1).SetEase(Ease.OutQuad));

    }

}