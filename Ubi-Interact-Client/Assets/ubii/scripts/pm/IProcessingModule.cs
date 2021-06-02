using System;
using System.Collections.Generic;

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
    Dictionary<string, Func<object, object>> OnProcessing(TimeSpan deltaTime, Dictionary<string, Func<object, object>> inputs);
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