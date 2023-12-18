using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public class TestSessions : MonoBehaviour
{
    private UbiiNode ubiiNode = null;
    private UbiiConstants ubiiConstants = null;

    private string inputTopic, outputTopic;

    private Ubii.Processing.ProcessingModule pmSpecs = null;
    private Ubii.Sessions.Session sessionSpecs = null;

    void Start()
    {
        ubiiNode = FindObjectOfType<UbiiNode>();
        ubiiConstants = UbiiConstants.Instance;

        RunTests();
    }

    void Update()
    {

    }

    async private void RunTests()
    {
        await ubiiNode.WaitForConnection();

        this.CreateSpecs();

        await Task.Delay(1000);
        RunTestStartStopSession();
    }

    private void CreateSpecs()
    {
        this.inputTopic = "/" + ubiiNode.Id + "/test-sessions/input/vec3";
        this.outputTopic = "/" + ubiiNode.Id + "/test-sessions/output/vec3";

        this.pmSpecs = new Ubii.Processing.ProcessingModule
        {
            Name = "Unity3D-Client-TestSessions-PM-01",
            OnProcessingStringified = "(inputs, outputs, state) => { outputs.outVec3 = inputs.inVec3; }",
            ProcessingMode = new Ubii.Processing.ProcessingMode { Frequency = new Ubii.Processing.ProcessingMode.Types.Frequency { Hertz = 10 } }
        };
        this.pmSpecs.Inputs.Add(new Ubii.Processing.ModuleIO { InternalName = "inVec3", MessageFormat = "ubii.dataStructure.Vector3" });
        this.pmSpecs.Outputs.Add(new Ubii.Processing.ModuleIO { InternalName = "outVec3", MessageFormat = "ubii.dataStructure.Vector3" });
        this.pmSpecs.Authors.Add("Sandro Weber");

        this.sessionSpecs = new Ubii.Sessions.Session
        {
            Name = "Unity3D-Client-TestSessions-Session-01"
        };
        this.sessionSpecs.Authors.Add("Sandro Weber");
        this.sessionSpecs.ProcessingModules.Add(this.pmSpecs);
        Ubii.Sessions.IOMapping ioMapping = new Ubii.Sessions.IOMapping
        {
            ProcessingModuleName = this.pmSpecs.Name
        };
        ioMapping.InputMappings.Add(new Ubii.Sessions.TopicInputMapping { InputName = "inVec3", Topic = this.inputTopic });
        ioMapping.OutputMappings.Add(new Ubii.Sessions.TopicOutputMapping { OutputName = "outVec3", Topic = this.outputTopic });
        this.sessionSpecs.IoMappings.Add(ioMapping);
    }

    async private void RunTestStartStopSession()
    {
        bool success = false;

        Ubii.Services.ServiceReply replyStart = await ubiiNode.CallService(
            new Ubii.Services.ServiceRequest { Topic = ubiiConstants.DEFAULT_TOPICS.SERVICES.SESSION_RUNTIME_START, Session = this.sessionSpecs }
        );

        if (replyStart.Session != null)
        {
            this.sessionSpecs = replyStart.Session;

            await Task.Delay(1000);

            Ubii.Services.ServiceReply replyStop = await ubiiNode.CallService(
                new Ubii.Services.ServiceRequest { Topic = ubiiConstants.DEFAULT_TOPICS.SERVICES.SESSION_RUNTIME_STOP, Session = this.sessionSpecs }
            );

            if (replyStop.Success != null)
            {
                Debug.Log("RunTestStartStopSession SUCCESS!");
            }
            else
            {
                Debug.LogError("RunTestStartStopSession FAILURE! Could not stop session.");
            }
        }
        else
        {
            Debug.LogError("RunTestStartStopSession FAILURE! Could not start session.");
        }
    }
}
