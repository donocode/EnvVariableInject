using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EnvVariableInect.Test
{
    [TestFixture]
    public class IntegrationTest
    {
        private Dictionary<string, string> _envVariables = new Dictionary<string, string>
        {
            ["ConstField"] = "ConstFieldTestValue",
            ["PrivateConstField"] = "PrivateConstFieldTestValue",
            ["StaticReadonlyField"] = "StaticReadonlyFieldTestValue",
            ["PrivateStaticReadonlyField"] = "PrivateStaticReadonlyFieldTestValue",
            ["StaticField"] = "StaticFieldTestValue",
            ["PrivateStaticField"] = "PrivateStaticFieldTestValue",
            ["Field"] = "FieldTestValue",
            ["PrivateField"] = "PrivateFieldTestValue",
            ["InstanceInt"] = "99",
            ["InstanceDouble"] = "99.99",
            ["InstanceBoolWord"] = "false",
            ["InstanceBoolNumber"] = "0",
            ["ConstInt"] = "22",
            ["ConstDouble"] = "22.22",
            ["ConstBoolWord"] = "false",
            ["ConstBoolNumber"] = "0",
        };

        Assembly assembly;
        string beforeAssemblyPath;
        string afterAssemblyPath;

        public IntegrationTest()
        {
            var currentAssemblyPath = Path.GetDirectoryName(typeof(IntegrationTest).Assembly.Location);
            var assemblyName = "AssemblyToProcess.dll";
            var testAssemblyName = "AssemblyToProcess2.dll";
            beforeAssemblyPath = Path.Combine(currentAssemblyPath, @"..\..\..\AssemblyToProcess\bin\Debug", assemblyName);
#if (!DEBUG)
        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif

            afterAssemblyPath = Path.Combine(currentAssemblyPath, testAssemblyName);
            File.Copy(beforeAssemblyPath, afterAssemblyPath, true);

            SetEnvironmentVariables();

            var moduleDefinition = ModuleDefinition.ReadModule(beforeAssemblyPath);
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
            };

            weavingTask.Execute();
            moduleDefinition.Write(afterAssemblyPath);

            moduleDefinition = ModuleDefinition.ReadModule(afterAssemblyPath);

            weavingTask = new ModuleWeaver { ModuleDefinition = moduleDefinition };
            weavingTask.Execute();

            assembly = Assembly.LoadFile(afterAssemblyPath);
        }

        private void SetEnvironmentVariables()
        {
            foreach (var kvp in _envVariables)
            {
                Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
            }
        }

        [TestCase("AssemblyToProcess.ClassWithFields", "Field", "Field", true)]
        [TestCase("AssemblyToProcess.ClassWithFields", "PrivateField", "PrivateField", true)]
        [TestCase("AssemblyToProcess.ClassWithFields", "StaticReadonlyField", "StaticReadonlyField", false)]
        [TestCase("AssemblyToProcess.ClassWithFields", "PrivateStaticReadonlyField", "PrivateStaticReadonlyField", false)]
        [TestCase("AssemblyToProcess.ClassWithFields", "StaticField", "StaticField", false)]
        [TestCase("AssemblyToProcess.ClassWithFields", "PrivateStaticField", "PrivateStaticField", false)]
        [TestCase("AssemblyToProcess.ClassWithFields", "ConstField", "ConstField", false)]
        [TestCase("AssemblyToProcess.ClassWithFields", "PrivateConstField", "PrivateConstField", false)]
        public void ShouldBeModified(string className, string envVariable, string field, bool isInstance)
        {
            var instance = isInstance ? ConstructClass("AssemblyToProcess.ClassWithFields") : null;
            var fieldValue = GetFieldValue("AssemblyToProcess.ClassWithFields", field, instance);

            Assert.AreEqual(_envVariables[envVariable], fieldValue);
        }

        [TestCase("AssemblyToProcess.ClassWithFields", "NotModified", "NotModified", true)]
        [TestCase("AssemblyToProcess.ClassWithFields", "PrivateNotModified", "PrivateNotModified", true)]
        [TestCase("AssemblyToProcess.ClassWithFields", "StaticNotModified", "StaticNotModified", true)]
        [TestCase("AssemblyToProcess.ClassWithFields", "PrivateStaticNotModified", "PrivateStaticNotModified", true)]
        [TestCase("AssemblyToProcess.ClassWithFields", "StaticReadonlyNotModified", "StaticReadonlyNotModified", true)]
        [TestCase("AssemblyToProcess.ClassWithFields", "PrivateStaticReadonlyNotModified", "PrivateStaticReadonlyNotModified", true)]
        [TestCase("AssemblyToProcess.ClassWithFields", "ConstNotModified", "ConstNotModified", true)]
        [TestCase("AssemblyToProcess.ClassWithFields", "PrivateConstNotModified", "PrivateConstNotModified", true)]
        public void ShouldNotBeModified(string className, string field, string value, bool isInstance)
        {
            var instance = isInstance ? ConstructClass("AssemblyToProcess.ClassWithFields") : null;
            var fieldValue = GetFieldValue("AssemblyToProcess.ClassWithFields", field, instance);

            Assert.AreEqual(value, fieldValue);
        }

        [Test]
        public void ShouldSetIntOnInstance()
        {
            var instance = ConstructClass("AssemblyToProcess.ClassWithFields");
            var fieldValue = GetFieldValue("AssemblyToProcess.ClassWithFields", "InstanceInt", instance);

            Assert.AreEqual(99, fieldValue);
        }

        [Test]
        public void ShouldSetDoubleOnInstance()
        {
            var instance = ConstructClass("AssemblyToProcess.ClassWithFields");
            var fieldValue = GetFieldValue("AssemblyToProcess.ClassWithFields", "InstanceDouble", instance);

            Assert.AreEqual(99.99, fieldValue);
        }

        [TestCase("InstanceBoolWord")]
        [TestCase("InstanceBoolNumber")]
        public void ShouldSetBoolOnInstance(string field)
        {
            var instance = ConstructClass("AssemblyToProcess.ClassWithFields");
            var fieldValue = GetFieldValue("AssemblyToProcess.ClassWithFields", field, instance);

            Assert.AreEqual(false, fieldValue);
        }

        [Test]
        public void ShouldSetConstInt()
        {
            var fieldValue = GetFieldValue("AssemblyToProcess.ClassWithFields", "ConstInt");

            Assert.AreEqual(22, fieldValue);
        }

        [Test]
        public void ShouldSetConstDouble()
        {
            var fieldValue = GetFieldValue("AssemblyToProcess.ClassWithFields", "ConstDouble");

            Assert.AreEqual(22.22, fieldValue);
        }

        [TestCase("ConstBoolWord")]
        [TestCase("ConstBoolNumber")]
        public void ShouldSetConstBool(string field)
        {
            var fieldValue = GetFieldValue("AssemblyToProcess.ClassWithFields", field);

            Assert.AreEqual(false, fieldValue);
        }

        private dynamic ConstructClass(string className)
        {
            var type = assembly.GetType(className, true);
            dynamic test = Activator.CreateInstance(type);
            return test;
        }

        private object GetFieldValue(string className, string fieldName, object instance = null)
        {
            var type = assembly.GetType(className, true);
            var field = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public).First(x => x.Name == fieldName);

            return field.GetValue(instance);
        }
    }
}
