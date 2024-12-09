namespace Zoodle
{
    /// <summary>
    /// Generic class that defines a state.
    /// </summary>
    /// <typeparam name="TTarget">Target type that is affected by the state.</typeparam>
    public class State<TTarget>
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        /// <param name="target">Target that is entering the state.</param>
        public virtual void OnStateEnter(TTarget target)
        {

        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        /// <param name="target">Target that is exiting the state.</param>
        public virtual void OnStateExit(TTarget target)
        {

        }
    }
}
