using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PavyonController : MonoBehaviour
{
    public Light directionalLight;

    private float _timeDiff = 2f;
    private float _nextTimeToChange = 0;

    private void Awake()
    {
        directionalLight = GetComponent<Light>();
    }

    private void Start()
    {

        StartCoroutine(PavyonCoroutine());
    }

    private IEnumerator PavyonCoroutine()
    {
        //bir frame bekle, sonra devam et
        yield return null;

        ChangeLightColor();

        yield return new WaitForSeconds(_timeDiff);

        ChangeLightColor();

        if (directionalLight.color == Color.red)
        {
            //exit coroutine
            yield break;
        }

        yield return new WaitForSeconds(_timeDiff);

        ChangeLightColor();

        yield return new WaitForSeconds(_timeDiff);

        ChangeLightColor();
    }




    /* private void Update()
    {
        if (Time.time <= _nextTimeToChange) 
            return; 

        _nextTimeToChange = Time.time + _timeDiff;
        ChangeLightColor();

    } */

    private void ChangeLightColor()
    {
        directionalLight.color = UnityEngine.Random.ColorHSV();
    }
}
