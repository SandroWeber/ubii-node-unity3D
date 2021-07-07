using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    private TopicDataProxy topicDataProxy;

    /// <summary>
    /// Stores all subscription tokens as a list for each pm
    /// </summary>
    private Dictionary<string, List<SubscriptionToken>> pmTopicSubscriptions = new Dictionary<string, List<SubscriptionToken>>();

    /// <summary>
    /// IOMappings
    /// </summary>
    private Dictionary<string, Ubii.Sessions.IOMapping> ioMappings = new Dictionary<string, Ubii.Sessions.IOMapping>();

    private ProcessingModuleDatabase pmDatabase = null;

    private string nodeID;

    public ProcessingModuleManager(string nodeID, object deviceManager, ProcessingModuleDatabase pmDatabase, TopicDataProxy topicdataProxy = null)
    {
        this.nodeID = nodeID;
        this.topicDataProxy = topicdataProxy;
        this.pmDatabase = pmDatabase;
    }

    /// <summary>
    /// Creates a PM from given specs and adds it to the dictionary on success
    /// </summary>
    /// <param name="specs">PM specs</param>
    /// <returns>Created PM</returns>
    public ProcessingModule CreateModule(Ubii.Processing.ProcessingModule specs)
    {
        ProcessingModule pm = null;
        if (pmDatabase.HasEntry(specs.Name))
        {
            Ubii.Processing.ProcessingModule detailedSpecs = pmDatabase.GetEntry(specs.Name).GetSpecifications();
            detailedSpecs.Id = specs.Id;
            detailedSpecs.NodeId = specs.NodeId;
            detailedSpecs.SessionId = specs.SessionId;

            pm = pmDatabase.CreateInstance(detailedSpecs);
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

        bool success = AddModule(pm);
        if (!success)
            return null;
        else
        {
            pm.OnCreated();
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
        if (pm.Id == null || pm.Id == string.Empty)
        {
            Debug.LogError("ProcessingModuleManager: Module " + pm.Name + " does not have an ID, can't add it");
            return false;
        }

        this.processingModules.Add(pm.Id, pm);
        //Debug.Log("ProcessingModuleManager.AddModule() - " + pm.ToString());
        return true;
    }

    /// <summary>
    /// Removes a module and unsubscribes all its tokens
    /// </summary>
    /// <param name="pm">PM to remove</param>
    /// <returns><see langword="true"/>, if pm could be removed successfully. <see langword="false"/> if no pm with that id is registered</returns>
    public bool RemoveModule(ProcessingModule pm)
    {
        if (pm.Id == null || pm.Id == string.Empty)
        {
            Debug.LogError("ProcessingModuleManager: Module " + pm.Name + " does not have an ID, can't remove it");
            return false;
        }

        if (pmTopicSubscriptions.ContainsKey(pm.Id))
        {
            List<SubscriptionToken> subscriptionTokens = pmTopicSubscriptions[pm.Id];
            subscriptionTokens?.ForEach(token => topicDataProxy.Unsubscribe(token));
            pmTopicSubscriptions.Remove(pm.Id);
        }

        pmTopicSubscriptions.Remove(pm.Id);
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
    public ProcessingModule GetModuleBySpecs(Ubii.Processing.ProcessingModule pmSpecs, string sessionID)
    {
        ProcessingModule module = GetModuleByID(pmSpecs.Id);
        if (module == null)
            module = GetModuleByName(pmSpecs.Name, sessionID);
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
    public void StartModule(Ubii.Processing.ProcessingModule pmSpec)
    {
        ProcessingModule pm = processingModules[pmSpec.Id];
        pm?.Start();
    }

    /// <summary>
    /// Stops module and unsubscribes its tokens from the topic data proxy
    /// </summary>
    /// <param name="pmSpec">Processing module to stop</param>
    public void StopModule(Ubii.Processing.ProcessingModule pmSpec)
    {
        ProcessingModule pm = processingModules[pmSpec.Id];
        pm?.Stop();

        List<SubscriptionToken> subs = pmTopicSubscriptions[pmSpec.Id];
        if (subs != null)
        {
            foreach (SubscriptionToken token in subs)
            {
                topicDataProxy.Unsubscribe(token);
            }
        }
    }

    /// <summary>
    /// Applies IO Mappings
    /// </summary>
    /// <param name="ioMappings"></param>
    /// <param name="sessionID"></param>
    public async Task<bool> ApplyIOMappings(RepeatedField<IOMapping> ioMappings, string sessionID)
    {
        //Debug.Log("ApplyIOMappings - ioMappings: " + ioMappings);

        // TODO: Check this when ioMappings type changes?
        IEnumerable<IOMapping> applicableIOMappings = ioMappings.Where(ioMapping => processingModules.ContainsKey(ioMapping.ProcessingModuleId));

        foreach (IOMapping mapping in applicableIOMappings)
        {
            //Debug.Log("ApplyIOMappings - applicableIOMapping: " + mapping);
            this.ioMappings[mapping.ProcessingModuleId] = mapping;
            ProcessingModule processingModule = GetModuleByID(mapping.ProcessingModuleId) != null ? GetModuleByID(mapping.ProcessingModuleId) : GetModuleByName(mapping.ProcessingModuleName, sessionID);

            if (processingModule is null)
            {
                Debug.LogError("ProcessingModuleManager: can't find processing module for I/O mapping, given: ID = " +
                    mapping.ProcessingModuleId + ", name = " + mapping.ProcessingModuleName + ", session ID = " + sessionID);
                return false;
            }

            foreach (TopicInputMapping inputMapping in mapping.InputMappings)
            {
                bool success = await ApplyInputMapping(processingModule, inputMapping);
                if (!success) return false;
            }
            foreach (TopicOutputMapping outputMapping in mapping.OutputMappings)
            {
                ApplyOutputMapping(processingModule, outputMapping);
            }
        }

        return true;
    }

    private async Task<bool> ApplyInputMapping(ProcessingModule processingModule, TopicInputMapping inputMapping)
    {
        if (!IsValidIOMapping(processingModule, inputMapping))
        {
            Debug.LogError("PM-Manager: IO-Mapping for module " + processingModule.Name + "->" + inputMapping.InputName + " is invalid");
            return false;
        }

        bool isLockstep = processingModule.ProcessingMode.Lockstep != null;

        if (inputMapping.TopicSourceCase == TopicInputMapping.TopicSourceOneofCase.Topic)
        {
            processingModule.SetInputGetter(inputMapping.InputName, () =>
            {
                var entry = this.topicDataProxy.Pull(inputMapping.Topic);
                return entry;
            });

            if (!isLockstep)
            {
                Action<TopicDataRecord> callback = null;

                if (processingModule.ProcessingMode?.TriggerOnInput != null)
                {
                    callback = _ => { processingModule.Emit(PMEvents.NEW_INPUT, inputMapping.InputName); };
                }
                else
                {
                    callback = _ => {};
                }

                // subscribe to topic and save token
                try
                {
                    SubscriptionToken subscriptionToken = await this.topicDataProxy.SubscribeTopic(inputMapping.Topic, callback);
                    if (!pmTopicSubscriptions.ContainsKey(processingModule.Id))
                    {
                        pmTopicSubscriptions.Add(processingModule.Id, new List<SubscriptionToken>());
                    }
                    pmTopicSubscriptions[processingModule.Id].Add(subscriptionToken);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return false;
                }
            }
        }
        else if (inputMapping.TopicSourceCase == TopicInputMapping.TopicSourceOneofCase.TopicMux)
        {
            // ~TODO, device Manager ?
            string multiplexer;
            if (inputMapping.TopicMux.Id != null)
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

        return true;
    }

    private void ApplyOutputMapping(ProcessingModule processingModule, TopicOutputMapping outputMapping)
    {
        if (!IsValidIOMapping(processingModule, outputMapping))
        {
            Debug.LogError("ProcessingModuleManager: IO-Mapping for module " + processingModule.Name + "->" + outputMapping.OutputName + " is invalid");
        }

        if (outputMapping.TopicDestinationCase == TopicOutputMapping.TopicDestinationOneofCase.Topic)
        {
            processingModule.SetOutputSetter(outputMapping.OutputName, (TopicDataRecord record) =>
            {
                record.Topic = outputMapping.Topic;
                topicDataProxy.Publish(record);
            });
        }
        else if (outputMapping.TopicDestinationCase == TopicOutputMapping.TopicDestinationOneofCase.TopicDemux)
        {
            //let demultiplexer = undefined;
            //if (topicDestination.Id)
            //{
            //    demultiplexer = this.deviceManager.getTopicDemux(topicDestination.Id);
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

    private bool IsValidIOMapping(ProcessingModule processingModule, TopicInputMapping inputMapping)
    {
        return processingModule.Inputs.Any(input => input.InternalName == inputMapping.InputName);
    }

    private bool IsValidIOMapping(ProcessingModule processingModule, TopicOutputMapping outputMapping)
    {
        return processingModule.Outputs.Any(output => output.InternalName == outputMapping.OutputName);
    }

    /// <summary>
    /// Starts modules from given session
    /// </summary>
    /// <param name="session"></param>
    public void StartSessionModules(Session session)
    {
        foreach (var pm in processingModules.Values)
        {
            if (pm.SessionId == session.Id)
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
            if (pm.SessionId == session.Id)
                pm.Stop();
        }
    }
}
