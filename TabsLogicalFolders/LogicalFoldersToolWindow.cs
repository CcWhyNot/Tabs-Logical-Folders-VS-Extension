using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace TabsLogicalFolders
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("371eae9a-611e-4783-8d09-a9416c76b7e1")]
    public class LogicalFoldersToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalFoldersToolWindow"/> class.
        /// </summary>
        public LogicalFoldersToolWindow() : base(null)
        {
            this.Caption = "LogicalFoldersToolWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new LogicalFoldersToolWindowControl();
        }

        protected override void Initialize()
        {
            base.Initialize();

            ThreadHelper.ThrowIfNotOnUIThread();

            var uiShell = this.GetService(typeof(SVsUIShell)) as IVsUIShell;

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

            LogicalFoldersToolWindowControl content = (LogicalFoldersToolWindowControl)this.Content;
            content.DocumentActivated += moniker =>
            {
                VsShellUtilities.IsDocumentOpen(this, moniker, VSConstants.LOGVIEWID.Primary_guid, out _, out _, out IVsWindowFrame frame);
                frame?.Show();
            };
            content.PopulateTree(tabs);

        }
    }
}
