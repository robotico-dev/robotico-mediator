namespace Robotico.Mediator;

/// <summary>
/// When applied at assembly scope, marks this assembly as contributing source-generated mediator handlers.
/// </summary>
/// <remarks>
/// <para>If <em>any</em> referenced assembly in the compilation defines this attribute, handler discovery is restricted to types whose containing assembly also defines this attribute. If no assembly defines it, all handlers are discovered (backward-compatible default).</para>
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class RoboticoMediatorHandlersAssemblyAttribute : Attribute
{
}
