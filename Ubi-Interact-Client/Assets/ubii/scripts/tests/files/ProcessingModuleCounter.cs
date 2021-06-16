
public class ProcessingModuleCounter : ProcessingModule
{
    public Ubii.Processing.ProcessingModule specs = new Ubii.Processing.ProcessingModule {
        Name = "ProcessingModuleCounter",
        Description = "Test Module counting up with set frequency",
        ProcessingMode = new Ubii.Processing.ProcessingMode {
            Frequency = new Ubii.Processing.ProcessingMode.Types.Frequency { Hertz = 1 }
        }
    };
    private int ticker = 0;
    private string name = "ProcessingModuleCounter";

    public ProcessingModuleCounter()
    {
    }

    void OnCreated()
    {

    }

    void OnProcessing()
    {

    }
}