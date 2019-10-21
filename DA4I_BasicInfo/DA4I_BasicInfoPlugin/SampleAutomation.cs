/////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved
// Written by Forge Partner Development
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Inventor;
using System.Threading;

namespace DA4I_BasicInfoPlugin
{
    [ComVisible(true)]
    public class SampleAutomation
    {
        InventorServer inventorApplication;

        public SampleAutomation(InventorServer inventorApp)
        {
            inventorApplication = inventorApp;
        }

        public void Run(Document doc)
        {
            LogTrace("Run called with {0}", doc.DisplayName);
        }

        private class HeartBeat : IDisposable
        {
            // default is 50s
            public HeartBeat(int intervalMillisec = 50000)
            {
                t = new Thread(() =>
                {

                    LogTrace("HeartBeating every {0}ms.", intervalMillisec);

                    for (; ; )
                    {
                        Thread.Sleep((int)intervalMillisec);
                        LogTrace("HeartBeat {0}.", (long)(new TimeSpan(DateTime.Now.Ticks - ticks).TotalSeconds));
                    }

                });

                ticks = DateTime.Now.Ticks;
                t.Start();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (t != null)
                    {
                        LogTrace("Ending HeartBeat");
                        t.Abort();
                        t = null;
                    }
                }
            }

            private Thread t;
            private long ticks;
        }



        public void RunWithArguments(Document doc, NameValueMap map)
        {
            LogTrace("Processing " + doc.FullFileName);

            try
            {
                if (doc.DocumentType == DocumentTypeEnum.kPartDocumentObject || 
                    doc.DocumentType == DocumentTypeEnum.kAssemblyDocumentObject)
                {
                    using (new HeartBeat())
                    {
                        dynamic ddoc = doc;

                        // Extents
                        Point mp = ddoc.ComponentDefinition.RangeBox.MinPoint;
                        Point xp = ddoc.ComponentDefinition.RangeBox.MaxPoint;

                        Double x = xp.X - mp.X;
                        Double y = xp.Y - mp.Y;
                        Double z = xp.Z - mp.Z;

                        // Surface and Volume
                        Double s = ddoc.ComponentDefinition.MassProperties.Area;
                        Double v = ddoc.ComponentDefinition.MassProperties.Volume;

                        // Svae it to result.json file
                        string result = string.Format(
                            "{{ \"extent\": {{" +
                                    "\"x\": {0}," +
                                    "\"y\": {1}," +
                                    "\"z\": {2} }}," +
                                "\"surface\": {3}," +
                                "\"volume\": {4} }}",
                            x, y, z, s, v);
                        System.IO.File.WriteAllText("result.json", result);
                    }
                }
            }
            catch (Exception e)
            {
                LogError("Processing failed. " + e.ToString());
            }
        }

        #region Logging utilities

        /// <summary>
        /// Log message with 'trace' log level.
        /// </summary>
        private static void LogTrace(string format, params object[] args)
        {
            Trace.TraceInformation(format, args);
        }

        /// <summary>
        /// Log message with 'trace' log level.
        /// </summary>
        private static void LogTrace(string message)
        {
            Trace.TraceInformation(message);
        }

        /// <summary>
        /// Log message with 'error' log level.
        /// </summary>
        private static void LogError(string format, params object[] args)
        {
            Trace.TraceError(format, args);
        }

        /// <summary>
        /// Log message with 'error' log level.
        /// </summary>
        private static void LogError(string message)
        {
            Trace.TraceError(message);
        }

        #endregion
    }
}