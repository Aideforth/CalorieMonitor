using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CalorieMonitor.Logic.Implementations
{
    public class SearchFilterHandler : ISearchFilterHandler
    {
        private readonly Dictionary<string, FilterOperation> filterOperations = Enum.GetValues(typeof(FilterOperation))
            .Cast<FilterOperation>()
            .Where(v => v != FilterOperation.Field)
            .ToDictionary(v => v.ToString().ToLower());
        private readonly Dictionary<string, FilterComparison> filterComparisons = new Dictionary<string, FilterComparison>
        {
            {"eq", FilterComparison.Equals },
            {"gt", FilterComparison.GreaterThan },
            {"lt", FilterComparison.LessThan },
            {"like", FilterComparison.Like },
            {"ne", FilterComparison.NotEquals }
        };

        //evaluate once then store here
        static Dictionary<Type, Dictionary<string, KeyValuePair<string, Type>>> entityPropNameAndTypes;

        public IFilter Parse(string filter, Type type)
        {
            if (string.IsNullOrWhiteSpace(filter)) return null;
            //validate that string has enough brackets
            ValidateBrackets(filter);
            return ParseFilter(filter, type);
        }

        private void ValidateBrackets(string filter)
        {
            int leftParag = 0;
            int rightParag = 0;
            bool isWithinQuotes = false;

            for (int i = 0; i < filter.Length; i++)
            {
                if (filter[i] == '\'') isWithinQuotes = !isWithinQuotes;
                if (isWithinQuotes) continue;

                if (filter[i] == '(') leftParag++;
                if (filter[i] == ')') rightParag++;

                if (rightParag > leftParag) break;
            }

            if (rightParag != leftParag)
            {
                string missing = rightParag > leftParag ? "(" : ")";
                throw new FormatException($"Invalid filter field {missing} missing");
            }
        }

        private IFilter ParseFilter(string filter, Type type)
        {
            int leftParag = 0;
            int rightParag = 0;
            bool isField = true;
            bool isWithinQuotes = false;
            List<string> filters = new List<string>();
            List<char> currentItems = new List<char>();

            for (int i = 0; i < filter.Length; i++)
            {
                switch (filter[i])
                {
                    case '\'':
                        currentItems.Add(filter[i]);
                        isWithinQuotes = !isWithinQuotes;
                        continue;
                    case ' ':
                        if (leftParag == 0 && !isWithinQuotes)
                        {
                            currentItems = ResetValues(filters, currentItems);
                        }
                        else
                        {
                            currentItems.Add(filter[i]);
                        }
                        continue;
                    case '(':
                        if (!isWithinQuotes)
                        {
                            if (leftParag == 0)
                            {
                                currentItems = ResetValues(filters, currentItems);
                            }
                            isField = false;
                            leftParag++;
                        }

                        currentItems.Add(filter[i]);
                        continue;
                    case ')':
                        currentItems.Add(filter[i]);
                        if (isWithinQuotes) continue;

                        rightParag++;
                        if (rightParag > 0 && rightParag == leftParag)
                        {
                            currentItems = ResetValues(filters, currentItems);
                            leftParag = 0;
                            rightParag = 0;
                        }
                        continue;
                    default:
                        currentItems.Add(filter[i]);
                        continue;
                }
            }
            ResetValues(filters, currentItems);

            return isField ? ProcessField(filters, filter, type) : ProcessStrings(filters, type);
        }

        private IFilter ProcessField(List<string> filters, string filterString, Type type)
        {
            if (filters?.Count != 3)
            {
                throw new FormatException($"Invalid filter field syntax ({filterString})");
            }
            if (!filterComparisons.TryGetValue(filters[1].ToLower(), out FilterComparison comparison))
            {
                throw new FormatException($"Invalid filter syntax at '{filters[1]}'");
            }
            if (!ValidatePropertyName(type, filters[0]))
            {
                throw new FormatException($"Invalid filter field {filters[0]}");
            }
            var fieldInfo = GetPropertyInfo(type, filters[0]);
            
            if (!TryParseValue(fieldInfo.Value, filters[2], out object value))
            {
                throw new FormatException($"Invalid value {filters[2]} for {filters[0]}");
            }
            if (!ValidateTypeComparison(fieldInfo.Value, comparison))
            {
                throw new FormatException($"Invalid filter comparator {filters[1]} for {filters[0]}");
            }
            string fieldName = fieldInfo.Key;
            //grant all MealEntry searches an allias
            if (type == typeof(MealEntry) && !fieldInfo.Key.Contains('.'))
            {
                fieldName = $"{type.Name}.{fieldInfo.Key}";
            }
            return new FieldFilter(fieldName, comparison, value);
        }

        private IFilter ProcessStrings(List<string> filters, Type type)
        {
            bool lastParsedWasFilter = false;
            IFilter currentFilter = null;

            for (int i = 0; i < filters.Count; i++)
            {
                if (filters[i].Length > 2 && IsSubFilterString(filters[i]))
                {
                    if (lastParsedWasFilter)
                    {
                        throw new FormatException($"Invalid filter comparator 'OR' or 'AND' missing (^) in '{filters[i - 1]} ^ {filters[i]}'");
                    }
                    var newFilter = ParseFilter(filters[i].Substring(1, filters[i].Length - 2), type);
                    newFilter.HasBrackets = true;
                    if (i == 0)
                    {
                        currentFilter = newFilter;
                    }
                    else
                    {
                        currentFilter.RightHandFilter = newFilter;
                    }
                    lastParsedWasFilter = true;
                }
                else if (filterOperations.TryGetValue(filters[i].ToLower(), out FilterOperation operation))
                {
                    if (!lastParsedWasFilter || i == filters.Count - 1)
                    {
                        throw new FormatException($"Invalid filter syntax at '{filters[i]}'");
                    }
                    currentFilter = new SearchFilter() { LeftHandFilter = currentFilter, Operation = operation };
                    lastParsedWasFilter = false;
                }
                else
                {
                    throw new FormatException($"Invalid filter syntax at '{filters[i]}'");
                }
            }
            return currentFilter;
        }
        private bool IsSubFilterString(string filter)
        {
            return filter[0] == '(' && filter[filter.Length - 1] == ')';
        }

        private List<char> ResetValues(List<string> filters, List<char> currentItems)
        {
            if (currentItems.Count == 0) return currentItems;
            filters.Add(new string(currentItems.ToArray()));
            currentItems = new List<char>();
            return currentItems;
        }

        private bool TryParseValue(Type fieldType, string valueString, out object value)
        {
            if (valueString.Length > 1)
            {
                if (valueString[0] == '\'' && valueString[valueString.Length - 1] == '\'')
                {
                    valueString = valueString.Substring(1, valueString.Length - 2);
                }
            }

            if (fieldType == typeof(string))
            {
                value = valueString;
                return true;
            }
            bool result;
            if (fieldType == typeof(int))
            {
                result = int.TryParse(valueString, out int intValue);
                value = intValue;
                return result;
            }
            if (fieldType == typeof(long))
            {
                result = long.TryParse(valueString, out long longValue);
                value = longValue;
                return result;
            }
            if (fieldType == typeof(double))
            {
                result = double.TryParse(valueString, out double doubleValue);
                value = doubleValue;
                return result;
            }
            if (fieldType == typeof(DateTime))
            {
                result = DateTime.TryParse(valueString, out DateTime dtValue);
                value = dtValue;
                return result;
            }
            if (fieldType.IsEnum)
            {
                return Enum.TryParse(fieldType, valueString, out value);
            }
            throw new NotImplementedException();
        }

        private bool ValidateTypeComparison(Type fieldType, FilterComparison comparison)
        {
            if (fieldType == typeof(string))
            {
                return comparison != FilterComparison.GreaterThan && comparison != FilterComparison.LessThan;
            }
            if (fieldType == typeof(int) || fieldType == typeof(long) || fieldType == typeof(double) || fieldType == typeof(DateTime))
            {
                return comparison != FilterComparison.Like;
            }
            if (fieldType.IsEnum)
            {
                return comparison == FilterComparison.Equals || comparison == FilterComparison.NotEquals;
            }
            throw new NotImplementedException();
        }

        private bool ValidatePropertyName(Type type, string name)
        {
            Dictionary<string, KeyValuePair<string, Type>> typeNames = new Dictionary<string, KeyValuePair<string, Type>>();
            if (!entityPropNameAndTypes?.TryGetValue(type, out typeNames) ?? true)
            {
                typeNames = GenerateValidFilterInfoForType(type);
            }
            return typeNames.ContainsKey(name.ToLower());
        }
        private KeyValuePair<string, Type> GetPropertyInfo(Type type, string name)
        {
            Dictionary<string, KeyValuePair<string, Type>> typeNames = new Dictionary<string, KeyValuePair<string, Type>>();
            if (!entityPropNameAndTypes?.TryGetValue(type, out typeNames) ?? true)
            {
                typeNames = GenerateValidFilterInfoForType(type);
            }
            return typeNames[name.ToLower()];
        }

        private Dictionary<string, KeyValuePair<string, Type>> GenerateValidFilterInfoForType(Type type)
        {
            if (entityPropNameAndTypes == null)
            {
                entityPropNameAndTypes = new Dictionary<Type, Dictionary<string, KeyValuePair<string, Type>>>();
            }
            Dictionary<string, KeyValuePair<string, Type>> typeNames = GetColumnNamesAndTypes(type).ToDictionary(c => c.Key.ToLower());
            entityPropNameAndTypes.Add(type, typeNames);
            return typeNames;
        }

        private List<KeyValuePair<string, Type>> GetColumnNamesAndTypes(Type type, string prependString = null)
        {
            List<string> ignoreColumns = new List<string> { "DateUpdated", "Password", "CaloriesStatus" };
            List<KeyValuePair<string, Type>> columnNames = new List<KeyValuePair<string, Type>>();

            foreach (var property in type.GetProperties())
            {
                if (ignoreColumns.Contains(property.Name)) continue;
                if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(string) || property.PropertyType.IsEnum)
                {
                    string name = prependString == null ? property.Name : $"{prependString}.{property.Name}";
                    columnNames.Add(new KeyValuePair<string, Type>(name, property.PropertyType));
                }
                else
                {
                    var newNames = GetColumnNamesAndTypes(property.PropertyType, property.Name);
                    columnNames.AddRange(newNames);
                }
            }
            return columnNames;
        }
    }
}
