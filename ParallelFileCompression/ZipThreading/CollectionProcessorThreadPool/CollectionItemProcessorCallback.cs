namespace ZipThreading.CollectionProcessorThreadPool
{
    /// <summary>Represents a callback method to be executed by a <see cref="CollectionProcessorThreadPool{T}"/> thread.</summary>
    /// <param name="state">An object containing information to be used by the callback method. </param>
    public delegate void CollectionItemProcessorCallback<in T>(T state);
}
