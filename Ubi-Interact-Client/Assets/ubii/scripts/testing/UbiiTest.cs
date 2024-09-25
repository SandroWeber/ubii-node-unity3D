using System;
using System.Threading.Tasks;

public struct UbiiTestResult
{
    public UbiiTestResult(bool success, string title, string message)
    {
        this.success = success;
        this.title = title;
        this.message = message;
    }

    public bool success;
    public string title, message;

    override public string ToString()
    {
        return "Test '" + title + "': success = " + success + ", message = '" + message + "'";
    }
}

public abstract class UbiiTest
{

    protected UbiiNode node = null;
    protected string testName;

    public UbiiTest(UbiiNode node)
    {
        this.node = node;
        this.testName = this.GetType().Name;
    }

    public abstract Task<UbiiTestResult> RunTest();

    public abstract Task<bool> CancelTest();

    public UbiiTestResult CreateTestResult(bool success, string message) {
        return new UbiiTestResult(success, testName, message);
    }
}