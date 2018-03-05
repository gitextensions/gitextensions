/*
 * Copyright 2007-2012 JetBrains s.r.o.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace JetBrains.Annotations
{
    /// <summary>
    /// Indicates that marked element should be localized or not.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class LocalizationRequiredAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizationRequiredAttribute"/> class.
        /// </summary>
        /// <param name="required"><c>true</c> if a element should be localized; otherwise, <c>false</c>.</param>
        public LocalizationRequiredAttribute(bool required)
        {
            Required = required;
        }

        /// <summary>
        /// Gets a value indicating whether a element should be localized.
        /// <value><c>true</c> if a element should be localized; otherwise, <c>false</c>.</value>
        /// </summary>
        [UsedImplicitly]
        public bool Required { get; private set; }

        /// <summary>
        /// Returns whether the value of the given object is equal to the current <see cref="LocalizationRequiredAttribute"/>.
        /// </summary>
        /// <param name="obj">The object to test the value equality of. </param>
        /// <returns>
        /// <c>true</c> if the value of the given object is equal to that of the current; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var attribute = obj as LocalizationRequiredAttribute;
            return attribute != null && attribute.Required == Required;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current <see cref="LocalizationRequiredAttribute"/>.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Indicates that marked method builds string by format pattern and (optional) arguments.
    /// Parameter, which contains format string, should be given in constructor.
    /// The format string should be in <see cref="string.Format(IFormatProvider,string,object[])"/> -like form
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class StringFormatMethodAttribute : Attribute
    {
        /// <summary>
        /// Initializes new instance of StringFormatMethodAttribute
        /// </summary>
        /// <param name="formatParameterName">Specifies which parameter of an annotated method should be treated as format-string</param>
        public StringFormatMethodAttribute(string formatParameterName)
        {
            FormatParameterName = formatParameterName;
        }

        /// <summary>
        /// Gets format parameter name
        /// </summary>
        [UsedImplicitly]
        public string FormatParameterName { get; private set; }
    }

    /// <summary>
    /// Indicates that the function argument should be string literal and match one of the parameters of the caller function.
    /// For example, <see cref="ArgumentNullException"/> has such parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class InvokerParameterNameAttribute : Attribute { }

    /// <summary>
    /// Indicates that the function is used to notify class type property value is changed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NotifyPropertyChangedInvocatorAttribute : Attribute
    {
        public NotifyPropertyChangedInvocatorAttribute() { }
        public NotifyPropertyChangedInvocatorAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        [UsedImplicitly]
        public string ParameterName { get; private set; }
    }

    /// <summary>
    /// Indicates that the value of marked element could be <c>null</c> sometimes, so the check for <c>null</c> is necessary before its usage
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class CanBeNullAttribute : Attribute { }

    /// <summary>
    /// Indicates that the value of marked element could never be <c>null</c>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class NotNullAttribute : Attribute { }

    /// <summary>
    /// Describes dependency between method input and output
    /// </summary>
    /// <syntax>
    /// <p>Function definition table syntax:</p>
    /// <list>
    /// <item>FDT      ::= FDTRow [;FDTRow]*</item>
    /// <item>FDTRow   ::= Input =&gt; Output | Output &lt;= Input</item>
    /// <item>Input    ::= ParameterName: Value [, Input]*</item>
    /// <item>Output   ::= [ParameterName: Value]* {halt|stop|void|nothing|Value}</item>
    /// <item>Value    ::= true | false | null | notnull | canbenull</item>
    /// </list>
    /// If method has single input parameter, it's name could be omitted. <br/>
    /// Using "halt" (or "void"/"nothing", which is the same) for method output means that method doesn't return normally. <br/>
    /// "canbenull" annotation is only applicable for output parameters. <br/>
    /// You can use multiple [ContractAnnotation] for each FDT row, or use single attribute with rows separated by semicolon. <br/>
    /// </syntax>
    /// <examples>
    /// <list>
    /// <item>[ContractAnnotation("=> halt")] public void TerminationMethod()</item>
    /// <item>[ContractAnnotation("halt &lt;= condition: false")] public void Assert(bool condition, string text) // Regular Assertion method</item>
    /// <item>[ContractAnnotation("s:null => true")] public bool IsNullOrEmpty(string s) // String.IsNullOrEmpty</item>
    /// <item>[ContractAnnotation("null => null; notnull => notnull")] public object Transform(object data) // Method which returns null if parameter is null, and not null if parameter is not null</item>
    /// <item>[ContractAnnotation("s:null=>false; =>true,result:notnull; =>false, result:null")] public bool TryParse(string s, out Person result)</item>
    /// </list>
    /// </examples>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class ContractAnnotationAttribute : Attribute
    {
        public ContractAnnotationAttribute([NotNull] string fdt)
            : this(fdt, false)
        {
        }

        public ContractAnnotationAttribute([NotNull] string fdt, bool forceFullStates)
        {
            FDT = fdt;
            ForceFullStates = forceFullStates;
        }

        public string FDT { get; private set; }
        public bool ForceFullStates { get; private set; }
    }

    /// <summary>
    /// Indicates that the value of marked type (or its derivatives) cannot be compared using '==' or '!=' operators.
    /// There is only exception to compare with <c>null</c>, it is permitted
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class CannotApplyEqualityOperatorAttribute : Attribute { }

    /// <summary>
    /// When applied to target attribute, specifies a requirement for any type which is marked with
    /// target attribute to implement or inherit specific type or types
    /// </summary>
    /// <example>
    /// <code>
    /// [BaseTypeRequired(typeof(IComponent)] // Specify requirement
    /// public class ComponentAttribute : Attribute
    /// {}
    ///
    /// [Component] // ComponentAttribute requires implementing IComponent interface
    /// public class MyComponent : IComponent
    /// {}
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    [BaseTypeRequired(typeof(Attribute))]
    public sealed class BaseTypeRequiredAttribute : Attribute
    {
        /// <summary>
        /// Initializes new instance of BaseTypeRequiredAttribute
        /// </summary>
        /// <param name="baseType">Specifies which types are required</param>
        public BaseTypeRequiredAttribute(Type baseType)
        {
            BaseTypes = new[] { baseType };
        }

        /// <summary>
        /// Gets enumerations of specified base types
        /// </summary>
        public Type[] BaseTypes { get; private set; }
    }

    /// <summary>
    /// Indicates that the marked symbol is used implicitly (e.g. via reflection, in external library),
    /// so this symbol will not be marked as unused (as well as by other usage inspections)
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    public sealed class UsedImplicitlyAttribute : Attribute
    {
        [UsedImplicitly]
        public UsedImplicitlyAttribute()
            : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default) { }

        [UsedImplicitly]
        public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
        {
            UseKindFlags = useKindFlags;
            TargetFlags = targetFlags;
        }

        [UsedImplicitly]
        public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags)
            : this(useKindFlags, ImplicitUseTargetFlags.Default) { }

        [UsedImplicitly]
        public UsedImplicitlyAttribute(ImplicitUseTargetFlags targetFlags)
            : this(ImplicitUseKindFlags.Default, targetFlags) { }

        [UsedImplicitly]
        public ImplicitUseKindFlags UseKindFlags { get; private set; }

        /// <summary>
        /// Gets value indicating what is meant to be used
        /// </summary>
        [UsedImplicitly]
        public ImplicitUseTargetFlags TargetFlags { get; private set; }
    }

    /// <summary>
    /// Should be used on attributes and causes ReSharper to not mark symbols marked with such attributes as unused (as well as by other usage inspections)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class MeansImplicitUseAttribute : Attribute
    {
        [UsedImplicitly]
        public MeansImplicitUseAttribute()
            : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default) { }

        [UsedImplicitly]
        public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
        {
            UseKindFlags = useKindFlags;
            TargetFlags = targetFlags;
        }

        [UsedImplicitly]
        public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags)
            : this(useKindFlags, ImplicitUseTargetFlags.Default)
        {
        }

        [UsedImplicitly]
        public MeansImplicitUseAttribute(ImplicitUseTargetFlags targetFlags)
            : this(ImplicitUseKindFlags.Default, targetFlags) { }

        [UsedImplicitly]
        public ImplicitUseKindFlags UseKindFlags { get; private set; }

        /// <summary>
        /// Gets value indicating what is meant to be used
        /// </summary>
        [UsedImplicitly]
        public ImplicitUseTargetFlags TargetFlags { get; private set; }
    }

    [Flags]
    public enum ImplicitUseKindFlags
    {
        Default = Access | Assign | InstantiatedWithFixedConstructorSignature,

        /// <summary>
        /// Only entity marked with attribute considered used
        /// </summary>
        Access = 1,

        /// <summary>
        /// Indicates implicit assignment to a member
        /// </summary>
        Assign = 2,

        /// <summary>
        /// Indicates implicit instantiation of a type with fixed constructor signature.
        /// That means any unused constructor parameters won't be reported as such.
        /// </summary>
        InstantiatedWithFixedConstructorSignature = 4,

        /// <summary>
        /// Indicates implicit instantiation of a type
        /// </summary>
        InstantiatedNoFixedConstructorSignature = 8,
    }

    /// <summary>
    /// Specify what is considered used implicitly when marked with <see cref="MeansImplicitUseAttribute"/> or <see cref="UsedImplicitlyAttribute"/>
    /// </summary>
    [Flags]
    public enum ImplicitUseTargetFlags
    {
        Default = Itself,

        Itself = 1,

        /// <summary>
        /// Members of entity marked with attribute are considered used
        /// </summary>
        Members = 2,

        /// <summary>
        /// Entity marked with attribute and all its members considered used
        /// </summary>
        WithMembers = Itself | Members
    }

    /// <summary>
    /// This attribute is intended to mark publicly available API which should not be removed and so is treated as used.
    /// </summary>
    [MeansImplicitUse]
    public sealed class PublicAPIAttribute : Attribute
    {
        public PublicAPIAttribute() { }
        public PublicAPIAttribute(string comment) { }
    }

    /// <summary>
    /// Tells code analysis engine if the parameter is completely handled when the invoked method is on stack.
    /// If the parameter is delegate, indicates that delegate is executed while the method is executed.
    /// If the parameter is enumerable, indicates that it is enumerated while the method is executed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
    public sealed class InstantHandleAttribute : Attribute { }

    /// <summary>
    /// Indicates that method doesn't contain observable side effects.
    /// The same as <see cref="System.Diagnostics.Contracts.PureAttribute"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class PureAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class PathReferenceAttribute : Attribute
    {
        public PathReferenceAttribute() { }

        [UsedImplicitly]
        public PathReferenceAttribute([PathReference] string basePath)
        {
            BasePath = basePath;
        }

        [UsedImplicitly]
        public string BasePath { get; private set; }
    }

    // ASP.NET MVC attributes

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public sealed class AspMvcActionAttribute : Attribute
    {
        [UsedImplicitly]
        public string AnonymousProperty { get; private set; }

        public AspMvcActionAttribute() { }

        public AspMvcActionAttribute(string anonymousProperty)
        {
            AnonymousProperty = anonymousProperty;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class AspMvcAreaAttribute : PathReferenceAttribute
    {
        [UsedImplicitly]
        public string AnonymousProperty { get; private set; }

        [UsedImplicitly]
        public AspMvcAreaAttribute() { }

        public AspMvcAreaAttribute(string anonymousProperty)
        {
            AnonymousProperty = anonymousProperty;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public sealed class AspMvcControllerAttribute : Attribute
    {
        [UsedImplicitly]
        public string AnonymousProperty { get; private set; }

        public AspMvcControllerAttribute() { }

        public AspMvcControllerAttribute(string anonymousProperty)
        {
            AnonymousProperty = anonymousProperty;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class AspMvcMasterAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class AspMvcModelTypeAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public sealed class AspMvcPartialViewAttribute : PathReferenceAttribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class AspMvcSupressViewErrorAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class AspMvcDisplayTemplateAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class AspMvcEditorTemplateAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public sealed class AspMvcViewAttribute : PathReferenceAttribute { }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    public sealed class AspMvcActionSelectorAttribute : Attribute { }

    // Razor attributes

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, Inherited = true)]
    public sealed class RazorSectionAttribute : Attribute { }
}