using System.Collections.Generic;
using Ubii.TopicData;
using UnityEngine;

public struct TestProcessingModuleCounterDatabaseEntry : IProcessingModuleDatabaseEntry
{
    public Ubii.Processing.ProcessingModule GetSpecifications()
    {
        return TestProcessingModuleCounter.specs;
    }

    public ProcessingModule CreateInstance()
    {
        return new TestProcessingModuleCounter();
    }
}

public class TestProcessingModuleCounter : ProcessingModule
{
    public static Ubii.Processing.ProcessingModule specs = new Ubii.Processing.ProcessingModule {
        Name = "ProcessingModuleCounter",
        Description = "Test Module counting up with set frequency",
        ProcessingMode = new Ubii.Processing.ProcessingMode {
            Frequency = new Ubii.Processing.ProcessingMode.Types.Frequency { Hertz = 1 }
        },
        Language = Ubii.Processing.ProcessingModule.Types.Language.Cs,
        Outputs = { new Ubii.Processing.ModuleIO { InternalName = "outCounter", MessageFormat = "int32" } }
    };

    // internal state
    private int count;

    void OnCreated()
    {
        count = 0;
        Debug.Log("TestProcessingModuleCounter.OnCreated() - count=" + count);
    }

    Dictionary<string, TopicDataRecord> OnProcessing()
    {
        count++;
        Debug.Log("TestProcessingModuleCounter.OnProcessing() - count=" + count);
        return new Dictionary<string, TopicDataRecord>();
    }
}