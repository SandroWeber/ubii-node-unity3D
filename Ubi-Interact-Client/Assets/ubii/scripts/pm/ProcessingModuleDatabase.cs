
using System.Collections.Generic;
using UnityEngine;

public class ProcessingModuleDatabase
{
    private Dictionary<string, Ubii.Processing.ProcessingModule> dictProcessingModules = new Dictionary<string, Ubii.Processing.ProcessingModule>();

    public bool AddModule(Ubii.Processing.ProcessingModule pmSpecs)
    {
        if (dictProcessingModules.ContainsKey(pmSpecs.Name))
        {
            return false;
        }

        dictProcessingModules.Add(pmSpecs.Name, pmSpecs);
        Debug.Log("ProcessingModuleDatabase.AddModule() - " + pmSpecs.Name);

        return true;
    }

    public Ubii.Processing.ProcessingModule GetModule(string name)
    {
        return dictProcessingModules.ContainsKey(name) ? dictProcessingModules[name] : null;
    }

    public List<Ubii.Processing.ProcessingModule> GetAllModules()
    {
        return new List<Ubii.Processing.ProcessingModule>(dictProcessingModules.Values);
    }
}