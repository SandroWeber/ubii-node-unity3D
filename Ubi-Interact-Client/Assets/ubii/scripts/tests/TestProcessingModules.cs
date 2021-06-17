using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestProcessingModules : MonoBehaviour
{
    private UbiiNode ubiiNode = null;

    private ProcessingModuleCounter pmFrequencyCounter = new ProcessingModuleCounter();

    private Ubii.Sessions.Session ubiiSession;

    private string topicFrequencyCounter;

    // Start is called before the first frame update
    void Start()
    {
        ubiiNode = FindObjectOfType<UbiiNode>();
        ubiiNode.processingModuleDatabase.AddModule(pmFrequencyCounter.specs);

        UbiiNode.OnInitialized += OnUbiiInitialized;
        ubiiNode.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnUbiiInitialized()
    {
        topicFrequencyCounter = "/" + ubiiNode.GetID() + "/test/pm_frequency_counter";
        
        ubiiSession = new Ubii.Sessions.Session { Name = "Test Processing Modules Counter" };

        ubiiSession.ProcessingModules.Add(new Ubii.Processing.ProcessingModule {
            Name = pmFrequencyCounter.specs.Name
        });
        
        Ubii.Sessions.IOMapping ioMapping = new Ubii.Sessions.IOMapping();
        ioMapping.ProcessingModuleName = pmFrequencyCounter.specs.Name;
        ioMapping.OutputMappings.Add(new Ubii.Sessions.TopicOutputMapping {
            OutputName = "outCounter",
            Topic = topicFrequencyCounter
        });
        ubiiSession.IoMappings.Add(ioMapping);

        RunTest();
    }

    public async void RunTest()
    {
        Ubii.Services.ServiceReply reply = await ubiiNode.CallService(new Ubii.Services.ServiceRequest {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SESSION_RUNTIME_START,
            Session = ubiiSession
        });
        Debug.Log(reply);
    }
}
