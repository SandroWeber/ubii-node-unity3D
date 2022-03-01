using System;
using UnityEngine;

public class ConnectionConfigUI : MonoBehaviour
{
    private UbiiNode ubiiNode = null;
    private bool uiVisible = false;
    private GameObject buttonConnect;
    private GameObject inputFieldMasterNodeAddressServices;

    private string inputStringMasterNodeAddressServices = "";

    // Start is called before the first frame update
    void Start()
    {
        ubiiNode = FindObjectOfType<UbiiNode>();
        inputStringMasterNodeAddressServices = ubiiNode?.serviceAddress;

        buttonConnect = transform.Find("ButtonConnect").gameObject;
        inputFieldMasterNodeAddressServices = transform.Find("InputFieldMasterNodeAddresss").gameObject;
        
        buttonConnect?.SetActive(uiVisible);
        inputFieldMasterNodeAddressServices?.SetActive(uiVisible);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnButtonUbiiConfig() {
        uiVisible = !uiVisible;
        buttonConnect?.SetActive(uiVisible);
        inputFieldMasterNodeAddressServices?.SetActive(uiVisible);
    }

    public async void OnButtonConnect()
    {
        ubiiNode.serviceAddress = inputStringMasterNodeAddressServices;
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
        inputStringMasterNodeAddressServices = value;
    }
}
