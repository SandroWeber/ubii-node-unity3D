using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public class TestSessions : MonoBehaviour
{
    private UbiiClient ubiiClient = null;
    private UbiiConstants ubiiConstants = null;

    private string inputTopic, outputTopic;

    private Ubii.Interactions.Interaction interactionSpecs = null;
    private Ubii.Sessions.Session sessionSpecs = null;

    void Start()
    {
        ubiiClient = FindObjectOfType<UbiiClient>();
        ubiiConstants = UbiiConstants.Instance;

        RunTests();
    }

    void Update()
    {

    }

    async private void RunTests()
    {
        await ubiiClient.WaitForConnection();

        this.CreateSpecs();

        await Task.Delay(1000);
        RunTestStartStopSession();
    }

    private void CreateSpecs()
    {
        this.inputTopic = "/" + ubiiClient.GetID() + "/test-sessions/input/vec3";
        this.outputTopic = "/" + ubiiClient.GetID() + "/test-sessions/output/vec3";

        this.interactionSpecs = new Ubii.Interactions.Interaction
        {
            Id = "Unity3D-Client-TestSessions-Interaction-01",
            Name = "Unity3D-Client-TestSessions-Interaction-01",
            ProcessingCallback = "(inputs, outputs, state) => { outputs.outVec3 = inputs.inVec3; }",
            ProcessFrequency = 10
        };
        this.interactionSpecs.InputFormats.Add(new Ubii.Interactions.IOFormat { InternalName = "inVec3", MessageFormat = "ubii.dataStructure.Vector3" });
        this.interactionSpecs.OutputFormats.Add(new Ubii.Interactions.IOFormat { InternalName = "outVec3", MessageFormat = "ubii.dataStructure.Vector3" });
        this.interactionSpecs.Authors.Add("Sandro Weber");

        this.sessionSpecs = new Ubii.Sessions.Session
        {
            Name = "Unity3D-Client-TestSessions-Session-01",
            ProcessMode = Ubii.Sessions.ProcessMode.IndividualProcessFrequencies
        };
        this.sessionSpecs.Authors.Add("Sandro Weber");
        this.sessionSpecs.Interactions.Add(this.interactionSpecs);
        Ubii.Sessions.IOMapping ioMapping = new Ubii.Sessions.IOMapping
        {
            InteractionId = this.interactionSpecs.Id
        };
        ioMapping.InputMappings.Add(new Ubii.Sessions.InteractionInputMapping { Name = "inVec3", Topic = this.inputTopic });
        ioMapping.OutputMappings.Add(new Ubii.Sessions.InteractionOutputMapping { Name = "outVec3", Topic = this.outputTopic });
        this.sessionSpecs.IoMappings.Add(ioMapping);
    }

    async private void RunTestStartStopSession()
    {
        bool success = false;

        Ubii.Services.ServiceReply replyStart = await ubiiClient.CallService(
            new Ubii.Services.ServiceRequest { Topic = ubiiConstants.DEFAULT_TOPICS.SERVICES.SESSION_START, Session = this.sessionSpecs }
        );

        if (replyStart.Session != null)
        {
            this.sessionSpecs = replyStart.Session;

            await Task.Delay(1000);

            Ubii.Services.ServiceReply replyStop = await ubiiClient.CallService(
                new Ubii.Services.ServiceRequest { Topic = ubiiConstants.DEFAULT_TOPICS.SERVICES.SESSION_STOP, Session = this.sessionSpecs }
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
