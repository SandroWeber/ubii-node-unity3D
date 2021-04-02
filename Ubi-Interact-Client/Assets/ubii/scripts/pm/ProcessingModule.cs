using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubii.Processing;

public class ProcessingModule
{
    public string nodeID;
	private Ubii.Processing.ProcessingModule specs;
	internal object state;
	internal string id;
	internal string name;

	// processingMode = enum?

	public ProcessingModule(Ubii.Processing.ProcessingModule specs)
	{
		// TODO: !!For that to work the whole class must be serializable, is it?
		JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(specs), this);
		
		// Auto-generated, remove if code above works
		this.specs = specs;
	}

	internal Ubii.Processing.ProcessingModule ToProtobuf()
	{
		throw new NotImplementedException();
	}

	internal void OnCreated(object state)
	{
		throw new NotImplementedException();
	}

	internal void Start()
	{
		throw new NotImplementedException();
	}

	internal void Stop()
	{
		throw new NotImplementedException();
	}
}
