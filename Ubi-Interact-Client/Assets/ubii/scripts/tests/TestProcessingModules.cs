using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TestProcessingModules : MonoBehaviour
{
    private UbiiNode ubiiNode = null;

    private TestProcessingModuleCounter pmFrequencyCounter = new TestProcessingModuleCounter();

    private Ubii.Sessions.Session ubiiSession;

    private string topicFrequencyCounter;

    // Start is called before the first frame update
    void Start()
    {
        ubiiNode = FindObjectOfType<UbiiNode>();
        ubiiNode.processingModuleDatabase.AddEntry(new TestProcessingModuleCounterDatabaseEntry());

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
            Name = TestProcessingModuleCounter.specs.Name
        });
        
        Ubii.Sessions.IOMapping ioMapping = new Ubii.Sessions.IOMapping();
        ioMapping.ProcessingModuleName = TestProcessingModuleCounter.specs.Name;
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
        Debug.Log("test PMs reply to start session: " + reply);
        if (reply.Session != null)
        {
            ubiiSession = reply.Session;
        }
        
        await Task.Delay(5000);
        
        reply = await ubiiNode.CallService(new Ubii.Services.ServiceRequest {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SESSION_RUNTIME_STOP,
            Session = ubiiSession
        });
        Debug.Log("test PMs reply to stop session: " + reply);
    }
}
