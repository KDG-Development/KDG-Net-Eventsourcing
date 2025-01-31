namespace KDG.EventSourcing.Events
{
    public interface IEvent<I,O>
        where O : class
    {
        public Models.EventEntities Entities(I data);
        public Task<O> Execute(Models.EventData<I> data);
        public Models.EventDelta Delta(O? original, O output);
        public Task<O?> GetOriginal(Models.EventData<I> data);
    }
}
