using KDG.EventSourcing.Events;
using KDG.EventSourcing.Events.Models;
namespace KDG.EventSourcing.Dispatch
{
    public interface IDispatcher
    {
        Task<O> Dispatch<I, O>(IEvent<I, O> e, Guid user, I data) where O : class;
    }
}
