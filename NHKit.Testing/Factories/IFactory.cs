using JetBrains.Annotations;
using NHibernate;

namespace NHKit.Testing.Factories;

public interface IFactory<out T>
{
    T Get([NotNull] ISession session);
}
