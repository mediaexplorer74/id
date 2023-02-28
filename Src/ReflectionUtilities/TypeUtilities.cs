using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml.Linq;

using Microsoft.MetadataReader;
using System.Reflection.Adds;
using System.Runtime.InteropServices;

namespace ReflectionUtilities
{
    /// <summary>
    /// Provides tools for querying a metadata assembly (or any managed assembly)
    /// for type information.
    /// </summary>
    public class TypeUtilities
    {
        ///////////////////////////////////////////////////////////////////////
        #region Private Fields

        private static Assembly _systemAssembly;
        private static string _runtimeDirectory;
        private static DefaultUniverse _symbolUniverse;
        private const string _systemAssemblyName = "mscorlib.dll";
        private const string _metadataAssemblyFileExt = ".winmd";
        private const string _assemblyFileExt = ".dll";
        private const string _memberTypeErrorString = "must have MemberType set to Constructor, Method, Property, or Event.";
        private const string _typeProxyName = "TypeProxy";

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Public Properties

        /// <summary>
        /// Gets the system assembly that contains the primitive .NET Framework types.
        /// </summary>
        /// <remarks>
        /// LMR needs a system assembly to resolve primitive types. WRL depends on very little, and certainly
        /// nothing that's changed since .NET 1.0, so any mscorlib will do. For convenience, <see cref="SystemAssembly"/> returns
        /// the currently loaded system assembly (mscorlib.dll). 
        /// </remarks>
        public static Assembly SystemAssembly
        {
            get
            {
                if( _systemAssembly == null )
                {
                    _systemAssembly = typeof( object ).Assembly;
                }

                return _systemAssembly;
            }
        }

        /// <summary>
        /// Gets the directory where the common language runtime is installed.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static string RuntimeDirectory
        {
            get
            {
                if( _runtimeDirectory == null )
                {
                    _runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
                }

                return _runtimeDirectory;
            }
        }

        /// <summary>
        /// Gets all of the types from all of the loaded assemblies. 
        /// </summary>
        /// <remarks>
        /// Types from the <see cref="SystemAssembly"/> are not returned.
        /// </remarks>
        public static List<Type> TypesInSymbolUniverse
        {
            get
            {
                List<Type> allTypes = new List<Type>();

                foreach( Assembly a in SymbolUniverse.Assemblies )
                {
                    // Filter out types from the system assembly.
                    if( Path.GetFileName( a.CodeBase ) == _systemAssemblyName )
                    {
                        continue;
                    }

                    Type[] types = a.GetTypes();
                    allTypes.AddRange( types );
                }

                return allTypes;
            }
        }

        /// <summary>
        /// Gets all of the types from all of the loaded assemblies. 
        /// </summary>
        /// <remarks>
        /// Types from the <see cref="SystemAssembly"/> are not returned.
        /// </remarks>
        public static List<ObservableType> ObservableTypesInSymbolUniverse
        {
            get
            {
                return ( TypesInSymbolUniverse.Select( t => new ObservableType( t ) ).ToList() );
            }
        }

        /// <summary>
        /// Gets the assemblies that have been loaded into the current process.
        /// </summary>
        public static IEnumerable<Assembly> LoadedAssemblies
        {
            get
            {
                return SymbolUniverse.Assemblies;
            }
        }

        /// <summary>
        /// Gets all folders that hold loaded metadata assemblies, which can be .winmd files
        /// or vanilla .NET assemblies. 
        /// </summary>
        /// <remarks></remarks>
        public static List<string> MetadataFolders
        {
            get
            {
                var assemblyPaths = LoadedAssemblies.Select( a => Path.GetDirectoryName( a.Location ) );
                var distinctAssemblyPaths = assemblyPaths.Distinct();

                return distinctAssemblyPaths.ToList();
            }
        }

        #endregion 

        ///////////////////////////////////////////////////////////////////////
        #region Public Methods

        //public static Assembly LoadAssemblyFromFile( string fullPath )
        //{
        //    return Assembly.ReflectionOnlyLoadFrom( fullPath );
        //}

        /// <summary>
        /// Loads a metadata assembly (or any managed assembly) from the 
        /// specified path.
        /// </summary>
        /// <param name="fullPath">The full path to the assembly.</param>
        /// <returns>An <see cref="Assembly"/> that represents the assembly 
        /// referred to by <paramref name="fullPath"/>.</returns>
        public static Assembly LoadMetadataAssemblyFromFile( string fullPath )
        {
            Assembly metadataAssembly = null;

            string assemblyName = Path.GetFileName( fullPath );

            if( !File.Exists( fullPath ) )
            {
                throw new ArgumentException( 
                    "must be a fully qualified path to a WinRT metadata (.winmd) file", 
                    "fullPath" );
            }

            if( !IsLoaded( assemblyName ) )
            {
                metadataAssembly = SymbolUniverse.LoadAssemblyFromFile( fullPath );
            }
            else
            {
                metadataAssembly = SymbolUniverse.Assemblies.First( a => a.ManifestModule.Name == assemblyName );
            }
            
            return metadataAssembly;
        }

        /// <summary>
        /// Gets a value indicating whether types in the specified assembly 
        /// have been loaded into the symbol universe.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to check.</param>
        /// <returns><code>true</code> if the assembly has been loaded; otherwise, <code>false</code>.</returns>
        public static bool IsLoaded( string assemblyName )
        {
            int count = SymbolUniverse.Assemblies.Where( a => a.ManifestModule.Name == assemblyName ).Count();

            return( count > 0 ); 
        }

        /// <summary>
        /// Gets all of the types contained in the specified assembly.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for types.</param>
        /// <returns>A list of all of the types in <paramref name="metadataAssembly"/>.</returns>
        public static List<Type> GetTypes( Assembly metadataAssembly )
        {
            if( metadataAssembly != null )
            {
                return metadataAssembly.GetTypes().ToList();
            }
            else
            {
                throw new ArgumentNullException( "metadataAssembly", "must be assigned" );
            }
        }

        /// <summary>
        /// Gets all of the types contained in the specified assembly and 
        /// represents each type as an <see cref="ObservableType"/>. 
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for types.</param>
        /// <returns>A list of all of the types in <paramref name="metadataAssembly"/>.</returns>
        public static List<ObservableType> GetObservableTypes( Assembly metadataAssembly )
        {
            return GetObservableTypes( metadataAssembly, true );
        }

