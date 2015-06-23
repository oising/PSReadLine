using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.PowerShell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestPSReadLine
{
    [TestClass]
    public class CompletionBusyTest
    {
        [TestMethod]
        public void TaskHelper_DueTime()
        {

            bool completed = false;
            var dummy = new CancellationTokenSource();
            try
            {
                var t = TaskHelper.Delay(500, dummy.Token);
                completed = t.Wait(1500);
            }
            finally
            {
                dummy.Dispose();
            }
            Assert.AreEqual(true, completed);
        }

        [TestMethod]
        public void TaskHelper_Cancel()
        {
            bool completed = false;
            bool cancelled = false;
            var source = new CancellationTokenSource();

            var t = TaskHelper.Delay(1500, source.Token);
            source.CancelAfter(500);
            try
            {
                completed = t.Wait(3000);
            }
            catch (AggregateException)
            {
                cancelled = true;
            }

            Assert.AreEqual(false, completed);
            Assert.AreEqual(true, cancelled);
        }
    }
}
