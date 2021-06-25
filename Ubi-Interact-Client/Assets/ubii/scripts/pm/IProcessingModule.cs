using System;
using System.Collections.Generic;
using Ubii.TopicData;

interface IProcessingModule
{
    void Start();
    void Stop();

    void OnCreated();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <param name="inputs"></param>
    /// <returns>Dictionary with outputs created during processing</returns>
    Dictionary<string, TopicDataRecord> OnProcessing(TimeSpan deltaTime, Dictionary<string, TopicDataRecord> inputs);
    void OnHalted();
    void OnDestroyed();

    void SetInputGetter(string inputName, Func<TopicDataRecord> getter);
    void SetOutputSetter(string outputName, Action<TopicDataRecord> setter);

    /*TopicDataRecord GetInput(string name);
    void SetOutput(string name, TopicDataRecord output);*/
    string GetIOMessageFormat(string ioName);
    string ToString();


}

public enum PMEvents
{
    NEW_INPUT = 1,
    LOCKSTEP_PASS = 2,
    PROCESSED = 3
}