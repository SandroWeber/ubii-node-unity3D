using System.Collections;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

//This script will pulish some test data every time the Cube is grabbed or released
public class HololensTest : MonoBehaviour
{
    [SerializeField]
    private UbiiNode ubiiNode;

    [SerializeField]
    private ObjectManipulator objectManipulator;
    // Start is called before the first frame update
    void Start()
    {
        objectManipulator.OnManipulationStarted.AddListener(OnManipulationStarted);
        objectManipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
    }

    private void OnManipulationStarted(ManipulationEventData arg0)
    {
        StartCoroutine(PublishTestData("started"));
    }

    private void OnManipulationEnded(ManipulationEventData arg0)
    {
        StartCoroutine(PublishTestData("ended"));
    }

    private IEnumerator PublishTestData(string eventType){
        yield return new WaitUntil(() => ubiiNode.WaitForConnection().IsCompleted);
        ubiiNode.Publish(new Ubii.TopicData.TopicDataRecord { Topic = "HololensTest", String = "Cube Manipulation " + eventType });
    }
}
