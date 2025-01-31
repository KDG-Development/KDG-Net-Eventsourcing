using System.Data;
using Npgsql;
using KDG.EventSourcing.Events;
using KDG.EventSourcing.Events.Models;
using KDG.Database.Common;
using KDG.Common;
namespace KDG.EventSourcing.Dispatch
{
    public class Dispatcher: IDispatcher
    {
        protected readonly Newtonsoft.Json.JsonSerializerSettings _serializerSettings;
        protected readonly NodaTime.IClock _clock;
        protected readonly KDG.Database.DML.PostgreSQL _db;
        public Dispatcher(Newtonsoft.Json.JsonSerializerSettings serializerSettings, NodaTime.IClock clock,KDG.Database.DML.PostgreSQL db)
        {
            _serializerSettings = serializerSettings;
            _clock = clock;
            _db = db;
        }
        private async Task LogEvent<T>(NpgsqlTransaction transaction, Events.Models.EventData<T> e, string delta)
        {
            if(transaction.Connection == null)
            {
                throw new NullReferenceException();
            }
            await _db.Insert(transaction, new Database.DML.InsertConfig<EventData<T>> {
                Table = "_events",
                Data = e,
                Fields = new Dictionary<string, Func<EventData<T>,ADbValue>> {
                    { "entity_a_id", e => new DbGuid(e.Entities.EntityA) },
                    { "entity_b_id", e => new DbNullable<Guid>(e.Entities.EntityB.ToOption(), g => new DbGuid(g)) },
                    { "entity_c_id", e => new DbNullable<Guid>(e.Entities.EntityC.ToOption(), g => new DbGuid(g)) },
                    { "event", e => new DbString(e.Name) },
                    { "data", e => new DbJson(Newtonsoft.Json.JsonConvert.SerializeObject(e.Data,_serializerSettings)) },
                    { "delta", _ => new DbJson(delta) },
                    { "occurred", e => new DbInstant(e.Occurred) },
                    { "user_id", e => new DbGuid(e.User) },
                }
            });
        }

        private async Task<O> Execute<I,O>(NpgsqlTransaction transaction, IEvent<I,O> dispatcherEvent, EventData<I> data) where O : class
        {
            var original = await dispatcherEvent.GetOriginal(data);
            var result = await dispatcherEvent.Execute(data);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(dispatcherEvent.Delta(original,result),_serializerSettings);
            await LogEvent(transaction,data, json);

            return result;
        }

        public async Task<O> Dispatch<I,O>(IEvent<I,O> e, Guid user, I input) where O : class
        {
            return await _db.withTransaction(async (transaction) => {
                var eventData = new EventData<I>()
                {
                    Transaction = transaction,
                    Entities = e.Entities(input),
                    Name = e.GetType().Name,
                    Occurred = _clock.GetCurrentInstant(),
                    User = user,
                    Data = input,
                };
                return await Execute(transaction,e, eventData);
            });
        }
    }
}
