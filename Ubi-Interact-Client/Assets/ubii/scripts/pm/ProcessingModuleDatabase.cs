
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IProcessingModuleDatabaseEntry
{
    Ubii.Processing.ProcessingModule GetSpecifications();
    ProcessingModule CreateInstance(Ubii.Processing.ProcessingModule specs);
}

public class ProcessingModuleDatabase
{
    private Dictionary<string, IProcessingModuleDatabaseEntry> dictEntries = new Dictionary<string, IProcessingModuleDatabaseEntry>();

    public bool AddEntry(IProcessingModuleDatabaseEntry entry)
    {
        if (dictEntries.ContainsKey(entry.GetSpecifications().Name))
        {
            return false;
        }

        dictEntries.Add(entry.GetSpecifications().Name, entry);
        //Debug.Log("PMDatabase.AddEntry() - " + entry.GetSpecifications());

        return true;
    }

    public bool HasEntry(string name)
    {
        return dictEntries.ContainsKey(name);
    }

    public IProcessingModuleDatabaseEntry GetEntry(string name)
    {
        return dictEntries[name];
    }

    public List<IProcessingModuleDatabaseEntry> GetAllEntries()
    {
        return new List<IProcessingModuleDatabaseEntry>(dictEntries.Values);
    }

    public List<Ubii.Processing.ProcessingModule> GetAllSpecifications()
    {
        List<Ubii.Processing.ProcessingModule> list = new List<Ubii.Processing.ProcessingModule>();
        foreach (var entry in dictEntries.Values)
        {
            list.Add(entry.GetSpecifications());
        }
        return list;
    }

    public ProcessingModule CreateInstance(Ubii.Processing.ProcessingModule specs)
    {
        try
        {
            IProcessingModuleDatabaseEntry entry = GetEntry(specs.Name);
            return entry.CreateInstance(specs);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return null;
        }

    }
}