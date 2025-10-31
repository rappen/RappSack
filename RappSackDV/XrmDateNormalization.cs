using Microsoft.Xrm.Sdk;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Rappen.XRM.RappSackDV
{
    internal static class XrmDateNormalization
    {
        // Matches /Date(1761850730000)/ or /Date(1761850730000+0200)/ or /Date(1761850730000-0500)/
        private static readonly Regex MsJsonDate = new(@"^/Date\((?<ms>-?\d+)(?<offset>[+-]\d{4})?\)/$", RegexOptions.Compiled);

        public static void NormalizeDates(RemoteExecutionContext? ctx)
        {
            if (ctx == null) return;
            NormalizeParameterCollection(ctx?.InputParameters);
            NormalizeParameterCollection(ctx?.OutputParameters);
            NormalizeParameterCollection(ctx?.SharedVariables);
            NormalizeEntityImageCollection(ctx?.PreEntityImages);
            NormalizeEntityImageCollection(ctx?.PostEntityImages);
            NormalizeDates(ctx?.ParentContext);
        }

        private static void NormalizeParameterCollection(ParameterCollection? pc)
        {
            pc?.Keys?.ToList()?.ForEach(key => pc[key] = NormalizeValue(pc[key]));
        }

        private static void NormalizeEntity(Entity? entity)
        {
            entity?.Attributes?.Keys?.ToList()?.ForEach(key => entity[key] = NormalizeValue(entity[key]));
            entity?.RelatedEntities?.Keys?.ToList()?.ForEach(key => entity.RelatedEntities[key].Entities?.ToList().ForEach(e => NormalizeEntity(e)));
        }

        private static void NormalizeEntityImageCollection(EntityImageCollection? images)
        {
            images?.Keys?.ToList()?.ForEach(key => NormalizeEntity(images[key]));
        }

        private static object? NormalizeValue(object? value)
        {
            if (value == null) return null;

            // Convert MS JSON date string -> DateTime (UTC)
            if (value is string s)
            {
                var dt = TryParseMsJsonDate(s);
                if (dt.HasValue) return dt.Value;
                return s;
            }

            if (value is Entity entity)
            {
                NormalizeEntity(entity);
                return entity;
            }

            if (value is EntityCollection ec && ec.Entities != null)
            {
                ec.Entities.ToList().ForEach(NormalizeEntity);
                return ec;
            }

            // Handle arrays/lists potentially containing entities or date strings
            if (value is IEnumerable enumerable && value is not string)
            {
                foreach (var item in enumerable)
                {
                    if (item is Entity e)
                        NormalizeEntity(e);
                    else if (item is string s2)
                    {
                        // in-place replacement in arrays is not straightforward; keep as-is
                        // arrays of primitive values rarely hold dates in this payload shape
                        _ = TryParseMsJsonDate(s2);
                    }
                }
                return value;
            }
            return value;
        }

        private static DateTime? TryParseMsJsonDate(string input)
        {
            var m = MsJsonDate.Match(input);
            if (!m.Success) return null;

            if (!long.TryParse(m.Groups["ms"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ms))
                return null;

            var dto = DateTimeOffset.FromUnixTimeMilliseconds(ms);

            // Optional timezone offset like +0200 or -0530
            var offsetStr = m.Groups["offset"].Success ? m.Groups["offset"].Value : null;
            if (!string.IsNullOrEmpty(offsetStr))
            {
                var sign = offsetStr[0] == '-' ? -1 : 1;
                var hours = int.Parse(offsetStr.AsSpan(1, 2));
                var minutes = int.Parse(offsetStr.AsSpan(3, 2));
                var offset = new TimeSpan(sign * hours, sign * minutes, 0);
                dto = new DateTimeOffset(dto.UtcDateTime).ToOffset(offset);
            }

            // Return UTC DateTime
            return dto.UtcDateTime;
        }
    }
}