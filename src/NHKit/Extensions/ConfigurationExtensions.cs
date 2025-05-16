using NHibernate.Event;
using NHKit.Callbacks;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace NHibernate.Cfg
{
    public static class ConfigurationExtensions
    {
        public static Configuration EnableEntityCallbacks(this Configuration cfg)
        {
            const string integratedKey = "NHKit.Callbacks.Integrated";

            if (cfg.GetProperty(integratedKey) != null) {
                return cfg;
            }

            // At the moment only persistent entity classes are supported, but a future
            // enhancement may allow finding callback handlers by dependency injection.
            var callbackHandlers = cfg.ClassMappings
                .Where(m => m.MappedClass != null)
                .Select(m => m.MappedClass)
                .Distinct()
                .SelectMany(t => CallbackHandler.Create(t, cfg.ClassMappings))
                .ToList();

            if (callbackHandlers.Count == 0) {
                // nothing to do
                return cfg;
            }

            var listeners = new[] { new CallbackEventListener(callbackHandlers) };
            // ReSharper disable CoVariantArrayConversion
            cfg.AppendListeners(ListenerType.PostInsert, listeners);
            cfg.AppendListeners(ListenerType.PostUpdate, listeners);
            cfg.AppendListeners(ListenerType.PostDelete, listeners);
            // ReSharper restore CoVariantArrayConversion

            cfg.SetProperty(integratedKey, "1");

            return cfg;
        }
    }
}
