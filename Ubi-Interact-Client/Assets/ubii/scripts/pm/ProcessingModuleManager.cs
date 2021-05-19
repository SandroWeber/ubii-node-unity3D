using Google.Protobuf.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ubii.Sessions;
using Ubii.TopicData;
using UnityEngine;

public class ProcessingModuleManager
{
    public Dictionary<string, ProcessingModule> processingModules = new Dictionary<string, ProcessingModule>();
    private string nodeID;

    private TopicDataProxy topicdataProxy;
    private Dictionary<string, List<SubscriptionToken>> pmTopicSubscriptions = new Dictionary<string, List<SubscriptionToken>>();
    private RuntimeTopicData lockstepTopicDate = new RuntimeTopicData();
    private Dictionary<string, Ubii.Sessions.IOMapping> ioMappings = new Dictionary<string, Ubii.Sessions.IOMapping>();

    public ProcessingModuleManager(string nodeID, object deviceManager, TopicDataProxy topicdataProxy = null)
    {
        this.nodeID = nodeID;
        this.topicdataProxy = topicdataProxy;
    }

    public ProcessingModule CreateModule(Ubii.Processing.ProcessingModule specs)
    {
        ProcessingModule pm = null;
        if (ProcessingModuleStorage.HasEntry(specs.Name))
        {
            pm = ProcessingModuleStorage.CreateInstance(specs);
            // Some TODOs from nodeJS implementation here..
        }
        else
        {
            if (specs.OnProcessingStringified == null || specs.OnProcessingStringified == string.Empty)
            {
                Debug.Log("ProcessingModuleManager can't create PM " + specs.Name + " based on specs, missing OnProcessing definition.");
                return null;
            }
            pm = new ProcessingModule(specs);
        }
        pm.nodeID = this.nodeID;

        bool success = AddModule(pm);
        if (!success)
            return null;
        else
        {
            pm?.OnCreated(pm.status);
            return pm;
        }
    }

    private bool AddModule(ProcessingModule pm)
    {
        if (pm.id == null || pm.id == string.Empty)
        {
            Debug.Log("ProcessingModuleManager: Module " + pm.name + " does not have an ID, can't add it");
            return false;
        }

        processingModules.Add(pm.id, pm);
        return true;
    }

    public bool RemoveModule(ProcessingModule pm)
    {
        if (pm.id == null || pm.id == string.Empty)
        {
            Debug.LogError("ProcessingModuleManager: Module " + pm.name + " does not have an ID, can't remove it");
            return false;
        }

        if (pmTopicSubscriptions.ContainsKey(pm.id))
        {
            List<SubscriptionToken> subscriptionTokens = pmTopicSubscriptions[pm.id];
            subscriptionTokens?.ForEach(token => topicdataProxy.Unsubscribe(token));
            pmTopicSubscriptions.Remove(pm.id);
        }

        pmTopicSubscriptions.Remove(pm.id);
        return true;
    }

    public bool HasModuleID(string id) => processingModules.ContainsKey(id);
    public ProcessingModule GetModuleBySpecs(ProcessingModule pmSpecs, string sessionID)
    {
        ProcessingModule module = GetModuleByID(pmSpecs.id);
        if (module == null)
            module = GetModuleByName(pmSpecs.name, sessionID);
        return module;
    }
    private ProcessingModule GetModuleByID(string id) => processingModules[id];

    private ProcessingModule GetModuleByName(string name, string sessionID)
    {
        throw new NotImplementedException();
    }

    public void StartModule(ProcessingModule pmSpec)
    {
        ProcessingModule pm = processingModules[pmSpec.id];
        pm?.Start();
    }

    public void StopModule(ProcessingModule pmSpec)
    {
        ProcessingModule pm = processingModules[pmSpec.id];
        pm?.Stop();

        List<SubscriptionToken> subs = pmTopicSubscriptions[pmSpec.id];
        if (subs != null)
        {
            foreach (SubscriptionToken token in subs)
            {
                topicdataProxy.Unsubscribe(token);
            }
        }
    }

