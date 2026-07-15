using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace TabsLogicalFolders
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class LogicalFoldersToolWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("239beb68-9e3e-4b30-955e-2511a16c47fe");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalFoldersToolWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private LogicalFoldersToolWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static LogicalFoldersToolWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in LogicalFoldersToolWindowCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new LogicalFoldersToolWindowCommand(package, commandService);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var uiShell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;
            uiShell.GetDocumentWindowEnum(out IEnumWindowFrames windowFramesEnum);

            var tabs = new List<LogicalFoldersToolWindowControl.TabInfo>();
            var frameBuffer = new IVsWindowFrame[1];
            while (windowFramesEnum.Next(1, frameBuffer, out uint fetched) == VSConstants.S_OK && fetched == 1)
            {
                IVsWindowFrame frame = frameBuffer[0];

                frame.GetProperty((int)__VSFPROPID.VSFPROPID_Caption, out object captionObj);
                frame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out object monikerObj);

                string caption = captionObj as string;
                string moniker = monikerObj as string;

                bool esRutaReal = !string.IsNullOrEmpty(moniker) && Path.IsPathRooted(moniker);

                tabs.Add(new LogicalFoldersToolWindowControl.TabInfo
                {
                    Caption = caption,
                    Moniker = moniker,
                    Kind = esRutaReal ? LogicalFoldersToolWindowControl.NodeKind.Document : LogicalFoldersToolWindowControl.NodeKind.Other,
                });

            }

            this.package.JoinableTaskFactory.RunAsync(async delegate
            {
                ToolWindowPane window = await this.package.ShowToolWindowAsync(typeof(LogicalFoldersToolWindow), 0, true, this.package.DisposalToken);
                if ((null == window) || (null == window.Frame))
                {
                    throw new NotSupportedException("Cannot create tool window");
                }

                LogicalFoldersToolWindowControl content = (LogicalFoldersToolWindowControl)window.Content;
                content.DocumentActivated += moniker =>
                {
                    VsShellUtilities.IsDocumentOpen(this.package, moniker, VSConstants.LOGVIEWID.Primary_guid, out _, out _, out IVsWindowFrame frame);
                    frame?.Show();
                };
                content.PopulateTree(tabs);
            });
        }
    }
}
