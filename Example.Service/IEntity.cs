namespace Example.Service
{
    public interface IEntity<out T>
    {
        T Id { get; }
    }
}