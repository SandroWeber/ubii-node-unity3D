using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using Google.Protobuf.Collections;
using Ubii.Processing;
using Ubii.TopicData;

[Serializable]
public class ProcessingModule : IProcessingModule
{
    private Ubii.Processing.ProcessingModule specs;
    public Ubii.Processing.ProcessingModule Specs { get { return specs; } }

    public Dictionary<string, TopicDataRecord> processingInputRecords;
    public Dictionary<string, TopicDataRecord> processingOutputRecords;

    public Dictionary<string, Func<TopicDataRecord>> dictInputGetters = new Dictionary<string, Func<TopicDataRecord>>();
    public Dictionary<string, Action<TopicDataRecord>> dictOutputSetters = new Dictionary<string, Action<TopicDataRecord>>();

    private CancellationTokenSource cts = null;

    // proto specification accessors
    public string Id { get { return this.specs.Id; } }
    public string Name { get { return this.specs.Name; } }
    public string SessionId { get { return this.specs.SessionId; } }
    public Ubii.Processing.ProcessingModule.Types.Language Language { get { return this.specs.Language; } }
    public ProcessingMode ProcessingMode { get { return this.specs.ProcessingMode; } }
    public Ubii.Processing.ProcessingModule.Types.Status Status { get { return this.specs.Status; } }
    public RepeatedField<Ubii.Processing.ModuleIO> Inputs { get { return this.specs.Inputs; } }
    public RepeatedField<Ubii.Processing.ModuleIO> Outputs { get { return this.specs.Outputs; } }

    public ProcessingModule(Ubii.Processing.ProcessingModule specs = null)
    {
        // TODO: !!For that to work the whole class must be serializable, is it?
        /*if (specs != null)
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(specs), this);*/

        // Auto-generated, remove if code above works
        this.specs = specs;

        this.specs.Id = this.specs.Id == null ? Guid.NewGuid().ToString() : this.specs.Id;
        this.specs.Language = Ubii.Processing.ProcessingModule.Types.Language.Cs;

        if (this.specs.ProcessingMode == null)
        {
            this.specs.ProcessingMode = new ProcessingMode
            {
                Frequency = new ProcessingMode.Types.Frequency { Hertz = 30 }
            };
        }

        this.processingInputRecords = new Dictionary<string, TopicDataRecord>();
        this.processingOutputRecords = new Dictionary<string, TopicDataRecord>();

        this.specs.Status = Ubii.Processing.ProcessingModule.Types.Status.Created;
    }

    /// <summary>
    /// Start processing by pm's processing mode
    /// </summary>
    public void Start()
    {
        if (this.specs.ProcessingMode.ModeCase == ProcessingMode.ModeOneofCase.Frequency) // Oder: processingMode.Frequency != null
        {
            StartProcessingByFrequency();
        }
        // TODO: Add other processingModes later

        if (this.specs.Status == Ubii.Processing.ProcessingModule.Types.Status.Processing)
            Debug.Log(this.ToString() + " started");
    }

    /// <summary>
    /// Stop this pm, sets status to halted and removes all listeners
    /// </summary>
    public void Stop()
    {
        if (this.specs.Status == Ubii.Processing.ProcessingModule.Types.Status.Halted)
            return;

        this.specs.Status = Ubii.Processing.ProcessingModule.Types.Status.Halted;
        OnHalted();
        RemoveAllListeners(PMEvents.NEW_INPUT);
        // this.OnProcessingLockstepPass = () => { ..} TODO ? -> later
        Debug.Log(this.ToString() + " stopped");
    }

    /// <summary>
    /// Start processing in frequency mode
    /// </summary>
    private void StartProcessingByFrequency()
    {
        this.specs.Status = Ubii.Processing.ProcessingModule.Types.Status.Processing;

        DateTime tLastProcess = DateTime.Now;
        int msFrequency = 1000 / this.specs.ProcessingMode.Frequency.Hertz;

        // TODO: Token must be used to cancel task before shutting this down -> cts.Cancel()
        this.cts = new CancellationTokenSource(); // global?
        CancellationToken token = cts.Token;

        Task processIteration = Task.Run(() =>
        {
            while (this.specs.Status == Ubii.Processing.ProcessingModule.Types.Status.Processing)
            {
                DateTime tNow = DateTime.Now;
                TimeSpan deltaTime = tNow.Subtract(tLastProcess);
                tLastProcess = tNow;

                //processing
                processingInputRecords.Clear();

                try
                {
                    foreach (ModuleIO input in specs.Inputs)
                    {
                        processingInputRecords.Add(input.InternalName, dictInputGetters[input.InternalName].Invoke());
                    }
                    processingOutputRecords = OnProcessing(deltaTime, processingInputRecords);

                    foreach (var entry in processingOutputRecords)
                    {
                        string outputName = entry.Key;
                        TopicDataRecord record = entry.Value;
                        if (dictOutputSetters.ContainsKey(outputName))
                            dictOutputSetters[outputName].Invoke(record);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                if (this.specs.Status == Ubii.Processing.ProcessingModule.Types.Status.Processing)
                {
                    Thread.Sleep(msFrequency);
                }
                else
                {
                    this.cts.Cancel();
                }
            }
        }, token);
    }

    #region lifecycle

    public virtual void OnCreated() { }

    /// <summary>
    /// Takes inputs, callbacks generate input dictionary
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <param name="inputs"></param>
    /// <returns></returns>
    public virtual Dictionary<string, TopicDataRecord> OnProcessing(TimeSpan deltaTime, Dictionary<string, TopicDataRecord> inputs)
    {
        throw new NotImplementedException();
    }
    public virtual void OnHalted() { }

    public virtual void OnDestroyed() { }

    #endregion

    private void RemoveAllListeners(PMEvents pmEvent)
    {
        //throw new NotImplementedException();
    }

    public void Emit(PMEvents pmEvent, string inputName)
    {
        //throw new NotImplementedException();
    }

    public void SetInputGetter(string inputName, Func<TopicDataRecord> getter)
    {
        //Debug.Log("SetInputGetter() - " + this.Name + "->" + inputName);
        if (this.dictInputGetters.ContainsKey(inputName))
        {
            this.dictInputGetters[inputName] = getter;
        }
        else
        {
            this.dictInputGetters.Add(inputName, getter);
        }
    }

    public void SetOutputSetter(string outputName, Action<TopicDataRecord> setter)
    {
        if (this.dictOutputSetters.ContainsKey(outputName))
        {
            this.dictOutputSetters[outputName] = setter;
        }
        else
        {
            this.dictOutputSetters.Add(outputName, setter);
        }
    }

    /*public TopicDataRecord GetInput(string name)
    {
        throw new NotImplementedException();
    }

    public void SetOutput(string name, TopicDataRecord output)
    {
        throw new NotImplementedException();
    }*/

    public bool CheckInternalName(string internalName)
    {
        throw new NotImplementedException();
    }

    #region utility

    public string GetIOMessageFormat(string ioName)
    {
        foreach (Ubii.Processing.ModuleIO input in this.specs.Inputs)
        {
            if (input.InternalName == ioName) return input.MessageFormat;
        }
        foreach (Ubii.Processing.ModuleIO output in this.specs.Outputs)
        {
            if (output.InternalName == ioName) return output.MessageFormat;
        }

        return null;
    }

    public Ubii.Processing.ProcessingModule ToProtobuf()
    {
        return this.specs;
    }

    override public string ToString()
    {
        return "ProcessingModule '" + this.Name + "' (ID " + this.Id + ")";
    }

    #endregion
}
