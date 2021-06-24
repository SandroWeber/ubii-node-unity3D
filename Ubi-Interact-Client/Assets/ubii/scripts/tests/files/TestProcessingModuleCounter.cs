using System;
using System.Collections.Generic;
using UnityEngine;

public struct TestProcessingModuleCounterDatabaseEntry : IProcessingModuleDatabaseEntry
{
    public Ubii.Processing.ProcessingModule GetSpecifications()
    {
        return TestProcessingModuleCounter.specs;
    }

    public ProcessingModule CreateInstance(Ubii.Processing.ProcessingModule specs)
    {
        return new TestProcessingModuleCounter(specs);
    }
}

public class TestProcessingModuleCounter : ProcessingModule
{
    public static Ubii.Processing.ProcessingModule specs = new Ubii.Processing.ProcessingModule
    {
        Name = "ProcessingModuleFrequencyCounter",
        Description = "Test Module counting up with set frequency",
        ProcessingMode = new Ubii.Processing.ProcessingMode
        {
            Frequency = new Ubii.Processing.ProcessingMode.Types.Frequency { Hertz = 1 }
        },
        Language = Ubii.Processing.ProcessingModule.Types.Language.Cs,
        Outputs = { new Ubii.Processing.ModuleIO { InternalName = "outCounter", MessageFormat = "int32" } }
    };

    // internal state
    private int count = 0;

    public TestProcessingModuleCounter(Ubii.Processing.ProcessingModule specs) : base(specs)
    {
        this.Specs.ProcessingMode = this.Specs.ProcessingMode != null ? this.Specs.ProcessingMode : TestProcessingModuleCounter.specs.ProcessingMode;
    }

    public override void OnCreated()
    {
        this.count = 0;
        Debug.Log("TestProcessingModuleCounter.OnCreated() - count=" + count);
    }

    public override Dictionary<string, Ubii.TopicData.TopicDataRecord> OnProcessing(TimeSpan deltaTime, Dictionary<string, Ubii.TopicData.TopicDataRecord> inputs)
    {
        this.count++;
        Debug.Log("TestProcessingModuleCounter.OnProcessing() - count=" + count);

        return new Dictionary<string, Ubii.TopicData.TopicDataRecord>() {
            {"outCounter", new Ubii.TopicData.TopicDataRecord { Int32 = count }}
        };
    }
}