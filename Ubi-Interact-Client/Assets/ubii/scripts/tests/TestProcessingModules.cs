using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestProcessingModules : MonoBehaviour
{
    private UbiiNode ubiiNode = null;

    private ProcessingModuleCounter pm = new ProcessingModuleCounter();

    // Start is called before the first frame update
    void Start()
    {
        ubiiNode = FindObjectOfType<UbiiNode>();
        ubiiNode.processingModuleDatabase.AddModule(pm.specs);
        ubiiNode.Connect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
