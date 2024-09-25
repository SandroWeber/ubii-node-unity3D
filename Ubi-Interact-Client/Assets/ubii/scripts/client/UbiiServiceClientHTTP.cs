using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

using Ubii.Services;

#if WINDOWS_UWP
using Windows.Web.Http;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
#else
using System.Net.Http;
#endif


class UbiiServiceClientHTTP : IUbiiServiceClient
{
    const string LOG_TAG = "[UBII] ServiceClientHTTP - ";
    
#if WINDOWS_UWP
    private Windows.Web.Http.HttpClient httpClient;
#else
    private System.Net.Http.HttpClient httpClient;
#endif

    private string serviceURL;
    private Google.Protobuf.JsonFormatter.Settings jsonFormatSettings;
    private Google.Protobuf.JsonFormatter jsonFormatter;

    public UbiiServiceClientHTTP(string serviceURL = "https://localhost:8102/services/binary")
    {
#if WINDOWS_UWP
        this.httpClient = new Windows.Web.Http.HttpClient();
        Debug.Warn(LOG_TAG + "make sure to use the binary message format endpoint, JSON does currently NOT work.");
#else
        this.httpClient = new System.Net.Http.HttpClient();
#endif

        this.serviceURL = serviceURL;
        jsonFormatSettings = new Google.Protobuf.JsonFormatter.Settings(true).WithFormatEnumsAsIntegers(true);
        jsonFormatter = new Google.Protobuf.JsonFormatter(jsonFormatSettings);
    }

    public async Task<ServiceReply> CallService(ServiceRequest request)
    {
        // JSON
        /*string requestJSON = jsonFormatter.Format(request);
        Debug.LogError("UBII hostURL.CallService() requestJSON=" + requestJSON);*/
        // BINARY
        MemoryStream memoryStream = new MemoryStream();
        CodedOutputStream codedOutputStream = new CodedOutputStream(memoryStream);
        request.WriteTo(codedOutputStream);
        codedOutputStream.Flush();
        byte[] bytebuffer = memoryStream.ToArray();

        Uri uri = new Uri(this.serviceURL);
        ServiceReply reply = null;
        try
        {
#if WINDOWS_UWP
        // JSON
        /*HttpStringContent content = new HttpStringContent(requestJSON, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
        string responseJSON = await httpResponseMessage.Content.ReadAsStringAsync();
        //NOTE: UWP does not support JSON parsing! Probably the use of reflections is the problem. 
        reply = Google.Protobuf.JsonParser.Default.Parse<ServiceReply>(responseJSON);*/
        // BINARY
        HttpBufferContent content = new HttpBufferContent(bytebuffer.AsBuffer());
        content.Headers.Add("Content-Type", "application/octet-stream");
        HttpResponseMessage httpResponseMessage = await this.httpClient.PostAsync(uri, content);
        Debug.LogError(LOG_TAG + "CallService() StatusCode: " + httpResponseMessage.StatusCode);
        // Make sure the post succeeded, and write out the response.
        httpResponseMessage.EnsureSuccessStatusCode();

        IBuffer buffer = await httpResponseMessage.Content.ReadAsBufferAsync();
        byte[] responseByteArray = buffer.ToArray();
        reply = ServiceReply.Parser.ParseFrom(responseByteArray, 0, responseByteArray.Length);
#else
        // JSON
        /*StringContent content = new StringContent(requestJSON, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await this.httpClient.PostAsync(this.serviceURL, content);
        response.EnsureSuccessStatusCode();
        string responseJSON = await response.Content.ReadAsStringAsync();
        reply = Google.Protobuf.JsonParser.Default.Parse<ServiceReply>(responseJSON);*/
        // BINARY
        ByteArrayContent content = new ByteArrayContent(bytebuffer);
        content.Headers.Add("Content-Type", "application/octet-stream");
        HttpResponseMessage httpResponseMessage = await this.httpClient.PostAsync(uri, content);
        // Make sure the post succeeded, and write out the response.
        httpResponseMessage.EnsureSuccessStatusCode();

        byte[] responseByteArray = await httpResponseMessage.Content.ReadAsByteArrayAsync();
        reply = ServiceReply.Parser.ParseFrom(responseByteArray, 0, responseByteArray.Length);
#endif
        }
        catch (Exception e)
        {
            Debug.LogError(LOG_TAG + "CallService() - " + e.ToString());
        }
        
        return reply;
    }

    public void TearDown()
    {
    }
}
