﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Aspire.Dashboard.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Traces {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Traces() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Aspire.Dashboard.Resources.Traces", typeof(Traces).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Traces.
        /// </summary>
        public static string TracesHeader {
            get {
                return ResourceManager.GetString("TracesHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Name filter.
        /// </summary>
        public static string TracesNameFilter {
            get {
                return ResourceManager.GetString("TracesNameFilter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No traces found.
        /// </summary>
        public static string TracesNoTraces {
            get {
                return ResourceManager.GetString("TracesNoTraces", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} Traces.
        /// </summary>
        public static string TracesPageTitle {
            get {
                return ResourceManager.GetString("TracesPageTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} spans.
        /// </summary>
        public static string TracesResourceSpans {
            get {
                return ResourceManager.GetString("TracesResourceSpans", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Spans.
        /// </summary>
        public static string TracesSpansColumnHeader {
            get {
                return ResourceManager.GetString("TracesSpansColumnHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Errored: {0}.
        /// </summary>
        public static string TracesTotalErroredTraces {
            get {
                return ResourceManager.GetString("TracesTotalErroredTraces", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Total: {0}.
        /// </summary>
        public static string TracesTotalTraces {
            get {
                return ResourceManager.GetString("TracesTotalTraces", resourceCulture);
            }
        }
    }
}
