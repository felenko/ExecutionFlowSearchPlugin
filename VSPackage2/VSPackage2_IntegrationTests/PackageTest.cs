﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using ControlFlowSearch.Analiser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace VSPackage2_IntegrationTests
{
    /// <summary>
    /// Integration test for package validation
    /// </summary>
    [TestClass]
    public class PackageTest
    {
        private delegate void ThreadInvoker();

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestMethod]
        [HostType("VS IDE")]
        public void PackageLoadTest()
        {
            var analizer = new Analizer();
            // CodeBlogPackage. private void ShowToolWindow(object sender, EventArgs e)
            analizer.FindDeclarationAtPosition("C:\\Users\\Kostiantyn\\Documents\\Visual Studio 2015\\Projects\\CodeBlog\\CodeBlog.sln", "",
                @"C:\Users\Kostiantyn\Documents\Visual Studio 2015\Projects\CodeBlog\CodeBlog\CodeBlogPackage.cs", 3214);
            UIThreadInvoker.Invoke((ThreadInvoker)delegate ()
            {
               

                //Get the Shell Service
                IVsShell shellService = VsIdeTestHostContext.ServiceProvider.GetService(typeof(SVsShell)) as IVsShell;
                Assert.IsNotNull(shellService);

                //Validate package load
                IVsPackage package;
                Guid packageGuid = new Guid(Company.VSPackage2.GuidList.guidVSPackage2PkgString);
                Assert.IsTrue(0 == shellService.LoadPackage(ref packageGuid, out package));
                Assert.IsNotNull(package, "Package failed to load");

            });
        }
    }
}
