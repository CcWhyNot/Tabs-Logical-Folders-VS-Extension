using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class LogicalFoldersToolWindow : ToolWindowPane, IVsRunningDocTableEvents
    {

        private RunningDocumentTable rdt;
        private uint rdtCookie;
        private LogicalFoldersToolWindowControl content;
        private HashSet<(string Caption, string Moniker)> lastTabs = new HashSet<(string, string)>();
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

            content = (LogicalFoldersToolWindowControl)this.Content;
            content.DocumentActivated += moniker =>
            {
                var frame = FindFrameByMoniker(moniker);
                frame?.Show();
            };

            rdt = new RunningDocumentTable(this);
            rdtCookie = rdt.Advise(this);

            Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterBackgroundSolutionLoadComplete += OnSolutionLoadComplete;

            RefreshTree();
        }

        private IVsWindowFrame FindFrameByMoniker(string moniker)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var uiShell = this.GetService(typeof(SVsUIShell)) as IVsUIShell;
            uiShell.GetDocumentWindowEnum(out IEnumWindowFrames windowFramesEnum);

            var frameBuffer = new IVsWindowFrame[1];
            while (windowFramesEnum.Next(1, frameBuffer, out uint fetched) == VSConstants.S_OK && fetched == 1)
            {
                frameBuffer[0].GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out object monikerObj);
                if (moniker.Equals(monikerObj as string)) return frameBuffer[0];
            }

            return null;
        }
        private void OnSolutionLoadComplete(object sender, EventArgs e)
        {
            RefreshTree();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                rdt?.Unadvise(rdtCookie);
                Microsoft.VisualStudio.Shell.Events.SolutionEvents.OnAfterBackgroundSolutionLoadComplete -= OnSolutionLoadComplete;
            }
            base.Dispose(disposing);
        }


        private void RefreshTree()
        {
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

            var currentTabs = new HashSet<(string, string)>(tabs.Select(t => (t.Caption, t.Moniker)));
            if (currentTabs.SetEquals(lastTabs)) return;

            lastTabs = currentTabs;
            content.PopulateTree(tabs);
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs) => VSConstants.S_OK;
        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame) => VSConstants.S_OK;
        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame) => VSConstants.S_OK;
        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            RefreshTree();
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            RefreshTree();
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            RefreshTree();
            return VSConstants.S_OK;
        }
    }
}
