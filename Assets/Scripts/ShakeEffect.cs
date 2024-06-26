using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShakeEffect : NetworkBehaviour
{
    [SerializeField] private float amplitudeGain = 1f;
    [SerializeField] private float frequencyGain = 1f;
    [SerializeField] private float shakeTimer = 1f;

    [SerializeField] private CinemachineVirtualCamera vcam;

    private CinemachineBasicMultiChannelPerlin multiChannelPerlin;

    private Coroutine shakeCoroutine;

    private float currentShakeTimer = 0f;

    public override void OnNetworkDespawn()
    {

    }

    public override void OnNetworkSpawn()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        multiChannelPerlin = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void StartShake()
    {
        if(shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        shakeCoroutine = StartCoroutine(shaker());
    }
    private IEnumerator shaker()
    {
        currentShakeTimer = 0;
        multiChannelPerlin.m_AmplitudeGain = amplitudeGain;
        multiChannelPerlin.m_FrequencyGain = frequencyGain;

        while(currentShakeTimer < shakeTimer) 
        {
            currentShakeTimer += Time.deltaTime;
            yield return null;
        }

        multiChannelPerlin.m_AmplitudeGain = 0;
        multiChannelPerlin.m_FrequencyGain = 0;

        shakeCoroutine = null;
        yield return null;
    }
}
