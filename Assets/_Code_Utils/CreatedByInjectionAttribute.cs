using System;
using JetBrains.Annotations;

namespace Hypnagogia.Utils {
    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class CreatedByInjectionAttribute : Attribute { }
}