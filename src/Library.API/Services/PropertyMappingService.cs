using System;
using System.Collections.Generic;
using System.Linq;
using Library.API.Entities;
using Library.API.Models;

namespace Library.API.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private readonly Dictionary<string, PropertyMappingValue> authorPropertyMapping = 
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                {"Id", new PropertyMappingValue(new List<string> {"Id"}) },
                {"Genre", new PropertyMappingValue(new List<string> {"Genre"}) },
                {"Age", new PropertyMappingValue(new List<string> {"DateOfBirth"}, true) },
                {"Name", new PropertyMappingValue(new List<string> {"FirstName", "LastName"}) }
            };

        private readonly IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(authorPropertyMapping));
        }
        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchingMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            var mappings = matchingMapping as IList<PropertyMapping<TSource, TDestination>> ?? matchingMapping.ToList();

            if (mappings.Count == 1)
            {
                return mappings.First().MappingDictionary;
            }

            throw new Exception($"Cannot find exact property mappings for {typeof(TSource)}");
        }

        public bool ValidMappingExistFor<TSource, TDestination>(string fields)
        {
            var propertyMappinmg = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                var trimmedField = field.Trim();
                var indexOfFirstSpace = trimmedField.IndexOf(" ", StringComparison.Ordinal);
                var propertyName = indexOfFirstSpace == -1
                    ? trimmedField
                    : trimmedField.Remove(indexOfFirstSpace);

                if (!propertyMappinmg.ContainsKey(propertyName))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
