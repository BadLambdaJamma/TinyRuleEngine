using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TinyRuleEngineTest
{
    public abstract class BaseUnitTest
    {
        protected string testFilePath = "";

        [TestInitialize]
        public void testInit()
        {
            testFilePath = AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
