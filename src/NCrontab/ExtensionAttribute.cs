namespace System.Runtime.CompilerServices
{
    /// <remarks>
    /// This attribute allows us to define extension methods without 
    /// requiring .NET Framework 3.5. For more information, see the section,
    /// <a href="http://msdn.microsoft.com/en-us/magazine/cc163317.aspx#S7">Extension Methods in .NET Framework 2.0 Apps</a>,
    /// of <a href="http://msdn.microsoft.com/en-us/magazine/cc163317.aspx">Basic Instincts: Extension Methods</a>
    /// column in <a href="http://msdn.microsoft.com/msdnmag/">MSDN Magazine</a>, 
    /// issue <a href="http://msdn.microsoft.com/en-us/magazine/cc135410.aspx">Nov 2007</a>.
    /// </remarks>

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    internal sealed class ExtensionAttribute : Attribute { }
}
