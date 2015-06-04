using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ControlFlowSearch.Analiser;

namespace ControlFlowSearch.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var analizer = new Analizer();

            // CodeBlogPackage. private void ShowToolWindow(object sender, EventArgs e)
            analizer.TextParseTreeRoundtrip(
                "C:\\Users\\Kostiantyn\\Documents\\Visual Studio 2015\\Projects\\CodeBlog\\CodeBlog.sln",
                @"C:\Users\Kostiantyn\Documents\Visual Studio 2015\Projects\CodeBlog\CodeBlog\CodeBlogPackage.cs", 3214);
        }
    }
}
