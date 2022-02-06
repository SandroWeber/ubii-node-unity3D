using System;
using UnityEngine;

public class ConnectionConfigUI : MonoBehaviour
{
    private UbiiNode ubiiNode = null;
    private bool uiVisible = false;
    private GameObject buttonConnect;
    private GameObject inputFieldMasterNodeAddress;

    private string inputStringMasterNodeAddress = "";

    // Start is called before the first frame update
    void Start()
    {
        ubiiNode = FindObjectOfType<UbiiNode>();
        inputStringMasterNodeAddress = ubiiNode?.masterNodeAddress;

        buttonConnect = transform.Find("ButtonConnect").gameObject;
        inputFieldMasterNodeAddress = transform.Find("InputFieldMasterNodeAddresss").gameObject;
        
        buttonConnect?.SetActive(uiVisible);
        inputFieldMasterNodeAddress?.SetActive(uiVisible);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnButtonUbiiConfig() {
        uiVisible = !uiVisible;
        buttonConnect?.SetActive(uiVisible);
        inputFieldMasterNodeAddress?.SetActive(uiVisible);
    }

    public async void OnButtonConnect()
    {
        ubiiNode.masterNodeAddress = inputStringMasterNodeAddress;
        try
        {
            await ubiiNode.Initialize();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void OnChangeInputMasterNodeAddress(string value) {
        Debug.Log("OnChangeInputMasterNodeAddress = " + value);
        inputStringMasterNodeAddress = value;
    }
}
