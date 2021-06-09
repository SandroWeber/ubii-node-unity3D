using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ubii.Sessions;
using Ubii.TopicData;
using UnityEngine;

public class ProcessingModuleManager
{
    /// <summary>
    /// References all processing modules with their id as key
    /// </summary>
    public Dictionary<string, ProcessingModule> processingModules = new Dictionary<string, ProcessingModule>();

    /// <summary>
    /// TopicDataProxy
    /// </summary>
    private TopicDataProxy topicdataProxy;

    /// <summary>
    /// Stores all subscription tokens as a list for each pm
    /// </summary>
    private Dictionary<string, List<SubscriptionToken>> pmTopicSubscriptions = new Dictionary<string, List<SubscriptionToken>>();

    /// <summary>
    /// IOMappings
    /// </summary>
    private Dictionary<string, Ubii.Sessions.IOMapping> ioMappings = new Dictionary<string, Ubii.Sessions.IOMapping>();

    private TopicDataBuffer lockstepTopicData = new TopicDataBuffer();

    private string nodeID;
    
    public ProcessingModuleManager(string nodeID, object deviceManager, TopicDataProxy topicdataProxy = null)
    {
        this.nodeID = nodeID;
        this.topicdataProxy = topicdataProxy;
    }

    /// <summary>
    /// Creates a PM from given specs and adds it to the dictionary on success
    /// </summary>
    /// <param name="specs">PM specs</param>
    /// <returns>Created PM</returns>
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

    /// <summary>
    /// Add given pm
    /// </summary>
    /// <param name="pm">PM to add</param>
    /// <returns><see langword="true"/> if pm was added successfully, <see langword="false"/> if given pm had no id</returns>
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

    /// <summary>
    /// Removes a module and unsubscribes all its tokens
    /// </summary>
    /// <param name="pm">PM to remove</param>
    /// <returns><see langword="true"/>, if pm could be removed successfully. <see langword="false"/> if no pm with that id is registered</returns>
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

    /// <summary>
    /// Checks if processing module list contains a pm with given id
    /// </summary>
    /// <param name="id">Processing module ID to look for</param>
    /// <returns></returns>
    public bool HasModuleID(string id) => processingModules.ContainsKey(id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pmSpecs"></param>
    /// <param name="sessionID"></param>
    /// <returns>Processing module matching given specs</returns>
    public ProcessingModule GetModuleBySpecs(ProcessingModule pmSpecs, string sessionID)
    {
        ProcessingModule module = GetModuleByID(pmSpecs.id);
        if (module == null)
            module = GetModuleByName(pmSpecs.name, sessionID);
        return module;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">Id of processing module</param>
    /// <returns>Processing module with given id, null if not existing</returns>
    private ProcessingModule GetModuleByID(string id) => processingModules[id];

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">Name of processing module</param>
    /// <param name="sessionID">Session ID</param>
    /// <returns>Processing module with given name, null if not existing</returns>
    private ProcessingModule GetModuleByName(string name, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Starts processing module
    /// </summary>
    /// <param name="pmSpec">Processing module to start</param>
    public void StartModule(ProcessingModule pmSpec)
    {
        ProcessingModule pm = processingModules[pmSpec.id];
        pm?.Start();
    }

    /// <summary>
    /// Stops module and unsubscribes its tokens from the topic data proxy
    /// </summary>
    /// <param name="pmSpec">Processing module to stop</param>
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

    /// <summary>
    /// Applies IO Mappings
    /// </summary>
    /// <param name="ioMappings"></param>
    /// <param name="sessionID"></param>
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
                    TopicDataRecord record = processingModule.outputs[outputMapping.OutputName];
                    processingModule.SetOutputSetter(outputMapping.OutputName, _ => topicdataProxy.Publish(record));
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

    private bool IsValidIOMapping(ProcessingModule processingModule, TopicOutputMapping outputMapping)
    {
        return processingModule.outputs.Any(element => element.Key == outputMapping.OutputName);
    }

    private bool IsValidIOMapping(ProcessingModule processingModule, TopicInputMapping inputMapping)
    {
        return processingModule.inputs.Any(element => element.Key == inputMapping.InputName);
    }

    /// <summary>
    /// Starts modules from given session
    /// </summary>
    /// <param name="session"></param>
    public void StartSessionModules(Session session)
    {
        foreach (var pm in processingModules.Values)
        {
            if (pm.sessionID == session.Id)
                pm.Start();
        }
    }

    /// <summary>
    /// Stops modules from given session
    /// </summary>
    /// <param name="session"></param>
    public void StopSessionModules(Session session)
    {
        foreach (var pm in processingModules.Values)
        {
            if (pm.sessionID == session.Id)
                pm.Stop();
        }
    }
}
