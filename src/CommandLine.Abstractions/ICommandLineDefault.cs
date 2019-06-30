using System.Threading.Tasks;

namespace Rocket.Surgery.Extensions.CommandLine
{
    /// <summary>
    /// Delegate OnRunDelegate
    /// </summary>
    /// <param name="state">The state.</param>
    /// <returns>System.Int32.</returns>
    public delegate int OnRunDelegate(IApplicationState state);

    /// <summary>
    /// Delegate OnParseDelegate
    /// </summary>
    /// <param name="state">The state.</param>
    public delegate void OnParseDelegate(IApplicationState state);

    /// <summary>
    ///  IDefaultCommand
    /// </summary>
    public interface IDefaultCommand
    {
        /// <summary>
        /// Runs the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>System.Int32.</returns>
        int Run(IApplicationState state);
    }

    /// <summary>
    /// OnRunDefaultCommand.
    /// Implements the <see cref="Rocket.Surgery.Extensions.CommandLine.IDefaultCommand" />
    /// </summary>
    /// <seealso cref="Rocket.Surgery.Extensions.CommandLine.IDefaultCommand" />
    class OnRunDefaultCommand : IDefaultCommand
    {
        private readonly OnRunDelegate @delegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnRunDefaultCommand"/> class.
        /// </summary>
        /// <param name="delegate">The delegate.</param>
        public OnRunDefaultCommand(OnRunDelegate @delegate)
        {
            this.@delegate = @delegate;
        }

        /// <summary>
        /// Runs the specified state.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>System.Int32.</returns>
        public int Run(IApplicationState state)
        {
            return @delegate(state);
        }
    }
}