        public static List<ObservableType> GetObservableTypes( Assembly metadataAssembly, bool publicTypesOnly )
        {
            if( metadataAssembly != null )
            {
                Type[] types = metadataAssembly.GetTypes();
                List<ObservableType> observableTypes = null;

                if( publicTypesOnly )
                {
                    var publicTypes = types.Where( t => t.IsPublic );
                    observableTypes = publicTypes.Select( t => new ObservableType( t ) ).ToList();
                }
                else
                {
                    // Filter mysterious "<>c__DisplayClass" types.
                    var notNestedPrivateTypes = types.Where( t => !t.IsNestedPrivate );
                    observableTypes = notNestedPrivateTypes.Select( t => new ObservableType( t ) ).ToList();
                }

                return observableTypes;
            }
            else
            {
                throw new ArgumentNullException( "metadataAssembly", "must be assigned" );
            }
        }


        /// <summary>
        /// Queries the specified assembly for the specified type.
        /// </summary>
        /// <param name="type">The type to search for in the assembly.</param>
        /// <param name="metadataAssembly">The assembly to query for <paramref name="type"/>.</param>
        /// <returns><code>true</code> if <paramref name="type"/> is in <paramref name="metadataAssembly"/>; 
        /// otherwise, <code>false</code>.</returns>
        public static bool HasType( ObservableType type, Assembly metadataAssembly )
        {
            if( metadataAssembly != null && type != null )
            {
                string fullName = type.FullName;

                // This is a workaround. Mysteriouslyn LMR returns null for FullName on some types.
                if( fullName == null )
                {
                    fullName = String.Format( "{0}.{1}", type.Namespace, type.Name );
                }

                return ( HasType( fullName, metadataAssembly ) );
            }
            else
            {
                throw new ArgumentNullException( "type and metadataAssembly", "must be assigned" );
            }
        }

        /// <summary>
        /// Queries the specified assembly for the specified type.
        /// </summary>
        /// <param name="type">The type to search for in the assembly.</param>
        /// <param name="metadataAssembly">The assembly to query for <paramref name="type"/>.</param>
        /// <returns><code>true</code> if <paramref name="type"/> is in <paramref name="metadataAssembly"/>; 
        /// otherwise, <code>false</code>.</returns>
        public static bool HasType( Type type, Assembly metadataAssembly )
        {
            ObservableType observableType = new ObservableType( type );

            return HasType( observableType, metadataAssembly );
        }

