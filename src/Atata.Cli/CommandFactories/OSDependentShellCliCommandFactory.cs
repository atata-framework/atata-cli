using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Atata.Cli
{
    /// <summary>
    /// Represents the <see cref="CliCommand"/> factory that executes the command
    /// through one of the registered in it shell <see cref="ICliCommandFactory"/> instances
    /// depending on the current operating system.
    /// </summary>
    public class OSDependentShellCliCommandFactory : ICliCommandFactory
    {
        private readonly List<OSPlatformCommandFactoryItem> _osPlatformCommandFactoryMap = new List<OSPlatformCommandFactoryItem>();

        private ICliCommandFactory _otherOSCommandFactory;

        /// <summary>
        /// Gets the <see cref="OSDependentShellCliCommandFactory"/> instance
        /// configured to use <see cref="CmdShellCliCommandFactory"/> for Windows.
        /// </summary>
        /// /// <returns>The configured <see cref="OSDependentShellCliCommandFactory"/> instance.</returns>
        public static OSDependentShellCliCommandFactory UseCmdForWindows() =>
            new OSDependentShellCliCommandFactory()
                .UseForOS(OSPlatform.Windows, new CmdShellCliCommandFactory());

        /// <summary>
        /// Gets the <see cref="OSDependentShellCliCommandFactory"/> instance
        /// configured to use <see cref="CmdShellCliCommandFactory"/> for Windows
        /// and <see cref="ShShellCliCommandFactory"/> for other operating systems.
        /// </summary>
        /// <returns>The configured <see cref="OSDependentShellCliCommandFactory"/> instance.</returns>
        public static OSDependentShellCliCommandFactory UseCmdForWindowsAndShForOthers() =>
            UseCmdForWindows()
                .UseForOtherOS(new ShShellCliCommandFactory());

        /// <summary>
        /// Gets the <see cref="OSDependentShellCliCommandFactory"/> instance
        /// configured to use <see cref="CmdShellCliCommandFactory"/> for Windows
        /// and <see cref="BashShellCliCommandFactory"/> for other operating systems.
        /// </summary>
        /// <returns>The configured <see cref="OSDependentShellCliCommandFactory"/> instance.</returns>
        public static OSDependentShellCliCommandFactory UseCmdForWindowsAndBashForOthers() =>
            UseCmdForWindows()
                .UseForOtherOS(new BashShellCliCommandFactory());

        /// <summary>
        /// Configures to use the specified <paramref name="commandFactory"/> for <paramref name="osPlatform"/>.
        /// </summary>
        /// <param name="osPlatform">The OS platform.</param>
        /// <param name="commandFactory">The command factory.</param>
        /// <returns>The configured <see cref="OSDependentShellCliCommandFactory"/> instance.</returns>
        public OSDependentShellCliCommandFactory UseForOS(string osPlatform, ICliCommandFactory commandFactory) =>
            UseForOS(OSPlatform.Create(osPlatform), commandFactory);

        /// <summary>
        /// Configures to use the specified <paramref name="commandFactory"/> for <paramref name="osPlatform"/>.
        /// </summary>
        /// <param name="osPlatform">The OS platform.</param>
        /// <param name="commandFactory">The command factory.</param>
        /// <returns>The configured <see cref="OSDependentShellCliCommandFactory"/> instance.</returns>
        public OSDependentShellCliCommandFactory UseForOS(OSPlatform osPlatform, ICliCommandFactory commandFactory)
        {
            osPlatform.CheckNotNull(nameof(osPlatform));
            commandFactory.CheckNotNull(nameof(commandFactory));

            _osPlatformCommandFactoryMap.RemoveAll(x => x.Platform == osPlatform);
            _osPlatformCommandFactoryMap.Add(new OSPlatformCommandFactoryItem(osPlatform, commandFactory));

            return this;
        }

        /// <summary>
        /// Configures to use the specified <paramref name="commandFactory"/> for other operating systems.
        /// </summary>
        /// <param name="commandFactory">The command factory.</param>
        /// <returns>The configured <see cref="OSDependentShellCliCommandFactory"/> instance.</returns>
        public OSDependentShellCliCommandFactory UseForOtherOS(ICliCommandFactory commandFactory)
        {
            _otherOSCommandFactory = commandFactory.CheckNotNull(nameof(commandFactory));
            return this;
        }

        /// <inheritdoc/>
        public CliCommand Create(string fileNameOrCommand, string arguments)
        {
            ICliCommandFactory factory = _osPlatformCommandFactoryMap.Find(x => x.IsMatchCurrentOS())
                ?.CommandFactory ?? _otherOSCommandFactory
                ?? throw new InvalidOperationException($"Failed to find {nameof(ICliCommandFactory)} matching the current operating system.");

            return factory.Create(fileNameOrCommand, arguments);
        }

        private sealed class OSPlatformCommandFactoryItem
        {
            public OSPlatformCommandFactoryItem(OSPlatform platform, ICliCommandFactory commandFactory)
            {
                Platform = platform;
                CommandFactory = commandFactory;
            }

            public OSPlatform Platform { get; }

            public ICliCommandFactory CommandFactory { get; }

            public bool IsMatchCurrentOS() =>
                RuntimeInformation.IsOSPlatform(Platform);
        }
    }
}
