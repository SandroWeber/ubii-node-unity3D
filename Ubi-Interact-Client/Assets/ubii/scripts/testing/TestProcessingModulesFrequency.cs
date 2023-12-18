using System.Threading.Tasks;
using UnityEngine;
using Ubii.TopicData;

public class TestProcessingModulesFrequency : MonoBehaviour
{
    private UbiiNode ubiiNode = null;

    private Ubii.Sessions.Session ubiiSession;

    private string topicFrequencyCounter, topicFrequencyCounterTickValue;

    private ProcessingModule pm = null;

    private int expectedCounter, tickValue;

    private bool testFailure = false;

    // Start is called before the first frame update
    void Start()
    {
        ubiiNode = FindObjectOfType<UbiiNode>();
        ubiiNode.processingModuleDatabase.AddEntry(new TestPMFrequencyCounterDBEntry());

        UbiiNode.OnInitialized += OnUbiiInitialized;
        ubiiNode.Initialize();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnDisable()
    {
        ubiiNode.ProcessingModuleManager.StopSessionModules(ubiiSession);
    }

    public async void OnUbiiInitialized()
    {
        topicFrequencyCounter = "/" + ubiiNode.Id + "/test/pm_frequency_counter";
        topicFrequencyCounterTickValue = "/" + ubiiNode.Id + "/test/pm_frequency_counter/tick_value";

        expectedCounter = 0;
        tickValue = 2;
        ubiiNode.PublishImmediately(new TopicDataRecord
        {
            Topic = topicFrequencyCounterTickValue,
            Int32 = tickValue
        });
        //await Task.Delay(2000);

        testFailure = false;
        await ubiiNode.SubscribeTopic(topicFrequencyCounter, (TopicDataRecord record) =>
        {
            expectedCounter += tickValue;
            testFailure = record.Int32 != expectedCounter;
            if (testFailure) Debug.LogError("counter from PM expected to be " + expectedCounter + " but was actually " + record.Int32);
        });

        ubiiSession = new Ubii.Sessions.Session { Name = "Test Processing Modules Counter" };

        ubiiSession.ProcessingModules.Add(new Ubii.Processing.ProcessingModule
        {
            Name = TestPMFrequencyCounter.specs.Name
        });

        Ubii.Sessions.IOMapping ioMapping = new Ubii.Sessions.IOMapping();
        ioMapping.ProcessingModuleName = TestPMFrequencyCounter.specs.Name;
        ioMapping.InputMappings.Add(new Ubii.Sessions.TopicInputMapping
        {
            InputName = "counterTick",
            Topic = topicFrequencyCounterTickValue
        });
        ioMapping.OutputMappings.Add(new Ubii.Sessions.TopicOutputMapping
        {
            OutputName = "outCounter",
            Topic = topicFrequencyCounter
        });
        ubiiSession.IoMappings.Add(ioMapping);

        RunTest();
    }

    public async void RunTest()
    {
        Debug.Log("TestProcessingModulesFrequency started ...");
        
        Ubii.Services.ServiceReply reply = await ubiiNode.CallService(new Ubii.Services.ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SESSION_RUNTIME_START,
            Session = ubiiSession
        });
        //Debug.Log("TestProcessingModules.RunTest() - reply to start session: " + reply);
        if (reply.Session != null)
        {
            ubiiSession = reply.Session;
        }

        await Task.Delay(5000);

        reply = await ubiiNode.CallService(new Ubii.Services.ServiceRequest
        {
            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.SESSION_RUNTIME_STOP,
            Session = ubiiSession
        });
        //Debug.Log("TestProcessingModules.RunTest() - reply to stop session: " + reply);

        if (testFailure)
        {
            Debug.LogError("TestProcessingModulesFrequency FAILURE");
        }
        else
        {
            Debug.Log("TestProcessingModulesFrequency SUCCESS");
        }

    }
}