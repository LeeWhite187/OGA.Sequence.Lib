using Microsoft.VisualStudio.TestTools.UnitTesting;
using OGA.Sequence.Factory;
using OGA.Sequence.Model.Config;
using OGA.Sequence.Model.Sequence;
using OGA.Sequence.Model.Steps;
using OGA.Sequence.Model.Transitions;
using OGA.Testing.Lib;
using System;
using System.Threading.Tasks;

namespace OGA.Sequence.Lib_Test
{
    /*  Unit Tests for TaskSequence

        //  Test_1_1_1  Create test sequence instance.
        //              Load it with single start step.
        //              Execute it.
        //              Verify it runs to completion.
        //  Test_1_1_2  Create test sequence instance.
        //              Load it with a start step.
        //              Add a delay step.
        //              Add a transition between them.
        //              Execute it.
        //              Verify it runs to completion.
     
     */

    [TestCategory(Test_Types.Unit_Tests)]
    [TestClass]
    public class TestStep_Tests : OGA.Testing.Lib.Test_Base_abstract
    {
        #region Setup

        /// <summary>
        /// This will perform any test setup before the first class tests start.
        /// This exists, because MSTest won't call the class setup method in a base class.
        /// Be sure this method exists in your top-level test class, and that it calls the corresponding test class setup method of the base.
        /// </summary>
        [ClassInitialize]
        static public void TestClass_Setup(TestContext context)
        {
            TestClassBase_Setup(context);

            // Initialize the sequence factory...
            SequenceFactory.Initialize();
        }
        /// <summary>
        /// This will cleanup resources after all class tests have completed.
        /// This exists, because MSTest won't call the class cleanup method in a base class.
        /// Be sure this method exists in your top-level test class, and that it calls the corresponding test class cleanup method of the base.
        /// </summary>
        [ClassCleanup]
        static public void TestClass_Cleanup()
        {
            TestClassBase_Cleanup();
        }

        /// <summary>
        /// Called before each test runs.
        /// Be sure this method exists in your top-level test class, and that it calls the corresponding test setup method of the base.
        /// </summary>
        [TestInitialize]
        override public void Setup()
        {
            //// Push the TestContext instance that we received at the start of the current test, into the common property of the test base class...
            //Test_Base.TestContext = TestContext;

            base.Setup();

            // Runs before each test. (Optional)
        }

        /// <summary>
        /// Called after each test runs.
        /// Be sure this method exists in your top-level test class, and that it calls the corresponding test cleanup method of the base.
        /// </summary>
        [TestCleanup]
        override public void TearDown()
        {
            // Runs after each test. (Optional)
        }

        #endregion


        #region Tests

        //  Test_1_1_1  Create test sequence instance.
        //              Load it with single start step.
        //              Execute it.
        //              Verify it runs to completion.
        [TestMethod]
        public async Task Test_1_1_1()
        {
            // Create the instance...
            var ts = new TaskSequence();


            // Build a simple config...
            var scfg = new SequenceConfig();
            scfg.Id = Guid.NewGuid();
            scfg.Name = Guid.NewGuid().ToString();
            scfg.Description = Guid.NewGuid().ToString();
            scfg.DisplayOrder = 1;

            // Create a start step...
            var ss1 = scfg.AddStep(nameof(TaskStep_SequenceStart));
            ss1.Name = Guid.NewGuid().ToString();
            ss1.Description = Guid.NewGuid().ToString();
            ss1.IsTerminalStep = true;


            // Load the sequence...
            var res1 = await ts.Load(scfg);
            if (res1 != 1)
                Assert.Fail("Wrong Value");


            // Run the sequence...
            var res2 = await ts.Execute();
            if (res2 != 1)
                Assert.Fail("Wrong Value");


            // Verify it completed...
            if(ts.State != Model.eStepState.Completed)
                Assert.Fail("Wrong Value");
        }

        //  Test_1_1_2  Create test sequence instance.
        //              Load it with a start step.
        //              Add a delay step.
        //              Add a transition between them.
        //              Execute it.
        //              Verify it runs to completion.
        [TestMethod]
        public async Task Test_1_1_2()
        {
            // Create the instance...
            var ts = new TaskSequence();


            // Build a simple config...
            var scfg = new SequenceConfig();
            scfg.Id = Guid.NewGuid();
            scfg.Name = Guid.NewGuid().ToString();
            scfg.Description = Guid.NewGuid().ToString();
            scfg.DisplayOrder = 1;


            // Create a start step...
            var ss1 = scfg.AddStep(nameof(TaskStep_SequenceStart));
            ss1.Name = Guid.NewGuid().ToString();
            ss1.Description = Guid.NewGuid().ToString();
            ss1.IsTerminalStep = false;


            // Create the delay step...
            var ss2 = scfg.AddStep(nameof(TaskStep_DelayType));
            ss2.Name = Guid.NewGuid().ToString();
            ss2.Description = Guid.NewGuid().ToString();
            ss2.IsTerminalStep = true;
            ss2.Parameters.Add(TaskStep_DelayType.CONST_CONFIGPARM_DelayTime, "1000");


            // Create a transition between them...
            var tr = scfg.AddTransition(nameof(Transition_AlwaysTrue), ss1.Id, ss2.Id);
            tr.Name = Guid.NewGuid().ToString();
            tr.Description = Guid.NewGuid().ToString();


            // Load the sequence...
            var res1 = await ts.Load(scfg);
            if (res1 != 1)
                Assert.Fail("Wrong Value");


            // Run the sequence...
            var res2 = await ts.Execute();
            if (res2 != 1)
                Assert.Fail("Wrong Value");


            // Verify it completed...
            if(ts.State != Model.eStepState.Completed)
                Assert.Fail("Wrong Value");

            OGA.SharedKernel.Logging_Base.Logger_Ref?.Info(ts.Results.ToLogEntry());
        }


        #endregion
    }
}
