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
    /*
    string id = 1;
    string name = 2;
    string processing_callback = 3;
    repeated ubii.interactions.IOFormat input_formats = 4;
    repeated ubii.interactions.IOFormat output_formats = 5;
    string on_created = 6;
    float process_frequency = 7;
    repeated string authors = 8;
    repeated string tags = 9;
    string description = 10;
    InteractionStatus status = 11;
*/
    private Ubii.Sessions.Session sessionSpecs = null;
    /*
    string id = 1;
    string name = 2;
    repeated ubii.interactions.Interaction interactions = 3;
    repeated ubii.sessions.IOMapping io_mappings = 4;
    repeated string tags = 5;
    string description = 6;
    repeated string authors = 7;
    ProcessMode process_mode = 8;
    SessionStatus status = 9;
     */

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
            Debug.Log("RunTestStartStopSession - session start ok");
            Debug.Log(replyStart.Session.ToString());

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