        /// <summary>
        /// Queries the specified assembly for the specified type.
        /// </summary>
        /// <param name="typeName">The name of the type to search for in the assembly.</param>
        /// <param name="metadataAssembly">The assembly to query for <paramref name="type"/>.</param>
        /// <returns><code>true</code> if <paramref name="typeName"/> is in <paramref name="metadataAssembly"/>; 
        /// otherwise, <code>false</code>.</returns>
        public static bool HasType( string typeName, Assembly metadataAssembly )
        {
            if( metadataAssembly != null && typeName != null && typeName.Length > 0 )
            {
                return ( metadataAssembly.GetType( typeName, false ) != null );
            }
            else
            {
                throw new ArgumentNullException( "typeName and metadataAssembly", "must be assigned" );
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether the specified type inherits from the specified base type.
        /// </summary>
        /// <param name="type">The type to test for inheritance.</param>
        /// <param name="baseType">The base type to test against.</param>
        /// <returns><code>true</code> if <paramref name="type"/> inherits from <paramref name="baseType"/>; otherwise, <code>false</code>.</returns>
        /// <remarks>Seemed too useful not to have:
        /// http://stackoverflow.com/questions/129277/how-do-you-determine-whether-or-not-a-give-type-system-type-inherits-from-a-spe/129299
        /// </remarks>
        public static bool Inherits(Type type, Type baseType) 
        {
            return( baseType.IsAssignableFrom( type ) );
        }

        /// <summary>
        /// Gets a value that indicates whether the specified type is a base type. 
        /// </summary>
        /// <param name="type">The type to test for base-ness. Must be a class or an interface.</param>
        /// <returns><code>true</code> is <paramref name="type"/> is a base type; otherwise, <code>false</code>.</returns>
        /// <remarks>The <see cref="IsBaseType"/> method defines a base type in the following way.
        /// <list type="bullet">
        /// <item><paramref name="type"/> is either a class or an interface, and</item>
        /// <item><paramref name="type"/> implements no interfaces, and</item>
        /// <item>if <paramref name="type"/> is a class, it inherits directly from <see cref="System.Object"/>. </item>
        /// </list>
        /// <para>The <see cref="IsBaseType"/> method is used for constructing the ancestors node in the syntax block 
        /// of WDCML reference topic.
        /// </para>
        /// </remarks>
        public static bool IsBaseType( ObservableType type )
        {
            if( !type.IsClass && !type.IsInterface )
            {
                throw new ArgumentException( "must be a class or an interface ", "type" );
            }

            bool isBaseType = false;

            if( type.HasInterfaces )
            {
                return false;
            }

            if( type.IsInterface ||
                type.BaseType is System.Object )
            {
                isBaseType = true;
            }

            return isBaseType;
        }

        /// <summary>
        /// Gets a value that indicates whether the specified type is a base type. 
        /// </summary>
        /// <param name="type">The type to test for base-ness.</param>
        /// <returns><code>true</code> is <paramref name="type"/> is a base type; otherwise, <code>false</code>.</returns>
        /// <remarks>The <see cref="IsBaseType"/> method defines a base type in the following way.
        /// <list type="bullet">
        /// <item><paramref name="type"/> is either a class or an interface, and</item>
        /// <item><paramref name="type"/> implements no interfaces, and</item>
        /// <item>if <paramref name="type"/> is a class, it inherits directly from <see cref="System.Object"/>. </item>
        /// </list>
        /// <para>The <see cref="IsBaseType"/> method is used for constructing the ancestors node in the syntax block 
        /// of WDCML reference topic.
        /// </para>
        /// </remarks>
        public static bool IsBaseType( Type type )
        {
            ObservableType ot = new ObservableType( type );

            return IsBaseType( ot );
        }

        // Retrieving a Type's leaf interfaces
        // http://stackoverflow.com/questions/1460332/retrieving-a-types-leaf-interfaces/1460369
        public static List<Type> GetLeafInterfaces( Type type ) 
        { 
            return type.FindInterfaces( ( candidateIfc, allIfcs ) => 
            { 
                foreach( Type ifc in (Type[])allIfcs ) 
                {
                    if( candidateIfc != ifc && 
                        candidateIfc.IsAssignableFrom( ifc ) )
                    {
                        return false;
                    }
                }
                
                return true; 
            }, 
            type.GetInterfaces() ).ToList(); 
        }

        /// <summary>
        /// Gets a value indicating whether the specified type has an initializer,
        /// i.e., constructor.
        /// </summary>
        /// <param name="type">The type to test for an initializer.</param>
        /// <returns>true if <paramref name="type"/> has an initializer, otherwise false.</returns>
        /// <remarks>The <see cref="Type.TypeInitializer"/> property does not seem to be entirely 
        /// reliable, so this method tests specifically for the presence of constructors.</remarks>
        public static bool IsCreatable( ObservableType type )
        {
            return ( type.IsClass && type.HasConstructors );
        }

        /// <summary>
        /// Finds the inheritance chain of the specified type.
        /// </summary>
        /// <param name="type">The type to find the inheritance chain for.</param>
        /// <returns>The inheritance chain for <paramref name="type"/>, starting with the
        /// base type.</returns>
        /// <remarks>Most of the time, System.Object is the first item in the list.
        /// </remarks>
        public static List<ObservableType> SuperClasses( ObservableType type )
        {
            List<ObservableType> inheritedClasses = new List<ObservableType>();

            ObservableType baseType = type.BaseType;

            while( baseType != null )
            {
                inheritedClasses.Add( baseType );

                baseType = baseType.BaseType;
            }

            // Reverse the order, so that the base type is the first in the list.
            inheritedClasses.Reverse();

            return inheritedClasses;
        }

        /// <summary>
        /// Gets all of the methods for the specified type.
        /// </summary>
        /// <param name="t">The type to get methods for.</param>
        /// <returns>A list of methods for <paramref name="t"/>. </returns>
        public static List<MethodInfo> GetMethodsForType( Type t )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether the specified member is implemented 
        /// by the specified type. 
        /// </summary>
        /// <param name="type">The type to query for <paramref name="member"/>.</param>
        /// <param name="member">The member to query for.</param>
        /// <returns><code>true</code> if <paramref name="member"/> is implemented 
        /// by <paramref name="type"/>; otherwise, <code>false</code>.</returns>
        /// <remarks><paramref name="member"/> may be public or non-public. </remarks>
        public static bool IsImplemented( ObservableType type, MemberInfo member )
        {
            return IsImplemented( type.UnderlyingType, member );
        }

        /// <summary>
        /// Gets a value indicating whether the specified member is implemented 
        /// by the specified type. 
        /// </summary>
        /// <param name="type">The type to query for <paramref name="member"/>.</param>
        /// <param name="member">The member to query for.</param>
        /// <returns><code>true</code> if <paramref name="member"/> is implemented 
        /// by <paramref name="type"/>; otherwise, <code>false</code>.</returns>
        /// <remarks><paramref name="member"/> may be public or non-public. </remarks>
        public static bool IsImplemented( Type type, MemberInfo member )
        {
            bool isImplemented = false;

            // Get the public and non-public members of the specified type.
            // TBD: BindingFlags.DeclaredOnly? I don't think it's necessary.
            MemberInfo[] members = type.GetMembers( 
                BindingFlags.Instance | 
                BindingFlags.Public | 
                BindingFlags.NonPublic |
                BindingFlags.Static ); 
            try
            {
                // Query the type's members for a member with the same name as
                // the specified member. The First method throws if no match is found.
                var findResult = members.First( mi => mi.Name == member.Name );
                if( findResult != null )
                {
                    isImplemented = true;
                }
            }
            catch( Exception ex )
            {
                Debug.WriteLine("Exception", ex.ToString(), ", source: ", ex.Source, ". This is expected whenever the specified member isn't found on type.");
            }

            return isImplemented;
        }

        /// <summary>
        /// Gets a value indicating whether the specified type has the
        /// most-derived implementation of the specified member in the 
        /// type's inheritance hierarchy.
        /// </summary>
        /// <param name="type">The type to query for the implementation 
        /// of <paramref name="member"/>.</param>
        /// <param name="member">The implementation to query for.</param>
        /// <returns>true</code> if <paramref name="type"/> has the most-derived 
        /// implementation of <paramref name="member"/>; otherwise, <code>false</code>.</returns>
        public static bool IsMostDerivedImplementation( Type type, MemberInfo member )
        {
            Type mostDerivedImplementation = FindMostDerivedImplementation( member );

            return ( type == mostDerivedImplementation );
        }

        /// <summary>
        /// Queries the inheritance hierarchy of the specified member for the
        /// most-derived implementation.
        /// </summary>
        /// <param name="member">The implementation to query for.</param>
        /// <returns>The type that supplies the most-derived implementation of 
        /// <paramref name="member"/>.</returns>
        public static Type FindMostDerivedImplementation( MemberInfo member )
        {
            Type mostDerivedImplementation = GetMostDerivedType( member );

            return mostDerivedImplementation;
        }

        /// <summary>
        /// Queries the inheritance hierarchy of the specified member for the
        /// most-derived implementation.
        /// </summary>
        /// <param name="method">The implementation to query for.</param>
        /// <returns>The type that supplies the most-derived implementation of 
        /// <paramref name="member"/>.</returns>
        /// <remarks>The <see cref="GetMostDerivedType"/> method performs a 
        /// recursive traversal up the inheritance hierarchy of the declaring
        /// type of <paramref name="member"/>. This turns out to have much
        /// better performance than calling the <see cref="GetBaseDefinition"/> method.</remarks>
        static Type GetMostDerivedType( MemberInfo method )
        {
            return GetMostDerivedType( method, method.DeclaringType );
        }

        /// <summary>
        /// Finds the most-derived implementation of the specified member in the
        /// specified type's inheritance hierarchy.
        /// </summary>
        /// <param name="member">The implementation to query for.</param>
        /// <param name="currentType">The current type in the recursive traversal.</param>
        /// <returns>The type that supplies the most-derived implementation of 
        /// <paramref name="member"/>.</returns>
        static Type GetMostDerivedType( MemberInfo member, Type currentType )
        {
            if( currentType == null )
            {
                Trace.Assert( false, "GetMostDerivedType should never return null" );
                return null;
            }

            if( IsImplemented( currentType, member ) )
            {
                return currentType;
            }
            else
            {
                return GetMostDerivedType( member, currentType.BaseType );
            }
        }

        /// <summary>
        /// Gets the base-most type that guarantees an implementation for the specified member.
        /// </summary>
        /// <param name="m">The member to query for its base-most implementation.</param>
        /// <returns>The type that supplies the bae=most implementation of 
        /// <paramref name="member"/>.</returns>
        public static Type GetIntroducingType( MemberInfo m )
        {
            return GetIntroducingType( m, m.DeclaringType );
        }

        static Type GetIntroducingType( MemberInfo m, Type current )
        {
            if( IsImplemented( current.BaseType, m ) )
            {
                return GetIntroducingType( m, current.BaseType );
            }
            else
            {
                return current;
            }
        }

        /// <summary>
        /// OBSOLETE. Finds the base-most type that declares the specified member.
        /// </summary>
        /// <param name="type">The type that provides the inheritance graph to query.</param>
        /// <param name="memberName">The name of the member to search the inheritance graph for.</param>
        /// <returns>The base-most type that declares <paramref name="memberName"/>. </returns>
        /// <remarks>
        /// <para>This method is retained for historical purposes only. Use <see cref="GetIntroducingType"/> instead.</para>
        /// <para>"Declaring type" has a different meaning than in the standard .NET Reflection 
        /// <see cref="DeclaringType"/> implementation (http://msdn.microsoft.com/en-us/library/system.reflection.memberinfo.declaringtype.aspx).
        /// In the following examples, Reflection returns the most-derived type for DeclaringType.
        /// 
        ///     interface i 
        ///     {
        ///         int MyVar() ;
        ///     };
        ///     // DeclaringType for MyVar is i.
        /// 
        ///     class A : i 
        ///     {
        ///         public int MyVar() { return 0; }
        ///     };
        ///     // DeclaringType for MyVar is A.
        /// 
        ///     class B : A 
        ///     {
        ///         new int MyVar() { return 0; }
        ///     };
        ///     // DeclaringType for MyVar is B.
        /// 
        /// Unlike the previous definition, the <see cref="FindDeclaringType"/> method returns the base-most type.
        /// Given the previous example, the <see cref="FindDeclaringType"/> method returns the interface i as the 
        /// declaring type.
        /// </para>
        /// <para>For the <see cref="FindDeclaringType"/> method to return the base-most type, the 
        /// <see cref="SuperClasses"/> method must return the inheritance list with the base type first.
        /// </para>
        /// <para>The search order is as follows.
        /// 1. Search interfaces -- assumes member is uniquely declared in the interface graph.
        /// 2. Search superclasses.
        /// 3. Search <paramref name="type"/>.
        /// </para>
        /// <para>It can be argued that searching by the member's name might return ambiguous results; 
        /// for example, if an interface inherits from another interface and adds method overloads, the 
        /// <see cref="FindDeclaringType"/> method will fail by returning the first interface in the
        /// type's <see cref="Interfaces"/> list. If this turns out to be a problem, it should be possible 
        /// to query by the method's hash code.
        /// </para>
        /// </remarks>
        public static ObservableType FindDeclaringType( ObservableType type, string memberName )
        {
            List<ObservableType> interfaces = type.Interfaces;

            // Query all of the members in all of the type's interfaces for memberName. 
            ObservableType declaringType = interfaces.Find( t => t.Members.Find( mi => memberName == mi.Name ) != null );

            if( declaringType == null )
            {
                // Didn't find the member in the interfaces, so search the superclasses.
                List<ObservableType> superClasses = SuperClasses( type );

                // Query all of the members in all of the type's superclasses for memberName. 
                // N.B.: This works only if SuperClasses returns the inheritance chain starting from the base class.
                // Most of the time, this means that System.Object is the first item in SuperClasses.
                declaringType = superClasses.Find( t => t.Members.Find( mi => memberName == mi.Name ) != null );

                if( declaringType == null )
                {
                    // Didn't find the member in interfaces of superclasses, so search the type's members.
                    if( type.DeclaredMembers.Find( mi => memberName == mi.Name ) != null )
                    {
                        declaringType = type;
                    }
                }
            }


            return declaringType;
        }

        /// <summary>
        /// Gets all of the types that inherit from the specified type.
        /// </summary>
        /// <param name="parentType">The type to find derived types for.</param>
        /// <param name="metadataAssembly">The assembly to query for derived types.</param>
        /// <returns>A list of types that inherit from <paramref name="parentType"/>.</returns>
        /// <remarks>Hasn't been tested.</remarks>
        public static List<ObservableType> FindDerivedTypes( Type parentType, Assembly metadataAssembly )
        {
            Type[] types = metadataAssembly.GetTypes();

            List<ObservableType> derivedTypes = types.Select( t => ( t.BaseType == parentType ) ? new ObservableType( t ) : null ).ToList();

            return derivedTypes;
        }

        /// <summary>
        /// Gets all of the types that inherit from the specified type.
        /// </summary>
        /// <param name="parentType">The type to find derived types for.</param>
        /// <param name="metadataAssembly">The assembly to query for derived types.</param>
        /// <returns>A list of types that inherit from <paramref name="parentType"/>.</returns>
        /// <remarks>Hasn't been tested.</remarks>
        public static List<ObservableType> FindDerivedTypes( ObservableType parentType, Assembly metadataAssembly )
        {
            List<Type> types = metadataAssembly.GetTypes().ToList();

            List<ObservableType> observableTypes = types.Select( t => new ObservableType( t ) ).ToList();

            List<ObservableType> typesWithInterfaces = observableTypes.FindAll( ot => ot.HasInterfaces );

            List<ObservableType> typesWithBase = observableTypes.FindAll( ot => ot.HasBaseType );

            List<ObservableType> observableDerivedTypes = typesWithInterfaces.Concat( typesWithBase ).ToList();

            return observableDerivedTypes;
        }

        /// <summary>
        /// Gets all of the public interface types in the specified assembly.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for interface types.</param>
        /// <returns>A list of public interface types in <paramref name="metadataAssembly"/>.</returns>
        public static List<ObservableType> GetAllInterfaceTypes( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return( GetAllInterfaceTypes( types ) );
        }

        /// <summary>
        /// Gets all of the public interface types in the specified assembly.
        /// </summary>
        /// <param name="allTypes">The list to query for interface types.</param>
        /// <returns>A list of public interface types in <paramref name="allTypes"/>.</returns>
        public static List<ObservableType> GetAllInterfaceTypes( List<ObservableType> allTypes )
        {
            List<ObservableType> interfaceTypes = allTypes.FindAll( ot => ot.IsInterface );

            return interfaceTypes;
        }

        /// <summary>
        /// Gets all of the public class types in the specified assembly.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for class types.</param>
        /// <returns>A list of public class types in <paramref name="metadataAssembly"/>.</returns>
        public static List<ObservableType> GetAllClassTypes( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return ( GetAllClassTypes( types ) );
        }

        /// <summary>
        /// Gets all of the public class types in the specified list of types.
        /// </summary>
        /// <param name="allTypes">The list to query for class types.</param>
        /// <returns>A list of public class types in <paramref name="allTypes"/>.</returns>
        public static List<ObservableType> GetAllClassTypes( List<ObservableType> allTypes )
        {
            List<ObservableType> classTypes = allTypes.FindAll( ot => ot.IsClass && !ot.IsDelegate );

            return classTypes;
        }

        /// <summary>
        /// Gets all of the public struct types in the specified assembly.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for struct types.</param>
        /// <returns>A list of public struct types in <paramref name="metadataAssembly"/>.</returns>
        public static List<ObservableType> GetAllStructTypes( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return( GetAllStructTypes( types ) );
        }

        /// <summary>
        /// Gets all of the public struct types in the specified list of types.
        /// </summary>
        /// <param name="allTypes">The list to query for struct types.</param>
        /// <returns>A list of public struct types in <paramref name="allTypes"/>.</returns>
        public static List<ObservableType> GetAllStructTypes( List<ObservableType> allTypes )
        {
            List<ObservableType> structTypes = allTypes.FindAll( ot => ot.IsStruct );

            return structTypes;
        }

        /// <summary>
        /// Gets all of the public delegate types in the specified assembly.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for delegate types.</param>
        /// <returns>A list of public delegate types in <paramref name="metadataAssembly"/>.</returns>
        public static List<ObservableType> GetAllDelegateTypes( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return ( GetAllDelegateTypes( types ) );
        }

        /// <summary>
        /// Gets all of the public delegate types in the specified list of types.
        /// </summary>
        /// <param name="allTypes">The list to query for delegate types.</param>
        /// <returns>A list of public delegate types in <paramref name="allTypes"/>.</returns>
        public static List<ObservableType> GetAllDelegateTypes( List<ObservableType> allTypes )
        {
            List<ObservableType> delegateTypes = allTypes.FindAll( ot => ot.IsDelegate );

            return delegateTypes;
        }

        /// <summary>
        /// Gets all of the public enum types in the specified assembly.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for enum types.</param>
        /// <returns>A list of public enum types in <paramref name="metadataAssembly"/>.</returns>
        public static List<ObservableType> GetAllEnumTypes( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return ( GetAllEnumTypes( types ) );
        }

        /// <summary>
        /// Gets all of the public enum types in the specified list of types.
        /// </summary>
        /// <param name="allTypes">The list to query for enum types.</param>
        /// <returns>A list of public enum types in <paramref name="allTypes"/>.</returns>
        public static List<ObservableType> GetAllEnumTypes( List<ObservableType> allTypes )
        {
            List<ObservableType> enumTypes = allTypes.FindAll( ot => ot.IsEnum );

            return enumTypes;
        }

        /// <summary>
        /// Gets all of the public methods in the specified assembly.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for methods.</param>
        /// <returns>A list of public methods in <paramref name="metadataAssembly"/>.</returns>
        public static List<MethodInfo> GetAllMethods( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return( GetAllMethods( types ) );
        }

        /// <summary>
        /// Gets all of the public methods in the specified list of types.
        /// </summary>
        /// <param name="allTypes">The list to query for methods.</param>
        /// <returns>A list of public methods in <paramref name="allTypes"/>.</returns>
        public static List<MethodInfo> GetAllMethods( List<ObservableType> allTypes )
        {  
            var methods = allTypes.SelectMany( ot => ot.Methods );

            return methods.ToList();
        }

        /// <summary>
        /// Gets all of the distinct public methods in the specified list of types.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for methods.</param>
        /// <returns>A list of distinct public methods in <paramref name="metadataAssembly"/>.</returns>
        public static List<MethodInfo> GetUniqueMethods( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return( GetUniqueMethods( types ) );
        }

        /// <summary>
        /// Gets all of the distinct public methods in the specified list of types.
        /// </summary>
        /// <param name="allTypes">The list to query for methods.</param>
        /// <returns>A list of distinct public methods in <paramref name="allTypes"/>.</returns>
        public static List<MethodInfo> GetUniqueMethods( List<ObservableType> allTypes )
        {
            List<MethodInfo> allMethods = GetAllMethods( allTypes );

            List<MethodInfo> uniqueMethods = allMethods.Distinct( new MethodInfoComparer() ).ToList();

            return uniqueMethods;
        }

        /// <summary>
        /// Gets all of the public properties in the specified list of types.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for properties.</param>
        /// <returns>A list of public properties in <paramref name="metadataAssembly"/>.</returns>
        public static List<PropertyInfo> GetAllProperties( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return( GetAllProperties( types ) );
        }

        /// <summary>
        /// Gets all of the public properties in the specified list of types.
        /// </summary>
        /// <param name="allTypes">The list to query for properties.</param>
        /// <returns>A list of public properties in <paramref name="allTypes"/>.</returns>
        public static List<PropertyInfo> GetAllProperties( List<ObservableType> allTypes )
        {
            var properties = allTypes.SelectMany( ot => ot.Properties );

            return properties.ToList();
        }

        /// <summary>
        /// Gets all of the distinct public properties in the specified list of types.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for properties.</param>
        /// <returns>A list of distinct public properties in <paramref name="metadataAssembly"/>.</returns>
        public static List<PropertyInfo> GetUniqueProperties( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return ( GetUniqueProperties( types ) );
        }

        /// <summary>
        /// Gets all of the distinct public properties in the specified list of types.
        /// </summary>
        /// <param name="allTypes">The list to query for properties.</param>
        /// <returns>A list of distinct public properties in <paramref name="allTypes"/>.</returns>
        public static List<PropertyInfo> GetUniqueProperties( List<ObservableType> allTypes )
        {
            List<PropertyInfo> allPropertes = GetAllProperties( allTypes );

            List<PropertyInfo> uniqueProperties = allPropertes.Distinct( new PropertyInfoComparer() ).ToList();

            return uniqueProperties;
        }

        /// <summary>
        /// Gets all of the public events in the specified list of types.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for events.</param>
        /// <returns>A list of public events in <paramref name="metadataAssembly"/>.</returns>
        public static List<EventInfo> GetAllEvents( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return ( GetAllEvents( types ) );
        }

        /// <summary>
        /// Gets all of the public events in the specified list of types.
        /// </summary>
        /// <param name="allTypes">The list to query for events.</param>
        /// <returns>A list of public events in <paramref name="allTypes"/>.</returns>
        public static List<EventInfo> GetAllEvents( List<ObservableType> allTypes )
        {
            var events = allTypes.SelectMany( ot => ot.Events );

            return events.ToList();
        }

        /// <summary>
        /// Gets all of the distinct public events in the specified list of types.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for events.</param>
        /// <returns>A list of distinct public events in <paramref name="metadataAssembly"/>.</returns>
        public static List<EventInfo> GetUniqueEvents( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return ( GetUniqueEvents( types ) );
        }

        /// <summary>
        /// Gets all of the distinct public events in the specified list of types.
        /// </summary>
        /// <param name="allTypes">The list to query for events.</param>
        /// <returns>A list of distinct public events in <paramref name="allTypes"/>.</returns>
        public static List<EventInfo> GetUniqueEvents( List<ObservableType> allTypes )
        {
            List<EventInfo> allEvents = GetAllEvents( allTypes );

            List<EventInfo> uniqueEvents = allEvents.Distinct( new EventInfoComparer() ).ToList();

            return uniqueEvents;
        }

        /// <summary>
        /// Gets all of the namespaces in the specified assembly.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for namespaces.</param>
        /// <returns>A list of namespaces in <paramref name="metadataAssembly"/>.</returns>
        public static List<string> GetNamespaces( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return( GetNamespaces( types ) );
        }

        /// <summary>
        /// Gets all of the namespaces in the specified list of types.
        /// </summary>
        /// <param name="allTypes">The list to query for namespaces.</param>
        /// <returns>A list of namespaces in <paramref name="allTypes"/>.</returns>
        public static List<string> GetNamespaces( List<ObservableType> allTypes )
        {
            var namespaces = allTypes.Select( t => t.Namespace ).Distinct();

            // It's possible for a type to a have null namespace, so 
            // filter nulls from the namespace list.
            var namespacesWithoutNulls = namespaces.Where( t => t != null );

            return namespacesWithoutNulls.ToList();
        }

        /// <summary>
        /// Gets all of the public types in the specified namespace.
        /// </summary>
        /// <param name="ns">The name of the namespace to query for types.</param>
        /// <param name="metadataAssembly">The assembly to query for types.</param>
        /// <returns>A list of types that are contained in <paramref name="ns"/>.</returns>
        public static List<ObservableType> GetTypesInNamespace( string ns, Assembly metadataAssembly )
        {
            List<ObservableType> allTypes = GetObservableTypes( metadataAssembly );

            return( GetTypesInNamespace( ns, allTypes ) );
        }

        /// <summary>
        /// Gets all of the public types in the specified namespace.
        /// </summary>
        /// <param name="ns">The name of the namespace to query for types.</param>
        /// <param name="metadataAssembly">A list of types to query for types in <paramref name="ns"/>.</param>
        /// <returns>A list of types that are contained in <paramref name="ns"/>.</returns>
        public static List<ObservableType> GetTypesInNamespace( string ns, List<ObservableType> allTypes )
        {
            List<ObservableType> typesInNamespace = allTypes.FindAll( ot => ot.Namespace == ns );

            return typesInNamespace;
        }

        /// <summary>
        /// Gets all of the public attributes in the specified list of types.
        /// </summary>
        /// <param name="metadataAssembly">The assembly to query for attributes.</param>
        /// <returns>A list of public attributes in <paramref name="metadataAssembly"/>.</returns>
        public static List<ObservableType> GetAttributes( Assembly metadataAssembly )
        {
            List<ObservableType> types = GetObservableTypes( metadataAssembly );

            return( GetAttributes( types ) );
        }

        /// <summary>
        /// Gets all of the public attributes in the specified list of types.
        /// </summary>
        /// <param name="allTypes">The list to query for attributes.</param>
        /// <returns>A list of public attributes in <paramref name="allTypes"/>.</returns>
        public static List<ObservableType> GetAttributes( List<ObservableType> allTypes )
        {
            List<ObservableType> attributes = allTypes.FindAll( ot => ot.IsAttribute );

            return attributes;
        }

        /// <summary>
        /// Extracts the name of the attribute that is represented by the specified 
        /// <see cref="CustomAttributeData"/>.
        /// </summary>
        /// <param name="attributeData">The attribute data to parse.</param>
        /// <param name="getFullName">true to return the fully-qualified name of the attribute; otherwise, false.</param>
        /// <returns>The name of the attribute that is represented by <paramref name="attributeData"/>.</returns>
        public static string GetAttributeName( CustomAttributeData attributeData, bool getFullName )
        {
            string fullName = attributeData.Constructor.DeclaringType.FullName;
            string attributeName = fullName;

            if( !getFullName )
            {
                int lastIndex = fullName.LastIndexOf( '.' );

                // HACK: This works because LastIndexOf returns -1 if there are no '.' characters
                // in fullName, e.g., for the "System" namespace. Kind of a side effect, 
                // so beware.
                attributeName = fullName.Substring( lastIndex + 1 );
            }

            return attributeName;
        }

        /// <summary>
        /// Serializes all of the public types in the specified namespace to an <see cref="XElement"/>.
        /// </summary>
        /// <param name="ns">The namespace to serialize.</param>
        /// <param name="assembly">The assembly that contains the namespace.</param>
        /// <returns>A new <see cref="XElement"/> that contains the tpe data.</returns>
        public static XElement SerializeNamespace( string ns, Assembly assembly )
        {
            XElement typeElements = new XElement( "types" );

            List<ObservableType> types = GetTypesInNamespace( ns, assembly );

            for( int j = 0; j < types.Count; j++ )
            {
                ObservableType type = types[j];

                XElement typeElement = type.SerializeType();

                typeElements.Add( typeElement );
            }

            return typeElements;
        }

        /// <summary>
        /// Serializes all of the namespaces in the specified assembly to an <see cref="XDocument"/>.
        /// </summary>
        /// <param name="assembly">The assembly to query for namespaces.</param>
        /// <returns>A new <see cref="XDocument"/> that contains the type data for 
        /// all of the namespaces in <paramref name="assembly"/>.</returns>
        public static XDocument SerializeNamespaces( Assembly assembly )
        {
            List<string> namespaces = TypeUtilities.GetNamespaces( assembly );

            XDocument xdoc = new XDocument();
            XElement rootElement = xdoc.Root;

            for( int i = 0; i < namespaces.Count; i++ )
            {
                string ns = namespaces[i];

                rootElement.Add( SerializeNamespace( ns, assembly ) );
            }

            return xdoc;
        }

        /// <summary>
        /// Computes a hash code for the specified method.
        /// </summary>
        /// <param name="method">The method to compute the hash code for.</param>
        /// <returns>The hash code that corresponds with <paramref name="method"/>.</returns>
        /// <remarks>
        /// <para>The <see cref="GetMethodHashCode"/> method is used for generating unique filenames
        /// and links (RIDs in the WDCML schema) for overloaded methods. It is also used for uniqueness
        /// testing in the <see cref="GetUniqueMethods"/> method.</para>
        /// <para>If anything goes wrong with uniqueness testing, this is the place to start looking.
        /// For example, as described in Win8 TFS Bug 176790, <see cref="GetParameterHashCode"/> failed
        /// to distinguish between PinAsync() and PinAsync( String, String, String, String ), so the
        /// parameter position is now hashed into the parameter's hash code.
        /// ___
        /// GenTopics emits duplicate RIDs for overloaded methods
        /// http://win8tfs:8080/tfs/web/wi.aspx?id=176790&pguid=655db467-bcb2-4f98-8521-d86dbe3f405e
        /// </para>
        /// </remarks>
        public static int GetMethodHashCode( MethodBase method )
        {
            //Check whether the object is null
            if( Object.ReferenceEquals( method, null ) ) return 0;

            // Get hash code for the Name field if it is not null.
            int hashCode = method.Name == null ? 0 : GetHashCode( method.Name );

            // Hash in the method's parameters.
            ParameterInfo[] parameters = method.GetParameters();

            for( int i = 0; i < parameters.Length; i++ )
            {
                hashCode ^= GetParameterHashCode( parameters[i] );
            }

            // Positive values are prettier. 
            hashCode = Math.Abs( hashCode );

            return hashCode;
        }

        /// <summary>
        /// Computes a hash code for the specified parameter.
        /// </summary>
        /// <param name="pi">The parameter to compute the hash code for.</param>
        /// <returns>The hash code that corresponds with <paramref name="pi"/>.</returns>
        public static int GetParameterHashCode( ParameterInfo pi )
        {
            //Check whether the object is null
            if( Object.ReferenceEquals( pi, null ) ) return 0;

            //Get hash code for the ParameterType field.
            int hashCode = GetHashCode( pi.ParameterType.ToString() );

            // TFS 176790: Distinguish this parameter from others of the same type
            // by hashing in the parameter's position in the method signature.
            hashCode <<= pi.Position;

            return hashCode;
        }

        /// <summary>
        /// Computes a hash code for the specified string.
        /// </summary>
        /// <param name="stringToHash">The string to generate a hash code for.</param>
        /// <returns>The hash code that corresponds with <paramref name="stringToHash"/>.</returns>
        /// <remarks>
        /// <para>This implementation is reverse-engineered from mscorlib.dll version 4.0.30319 via
        /// Reflector (http://reflector.red-gate.com/).</para>
        /// <para>This local implementation is required to fix bug #256867:
        /// "GenTopics emits incorrect method overload filenames and links" 
        /// http://win8tfs:8080/tfs/web/wi.aspx?id=256867&pguid=655db467-bcb2-4f98-8521-d86dbe3f405e.
        /// The default .NET Framework implementations of GetHashCode are unreliable between framework 
        /// versions and  between different builds of the target metadata assemblies.</para>
        /// <para>NOTE: Requires the AllowUnsafeBlocks compiler option.</para>
        /// </remarks>
        public static unsafe int GetHashCode( string stringToHash )
        {
            // Check for null reference and empty string.
            if( Object.ReferenceEquals( stringToHash, null ) ||
                stringToHash.Length == 0 ) return 0;

            // Get a pointer to the start of the character array that represents the string,
            // and pin the string in memory. This enables reliable pointer operations.
            fixed( char* str = stringToHash.ToCharArray() )
            {
                char* chPtr = str;
                int num = 0x15051505; // Magic hash code cookie
                int num2 = num;
                int* numPtr = (int*)chPtr;

                // Traverse the string from end to start, and hash in bytes. 
                for( int i = stringToHash.Length; i > 0; i -= 4 )
                {
                    num = ( ( ( num << 5 ) + num ) + ( num >> 0x1b ) ) ^ numPtr[0];

                    if( i <= 2 )
                    {
                        break;
                    }

                    num2 = ( ( ( num2 << 5 ) + num2 ) + ( num2 >> 0x1b ) ) ^ numPtr[1];

                    numPtr += 2;
                }

                return ( num + ( num2 * 0x5d588b65 ) ); // Another magic hash code cookie
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Implementation

        private static DefaultUniverse SymbolUniverse
        {
            get
            {
                if( _symbolUniverse == null )
                {
                    _symbolUniverse = new DefaultUniverse();
                    _symbolUniverse.SetSystemAssembly( SymbolUniverse.LoadAssemblyFromFile( SystemAssembly.Location ) );
                    _symbolUniverse.OnResolveEvent += MetadataReaderUniverseOnResolveEvent;

                }

                return ( _symbolUniverse );
            }
        }

        // Try to resolve the new assembly against the directory of the last assembly loaded.
        /// <summary>
        /// Resolves an assembly name to a physical assembly.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data.</param>
        /// <remarks><para>In simpler times, this event handler would resolve only .winmd files, all of which
        /// were assumed to be in the same folder.</para>
        /// <para>Nowadays, requests come from various teams to generate stub topics from standalone 
        /// metadata assemblies, or even regular .NET assemblies that have implementations. This introduces the
        /// need to resolve against both .winmd files and system assemblies.</para>
        /// <para>Unfortunately, we don't get the extension of an assembly in the provided <see cref="AssemblyName"/>,
        /// so we have to fish around. This is done by creating both a *.dll and *.winmd filename and searching
        /// the directories of the loaded assemblies for matching files. This assumes that there's nothing 
        /// diabolical happening, like both assemblies present in the same or different folders, e.g., "System.dll" 
        /// and "System.winmd".</para>
        /// <para>For this to work, the source assembly called out on the command line must reside in a 
        /// metadata folder with, at least, windows.winmd, and possibly other metadata assemblies, like
        /// windows.foundation.winmd, if the source assembly was built against multiple WinRT metadata files
        /// (instead of the single, big windows.winmd file). Also, any assemblies that the source assembly 
        /// depends on (but not System assemblies) must be present in this folder.</para>
        /// </remarks>
        private static void MetadataReaderUniverseOnResolveEvent( Object sender, System.Reflection.Adds.ResolveAssemblyNameEventArgs e )
        {
            // Get the name of the assembly to resolve.
            string name = e.Name.Name;
            
            // Create both *.dll and *.winmd filenames.
            string dllName = name + _assemblyFileExt;
            string winmdName = name + _metadataAssemblyFileExt;

            // pathToNewAssembly will hold the fully qualified path, if either the 
            // *.dll or *.winmd file is found.
            string pathToNewAssembly = null;

            // Search the folders of all loaded assemblies for both filename flavors.
            for( int i = 0; i < MetadataFolders.Count; i++ )
            {
                // Check in the current folder for the new assembly with the ".dll" extension.
                string dllPath = Path.Combine( MetadataFolders[i], dllName );
                if( File.Exists( dllPath ) )
                {
                    pathToNewAssembly = dllPath;
                    break;
                }

                // The *.dll flavor wasn't found in the current folder, so 
                // check the current folder for the ".winmd" extension.
                dllPath = Path.Combine( MetadataFolders[i], winmdName );
                if( File.Exists( dllPath ) )
                {
                    pathToNewAssembly = dllPath;
                    break;
                }
            }

            // If we found the new assembly, load it. If not, LMR will raise an exception
            // after this event handler returns.
            if( System.IO.File.Exists( pathToNewAssembly ) )
            {
                e.Target = _symbolUniverse.LoadAssemblyFromFile( pathToNewAssembly );
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////
    #region Uniqueness Testing

    // As descrbed in the remarks for the GetMethodHashCode method, if anything
    // goes wrong with uniqueness testing (e.g., GetUniqueMethods) this is the
    // place to look. 

    class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
    {
        public bool Equals( PropertyInfo lhs, PropertyInfo rhs )
        {
            // Check whether the compared objects reference the same data.
            if( Object.ReferenceEquals( lhs, rhs ) )
            {
                return true;
            }

            //Check whether either of the compared objects is null.
            if( Object.ReferenceEquals( lhs, null ) ||
                Object.ReferenceEquals( rhs, null ) )
            {
                return false;
            }

            //if( lhs.DeclaringType != rhs.DeclaringType )
            //{
            //    return false;
            //}

            if( lhs.Name != rhs.Name )
            {
                return false;
            }

            if( lhs.PropertyType != rhs.PropertyType )
            {
                return false;
            }

            return true;
        }

        // If Equals() returns true for a pair of objects, the
        // GetHashCode method must return the same value for these objects.
        public int GetHashCode( PropertyInfo pi )
        {
            //Check whether the object is null
            if( Object.ReferenceEquals( pi, null ) ) return 0;

            //Get hash code for the Name field if it is not null.
            int hashPropertyName = pi.Name == null ? 0 : pi.Name.GetHashCode();

            //Calculate the hash code for the property.
            int hashCode = hashPropertyName.GetHashCode();

            int propertyTypeHashCode = pi.PropertyType.Name.GetHashCode();
            hashCode ^= propertyTypeHashCode;

            //int declaringTypeHashCode = mi.DeclaringType.Name.GetHashCode();
            //hashCode ^= declaringTypeHashCode;

            return hashCode;
        }
    }

    class MethodInfoComparer : IEqualityComparer<MethodInfo>
    {
        public bool Equals( MethodInfo lhs, MethodInfo rhs )
        {
            // Check whether the compared objects reference the same data.
            if( Object.ReferenceEquals( lhs, rhs ) )
            {
                return true;
            }

            //Check whether either of the compared objects is null.
            if( Object.ReferenceEquals( lhs, null ) || 
                Object.ReferenceEquals( rhs, null ) )
            {
                return false;
            }

            if( lhs.ReturnType != rhs.ReturnType )
            {
                return false;
            }

            //if( lhs.DeclaringType != rhs.DeclaringType )
            //{
            //    return false;
            //}

            ParameterInfo[] lhsParams = lhs.GetParameters();
            ParameterInfo[] rhsParams = rhs.GetParameters();

            if( lhsParams.Length == 0 && 
                rhsParams.Length == 0 )
            {
                return true;
            }

            if( lhsParams.Length != rhsParams.Length )
            {
                return false;
            }

            int intersection = lhsParams.Intersect( rhsParams, new ParameterInfoComparer() ).Count();

            if( intersection != lhsParams.Length )
            {
                return false;
            }

            return true;
        }

        // If Equals() returns true for a pair of objects, the
        // GetHashCode method must return the same value for these objects.
        public int GetHashCode( MethodInfo mi )
        {
            int hashCode = TypeUtilities.GetMethodHashCode( mi );

            return hashCode;         }
    }


    class ParameterInfoComparer : IEqualityComparer<ParameterInfo>
    {
        public bool Equals( ParameterInfo lhs, ParameterInfo rhs )
        {
            //Check whether the compared objects reference the same data.
            if( Object.ReferenceEquals( lhs, rhs ) )
            {
                return true;
            }

            //Check whether any of the compared objects is null.
            if( Object.ReferenceEquals( lhs, null ) || Object.ReferenceEquals( rhs, null ) )
            {
                return false;
            }

            if( lhs.Position != rhs.Position )
            {
                return false;
            }

            //if( lhs.Name != rhs.Name )
            //{
            //    return false;
            //}

            if( lhs.ParameterType != rhs.ParameterType )
            {
                return false;
            }

            return true;
        }

        // If Equals() returns true for a pair of objects, the
        // GetHashCode method must return the same value for these objects.
        public int GetHashCode( ParameterInfo pi )
        {
            return TypeUtilities.GetParameterHashCode( pi );
        }
    }

    class EventInfoComparer : IEqualityComparer<EventInfo>
    {
        public bool Equals( EventInfo lhs, EventInfo rhs )
        {
            // Check whether the compared objects reference the same data.
            if( Object.ReferenceEquals( lhs, rhs ) )
            {
                return true;
            }

            //Check whether either of the compared objects is null.
            if( Object.ReferenceEquals( lhs, null ) ||
                Object.ReferenceEquals( rhs, null ) )
            {
                return false;
            }

            //if( lhs.DeclaringType != rhs.DeclaringType )
            //{
            //    return false;
            //}

            if( lhs.Name != rhs.Name )
            {
                return false;
            }

            if( lhs.EventHandlerType != rhs.EventHandlerType )
            {
                return false;
            }

            return true;
        }

        // If Equals() returns true for a pair of objects, the
        // GetHashCode method must return the same value for these objects.
        public int GetHashCode( EventInfo ei )
        {
            //Check whether the object is null
            if( Object.ReferenceEquals( ei, null ) ) return 0;

            //Get hash code for the Name field if it is not null.
            int hashEventName = ei.Name == null ? 0 : ei.Name.GetHashCode();

            //Calculate the hash code for the Event.
            int hashCode = hashEventName.GetHashCode();

            int EventTypeHashCode = ei.EventHandlerType.Name.GetHashCode();
            hashCode ^= EventTypeHashCode;

            //int declaringTypeHashCode = ei.DeclaringType.Name.GetHashCode();
            //hashCode ^= declaringTypeHashCode;

            return hashCode;
        }

        #endregion

        #endregion
    }
}
