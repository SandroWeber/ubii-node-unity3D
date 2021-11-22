using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubii.Services;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Net.Http;

class UbiiServiceClientREST : IUbiiServiceClient
{
    private string host;
    private int port;
    private string serviceRoute;

    private HttpClient httpClient;
    private string serviceURL;

    RequestSocket socket;

    public UbiiServiceClientREST(string host = "https://localhost", int port = 8102, string serviceRoute = "/services")
    {
        this.host = host;
        this.port = port;
        this.serviceRoute = serviceRoute;

        this.httpClient = new HttpClient();
        this.serviceURL += this.host + ":" + this.port + this.serviceRoute;
        Debug.Log("this.serviceURL = " + this.serviceURL);
        //StartSocket();
    }

    // creates tcp connection to given host and port
    /*private void StartSocket()
    {
    }*/
    public async Task<ServiceReply> CallService(ServiceRequest request)
    {
        Debug.Log("UbiiServiceClientREST.CallService()");
        Debug.Log(request.ToString());
        try
        {
            StringContent content = new StringContent(request.ToString(), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await this.httpClient.PostAsync(this.serviceURL, content);
            Debug.Log(response.ToString());
            string responseJSON = await response.Content.ReadAsStringAsync();
            Debug.Log("responseJSON:");
            Debug.Log(responseJSON);
            /*ServiceReply serviceReply = new ServiceReply();
            JsonUtility.FromJsonOverwrite(responseJSON, serviceReply);*/
            ServiceReply serviceReply = Google.Protobuf.JsonParser.Default.Parse<ServiceReply>(responseJSON);
            Debug.Log("serviceReply:");
            Debug.Log(serviceReply);
        }
        catch (Exception exception)
        {
            Debug.LogError(exception.ToString());
        }
        /*try
        {
            Debug.Log("UbiiServiceClientREST.CallService()");
            string requestJSON = request.ToString();
            Debug.Log(requestJSON);
            UnityWebRequest webRequest = UnityWebRequest.Post(this.serviceURL, requestJSON);

            UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();
            Debug.Log("asyncOperation.isDone = " + asyncOperation.isDone);
            while (!asyncOperation.isDone)
            {
                Debug.Log("waiting for web request to be done");
                await Task.Delay(100);
            }

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
                Debug.Log("\nResponse: " + webRequest.downloadHandler.text);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(exception.ToString());
        }
        */

        return new ServiceReply { Error = new Ubii.General.Error { Message = "not implemented" } };
    }

    /*private IEnumerator CoroutineInterfaceCallService(ServiceRequest request)
    {
        Debug.Log("UbiiServiceClientREST.CoroutineInterfaceCallService()");
        string requestJSON = request.ToString();
        Debug.Log(requestJSON);

        using (UnityWebRequest www = UnityWebRequest.Post(this.serviceURL, requestJSON))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }*/

    public void TearDown()
    {
    }
}
