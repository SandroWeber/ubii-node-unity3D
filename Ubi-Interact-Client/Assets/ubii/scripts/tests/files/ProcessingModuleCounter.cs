using System.Collections.Generic;
using Ubii.TopicData;

public class ProcessingModuleCounter : ProcessingModule
{
    public Ubii.Processing.ProcessingModule specs = new Ubii.Processing.ProcessingModule {
        Name = "ProcessingModuleCounter",
        Description = "Test Module counting up with set frequency",
        ProcessingMode = new Ubii.Processing.ProcessingMode {
            Frequency = new Ubii.Processing.ProcessingMode.Types.Frequency { Hertz = 1 }
        },
        Language = Ubii.Processing.ProcessingModule.Types.Language.Cs
    };
    private int count;
    private string name = "ProcessingModuleCounter";

    public ProcessingModuleCounter()
    {
        specs.Outputs.Add(new Ubii.Processing.ModuleIO { InternalName = "outCounter", MessageFormat = "int32" });
    }

    void OnCreated()
    {
        count = 0;
    }
    Dictionary<string, TopicDataRecord> OnProcessing()
    {
        return new Dictionary<string, TopicDataRecord>();
    }
}