
using System.Collections.Generic;

public class ProcessingModuleDatabase
{
    private Dictionary<string, ProcessingModule> dictProcessingModules;

    public bool AddModule(ProcessingModule pm)
    {
        if (dictProcessingModules.ContainsKey(pm.name))
        {
            return false;
        }

        dictProcessingModules.Add(pm.name, pm);

        return true;
    }

    public ProcessingModule GetModule(string name)
    {
        return dictProcessingModules.ContainsKey(name) ? dictProcessingModules[name] : null;
    }

    public List<ProcessingModule> GetAllModules()
    {
        return new List<ProcessingModule>(dictProcessingModules.Values);
    }
}