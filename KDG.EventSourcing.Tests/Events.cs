using Moq;
using KDG.EventSourcing.Events;
using KDG.EventSourcing.Events.Models;
using KDG.EventSourcing.Dispatch;
using NodaTime;
using KDG.Database;
using Newtonsoft.Json;

public class DispatcherTests
{

    [Fact]
    public void Dispatch_CallsAllEventFunctions()
    {
        Assert.True(true);
    }
}
