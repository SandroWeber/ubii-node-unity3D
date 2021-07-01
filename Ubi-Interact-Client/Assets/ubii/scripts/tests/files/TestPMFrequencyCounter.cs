using System;
using System.Collections.Generic;
using UnityEngine;

public struct TestPMFrequencyCounterDBEntry : IProcessingModuleDatabaseEntry
{
    public Ubii.Processing.ProcessingModule GetSpecifications()
    {
        return TestPMFrequencyCounter.specs;
    }

    public ProcessingModule CreateInstance(Ubii.Processing.ProcessingModule specs)
    {
        return new TestPMFrequencyCounter(specs);
    }
}

public class TestPMFrequencyCounter : ProcessingModule
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
        Inputs = { new Ubii.Processing.ModuleIO { InternalName = "counterTick", MessageFormat = "int32" } },
        Outputs = { new Ubii.Processing.ModuleIO { InternalName = "outCounter", MessageFormat = "int32" } }
    };

    // internal state
    private int counter;

    public TestPMFrequencyCounter(Ubii.Processing.ProcessingModule specs) : base(specs)
    {
        this.Specs.ProcessingMode = this.Specs.ProcessingMode != null ? this.Specs.ProcessingMode : TestPMFrequencyCounter.specs.ProcessingMode;
    }

    public override void OnCreated()
    {
        counter = 0;
    }

    public override Dictionary<string, Ubii.TopicData.TopicDataRecord> OnProcessing(TimeSpan deltaTime, Dictionary<string, Ubii.TopicData.TopicDataRecord> inputs)
    {
        if (inputs["counterTick"] != null)
        {
            this.counter += inputs["counterTick"].Int32;
        }
        else
        {
            this.counter++;
        }

        return new Dictionary<string, Ubii.TopicData.TopicDataRecord>() {
            {"outCounter", new Ubii.TopicData.TopicDataRecord { Int32 = counter }}
        };
    }
}