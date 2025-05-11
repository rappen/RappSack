using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Rappen.Dataverse.Canary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rappen.XRM.RappSack
{
    public abstract class RappSackPlugin : RappSackCore, IPlugin, ITracingService
    {
        #region Easier to get relevant from IPlugin

        public IPluginExecutionContext5 Context { get; private set; }
        public ContextEntity ContextEntity { get; private set; }
        public ContextEntityCollection ContextEntityCollection { get; private set; }
        public Entity Target => ContextEntity?[ContextEntityType.Target];

        #endregion Easier to get relevant from IPlugin

        #region Execute plugin as

        public virtual ServiceAs ServiceAs { get; } = ServiceAs.User;
        public virtual string ExecuterEnvVar { get; } = string.Empty;

        #endregion Execute plugin as

        #region Need details from the plugin, override in plugins if needed

        /// <summary>If true, throw if not matching all needs, default: False</summary>
        public virtual bool NeedThrowIfNotMatch { get; } = false;
        /// <summary>Message name to check, default: accept all messages</summary>
        public virtual string NeedMessage { get; } = string.Empty;
        /// <summary>Stage to check, default: accept all stages</summary>
        public virtual int NeedStage { get; } = -1;
        /// <summary>Entity name to check, default: accept all entities</summary>
        public virtual string NeedEntity { get; } = string.Empty;
        /// <summary>Attributes to check, default: don't care about attributes</summary>
        public virtual string[] NeedAttributes { get; } = new string[0];
        /// <summary>Define if pre image is needed, default: False</summary>
        public virtual bool NeedPreImage { get; } = false;
        /// <summary>Define if post image is needed, default: False</summary>
        public virtual bool NeedPostImage { get; } = false;
        /// <summary>Multiple Messages are accepted, default: no validate</summary>
        public virtual string[] NeedMessages { get; private set; } = new string[0];
        /// <summary>Multiple Stages are accepted, default: no validate</summary>
        public virtual int[] NeedStages { get; private set; } = new int[0];
        /// <summary>Multiple Entities are accepted, default: no validate</summary>
        public virtual string[] NeedEntities { get; private set; } = new string[0];

        #endregion Need details from the plugin, override in plugins if needed

        #region Abstract methods

        public abstract void Execute();

        #endregion Abstract methods

        #region Implemented Public methods

        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                serviceProvider.TraceContext(false, true, false, false, true);
                Context = serviceProvider.Get<IPluginExecutionContext5>();
                ContextEntity = new ContextEntity(Context);
                ContextEntityCollection = new ContextEntityCollection(Context);
                SetTracer(new RappSackPluginTracer(serviceProvider));
                if (!NeedsVerified())
                {
                    return;
                }
                Guid? svcuser = null;
                switch (ServiceAs)
                {
                    case ServiceAs.Initiating:
                        svcuser = Context.InitiatingUserId;
                        break;

                    case ServiceAs.User:
                        svcuser = Context.UserId;
                        break;

                    case ServiceAs.Specific:
                        if (string.IsNullOrWhiteSpace(ExecuterEnvVar))
                        {
                            throw new InvalidPluginExecutionException("ServiceAs is Specific, but ExecuterEnvVar is not set");
                        }
                        SetService(serviceProvider, null);
                        svcuser = GetEnvironmentVariableValue<Guid>(ExecuterEnvVar, true);
                        break;
                }
                SetService(serviceProvider, svcuser);
                var starttime = DateTime.Now;
                TraceRaw($"Execution {CallerMethodName() ?? "RappSackPlugin"} at {starttime:yyyy-MM-dd HH:mm:ss.fff}");
                Execute();
                TraceRaw($"Exiting after {(DateTime.Now - starttime).ToSmartString()}");
            }
            catch (Exception ex)
            {
                serviceProvider.TraceError(ex);
                if (ex is InvalidPluginExecutionException)
                {
                    throw;
                }
                throw new InvalidPluginExecutionException($"Unhandled {ex.GetType()} in RappSackPlugin: {ex.Message}", ex);
            }
        }

        public void Trace(string format, params object[] args) => base.Trace(string.Format(format, args));

        #endregion Implemented Public methods

        #region Private methods

        private bool NeedsVerified()
        {
            if (!string.IsNullOrEmpty(NeedMessage))
            {
                NeedMessages = new[] { NeedMessage };
            }
            if (NeedStage > -1)
            {
                NeedStages = new[] { NeedStage };
            }
            if (!string.IsNullOrEmpty(NeedEntity))
            {
                NeedEntities = new[] { NeedEntity };
            }

            var needtexts = new List<string>();
            if (NeedMessages?.Length > 0 && !NeedMessages.Any(m => m.Equals(Context.MessageName, StringComparison.OrdinalIgnoreCase)))
            {
                needtexts.Add($"Wrong message: {Context.MessageName}, need: {string.Join(", ", NeedMessages)}");
            }
            if (NeedStages?.Length > 0 && !NeedStages.Any(s => s == Context.Stage))
            {
                needtexts.Add($"Wrong stage: {Context.Stage}, need: {string.Join(", ", NeedStages)}");
            }
            if (NeedEntities?.Length > 0 && !NeedEntities.Any(e => e.Equals(Context.PrimaryEntityName, StringComparison.OrdinalIgnoreCase)))
            {
                needtexts.Add($"Wrong entity: {Context.PrimaryEntityName}, need: {string.Join(", ", NeedEntities)}");
            }
            if (NeedAttributes?.Length > 0 && !NeedAttributes.Any(na => Target.Attributes.ContainsKey(na)))
            {
                needtexts.Add($"Need any attributes: {string.Join(", ", NeedAttributes)}");
            }
            if (NeedPreImage && ContextEntity[ContextEntityType.PreImage] == null)
            {
                needtexts.Add($"Missing pre image");
            }
            if (NeedPostImage && ContextEntity[ContextEntityType.PostImage] == null)
            {
                needtexts.Add($"Missing post image");
            }
            if (needtexts.Count == 0)
            {
                return true;
            }
            var needtext = string.Join(Environment.NewLine, needtexts);
            if (NeedThrowIfNotMatch)
            {
                throw new InvalidPluginExecutionException(needtext);
            }
            Trace(needtext);
            return false;
        }

        private void SetService(IServiceProvider provider, Guid? userid) => SetService(provider.Get<IOrganizationServiceFactory>(), userid);

        #endregion Private methods
    }

    internal class RappSackPluginTracer : RappSackTracerCore
    {
        private readonly ITracingService tracing;

        public RappSackPluginTracer(IServiceProvider provider) : base(TraceTiming.ElapsedSinceLast)
        {
            if (!(provider.GetService(typeof(ITracingService)) is ITracingService tracingService))
            {
                throw new InvalidPluginExecutionException("Failed to get tracing service");
            }
            tracing = tracingService;
        }

        protected override void TraceInternal(string message, string timestamp, int indent, TraceLevel level = TraceLevel.Information)
        {
            if (tracing == null)
            {
                throw new InvalidPluginExecutionException("Tracer is not initialized");
            }
            tracing.Trace(timestamp + new string(' ', indent * 2) + message);
        }
    }

    public enum ServiceAs
    {
        User,
        Initiating,
        System,
        Specific
    }
}