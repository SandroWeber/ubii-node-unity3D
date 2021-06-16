using System;
using System.Collections.Generic;
using Ubii.TopicData;

interface IProcessingModule
{
    void Start();
    void Stop();

    void OnCreated(Ubii.Processing.ProcessingModule.Types.Status status);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <param name="inputs"></param>
    /// <returns>Dictionary with outputs created during processing</returns>
    Dictionary<string, TopicDataRecord> OnProcessing(TimeSpan deltaTime, Dictionary<string, TopicDataRecord> inputs);
    void OnHalted();
    void OnDestroyed();

    void SetInputGetter();
    void SetOutputGetter();

    Action ReadInput(string name);
    void WriteOutput(string name, string value);

    bool CheckInternalName(string internalName);
    string GetIOMessageFormat(string outputName);
    string ToString();


}

public enum PMEvents
{
    NEW_INPUT = 1,
    LOCKSTEP_PASS = 2,
    PROCESSED = 3
}