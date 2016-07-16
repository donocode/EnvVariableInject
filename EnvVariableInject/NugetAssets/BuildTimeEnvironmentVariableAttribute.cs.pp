using System;

namespace $rootnamespace$
{
    [AttributeUsage(AttributeTargets.Field)]
    public class BuildTimeEnvironmentVariableAttribute : Attribute
    {
        private string environmentVariable;

        public BuildTimeEnvironmentVariableAttribute(string environmentVariable)
        {
            this.environmentVariable = environmentVariable;
        }

        public string GetEvironmentVariable()
        {
            return environmentVariable;
        }
    }
}