using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Xrm.Sdk;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Rappen.XRM.RappSackDV
{
    public static class RemoteExecutionContextConverter
    {
        private static DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings
        {
            UseSimpleDictionaryFormat = false,                  // keep key/value arrays
            EmitTypeInformation = EmitTypeInformation.Always,   // include "__type" when needed
            KnownTypes = knownTypes
        };

        private static IEnumerable<Type> knownTypes => new[]
        {
            typeof(Entity),
            typeof(EntityReference),
            typeof(EntityCollection),
            typeof(OptionSetValue),
            typeof(Money),
            typeof(ParameterCollection),
            typeof(KeyAttributeCollection),
            typeof(FormattedValueCollection),
            typeof(RelatedEntityCollection)
        };

        public static RemoteExecutionContext Deserialize(HttpRequestData req) =>
            Deserialize(req.ReadAsString() ?? string.Empty);

        public static RemoteExecutionContext Deserialize(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return null;
            var serializer = new DataContractJsonSerializer(typeof(RemoteExecutionContext), settings);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(body));
            var result = (RemoteExecutionContext?)serializer.ReadObject(ms);
            XrmDateNormalization.NormalizeDates(result);
            return result;
        }

        public static string Serialize(this RemoteExecutionContext ctx)
        {
            if (ctx == null) return string.Empty;
            var serializer = new DataContractJsonSerializer(typeof(RemoteExecutionContext), settings);
            using var ms = new MemoryStream();
            serializer.WriteObject(ms, ctx);
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}