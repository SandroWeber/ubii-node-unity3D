using System.Collections;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

//This script will publish test data every time the Cube is grabbed or released
public class HololensTest : MonoBehaviour
{
    [SerializeField]
    private UbiiNode ubiiNode;

    [SerializeField]
    private ObjectManipulator objectManipulator;
    
    void Start()
    {
        objectManipulator.OnManipulationStarted.AddListener(OnManipulationStarted);
        objectManipulator.OnManipulationEnded.AddListener(OnManipulationEnded);
    }

    //Callback methods for the manipulation events
    private void OnManipulationStarted(ManipulationEventData arg0)
    {
        StartCoroutine(PublishTestData("started"));
    }

    private void OnManipulationEnded(ManipulationEventData arg0)
    {
        StartCoroutine(PublishTestData("ended"));
    }

    //Coroutine to asynchronously publish test data
    private IEnumerator PublishTestData(string eventType){
        yield return new WaitUntil(() => ubiiNode.WaitForConnection().IsCompleted);
        ubiiNode.Publish(new Ubii.TopicData.TopicDataRecord { Topic = "HololensTest", String = "Cube Manipulation " + eventType });
    }
}
