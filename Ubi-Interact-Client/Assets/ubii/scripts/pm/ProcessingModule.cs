﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubii.Processing;
using Ubii.TopicData;
using System.Threading;
using System.Threading.Tasks;

[Serializable]
public class ProcessingModule : IProcessingModule // add the generic stuff for ioProxy, OnProcessing input parameter types
{
    public string nodeID;
    public string sessionID;
    public string name;
    public string id;
    private Ubii.Processing.ProcessingModule specs;
    private Ubii.Processing.ProcessingModule.Types.Language language;

    public ProcessingMode processingMode;
    public Ubii.Processing.ProcessingModule.Types.Status status;

    public Dictionary<string, Func<object, object>> ioProxy;

    public ProcessingModule(Ubii.Processing.ProcessingModule specs, string id = null)
    {
        // TODO: !!For that to work the whole class must be serializable, is it?
        if (specs != null)
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(specs), this);

        // Auto-generated, remove if code above works
        this.specs = specs;

        this.id = id == null ? Guid.NewGuid().ToString() : id;
        this.language = Ubii.Processing.ProcessingModule.Types.Language.Cs;

        processingMode = new ProcessingMode
        {
            Frequency = new ProcessingMode.Types.Frequency { Hertz = 30 }
        };
        status = Ubii.Processing.ProcessingModule.Types.Status.Created;
        ioProxy = new Dictionary<string, Func<object, object>>();
    }

    public Ubii.Processing.ProcessingModule ToProtobuf()
    {
        throw new NotImplementedException();
    }

    public void Start()
    {
        if (processingMode.ModeCase == ProcessingMode.ModeOneofCase.Frequency) // Oder: processingMode.Frequency != null
        {
            StartProcessingByFrequency();
        }
        // TODO: Add other processingModes later

        if (status == Ubii.Processing.ProcessingModule.Types.Status.Processing)
            Debug.Log(this.ToString() + " started");
    }


    public void Stop()
    {
        if (status == Ubii.Processing.ProcessingModule.Types.Status.Halted)
            return;

        OnHalted();
        status = Ubii.Processing.ProcessingModule.Types.Status.Halted;
        RemoveAllListeners(PMEvents.NEW_INPUT);
        // this.OnProcessingLockstepPass = () => { ..} TODO ? -> later
        Debug.Log(this.ToString() + " stopped");
    }

    private void StartProcessingByFrequency()
    {
        status = Ubii.Processing.ProcessingModule.Types.Status.Processing;

        DateTime tLastProcess = DateTime.Now;
        int msFrequency = 1000 / processingMode.Frequency.Hertz;


        // TODO: Token must be used to cancel task before shutting this down -> cts.Cancel()
        CancellationTokenSource cts = new CancellationTokenSource(); // global?
        CancellationToken token = cts.Token;

        Task processIteration = Task.Run(() =>
        {
            while (true)
            {

                DateTime tNow = DateTime.Now;
                TimeSpan deltaTime = tNow.Subtract(tLastProcess);
                tLastProcess = tNow;
                //processing
                OnProcessing(deltaTime, ioProxy, ioProxy);
                if (status == Ubii.Processing.ProcessingModule.Types.Status.Processing)
                {
                    Thread.Sleep(msFrequency);
                }
                else if( status == Ubii.Processing.ProcessingModule.Types.Status.Destroyed)
                    cts.Cancel();
            }
        }, token);
    }

    private void SetTimeout(Action p, float msFrequency)
    {
        throw new NotImplementedException();
    }

    public void OnProcessing(TimeSpan deltaTime, Dictionary<string, Func<object,object>> inputs, Dictionary<string, Func<object, object>> outputs)
    {
        throw new NotImplementedException();
    }

    private void RemoveAllListeners(PMEvents nEW_INPUT)
    {
        throw new NotImplementedException();
    }

    public void OnHalted()
    {
        throw new NotImplementedException();
    }

    public void SetInputGetter(string inputName, Func<TopicDataRecord> p)
    {
        throw new NotImplementedException();
    }

    public void Emit(PMEvents nEW_INPUT, string inputName)
    {
        throw new NotImplementedException();
    }

    public string GetIOMessageFormat(string outputName)
    {
        throw new NotImplementedException();
    }

    public void OnCreated(Ubii.Processing.ProcessingModule.Types.Status status)
    {
        throw new NotImplementedException();
    }

    public void OnProcessing()
    {
        throw new NotImplementedException();
    }

    public void OnDestroyed()
    {
        throw new NotImplementedException();
    }

    public void SetInputGetter()
    {
        throw new NotImplementedException();
    }

    public void SetOutputGetter()
    {
        throw new NotImplementedException();
    }

    public Action ReadInput(string name)
    {
        throw new NotImplementedException();
    }

    public void WriteOutput(string name, string value)
    {
        throw new NotImplementedException();
    }

    public bool CheckInternalName(string internalName)
    {
        throw new NotImplementedException();
    }

    internal void SetOutputSetter(string outputName, object record)
    {
        throw new NotImplementedException();
    }
}
