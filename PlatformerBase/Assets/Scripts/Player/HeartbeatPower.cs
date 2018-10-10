﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Heartbeat))]
public class HeartbeatPower : Global {

    [SerializeField] private float initialBPM; //initial value for the BPM
    [SerializeField] private float maxHeartRate; //maximum value for bpm
    [SerializeField] private float deltaHeartRate; //rate of change when bpm is being updated 

    private float targetBPM; //value bpm is animating towards

    private Heartbeat heartbeat;

    void Awake()
    {
        heartbeat = GetComponent<Heartbeat>();
        heartbeat.BPM = initialBPM;
        targetBPM = initialBPM;
    }

    /// <summary>
    /// increase target BPM
    /// </summary>
    /// <param name="restore">amount to add</param>
    public void RestoreBPM(float restore)
    {
        targetBPM = targetBPM + restore;
        targetBPM = Mathf.Clamp(targetBPM, 0, maxHeartRate);
    }

    /// <summary>
    /// decrease target BPM
    /// </summary>
    /// <param name="remove">amount to take away</param>
    public void RemoveBPM(float remove)
    {
        targetBPM = targetBPM - remove;
        targetBPM = Mathf.Clamp(targetBPM, 0, maxHeartRate);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RemoveBPM(20);
        }

        if (!paused && targetBPM != heartbeat.BPM)
        {
            float difference = targetBPM - heartbeat.BPM;
            if (difference * Mathf.Sign(difference) < deltaHeartRate * Time.deltaTime)
            {
                heartbeat.BPM = targetBPM;
            }
            else
            {
                heartbeat.BPM += Mathf.Sign(difference) * deltaHeartRate * Time.deltaTime;
            }
        }
    }
}