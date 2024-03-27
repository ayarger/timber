using System;

namespace MoonSharp.Interpreter.Interop
{
	public interface IProxyFactory
	{
		object CreateProxyObject(object o);
		Type TargetType { get; }
		Type ProxyType { get; }
	}
	public interface IProxyFactory<TProxy, TTarget> : IProxyFactory
		where TProxy : class
		where TTarget : class
	{
		TProxy CreateProxyObject(TTarget target);
	}

}
