using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Rocks;

namespace EnvVariableInect
{
    public class ModuleWeaver
    {
        public Action<string> LogInfo { get; set; }
        public Action<string> LogWarning { get; set; }
        public Action<string> LogDebug { get; set; }
        public ModuleDefinition ModuleDefinition { get; set; }

        public ModuleWeaver()
        {
            LogInfo = s => { };
            LogWarning = s => { };
            LogDebug = m => { };
        }

        public void Execute()
        {
            LogDebug("started");
            foreach (var typeDefinition in ModuleDefinition.Types.Where(x => x.HasFields))
            {
                Dictionary<string, string> variableDict = GetReplacementFieldsForType(typeDefinition);
                ReplaceFieldsForType(typeDefinition, variableDict);
            }
        }

        private Dictionary<string, string> GetReplacementFieldsForType(TypeDefinition typeDefinition)
        {
            LogDebug($"Get Fields to replace for type {typeDefinition.FullName}");
            var variableDict = new Dictionary<string, string>();
            var attributeType = typeof(BuildTimeEnvironmentVariableAttribute);
            var replaceFields = typeDefinition.Fields.Where(x => x.HasCustomAttributes && x.CustomAttributes.ContainsAttribute(attributeType));

            foreach (var field in replaceFields)
            {
                var attr = field.CustomAttributes.GetAttribute(attributeType);
                if (attr.ConstructorArguments.Count == 1 && !string.IsNullOrEmpty((string)attr.ConstructorArguments[0].Value))
                {
                    var variableName = (string)attr.ConstructorArguments[0].Value;
                    var variableValue = Environment.GetEnvironmentVariable(variableName);

                    if (field.Attributes.HasFlag(FieldAttributes.Literal))
                    {
                        LogDebug($"Const {field.Name} - {field.FieldType} - {variableValue}");
                        field.Constant = ConvertReplacement(field.FieldType, variableValue);
                    }
                    else if (variableValue != null)
                    {
                        variableDict[field.FullName] = variableValue;
                    }
                }
            }

            foreach (var kvp in variableDict)
            {
                LogDebug($"Variable Set : {kvp.Key} - {kvp.Value}");
            }

            return variableDict;
        }

        private void ReplaceFieldsForType(TypeDefinition typeDefinition, Dictionary<string, string> variableDict)
        {
            LogDebug($"Replace fields for type {typeDefinition.FullName} - {variableDict.Count} fields to replace");
            var constructors = typeDefinition.Methods.Where(x => x.IsConstructor && x.HasBody);

            foreach (var ctor in constructors)
            {
                var instructions = ctor.Body.Instructions;
                foreach (var instruction in instructions)
                {
                    var field = instruction.Operand as FieldDefinition;
                    if (field == null)
                    {
                        continue;
                    }

                    string value;
                    if (!variableDict.TryGetValue(field.FullName, out value)
                            || instruction.Previous == null)
                    {
                        continue;
                    }

                    ReplaceValue(instruction, value, field.FullName);
                }
            }
        }

        private void ReplaceValue(Mono.Cecil.Cil.Instruction instruction, string value, string fieldName)
        {
            var previous = instruction.Previous;

            if (previous.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_0 || previous.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_1)
            {
                previous.OpCode = value.AsBool() ? Mono.Cecil.Cil.OpCodes.Ldc_I4_1 : Mono.Cecil.Cil.OpCodes.Ldc_I4_0;
                return;
            }

            if (previous.OpCode == Mono.Cecil.Cil.OpCodes.Ldnull && value != null)
            {
                previous.OpCode = Mono.Cecil.Cil.OpCodes.Ldstr;
            }
            else if (previous.Operand == null)
            {
                return;
            }

            var operandType = previous.Operand != null ? previous.Operand.GetType() : typeof(string);
            object replacement = ConvertReplacement(operandType, value);

            if(replacement != null)
            {
                previous.Operand = replacement;
            }
            else
            {
                LogWarning($"Value to set field {fieldName} is null, check the desired environment variable is set");
            }
        }

        private object ConvertReplacement(TypeReference typeRef, string value)
        {
            var typeName = $"{typeRef.FullName}, {typeRef.Scope.Name}";
            LogDebug($"Attempting to get Type {typeName}");

            var type = Type.GetType(typeName);
            if(type == null)
            {
                // if pcl dll being used may actually be mscorlib and not System.Runtime.dll
                typeName = typeName.Replace("System.Runtime", "mscorlib");
                LogDebug($"Attempting to get Type {typeName}");
                type = type ?? Type.GetType(typeName);
            }

            return ConvertReplacement(type, value);
        }

        private object ConvertReplacement(Type type, string value)
        {
            LogDebug($"Convert replacement - {value} - to {type?.Name ?? "type null"}");

            if ((value == null && type.IsClass) || type == typeof(string)) return value;

            if (type == typeof(bool)) return value.AsBool();

            return Convert.ChangeType(value, type);
        }
    }
}
