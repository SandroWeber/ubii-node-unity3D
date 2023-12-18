using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

using Ubii.TopicData;

public class TestSubscriptions : MonoBehaviour
{
    private UbiiNode ubiiNode = null;

    void Start()
    {
        ubiiNode = FindObjectOfType<UbiiNode>();
        if (ubiiNode == null)
        {
            Debug.LogError("TestSubscriptions - ubii node not found!");
            return;
        }

        RunTests();
    }

    void Update()
    {

    }

    async private void RunTests()
    {
        RunTestSubscribePublish();
        await Task.Delay(1000);
        RunTestSubscribeRegex();
    }

    async private void RunTestSubscribePublish()
    {
        await ubiiNode.WaitForConnection();

        string topic = "/" + ubiiNode.Id + "/unity3D_client/test/subcribe_publish_simple";
        bool success = false;

        Action<TopicDataRecord> callback = (TopicDataRecord record) =>
        {
            success = record.Bool;
        };

        SubscriptionToken subToken = await ubiiNode.SubscribeTopic(topic, callback);
        ubiiNode.Publish(new Ubii.TopicData.TopicDataRecord { Topic = topic, Bool = true });

        await Task.Delay(1000).ContinueWith(async (Task t) =>
        {
            bool successUnsubscribe = await ubiiNode.Unsubscribe(subToken);
            if (!successUnsubscribe) Debug.LogError("RunTestSubscribePublish Unsubscribe() FAILURE!");

            if (success)
            {
                Debug.Log("RunTestSubscribePublish SUCCESS!");
            }
            else
            {
                Debug.LogError("RunTestSubscribePublish FAILURE!");
            }
        });
    }

    async private void RunTestSubscribeRegex()
    {
        // setup
        await ubiiNode.WaitForConnection();

        string common_topic_substring = "/unity3D_client/test/subcribe_publish_regex";
        string regex = "/*" + common_topic_substring + "/[0-9]";
        string[] topics = new string[10];
        for (int i = 0; i < 10; i++)
        {
            topics[i] = "/" + ubiiNode.Id + common_topic_substring + "/" + i.ToString();
        }
        List<string> topics_received = new List<string>();

        // publish some topics first to have pre-existing topics before subscription
        for (int i = 0; i < 5; i++)
        {
            ubiiNode.Publish(new Ubii.TopicData.TopicDataRecord { Topic = topics[i], Bool = true });
        }

        // subscribe, should cover existing topics (already published) and future new topics that have yet to be published for the first time
        SubscriptionToken subToken = await ubiiNode.SubscribeRegex(regex, (Ubii.TopicData.TopicDataRecord record) =>
        {
            topics_received.Add(record.Topic);
        });

        // publish all topics, including ones already published and new ones
        for (int i = 0; i < 10; i++)
        {
            ubiiNode.Publish(new Ubii.TopicData.TopicDataRecord { Topic = topics[i], Bool = true });
        }

        await Task.Delay(1000).ContinueWith(async (Task t) =>
        {
            bool successUnsubscribe = await ubiiNode.Unsubscribe(subToken);
            if (!successUnsubscribe) Debug.LogError("RunTestSubscribeRegex Unsubscribe() FAILURE!");
            
            bool success = true;
            foreach (string topic in topics)
            {
                if (!topics_received.Contains(topic))
                {
                    success = false;
                }
            }

            if (success)
            {
                Debug.Log("RunTestSubscribeRegex SUCCESS!");
            }
            else
            {
                Debug.LogError("RunTestSubscribeRegex FAILURE!");
            }
        });
    }
}
