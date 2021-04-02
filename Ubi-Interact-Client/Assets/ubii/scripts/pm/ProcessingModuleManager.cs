using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessingModuleManager
{
	public Dictionary<string, ProcessingModule> processingModules = new Dictionary<string, ProcessingModule>();
	private string nodeID;
	private object deviceManager;
	private TopicDataProxy topicdataProxy;
	private Dictionary<string, List<string>> pmTopicSubscriptions = new Dictionary<string, List<string>>();
	// TODO: private List<iomappings> ioMappings =
	// lockstepTopicData...

	public ProcessingModuleManager(string nodeID, object deviceManager, TopicDataProxy topicdataProxy = null)
	{
		this.nodeID = nodeID;
		this.deviceManager = deviceManager;
		this.topicdataProxy = topicdataProxy;
	}

	internal ProcessingModule CreateModule(Ubii.Processing.ProcessingModule specs)
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
			pm?.OnCreated(pm.state);
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
	internal bool RemoveModule(ProcessingModule pm)
	{
		if (pm.id == null || pm.id == string.Empty)
		{
			Debug.LogError("ProcessingModuleManager: Module " + pm.name + " does not have an ID, can't remove it");
			return false;
		}

		if (pmTopicSubscriptions.ContainsKey(pm.id))
		{
			List<string> subscriptionTokens = pmTopicSubscriptions[pm.id];
			if (subscriptionTokens != null)
			{
				foreach (string token in subscriptionTokens)
				{
					topicdataProxy.Unsubscribe(token);
				}
			}
			pmTopicSubscriptions.Remove(pm.id);
		}

		pmTopicSubscriptions.Remove(pm.id);
		return true;
	}

	internal bool HasModuleID(string id) => processingModules.ContainsKey(id);
	internal ProcessingModule GetModuleBySpecs(ProcessingModule pmSpecs, string sessionID)
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

	internal void StartModule(ProcessingModule pmSpec)
	{
		ProcessingModule pm = processingModules[pmSpec.id];
		pm?.Start();
	}

	internal void StopModule(ProcessingModule pmSpec)
	{
		ProcessingModule pm = processingModules[pmSpec.id];
		pm?.Stop();

		List<string> subs = pmTopicSubscriptions[pmSpec.id];
		if (subs != null)
		{
			foreach (string token in subs)
			{
				topicdataProxy.Unsubscribe(token);
			}
		}
	}

}
