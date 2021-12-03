using System;
using DependencyInjectionContainerLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DICTests
{
    [TestClass]
    public class DICTests
    {
        private DICConfig diconfig;
        private DIC dicontainer;

        [TestInitialize]
        public void init()
        {
            diconfig = new DICConfig();
            dicontainer = new DIC(diconfig);
        }

        [TestMethod]
        public void ResolutionTest()
        {           
            diconfig.RegisterAsSelf<TestNonAbstractClass1>();            
            TestNonAbstractClass1 tnac1 = dicontainer.Resolve<TestNonAbstractClass1>();
            Assert.IsNotNull(tnac1);
        } 
        
        [TestMethod]
        public void TestResolveClass_ShouldResolve_WhenClassHasOneMoreDependency()
        {
            diconfig.Register<ITestInterface2, TestNonAbstracClass3>();  
            diconfig.Register<TestAbstractClass2, TestAbstractInheritance2>();  
            ITestInterface2 tnac1 = dicontainer.Resolve<ITestInterface2>();
            Assert.IsNotNull(tnac1);
        }

        [TestMethod]
        public void SingletonTest()
        {
            diconfig.RegisterAsSelf<TestNonAbstractClass1>(LifecycleType.Singleton);
            TestNonAbstractClass1 tnac1 = dicontainer.Resolve<TestNonAbstractClass1>();
            TestNonAbstractClass1 tnac2 = dicontainer.Resolve<TestNonAbstractClass1>();
            Assert.AreSame(tnac1, tnac2);
        }

        [TestMethod]
        public void InstancePerDependencyTest()
        {
            diconfig.RegisterAsSelf<TestNonAbstractClass1>();
            TestNonAbstractClass1 tnac1 = dicontainer.Resolve<TestNonAbstractClass1>();
            TestNonAbstractClass1 tnac2 = dicontainer.Resolve<TestNonAbstractClass1>();
            Assert.AreNotSame(tnac1, tnac2);
        }

        [TestMethod]
        public void AbstractRegistrationTest()
        {
            Exception expectedException = null;
            try
            {
                diconfig.RegisterAsSelf<TestAbstractClass1>();
            }
            catch (Exception e)
            {
                expectedException = e;
            }
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public void AbstractCreationTest()
        {
            Exception expectedException = null;
            try
            {
                diconfig.RegisterAsSelf<TestAbstractClass1>();
            }
            catch (Exception e)
            {
                expectedException = e;
            }
            Assert.IsNotNull(expectedException);
        }

        [TestMethod]
        public void ResolveGenericTypeTest()
        {
            diconfig.Register<ITestInterface1, TestNonAbstractClass1>();
            diconfig.Register<TestGenericClass1<ITestInterface1>, TestGenericClass1<ITestInterface1>>();
            TestGenericClass1<ITestInterface1> genericTestClass = dicontainer.Resolve<TestGenericClass1<ITestInterface1>>();
            Assert.IsNotNull(genericTestClass);
        }

        [TestMethod]
        public void OpenGenericTest()
        {
            diconfig.Register<ITestInterface1, TestNonAbstractClass1>();
            diconfig.Register(typeof(TestGenericClass2<>), typeof(TestGenericClass2<>));
            TestGenericClass2<ITestInterface1> test = dicontainer.Resolve<TestGenericClass2<ITestInterface1>>();
            Assert.IsInstanceOfType(test._type, typeof(TestNonAbstractClass1));
            Assert.IsNotNull(test);
        }

        [TestMethod]
        public void ResolveAllTest()
        {
            diconfig.Register<ITestInterface1, TestNonAbstractClass1>();
            diconfig.Register<ITestInterface1, TestNonAbstractClass2>();
            var allImpls = dicontainer.ResolveAll<ITestInterface1>();
            Assert.IsNotNull(allImpls);
        }
    }
}
