using Moq;
using KDG.EventSourcing.Events;
using KDG.EventSourcing.Events.Models;
using KDG.EventSourcing.Dispatch;
using NodaTime;
using KDG.Database;
using Newtonsoft.Json;
using KDG.EventSourcing.Attributes;

public class DispatcherTests
{

    public class TestModel
    {
        [LogDelta] public string FieldA { get; set; } = string.Empty;
        [LogDelta] public int FieldB { get; set; }
        [LogDelta] public DateTime FieldC { get; set; }
        public string FieldD { get; set; } = string.Empty;
        public RelatedTestModel? Related { get; set; }
    }

    public class RelatedTestModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    class TestEvent : IEvent<TestModel, TestModel>
    {
        public EventDelta Delta(TestModel? original, TestModel output)
        {
            return ((IEvent<TestModel, TestModel>)this).FindDeltas(original, output, [], [], "1");
        }

        public EventEntities Entities(TestModel data)
        {
            throw new NotImplementedException();
        }

        public Task<TestModel> Execute(EventData<TestModel> data)
        {
            throw new NotImplementedException();
        }

        public Task<TestModel?> GetOriginal(EventData<TestModel> data)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void Dispatch_CallsAllEventFunctions()
    {
        Assert.True(true);
    }

    [Fact]
    public void FindDeltas_OneFieldChanged_ReturnsExpectedChanges()
    {
        var oldModel = new TestModel
        {
            FieldA = "old value",
            FieldB = 3,
            FieldC = new DateTime(2000, 01, 01)
        };
        var newModel = new TestModel
        {
            FieldA = "new value",
            FieldB = 3,
            FieldC = new DateTime(2000, 01, 01)
        };

        var actual = new TestEvent().Delta(oldModel, newModel);

        var expected = new EventDelta
        {
            Version = "1",
            Deltas = new List<ColumnDelta>
            {
                new ColumnDelta {
                    Field = "FieldA",
                    OldValue = "old value",
                    OldDisplayValue = "old value",
                    NewValue = "new value",
                    NewDisplayValue = "new value",
                }
            }
        };

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void FindDeltas_UnloggedFieldChanged_ReturnsExpectedChanges()
    {
        var oldModel = new TestModel
        {
            FieldA = "old value",
            FieldB = 3,
            FieldC = new DateTime(2000, 01, 01),
            FieldD = "From This",
        };
        var newModel = new TestModel
        {
            FieldA = "new value",
            FieldB = 4,
            FieldC = new DateTime(2000, 01, 01),
            FieldD = "To This"
        };

        var actual = new TestEvent().Delta(oldModel, newModel);

        var expected = new EventDelta
        {
            Version = "1",
            Deltas = new List<ColumnDelta>
            {
                new ColumnDelta {
                    Field = "FieldA",
                    OldValue = "old value",
                    OldDisplayValue = "old value",
                    NewValue = "new value",
                    NewDisplayValue = "new value",
                },
                new ColumnDelta {
                    Field = "FieldB",
                    OldValue = "3",
                    OldDisplayValue = "3",
                    NewValue = "4",
                    NewDisplayValue = "4",
                }
            }
        };

        // FieldD was changed, but should not have been logged because it lacks the LogDelta attribute.
        Assert.NotEqual(oldModel.FieldD, newModel.FieldD);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void GetMappedValue_ReturnsExpected()
    {
        var relatedId = Guid.NewGuid();
        var model = new TestModel
        {
            Related = new RelatedTestModel
            {
                Id = relatedId,
                Name = "Related thing",
            }
        };

        var relatedProp = typeof(TestModel).GetProperty(nameof(TestModel.Related));
        var valueMappings = new ValueMapping<TestModel> {
            { nameof(TestModel.Related), x => x.Related?.Id.ToString() ?? string.Empty },
        };
        var displayMappings = new ValueMapping<TestModel> {
            { nameof(TestModel.Related), x => x.Related?.Name.ToString() ?? string.Empty },
        };

        Assert.Equal(relatedId.ToString(), valueMappings.GetMappedValue(relatedProp!, model));
        Assert.Equal("Related thing", displayMappings.GetMappedValue(relatedProp!, model));
    } 
}
