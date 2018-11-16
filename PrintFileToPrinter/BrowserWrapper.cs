using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PrintFileToPrinter
{
    public class BrowserWrapper : System.Windows.Forms.WebBrowser
    {

        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// Disposes resources used by the control.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
            //
            //browserwrapper
            //
            //Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            this.Name = "browserwrapper";
        }

        /// <summary>
        /// This event is raised when the current document is entirely complete, 
        /// and no more navigation is expected to take place for it to load.
        /// </summary>
        /// <author>hmcclungiii</author>
        /// <date>2/21/2014</date>
        public event AbsolutelyCompleteEventHandler AbsolutelyComplete;
        public delegate void AbsolutelyCompleteEventHandler(object sender, EventArgs e);

        private int _onNavigatingCount = 0;
        private int _onNavigatedCount = 0;
        private int _onDocumentCompleteCount = 0;

        /// <summary>
        /// This method is used to clear the counters.  Should not be used
        /// externally, but I left it open for testing, and just in case
        /// scenarios
        /// </summary>
        /// <author>hmcclungiii</author>
        /// <date>2/21/2014</date>
        public void ClearCounters()
        {
            _onNavigatingCount = 0;
            _onNavigatedCount = 0;
            _onDocumentCompleteCount = 0;
        }


        /// <summary>
        /// This property returns true when all the counters have become equal
        /// signifying that the navigation has completely completed
        /// </summary>
        /// <author>hmcclungiii</author>
        /// <date>2/21/2014</date>
        public bool Busy
        {
            get
            {
                //sometimes the first navigating event isn't fired so we just have to make sure the navigating count is 
                //more than the navigated, navigated should never be more than navigating
                bool bBusy = !(_onNavigatingCount <= _onNavigatedCount);
                //if our navigating counts check out, we should always have a documentcompleted count
                //for each navigated event that is fired
                if (!bBusy)
                {
                    bBusy = (_onNavigatedCount > _onDocumentCompleteCount);
                }
                else
                {
                    bBusy = !(_onNavigatedCount == _onDocumentCompleteCount);
                    if (!bBusy) bBusy = !(_onNavigatedCount > 0);
                }
                return bBusy;
            }
        }

        /// <summary>
        /// This method is used to wait until the page has completely loaded.  Use
        /// after calling a submit, or click, or similar method to not execute further
        /// code in the calling class until it has completed.  Also helps to reduce
        /// processor load
        /// </summary>
        /// <author>hmcclungiii</author>
        /// <date>2/21/2014</date>
        public void WaitUntilComplete()
        {
            //first we wait to make sure it starts
            while (!Busy)
            {
                Application.DoEvents();
                //we should sleep for a moment to let the processor have a timeslice
                //for something else - in other words, don't hog the resources
                System.Threading.Thread.Sleep(1);
            }
            //now we wait until it is done
            while (Busy)
            {
                Application.DoEvents();
                //we should sleep for a moment to let the processor have a timeslice
                //for something else - in other words, don't hog the resources
                System.Threading.Thread.Sleep(1);
            }
        }

        public BrowserWrapper()
        {
            this.InitializeComponent();
        }

        //we have to catch the following three event callers to keep a count
        //of them so that we will be able to determine when the navigation
        //process actually completes
        protected override void OnNavigating(WebBrowserNavigatingEventArgs e)
        {
            _onNavigatingCount += 1;
            base.OnNavigating(e);
            if (!Busy)
                OnAbsolutelyComplete();
        }
        protected override void OnNavigated(WebBrowserNavigatedEventArgs e)
        {
            _onNavigatedCount += 1;
            base.OnNavigated(e);
            if (!Busy)
                OnAbsolutelyComplete();
        }
        protected override void OnDocumentCompleted(WebBrowserDocumentCompletedEventArgs e)
        {
            _onDocumentCompleteCount += 1;
            base.OnDocumentCompleted(e);
            if (!Busy)
                OnAbsolutelyComplete();
        }

        /// <summary>
        /// This method should be used in place of the Navigate method to navigate to
        /// a specific URL.  The navigate method was not overridden because it might
        /// be required in future modifications to have access to both methods.  When
        /// calling this NavigateAndWait method, control will not be returned to the
        /// calling class until the document has completely loaded
        /// </summary>
        /// <author>hmcclungiii</author>
        /// <date>2/21/2014</date>
        public void NavigateAndWait(string URL)
        {
            ClearCounters();
            Navigate(URL);
            WaitUntilComplete();
        }

        protected void OnAbsolutelyComplete()
        {
            ClearCounters();
            if (AbsolutelyComplete != null)
            {
                AbsolutelyComplete(this, new EventArgs());
            }
        }

    }
}