    public void ApplyIOMappings(RepeatedField<IOMapping> ioMappings, string sessionID)
    {
        Debug.Log("\nApplyIOMappings");

        // TODO: Check this when ioMappings type changes?
        IEnumerable<IOMapping> applicableIOMappings = ioMappings.Where(ioMapping => processingModules.ContainsKey(ioMapping.ProcessingModuleId));

        foreach (IOMapping mapping in applicableIOMappings)
        {
            this.ioMappings[mapping.ProcessingModuleId] = mapping;
            ProcessingModule processingModule = GetModuleByID(mapping.ProcessingModuleId) != null ? GetModuleByID(mapping.ProcessingModuleId) : GetModuleByName(mapping.ProcessingModuleName, sessionID);

            if (processingModule is null)
            {
                Debug.LogError("ProcessingModuleManager: can't find processing module for I/O mapping, given: ID = " +
                    mapping.ProcessingModuleId + ", name = " + mapping.ProcessingModuleName + ", session ID = " + sessionID);
                return;
            }

            bool isLockstep = processingModule.processingMode.Lockstep != null;

            foreach (TopicInputMapping inputMapping in mapping.InputMappings)
            {
                if (!IsValidIOMapping(processingModule, inputMapping))
                {
                    Debug.LogError("ProcessingModuleManager: IO-Mapping for module " + processingModule.name + "->" + inputMapping.InputName + " is invalid");
                    return;
                }

                if (inputMapping.TopicSourceCase == TopicInputMapping.TopicSourceOneofCase.Topic)
                {
                    var topicDataBuffer = this.topicdataProxy; // What about isLockstep?
                    processingModule.SetInputGetter(inputMapping.InputName, () =>
                    {
                        var entry = topicDataBuffer.Pull(inputMapping.Topic);
                        return entry;
                    });

                    if (!isLockstep)
                    {
                        Action<TopicDataRecord> callback = null;

                        if (processingModule.processingMode?.TriggerOnInput != null)
                        {
                            callback = _ => { processingModule.Emit(PMEvents.NEW_INPUT, inputMapping.InputName); }; // TODO: what kind of callback event?
                        }

                        SubscriptionToken subscriptionToken = topicdataProxy.Subscribe(inputMapping.Topic, callback);

                        if (!pmTopicSubscriptions.ContainsKey(processingModule.id))
                        {
                            pmTopicSubscriptions.Add(processingModule.id, new List<SubscriptionToken>());
                        }
                        pmTopicSubscriptions[processingModule.id].Add(subscriptionToken);
                    }
                }
                else if (inputMapping.TopicSourceCase == TopicInputMapping.TopicSourceOneofCase.TopicMux)
                {
                    // ~TODO, device Manager ?
                    string multiplexer;
                    if(inputMapping.TopicMux.Id != null)
                    {
                        multiplexer = "missing code"; // this.deviceManager.getTopicMux(inputMapping.TopicMux.Id) in js file?
                    }
                    else
                    {
                        //multiplexer = this.deviceManager.createTopicMuxerBySpecs(inputMapping.TopicMux, topicDataProxy); in js file?
                    }
                    processingModule.SetInputGetter(inputMapping.InputName, () =>
                    {
                        //return multiplexer.Get()
                        return null;
                    });
                }
            }

            foreach (TopicOutputMapping outputMapping in mapping.OutputMappings)
            {
                if (!IsValidIOMapping(processingModule, outputMapping))
                {
                    Debug.LogError("ProcessingModuleManager: IO-Mapping for module " + processingModule.name + "->" + outputMapping.OutputName + " is invalid");
                    return;
                }

                if (outputMapping.TopicDestinationCase == TopicOutputMapping.TopicDestinationOneofCase.Topic)
                {
                    string messageFormat = processingModule.GetIOMessageFormat(outputMapping.OutputName);
                    TopicDataRecord type = GetTopicDataTypeFromMessageFormat(messageFormat);
                    processingModule.SetOutputSetter(outputMapping.OutputName, null);
                    topicdataProxy.Publish(type);

                }
                else if (outputMapping.TopicDestinationCase == TopicOutputMapping.TopicDestinationOneofCase.TopicDemux)
                {
                    //let demultiplexer = undefined;
                    //if (topicDestination.id)
                    //{
                    //    demultiplexer = this.deviceManager.getTopicDemux(topicDestination.id);
                    //}
                    //else
                    //{
                    //    let topicDataBuffer = isLockstep ? this.lockstepTopicData : this.topicData;
                    //    demultiplexer = this.deviceManager.createTopicDemuxerBySpecs(topicDestination, topicDataBuffer);
                    //}
                    //processingModule.setOutputSetter(outputMapping.outputName, (value) => {
                    //    demultiplexer.push(value);
                    //});
                }
            }
        }
    }

    /// <summary>
    /// Finds out type and returns topicdatarecord
    /// </summary>
    /// <param name="messageFormat"></param>
    /// <returns></returns>
    private TopicDataRecord GetTopicDataTypeFromMessageFormat(string messageFormat)
    {
        throw new NotImplementedException();
    }

    private bool IsValidIOMapping(ProcessingModule processingModule, TopicOutputMapping outputMapping)
    {
        throw new NotImplementedException();
    }

    private bool IsValidIOMapping(ProcessingModule processingModule, TopicInputMapping inputMapping)
    {
        throw new NotImplementedException();
    }

    public void StartSessionModules(Session session)
    {
        foreach (var pm in processingModules.Values)
        {
            if (pm.sessionID == session.Id)
                pm.Start();
        }
    }

    public void StopSessionModules(Session session)
    {
        foreach (var pm in processingModules.Values)
        {
            if (pm.sessionID == session.Id)
                pm.Stop();
        }
    }
}
