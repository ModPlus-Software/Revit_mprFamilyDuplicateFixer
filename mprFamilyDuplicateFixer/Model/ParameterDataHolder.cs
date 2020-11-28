namespace mprFamilyDuplicateFixer.Model
{
    using System;
    using Autodesk.Revit.DB;

    /// <summary>
    /// Вспомогательный объект хранения значений параметров
    /// </summary>
    public class ParameterDataHolder
    {
        public ParameterDataHolder(Parameter parameter)
        {
            Name = parameter.Definition.Name;
            StorageType = parameter.StorageType;
            switch (StorageType)
            {
                case StorageType.Double:
                    DoubleValue = parameter.AsDouble();
                    break;
                case StorageType.Integer:
                    IntegerValue = parameter.AsInteger();
                    break;
                case StorageType.String:
                    StringValue = parameter.AsString() ?? string.Empty;
                    break;
                case StorageType.ElementId:
                    ElementIdValue = parameter.AsElementId();
                    break;
            }
        }

        public string Name { get; }

        public StorageType StorageType { get; }

        public double DoubleValue { get; }

        public int IntegerValue { get; }

        public string StringValue { get; }

        public ElementId ElementIdValue { get; }

        /// <summary>
        /// Является ли параметр допустимым для обработки копирования
        /// </summary>
        /// <param name="parameter">Проверяемый параметр</param>
        public static bool IsAllowableParameter(Parameter parameter)
        {
            if (parameter.IsReadOnly ||
                parameter.StorageType == StorageType.None)
                return false;
            if (parameter.Definition is InternalDefinition internalDefinition)
            {
                if (internalDefinition.BuiltInParameter == BuiltInParameter.ELEM_TYPE_PARAM ||
                    internalDefinition.BuiltInParameter == BuiltInParameter.ELEM_FAMILY_PARAM ||
                    internalDefinition.BuiltInParameter == BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM)
                    return false;
            }

            return true;
        }

        public void SetTo(Parameter parameter)
        {
            switch (StorageType)
            {
                case StorageType.Double:
                    if (Math.Abs(parameter.AsDouble() - DoubleValue) < 0.0001)
                        return;
                    parameter.Set(DoubleValue);
                    break;
                case StorageType.Integer:
                    if (parameter.AsInteger() == IntegerValue)
                        return;
                    parameter.Set(IntegerValue);
                    break;
                case StorageType.String:
                    if (parameter.AsString() == StringValue)
                        return;
                    parameter.Set(StringValue);
                    break;
                case StorageType.ElementId:
                    if (parameter.AsElementId() == ElementIdValue)
                        return;
                    parameter.Set(ElementIdValue);
                    break;
            }
        }
    }
}