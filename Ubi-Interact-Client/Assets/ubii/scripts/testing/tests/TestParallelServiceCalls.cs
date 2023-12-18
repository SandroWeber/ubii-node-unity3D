using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Ubii.Services;

public class TestParallelServiceCalls : UbiiTest
{
    static int NUM_TASKS = 5, TEST_DURATION_SECONDS = 3;
    private List<Task> tasks = new List<Task>();
    private List<CancellationTokenSource> listCts;
    private bool failure = false;

    public TestParallelServiceCalls(UbiiNode node) : base(node) { }

    override public async Task<UbiiTestResult> RunTest()
    {
        await node.WaitForConnection();

        listCts = new List<CancellationTokenSource>();
        for (int i = 0; i < NUM_TASKS; i++)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            listCts.Add(cts);
            tasks.Add(Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        ServiceReply reply = await node.CallService(new ServiceRequest
                        {
                            Topic = UbiiConstants.Instance.DEFAULT_TOPICS.SERVICES.CLIENT_GET_LIST
                        });
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.Log("CallService caused exception");
                        failure = true;
                    }
                }
            }, cts.Token));
        }

        return await WaitForTestToFinish();
    }

    override public Task<bool> CancelTest()
    {
        return Task.FromResult(true);
    }

    private async Task<UbiiTestResult> WaitForTestToFinish()
    {
        await Task.Delay(TimeSpan.FromSeconds(TEST_DURATION_SECONDS));

        foreach (CancellationTokenSource cts in listCts)
        {
            cts.Cancel();
        }

        foreach (Task task in tasks)
        {
            await task;
        }

        foreach (CancellationTokenSource cts in listCts)
        {
            cts.Dispose();
        }

        listCts.Clear();

        if (failure)
        {
            return CreateTestResult(false, "service calls caused an exception");
        }
        else
        {
            return CreateTestResult(true, "test completed successfully");
        }
    }
}