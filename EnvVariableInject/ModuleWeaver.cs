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
        public ModuleDefinition ModuleDefinition { get; set; }

        public ModuleWeaver()
        {
            LogInfo = s => { };
        }

        public void Execute()
        {
            foreach (var typeDefinition in ModuleDefinition.Types.Where(x => x.HasFields))
            {
                Dictionary<string, string> variableDict = GetReplacementFieldsForType(typeDefinition);
                ReplaceFieldsForType(typeDefinition, variableDict);
            }
        }

        private static Dictionary<string, string> GetReplacementFieldsForType(TypeDefinition typeDefinition)
        {
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
                        field.Constant = ConvertReplacement(field.FieldType, variableValue);
                    }
                    else if (variableValue != null)
                    {
                        variableDict[field.FullName] = variableValue;
                    }
                }
            }

            return variableDict;
        }

        private static void ReplaceFieldsForType(TypeDefinition typeDefinition, Dictionary<string, string> variableDict)
        {
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

                    ReplaceValue(instruction, value);
                }
            }
        }

        private static void ReplaceValue(Mono.Cecil.Cil.Instruction instruction, string value)
        {
            var previous = instruction.Previous;

            if (previous.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_0 || previous.OpCode == Mono.Cecil.Cil.OpCodes.Ldc_I4_1)
            {
                previous.OpCode = value.AsBool() ? Mono.Cecil.Cil.OpCodes.Ldc_I4_1 : Mono.Cecil.Cil.OpCodes.Ldc_I4_0;
                return;
            }

            if(previous.Operand == null)
            {
                return;
            }

            var operandType = previous.Operand.GetType();
            object replacement = ConvertReplacement(operandType, value);

            previous.Operand = replacement;
        }

        private static object ConvertReplacement(TypeReference typeRef, string value)
        {
            var typeName = $"{typeRef.FullName}, {typeRef.Scope.Name}";
            var type = Type.GetType(typeName);

            return ConvertReplacement(type, value);
        }

        private static object ConvertReplacement(Type type, string value)
        {
            return type != typeof(string) ? type != typeof(bool) ? Convert.ChangeType(value, type) : value.AsBool() : value;
        }
    }
}
