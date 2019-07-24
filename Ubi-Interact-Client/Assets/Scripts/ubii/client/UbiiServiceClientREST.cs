using UnityEngine;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System;

using Ubii.Services;

public class UbiiServiceClientREST : IUbiiServiceClient
{
    private string host;
    private int port;

    public UbiiServiceClientREST(string host = "localhost", int port = 8102)
    {
        this.host = host;
        this.port = port;
    }

    public Task<ServiceReply> CallService(ServiceRequest request)
    {
        return Task.Run<ServiceReply>(() =>
        {
            string json = request.ToString();

            string url = "http://" + this.host + ":" + this.port + "/services";

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            var service_reply = new ServiceReply();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                try
                {
                    service_reply = ServiceReply.Parser.ParseJson(responseText);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
            }

            return service_reply;
        });
    }
}
