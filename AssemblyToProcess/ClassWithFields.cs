using EnvVariableInect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyToProcess
{
    public class ClassWithFields
    {
        [BuildTimeEnvironmentVariable("ConstField")]
        public const string ConstField = "test";

        [BuildTimeEnvironmentVariable("PrivateConstField")]
        private const string PrivateConstField = "test";

        [BuildTimeEnvironmentVariable("StaticReadonlyField")]
        public static readonly string StaticReadonlyField = "test";

        [BuildTimeEnvironmentVariable("PrivateStaticReadonlyField")]
        private static readonly string PrivateStaticReadonlyField = "test";

        [BuildTimeEnvironmentVariable("StaticField")]
        public static string StaticField = "test";

        [BuildTimeEnvironmentVariable("PrivateStaticField")]
        public static string PrivateStaticField = "test";

        [BuildTimeEnvironmentVariable("Field")]
        public string Field = "test";

        [BuildTimeEnvironmentVariable("PrivateField")]
        public string PrivateField = "test";

        public string NotModified = "NotModified";

        private string PrivateNotModified = "PrivateNotModified";

        public static string StaticNotModified = "StaticNotModified";

        private static string PrivateStaticNotModified = "PrivateStaticNotModified";

        public static readonly string StaticReadonlyNotModified = "StaticReadonlyNotModified";

        private static readonly string PrivateStaticReadonlyNotModified = "PrivateStaticReadonlyNotModified";

        public const string ConstNotModified = "ConstNotModified";

        private const string PrivateConstNotModified = "PrivateConstNotModified";

        [BuildTimeEnvironmentVariable("InstanceInt")]
        public int InstanceInt = 1000;

        [BuildTimeEnvironmentVariable("InstanceDouble")]
        public double InstanceDouble = 1.11;

        [BuildTimeEnvironmentVariable("InstanceBoolWord")]
        public bool InstanceBoolWord = true;

        [BuildTimeEnvironmentVariable("InstanceBoolNumber")]
        public bool InstanceBoolNumber = true;

        [BuildTimeEnvironmentVariable("ConstInt")]
        public const int ConstInt = 1000;

        [BuildTimeEnvironmentVariable("ConstDouble")]
        public const double ConstDouble = 1.11;

        [BuildTimeEnvironmentVariable("ConstBoolWord")]
        public const bool ConstBoolWord = true;

        [BuildTimeEnvironmentVariable("ConstBoolNumber")]
        public const bool ConstBoolNumber = true;

        public ClassWithFields()
        {

        }

        public ClassWithFields(string test)
        {

        }
    }
}
