using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvVariableInect
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
