using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace ReflectionUtilities
{
    /// <summary>
    /// Provides a read-only representation of a <see cref="System.Type"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use the <see cref="ObservableType"/> class to display and query a <see cref="System.Type"/>.
    /// </para>
    /// <para>
    /// The <see cref="ObservableType"/> class is a read-only view of a <see cref="System.Type"/>. 
    /// Reflection methods such as <see cref="Type.GetProperties"/> have been wrapped with 
    /// properties that enable data binding and LINQ scenarios.
    /// </para>
    /// <para>
    /// Several static methods enable serializing the type and any of its members to XML. 
    /// </para></remarks>
    public class ObservableType
    {
        ///////////////////////////////////////////////////////////////////////
        #region Contruction
        /// <summary>
        /// Creates a new <see cref="ObservableType"/> to encapsulate the specified <see cref="System.Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="System.Type"/> to be encapsulated.</param>
        /// <exception cref="ArgumentNullException"/>
        public ObservableType( Type type )
        {
            if( type != null )
            {
                this._type = type;
            }
            else
            {
                throw new ArgumentNullException( "type", "must be assigned" );
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Public Properties

        /// <summary>
        /// Gets the assembly that hosts the underlying <see cref="System.Type"/>.
        /// </summary>
        public Assembly Assembly
        {
            get
            {
                return this.UnderlyingType.Assembly;
            }
        }

        /// <summary>
        /// Gets the full name of the <see cref="Assembly"/> that hosts the underlying <see cref="System.Type"/>.
        /// </summary>
        public string AssemblyName
        {
            get
            {
                return this.Assembly.FullName;
            }
        }

        /// <summary>
        /// Gets the simple name of the underlying <see cref="System.Type"/>.
        /// </summary>
        /// <remarks>Use the <see cref="FullName"/> property to get the fully qualified name of the <see cref="System.Type"/>.</remarks>
        public string Name
        {
            get
            {
                return this.UnderlyingType.Name;
            }
        }

        /// <summary>
        /// Gets the fully qualified name of the <see cref="System.Type"/>, 
        /// including the namespace of the <see cref="System.Type"/> but not the assembly.
        /// </summary>
        /// <remarks>Use the <see cref="Name"/> property to get the simple name of the <see cref="System.Type"/>.
        /// Use the <see cref="Namespace"/> property to get the namespace of the <see cref="System.Type"/>.
        /// </remarks>
        public string FullName
        {
            get
            {
                return this.UnderlyingType.FullName;
            }
        }

        /// <summary>
        /// Gets the namespace of the <see cref="System.Type"/>.
        /// </summary>
        public string Namespace
        {
            get
            {
                return this.UnderlyingType.Namespace;
            }
        }

        /// <summary>
        /// Gets the encapsulated <see cref="System.Type"/>. 
        /// </summary>
        public Type UnderlyingType
        {
            get
            {
                return this._type;
            }

            set
            {
                if( this._type == null )
                {
                    this._type = value;
                }
                else
                {
                    throw new ArgumentException( "may be set only once", "UnderlyingType" );
                }
            }
        }

        /// <summary>
        /// Gets the type from which the current <see cref="System.Type"/> directly inherits.
        /// </summary>
        /// <remarks><para>The <see cref="BaseType"/> property does not return interfaces. Use the 
        /// <see cref="Interfaces"/> property to get the interfaces that are implemented by 
        /// the current type.</para>
        /// <para>TBD: The base type for value types is always <see cref="ValueType"/>. Apparently,
        /// LMR can't get the underlying system type.
        /// </para>
        /// </remarks>
        public ObservableType BaseType
        {
            get
            {
                if( this._baseType == null )
                {
                    if( this.UnderlyingType.BaseType != null )
                    {
                        this._baseType = new ObservableType( this.UnderlyingType.BaseType );
                    }
                }

                return ( this._baseType );
            }
        }

        /// <summary>
        /// Gets the interfaces that are implemented by the current <see cref="System.Type"/>.
        /// </summary>
        public List<ObservableType> Interfaces
        {
            get
            {
                if( this._interfaces == null )
                {
                    Type[] interfaces = this.UnderlyingType.GetInterfaces();
                    this._interfaces = interfaces.Select( iface => new ObservableType( iface ) ).ToList();
                }

                return this._interfaces;
            }
        }

        /// <summary>
        /// Gets all public and protected members of the current <see cref="System.Type"/>.
        /// </summary>
        public List<MemberInfo> Members
        {
            get
            {
                if( this._members == null )
                {
                    //MemberInfo[] members = this.UnderlyingType.GetMembers(
                    //    BindingFlags.Public | 
                    //    BindingFlags.NonPublic |
                    //    BindingFlags.Instance |
                    //    BindingFlags.Static );

                    this._members = new List<MemberInfo>(); 

                    this._members.AddRange( this.Properties ); 
                    this._members.AddRange( this.Methods );
                    this._members.AddRange( this.Events );
                    this._members.AddRange( this.Fields ); 
                }

                return this._members;
            }
        }

        /// <summary>
        /// Gets all public and protected properties of the current <see cref="System.Type"/>.
        /// </summary>
        public List<PropertyInfo> Properties
        {
            get
            {
                if( this._properties == null )
                {
                    PropertyInfo[] properties = this.UnderlyingType.GetProperties(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance |
                        BindingFlags.Static );

                    // Filter out properties with private accessors.
                    this._properties = properties.Where(
                        pi => !pi.GetAccessors(true)[0].Attributes.HasFlag( MethodAttributes.Private ) ).ToList();
                }

                return this._properties;
            }
        }


        public List<PropertyInfo> InheritedProperties
        {
            get
            {
                if( this._inheritedProperties == null )
                {
                    this._inheritedProperties = new List<PropertyInfo>();

                    if( this.HasBaseType && this.BaseType.HasMethods )
                    {
                        this._inheritedProperties.AddRange( this.BaseType.Properties );
                    }

                    if( this.HasInterfaces )
                    {
                        this._inheritedProperties.AddRange( this.InterfaceProperties );
                    }
                }

                return this._inheritedProperties;
            }
        }


        public List<PropertyInfo> InterfaceProperties
        {
            get
            {
                if( this._interfaceProperties == null )
                {
                    this._interfaceProperties = TypeUtilities.GetAllProperties( this.Interfaces );
                }

                return this._interfaceProperties;
            }
        }

        /// <summary>
        /// Gets all public declared properties of the current <see cref="System.Type"/>, which means
        /// that inherited properties are not included.
        /// </summary>
        public List<PropertyInfo> DeclaredProperties
        {
            get
            {
                if( this._declaredProperties == null )
                {
                    this._declaredProperties = new List<PropertyInfo>();

                    // TBD: For "Bob's" sake, figure out a LINQ query that does this.
                    for( int i = 0; i < this.Properties.Count; i++ )
                    {
                        PropertyInfo property = this.Properties[i];

                        PropertyInfo fubar = this.InheritedProperties.Find( pi => property.Name == pi.Name );

                        if( fubar == null )
                        {
                            this._declaredProperties.Add( property );
                        }
                    }
                }

                return this._declaredProperties;
            }
        }

        /// <summary>
        /// Gets all public methods and constructors of the current <see cref="System.Type"/>. 
        /// </summary>
        public List<MethodBase> MethodBases
        {
            get
            {
                if( this._methodBases == null )
                {
                    this._methodBases = this.Methods.Select( mi => mi as MethodBase ).ToList();

                    // Add the constructors to the other methods.
                    this._methodBases.AddRange( this.Constructors.Select( ci => ci as MethodBase ).ToList() );
                }

                return this._methodBases;
            }
        }

        /// <summary>
        /// Gets all public and protected methods of the current <see cref="System.Type"/>.
        /// </summary>
        public List<MethodInfo> Methods
        {
            get
            {
                if( this._methods == null )
                {
                    MethodInfo[] methods = this.UnderlyingType.GetMethods(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance |
                        BindingFlags.Static );

                    // Filter out private methods.
                    this._methods = methods.Where( 
                        mi => !mi.Attributes.HasFlag( MethodAttributes.Private ) ).ToList();
                }

                return this._methods;
            }
        }

        public List<MethodInfo> InterfaceMethods
        {
            get
            {
                if( this._interfaceMethods == null )
                {
                    this._interfaceMethods = TypeUtilities.GetAllMethods( this.Interfaces );
                }

                return this._interfaceMethods;
            }
        }

        public List<MethodInfo> InheritedMethods
        {
            get
            {
                if( this._inheritedMethods == null )
                {
                    this._inheritedMethods = new List<MethodInfo>();

                    if( this.HasBaseType && this.BaseType.HasMethods )
                    {
                        this._inheritedMethods.AddRange( this.BaseType.Methods );
                    }

                    if( this.HasInterfaces )
                    {
                        this._inheritedMethods.AddRange( this.InterfaceMethods );
                    }
                }

                return this._inheritedMethods;
            }
        }


        /// <summary>
        /// Gets all public declared methods and constructors of the current <see cref="System.Type"/>, 
        /// which means that inherited methods are not included.
        /// </summary>
        public List<MethodBase> DeclaredMethodBases
        {
            get
            {
                if( this._declaredMethodBases == null )
                {
                    this._declaredMethodBases = this.DeclaredMethods.Select( mi => mi as MethodBase ).ToList();

                    // Add the constructors to the other methods.
                    this._declaredMethodBases.AddRange( this.Constructors.Select( ci => ci as MethodBase ) );
                }

                return this._declaredMethodBases;
            }
        }



        /// <summary>
        /// Gets all public declared methods of the current <see cref="System.Type"/>, which means
        /// that inherited methods are not included.
        /// </summary>
        public List<MethodInfo> DeclaredMethods
        {
            get
            {
                if( this._declaredMethods == null )
                {
                    List<MethodInfo> fubarDeclaredOnly = this.UnderlyingType.GetMethods( BindingFlags.DeclaredOnly ).ToList();

                    this._declaredMethods = new List<MethodInfo>();

                    //this._declaredMethods = this.Methods.Where( mi => !this.InheritedMethods.Contains( mi ) ).ToList();
                    // TBD: For "Bob's" sake, figure out a LINQ query that does this.
                    for( int i = 0; i < this.Methods.Count; i++ )
                    {
                        MethodInfo method = this.Methods[i];

                        MethodInfo fubar = this.InheritedMethods.Find( mi => method.Name == mi.Name );

                        if( fubar == null )
                        {
                            this._declaredMethods.Add( method );
                        }
                    }
                }

                return this._declaredMethods;
            }
        }

        /// <summary>
        /// Gets all public declared events of the current <see cref="System.Type"/>, which means
        /// that inherited events are not included.
        /// </summary>
        public List<EventInfo> DeclaredEvents
        {
            get
            {
                if( this._declaredEvents == null )
                {
                    List<EventInfo> fubarDeclaredOnly = this.UnderlyingType.GetEvents( BindingFlags.DeclaredOnly ).ToList();

                    this._declaredEvents = new List<EventInfo>();

                    //this._declaredEvents = this.Events.Where( mi => !this.InheritedEvents.Contains( mi ) ).ToList();
                    // TBD: For "Bob's" sake, figure out a LINQ query that does this.
                    for( int i = 0; i < this.Events.Count; i++ )
                    {
                        EventInfo Event = this.Events[i];

                        EventInfo fubar = this.InheritedEvents.Find( ei => Event.Name == ei.Name );

                        if( fubar == null )
                        {
                            this._declaredEvents.Add( Event );
                        }
                    }
                }

                return this._declaredEvents;
            }
        }


        public List<EventInfo> InterfaceEvents
        {
            get
            {
                if( this._interfaceEvents == null )
                {
                    this._interfaceEvents = TypeUtilities.GetAllEvents( this.Interfaces );
                }

                return this._interfaceEvents;
            }
        }

        public List<EventInfo> InheritedEvents
        {
            get
            {
                if( this._inheritedEvents == null )
                {
                    this._inheritedEvents = new List<EventInfo>();

                    if( this.HasBaseType && this.BaseType.HasEvents )
                    {
                        this._inheritedEvents.AddRange( this.BaseType.Events );
                    }

                    if( this.HasInterfaces )
                    {
                        this._inheritedEvents.AddRange( this.InterfaceEvents );
                    }
                }

                return this._inheritedEvents;
            }
        }


        public List<MemberInfo> DeclaredMembers
        {
            get
            {
                if( this._declaredMembers == null )
                {
                    this._declaredMembers = new List<MemberInfo>();
                    this._declaredMembers.AddRange( this.DeclaredProperties );
                    this._declaredMembers.AddRange( this.DeclaredMethods );
                    this._declaredMembers.AddRange( this.DeclaredEvents );
                }

                return this._declaredMembers;
            }
        }


        public List<MemberInfo> InheritedMembers
        {
            get
            {
                if( this._inheritedMembers == null )
                {
                    this._inheritedMembers = new List<MemberInfo>();
                    this._inheritedMembers.AddRange( this.InheritedProperties );
                    this._inheritedMembers.AddRange( this.InheritedMethods );
                    this._inheritedMembers.AddRange( this.InheritedEvents );
                }

                return this._inheritedMembers;
            }
        }



        /// <summary>
        /// Gets all public and protected constructors of the current <see cref="System.Type"/>.
        /// </summary>
        public List<ConstructorInfo> Constructors
        {
            get
            {
                if( this._constructors == null )
                {
                    ConstructorInfo[] ctors = this.UnderlyingType.GetConstructors(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance |
                        BindingFlags.Static );

                    // Filter out private constructors.
                    this._constructors = ctors.Where(
                        ci => !ci.Attributes.HasFlag( MethodAttributes.Private ) ).ToList();
                }

                return this._constructors;
            }
        }

        /// <summary>
        /// Gets all public and protected events of the current <see cref="System.Type"/>.
        /// </summary>
        public List<EventInfo> Events
        {
            get
            {
                if( this._events == null )
                {
                    EventInfo[] events = this.UnderlyingType.GetEvents(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance |
                        BindingFlags.Static );

                    // Filter out events with private accessors.
                    this._events = events.Where(
                            ei => !ei.GetAddMethod(true).Attributes.HasFlag( MethodAttributes.Private ) ).ToList();
                }

                return this._events;
            }
        }

        /// <summary>
        /// Gets all public and protected fields of the current <see cref="System.Type"/>.
        /// </summary>
        public List<FieldInfo> Fields
        {
            get
            {
                if( this._fields == null )
                {
                    FieldInfo[] fields = this.UnderlyingType.GetFields(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance |
                        BindingFlags.Static );

                    // Filter out the value__ field.
                    var filedsNoValue__ = fields.Where(
                        fi => fi.Name != "value__" );

                    // Filter out private fields.
                    this._fields = filedsNoValue__.Where(
                        fi => !fi.Attributes.HasFlag( FieldAttributes.Private ) ).ToList();
                }

                return this._fields;
            }
        }

        /// <summary>
        /// Gets the arguments of a generic type or the type parameters of a 
        /// generic type definition.
        /// </summary>
        /// <remarks>
        /// TBD: LMR value types are broken. </remarks>
        public List<ObservableType> GenericArguments
        {
            get
            {
                if( this._genericArguments == null )
                {
                    Type[] genericArguments = this.UnderlyingType.GetGenericArguments();
                    this._genericArguments = genericArguments.Select( ga => new ObservableType( ga ) ).ToList();
                }

                return this._genericArguments;
            }
        }

        /// <summary>
        /// Gets the attributes that are applied to the current <see cref="System.Type"/>.
        /// </summary>
        public List<CustomAttributeData> Attributes
        {
            get
            {
                if( this._attributes == null )
                {
                    IList<CustomAttributeData> customAttributes = this.UnderlyingType.GetCustomAttributesData();

                    if( customAttributes.Count > 0 )
                    {
                        this._attributes = customAttributes.ToList();

                        //string name = customAttributes[0].GetType().Name;
                        //this._attributes = customAttributes.Select( ca => ca.ToString() ).ToList();
                    }
                    else
                    {
                        this._attributes = new List<CustomAttributeData>();
                    }
                }

                return this._attributes;
            }
        }

        /// <summary>
        /// Gets all types that are nested within the current <see cref="System.Type"/>.
        /// </summary>
        public List<ObservableType> NestedTypes
        {
            get
            {
                if( this._nestedTypes == null )
                {
                    Type[] nestedTypes = this.UnderlyingType.GetNestedTypes(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance |
                        BindingFlags.Static );

                    this._nestedTypes = nestedTypes.Select( nt => new ObservableType( nt ) ).ToList();
                }

                return this._nestedTypes;
            }
        }


        /// <summary>
        /// Gets the attributes that are applied to the specified member.
        /// </summary>
        /// <param name="memberInfo">The member to get attributes for.</param>
        /// <returns>The custom attributes that applied to <paramref name="memberInfo"/>.</returns>
        public static List<CustomAttributeData> GetMemberAttributes( MemberInfo memberInfo )
        {
            IList<CustomAttributeData> customAttributes = memberInfo.GetCustomAttributesData();

            return customAttributes.ToList();
        }



        /// <summary>
        /// Gets the attributes that are applied to the specified method.
        /// </summary>
        /// <param name="mi">The method to get attriubutes for.</param>
        /// <returns>The custom attributes that applied to <paramref name="mi"/>.</returns>
        public static List<CustomAttributeData> GetMethodAttributes( MethodInfo mi )
        {
            IList<CustomAttributeData> customAttributes = mi.GetCustomAttributesData();

            return customAttributes.ToList();
        }


        /// <summary>
        /// Gets the attributes that are applied to the specified event.
        /// </summary>
        /// <param name="ei">The event to get attriubutes for.</param>
        /// <returns>The custom attributes that applied to <paramref name="ei"/>.</returns>
        public static List<CustomAttributeData> GetEventAttributes( EventInfo ei )
        {
            IList<CustomAttributeData> customAttributes = ei.GetCustomAttributesData();

            return customAttributes.ToList();
        }


        //public List<ObservableType> DerivedTypes
        //{
        //    get
        //    {
        //        if( this._derivedTypes == null )
        //        {
        //            this._derivedTypes = Utilities.FindDerivedTypes( this, 
        //        }

        //        return this._derivedTypes;
        //    }
        //}

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> derives from
        /// a base type.
        /// </summary>
        public bool HasBaseType
        {
            get
            {
                return ( this.UnderlyingType.BaseType != null );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> implements
        /// any constructors.
        /// </summary>
        public bool HasConstructors
        {
            get
            {
                return ( this.Constructors.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> implements
        /// any interfaces.
        /// </summary>
        public bool HasInterfaces
        {
            get
            {
                return ( this.Interfaces.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> implements
        /// any public methods.
        /// </summary>
        public bool HasMethods
        {
            get
            {
                return ( this.Methods.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> implements
        /// any public properties.
        /// </summary>
        public bool HasProperties
        {
            get
            {
                return ( this.Properties.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> implements
        /// any public events.
        /// </summary>
        public bool HasEvents
        {
            get
            {
                return ( this.Events.Count > 0 );
            }
        }

        public bool HasWebHostHiddenAttribute
        {
            get
            {
                bool hasWebHostHiddenAttribute = false;

                if( this.HasAttributes )
                {
                    var whha = this.Attributes.Find(
                        a => TypeUtilities.GetAttributeName( a, false ) == webHostHiddenAttributeName );
                    if( whha != null )
                    {
                        hasWebHostHiddenAttribute = true;
                    }
                }

                return hasWebHostHiddenAttribute;
            }
        }


        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> exposes
        /// any public fields.
        /// </summary>
        public bool HasFields
        {
            get
            {
                return ( this.Fields.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> has 
        /// any metadata attributes applied.
        /// </summary>
        public bool HasAttributes
        {
            get
            {
                return ( this.Attributes.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> implements
        /// any public nested classes.
        /// </summary>
        public bool HasNestedTypes
        {
            get
            {
                return ( this.NestedTypes.Count > 0 );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> is 
        /// an enumeration.
        /// </summary>
        public bool IsEnum
        {
            get
            {
                return this.UnderlyingType.IsEnum;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> is 
        /// a struct.
        /// </summary>
        /// <remarks>
        /// Reflection doesn't provide a handy property, so the following test is performed:
        /// IsStruct returns true if the current type is a value type and it is not an enum.
        /// </remarks>
        public bool IsStruct
        {
            get
            {
                return ( this.IsValueType && !this.IsEnum );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> is a class.
        /// </summary>
        public bool IsClass
        {
            get
            {
                return this.UnderlyingType.IsClass;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> is an
        /// abstract class.
        /// </summary>
        public bool IsAbstract
        {
            get
            {
                return this.UnderlyingType.IsAbstract;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> is an
        /// attribute.
        /// </summary>
        public bool IsAttribute
        {
            get
            {
                bool isAttribute = false;

                // TBD: string literal
                // TBD: walk the inheritance hierarchy?
                if( this.HasBaseType && this.BaseType.Name.EndsWith( "Attribute" ) )
                {
                    isAttribute = true;
                }

                return isAttribute;
            }
        }


        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> is a 
        /// generic type.
        /// </summary>
        public bool IsGeneric
        {
            get
            {
                return( 
                    this.UnderlyingType.IsGenericType || 
                    this.UnderlyingType.ContainsGenericParameters );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> is
        /// an interface.
        /// </summary>
        public bool IsInterface
        {
            get
            {
                return this.UnderlyingType.IsInterface;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> is 
        /// nested in a containing class.
        /// </summary>
        public bool IsNested
        {
            get
            {
                return this.UnderlyingType.IsNested;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> disallows derivation.
        /// </summary>
        public bool IsSealed
        {
            get
            {
                return this.UnderlyingType.IsSealed;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> is a 
        /// value type, such as <see cref="integer"/>.
        /// </summary>
        public bool IsValueType
        {
            get
            {
                return this.UnderlyingType.IsValueType;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> is a 
        /// delegate.
        /// </summary>
        public bool IsDelegate
        {
            get
            {
                return (
                    this.HasBaseType &&
                    this.UnderlyingType.BaseType.Name == "MulticastDelegate" );
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the current <see cref="System.Type"/> is a base type. 
        /// </summary>
        /// <remarks>The current type must be a class or an interface. 
        /// See the <see cref="TypeUtilities.IsBaseType"/> method for details on the definition of base type.</remarks>
        public bool IsBaseType
        {
            get
            {
                return TypeUtilities.IsBaseType( this );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="System.Type"/> has
        /// the public access modifier.
        /// </summary>
        public bool IsPublic
        {
            get
            {
                return this.UnderlyingType.IsPublic;
            }
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Public Methods

        /// <summary>
        /// Gets a value that indicateds whether the specified <see cref="MethodBase"/>
        /// represents an overloaded method.
        /// </summary>
        /// <param name="method">The method to match. </param>
        /// <returns>true if <paramref name="method"/> is overloaded; otherwise, false.</returns>
        public bool IsOverload( MethodBase method )
        {
            return( this.IsOverload( method.Name ) );
        }

        /// <summary>
        /// Gets a value that indicateds whether the specified method is overloaded.
        /// </summary>
        /// <param name="methodName">The method to match. </param>
        /// <returns>true if <paramref name="methodName"/> is overloaded; otherwise, false.</returns>
        /// <remarks>The <see cref="DeclaredMethodBases"/> property is queried, so constructors are included.</remarks>
        public bool IsOverload( string methodName )
        {
            int occurrences = MethodBases.Count( mi => mi.Name == methodName );

            return ( occurrences > 1 );
        }

        /// <summary>
        /// Gets the overloads for the specified method.
        /// </summary>
        /// <param name="methodName">The method to get overloads for.</param>
        /// <returns>A list of overloaded methods.</returns>
        /// <remarks>The <see cref="DeclaredMethodBases"/> property is queried, so constructors are included.</remarks>
        public List<MethodBase> GetOverloads( string methodName )
        {
            List<MethodBase> overloads = MethodBases.Where( mi => mi.Name == methodName ).ToList();

            return overloads;
        }

        /// <summary>
        /// Gets the overloads for the specified method.
        /// </summary>
        /// <param name="methodName">The method to get overloads for.</param>
        /// <returns>A list of overloaded methods.</returns>
        public List<MethodBase> GetOverloads( MethodBase method )
        {
            return( GetOverloads( method.Name ) ); 
        }

        /// <summary>
        /// Serializes the specified assembly's types to XML.
        /// </summary>
        /// <param name="assembly">An <see cref="Assembly"/> to represent as an XML document.</param>
        /// <returns>An <see cref="XElement"/> that represents the specified <see cref="Assembly"/>. </returns>
        //public static XElement SerializeAssembly( Assembly assembly )
        //{
        //    XElement assemblyElement = new XElement( assemblyElementName, null );
        //    assemblyElement.Add( new XAttribute( nameAttributeName, assembly.GetName().Name ) );
        //    assemblyElement.Add( new XAttribute( versionAttributeName, assembly.GetName().Version ) );

        //    IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes( assembly );

        //    for( int i = 0; i < attributes.Count; i++ )
        //    {
        //        assemblyElement.Add( SerializeAttributeData( attributes[i] ) );
        //    }

        //    List<ObservableType> observableTypes = Utilities.GetObservableTypes( assembly );

        //    for( int i = 0; i < observableTypes.Count; i++ )
        //    {
        //        ObservableType type = observableTypes[i];

        //        assemblyElement.Add( type.SerializeType() );
        //    }

        //    return assemblyElement;
        //}

        /// <summary>
        /// Serializes the specified <see cref="ObservableType"/> to XML.
        /// </summary>
        /// <param name="type">An <see cref="ObservableType"/> to represent as an XML document.</param>
        /// <returns>An <see cref="XElement"/> that represents the specified <see cref="ObservableType"/>.</returns>
        public static XElement SerializeType( ObservableType type )
        {
            XElement typeElement = new XElement( typeElementName, null );
            typeElement.Add( new XAttribute( nameAttributeName, type.Name ) );

            // Serialize the type's type: interface, class, or value type.

            if( type.IsInterface )
            {
                typeElement.Add( new XAttribute( typeAttributeName, interfaceAttributeValue ) );
            }

            if( type.IsClass )
            {
                typeElement.Add( new XAttribute( typeAttributeName, classAttributeValue ) );
            }

            if( type.IsValueType )
            {
                // Value type can be either enum or struct.
                if( type.IsEnum )
                {
                    typeElement.Add( new XAttribute( typeAttributeName, enumTypeAttributeValue ) );
                }
                else
                {
                    typeElement.Add( new XAttribute( typeAttributeName, structTypeAttributeValue ) );
                }
            }

            // Serialize the base type.
            if( type.HasBaseType )
            {
                if( !type.IsValueType )
                {
                    typeElement.Add( new XAttribute( baseTypeElementName, type.BaseType.Name ) );
                }
            }

            // Serialize the attributes that are applied to the type.
            if( type.HasAttributes )
            {
                for( int i = 0; i < type.Attributes.Count; i++ )
                {
                    typeElement.Add( SerializeAttributeData( type.Attributes[i] ) );
                }
            }

            // Serialize the type's properties.
            if( type.HasProperties )
            {
                for( int i = 0; i < type.Properties.Count; i++ )
                {
                    typeElement.Add( SerializePropertyInfo( type.Properties[i] ) );
                }
            }

            // Serialize the type's methods.
            if( type.HasMethods )
            {
                for( int i = 0; i < type.Methods.Count; i++ )
                {
                    typeElement.Add( SerializeMethodInfo( type.Methods[i] ) );
                }
            }

            // Serialize the type's events.
            if( type.HasEvents )
            {
                for( int i = 0; i < type.Events.Count; i++ )
                {
                    typeElement.Add( SerializeEventInfo( type.Events[i] ) );
                }
            }

            // Serialize the type's interfaces.
            if( type.HasInterfaces )
            {
                for( int i = 0; i < type.Interfaces.Count; i++ )
                {
                    typeElement.Add( new XElement( interfaceElementName, type.Interfaces[i].Name ) );
                }
            }

            // Serialize the type's fields.
            if( type.HasFields )
            {
                for( int i = 0; i < type.Fields.Count; i++ )
                {
                    typeElement.Add( SerializeFieldInfo( type.Fields[i] ) );
                }
            }

            // Serialize generic arguments.
            if( type.IsGeneric )
            {
                typeElement.Add( new XElement( isGenericAttributeName, type.IsGeneric ) );

                for( int i = 0; i < type.GenericArguments.Count; i++ )
                {
                    typeElement.Add( new XElement( genericArgumentTypeElementName, type.GenericArguments[i].UnderlyingType.Name ) );
                }
            }
            return typeElement;
        }

        /// <summary>
        /// Serializes the current type to XML.
        /// </summary>
        /// <returns>An <see cref="XElement"/> that represents the current <see cref="ObservableType"/>.</returns>
        public XElement SerializeType()
        {
            return SerializeType( this );
        }

        /// <summary>
        /// Serializes the specified <see cref="CustomAttributeData"/> to XML.
        /// </summary>
        /// <param name="attributeData">A <see cref="CustomAttributeData"/> that represents
        /// a metadata attribute.</param>
        /// <returns>An <see cref="XElement"/> that represents <paramref name="attributeData"/>.</returns>
        /// <remarks>
        /// <para>
        /// Use the <see cref="SerializeAttributeData"/> method to serialize an IDL attribute to XML.
        /// </para>
        /// <para>
        /// TBD: There is no ordering property for constructor arguments, such as <see cref="ParameterInfo.Position"/>, 
        /// so assume that the argument order is the same as the <see cref="CustomAttributeData.ConstructorArguments"/>
        /// list.
        /// </para>
        /// <para>
        /// LMR doesn't parse the attribute name from the token blob, so this method finds the 
        /// attribute name substring.
        /// </para>
        /// <para>
        /// Integers in some IDL attributes, such as GuidAttribute, are represented in hexadecimal. These
        /// elements have an additional XML attribute, named "Reperesentation", with a value of "Hexadecimal".  
        /// </para>
        /// </remarks>
        public static XElement SerializeAttributeData( CustomAttributeData attributeData )
        {
            XElement attributeElement = new XElement( attributeElementName, null );

            // Extract the attribute name substring.
            string attributeName = TypeUtilities.GetAttributeName( attributeData, true );
            attributeElement.Add( new XAttribute( nameAttributeName, attributeName ) );

            bool isGuidAttribute = attributeName.Contains( guidAttributeString );
            bool isVersionAttribute = attributeName == versionAttributeString;
            bool isActivatableAttribute = attributeName.Contains( activatableAttributeString );
            bool isCustomTrustLevelAttribute = attributeName.Contains( customTrustLevelAttributeString );

            if( isCustomTrustLevelAttribute )
            {
                // Task #260495: Unresolved type 'Windows.Foundation.RoTrustLevel' in Windows.ApplicationModel project 
                // http://win8tfs:8080/tfs/web/Index.aspx?puri=vstfs%3A%2F%2F%2FClassification%2FTeamProject%2F655db467-bcb2-4f98-8521-d86dbe3f405e
                // TBD: Currently, LMR throws when parsing CustomTrustLevelAttribute constructor arguments.
                Console.WriteLine( "ObservableType.SerializeAttributeData: Found CustomTrustLevelAttribute -- ignoring constructor arguments. See TFS #260495." );
                return attributeElement;
            }

            // Create XML attributes for the constructor arguments. 
            // TBD: Cross our fingers that the array order is the same as the actual order
            // of the constructor arguments in code.
            for( int i = 0; i < attributeData.ConstructorArguments.Count; i++ )
            {
                XElement constructorElement = new XElement( constructorArgumentElementName, null );
                constructorElement.Add( new XAttribute( argumentTypeAttributeName, attributeData.ConstructorArguments[i].ArgumentType.Name ) );

                object value = attributeData.ConstructorArguments[i].Value;

                string representation = null;

                // TBD: Special-case the GuidAttribute. LMR parses some of the ctor arguments
                // into individual bytes, which causes the GUID to render differently 
                // from the IDL.
                // 
                // For example, this IDL 
                //
                // [version(NTDDI_WIN8), uuid(857C4CED-4154-476B-85DD-C36250ED5861)]
                // interface IChef : IInspectable { 
                //
                // is parsed by LMR as
                // 
                // 857C4CED  4154 476B  85 DD C3 62 50 ED 58 61
                // 
                // The integer constructor arguments are rendered in hexadecimal.
                if( isGuidAttribute )
                {
                    //object value = attributeData.ConstructorArguments[i].Value;

                    if( attributeData.ConstructorArguments[i].ArgumentType.Name == "UInt16" )
                    {
                        UInt16 uintValue = (UInt16)value;
                        value = String.Format( "{0:X}", uintValue );
                    }
                    else if( attributeData.ConstructorArguments[i].ArgumentType.Name == "UInt32" )
                    {
                        UInt32 uintValue = (UInt32)value;
                        value = String.Format( "{0:X}", uintValue );
                    }
                    else if( attributeData.ConstructorArguments[i].ArgumentType.Name == "Byte" )
                    {
                        Byte uintValue = (Byte)value;
                        value = String.Format( "{0:X}", uintValue );
                    }

                    representation = argumentRepresentationAttributeHexValue;
                }

                // Special-case the Windows.Foundation.VersionAttribute to render the 
                // integer constructor argument in hexadecimal.
                if( isVersionAttribute )
                {
                    UInt32 uintValue = (UInt32)value;
                    value = String.Format( "{0:X}", uintValue );

                    representation = argumentRepresentationAttributeHexValue;
                }

                // Special-case the ActivatableAttribute to render the integer constructor argument 
                // in hexadecimal.
                //
                // TBD: What to do about the case when IDL specifies an interface argument:
                //
                // [version(NTDDI_WIN8), activatable(IChefFactory, NTDDI_WIN8)]
                // runtimeclass Chef 
                //
                // It's not clear how the IChefFactory argument should be rendered. For now, it's hex,
                // but that looks odd:
                //
                // <Attribute Name="Windows.Foundation.ActivatableAttribute">
                //    <ConstructorArgument ArgumentType="UInt32" ArgumentValue="200000C" Representation="Hexadecimal" /> 
                //    <ConstructorArgument ArgumentType="UInt32" ArgumentValue="8000000" Representation="Hexadecimal" /> 
                // </Attribute>
                //
                if( isActivatableAttribute )
                {
                    //UInt32 uintValue = (UInt32)value;
                    value = "TBD"; //String.Format( "{0:X}", uintValue );

                    representation = argumentRepresentationAttributeHexValue;
                }

                constructorElement.Add( new XAttribute( argumentValueAttributeName, value ) );

                // Add the Representation XML attribute if the argument is rendered in hex.
                if( representation != null )
                {
                    constructorElement.Add( new XAttribute( argumentRepresentationAttributeName, argumentRepresentationAttributeHexValue ) );
                }

                attributeElement.Add( constructorElement );
            }

            return attributeElement;
        }

        /// <summary>
        /// Serializes the specified <see cref="MethodInfo"/> to XML.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> to represent as an XML element.</param>
        /// <returns>An <see cref="XElement"/> that represents the specified <see cref="MethodInfo"/>.</returns>
        public static XElement SerializeMethodInfo( MethodInfo methodInfo )
        {
            XElement methodElement = new XElement( methodElementName, null );
            methodElement.Add( new XAttribute( nameAttributeName, methodInfo.Name ) );
            methodElement.Add( new XAttribute( returnTypeAttributeName, methodInfo.ReturnType.Name ) );

            ParameterInfo[] parameters = methodInfo.GetParameters();
            for( int i = 0; i < parameters.Length; i++ )
            {
                methodElement.Add( SerializeParameterInfo( parameters[i] ) );
            }

            IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes( methodInfo );

            for( int i = 0; i < attributes.Count; i++ )
            {
                methodElement.Add( SerializeAttributeData( attributes[i] ) );
            }
            return methodElement;
        }

        /// <summary>
        /// Serializes the specified <see cref="ParameterInfo"/> to XML.
        /// </summary>
        /// <param name="methodInfo">A <see cref="ParameterInfo"/> to represent as an XML element.</param>
        /// <returns>An <see cref="XElement"/> that represents the specified <see cref="ParameterInfo"/>.</returns>
        /// <remarks>
        /// <para>
        /// An out parameter is represented as a pointer in IDL, or as a pointer-to-pointer in the case of
        /// interface pointers. LMR renders this as a reference and appends the '&' character to the type name.
        /// The <see cref="SerializeParameterInfo"/> method removes the '&' character for all out parameters.
        /// Tools which consume the XML generated from this method may test the IsOut XML attribute and add pointer 
        /// syntax as appropriate.
        /// </para>
        /// </remarks>
        public static XElement SerializeParameterInfo( ParameterInfo parameterInfo )
        {
            XElement parameterElement = new XElement( parameterElementName, null );
            parameterElement.Add( new XAttribute( nameAttributeName, parameterInfo.Name ) );

            string parameterTypeName = parameterInfo.ParameterType.Name;

            // For out parameters, remove the '&' character that LMR appends to the type name. 
            if( parameterInfo.IsOut )
            {
                if( parameterTypeName.Contains( '&' ) )
                {
                    parameterTypeName = parameterTypeName.Remove( parameterTypeName.Length - 1, 1 );
                }
            }

            parameterElement.Add( new XAttribute( typeAttributeName, parameterTypeName ) );

            parameterElement.Add( new XAttribute( positionAttributeName, parameterInfo.Position ) );
            parameterElement.Add( new XAttribute( inAttributeName, parameterInfo.IsIn ) );
            parameterElement.Add( new XAttribute( outAttributeName, parameterInfo.IsOut ) );
            parameterElement.Add( new XAttribute( retvalAttributeName, parameterInfo.IsRetval ) );
            parameterElement.Add( new XAttribute( optionalAttributeName, parameterInfo.IsOptional ) );


            // TBD: LMR doesn't do DefaultValue and RawDefaultValue.
            //parameterElement.Add( new XAttribute( defaultValueAttributeName, parameterInfo.RawDefaultValue ) );

            return parameterElement;
        }

        /// <summary>
        /// Serializes the specified <see cref="PropertyInfo"/> to XML.
        /// </summary>
        /// <param name="methodInfo">A <see cref="PropertyInfo"/> to represent as an XML element.</param>
        /// <returns>An <see cref="XElement"/> that represents the specified <see cref="PropertyInfo"/>.</returns>
        public static XElement SerializePropertyInfo( PropertyInfo propertyInfo )
        {
            XElement propertyElement = new XElement( propertyElementName, null );
            propertyElement.Add( new XAttribute( nameAttributeName, propertyInfo.Name ) );
            propertyElement.Add( new XAttribute( typeAttributeName, propertyInfo.PropertyType ) );

            IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes( propertyInfo );

            for( int i = 0; i < attributes.Count; i++ )
            {
                propertyElement.Add( SerializeAttributeData( attributes[i] ) );
            }

            return propertyElement;
        }

        /// <summary>
        /// Serializes the specified <see cref="EventInfo"/> to XML.
        /// </summary>
        /// <param name="methodInfo">A <see cref="EventInfo"/> to represent as an XML element.</param>
        /// <returns>An <see cref="XElement"/> that represents the specified <see cref="EventInfo"/>.</returns>
        public static XElement SerializeEventInfo( EventInfo eventInfo )
        {
            XElement eventElement = new XElement( eventElementName, null );
            eventElement.Add( new XAttribute( nameAttributeName, eventInfo.Name ) );

            IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes( eventInfo );

            for( int i = 0; i < attributes.Count; i++ )
            {
                eventElement.Add( SerializeAttributeData( attributes[i] ) );
            }

            return eventElement;
        }

        /// <summary>
        /// Serializes the specified <see cref="FieldInfo"/> to XML.
        /// </summary>
        /// <param name="fieldInfo">A <see cref="FieldInfo"/> to represent as an XML element.</param>
        /// <returns>An <see cref="XElement"/> that represents the specified <see cref="FieldInfo"/>.</returns>
        public static XElement SerializeFieldInfo( FieldInfo fieldInfo )
        {
            XElement fieldElement = new XElement( fieldElementName, null );
            fieldElement.Add( new XAttribute( nameAttributeName, fieldInfo.Name ) );
            fieldElement.Add( new XAttribute( typeAttributeName, fieldInfo.FieldType ) );

            IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes( fieldInfo );

            for( int i = 0; i < attributes.Count; i++ )
            {
                fieldElement.Add( SerializeAttributeData( attributes[i] ) );
            }

            return fieldElement;
        }

        /// <summary>
        /// Returns a <see cref="string"/> representing the name of the current <see cref="System.Type"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> representing the name of the current <see cref="System.Type"/>.</returns>
        public override string ToString()
        {
            return this.UnderlyingType.ToString();
        }

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Private Fields

        private Type _type;
        private ObservableType _baseType;
        private List<ObservableType> _interfaces;
        private List<MemberInfo> _members;
        private List<MemberInfo> _declaredMembers;
        private List<MemberInfo> _inheritedMembers;
        private List<PropertyInfo> _properties;
        private List<PropertyInfo> _declaredProperties;
        private List<PropertyInfo> _inheritedProperties;
        private List<PropertyInfo> _interfaceProperties;
        private List<MethodBase> _methodBases;
        private List<MethodInfo> _methods;
        private List<MethodInfo> _inheritedMethods;
        private List<MethodInfo> _interfaceMethods;
        private List<MethodBase> _declaredMethodBases;
        private List<MethodInfo> _declaredMethods;
        private List<ConstructorInfo> _constructors;
        private List<EventInfo> _events;
        private List<EventInfo> _declaredEvents;
        private List<EventInfo> _inheritedEvents;
        private List<EventInfo> _interfaceEvents;
        private List<FieldInfo> _fields;
        private List<CustomAttributeData> _attributes;
        private List<ObservableType> _nestedTypes;
        private List<ObservableType> _genericArguments;

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region Misc. Strings

        private const string versionAttributeString = "Windows.Foundation.VersionAttribute";
        private const string activatableAttributeString = "ActivatableAttribute";
        private const string guidAttributeString = "GuidAttribute";
        private const string customTrustLevelAttributeString = "CustomTrustLevelAttribute";

        #endregion

        ///////////////////////////////////////////////////////////////////////
        #region XML Element and Attribute Name Strings

        // Misc. element and attribute names
        private const string assemblyElementName = "Assembly";
        private const string typeElementName = "Type";
        private const string typeAttributeName = "Type";
        private const string nameAttributeName = "Name";
        private const string versionAttributeName = "Version";
        private const string isValueTypeAttribute = "IsValueType";
        private const string attributeElementName = "Attribute";
        private const string attributeAttributeName = "Attribute";
        private const string constructorArgumentElementName = "ConstructorArgument";
        private const string propertyElementName = "Property";
        private const string baseTypeElementName = "BaseType";
        private const string argumentTypeAttributeName = "ArgumentType";
        private const string argumentValueAttributeName = "ArgumentValue";
        private const string argumentRepresentationAttributeName = "Representation";
        private const string argumentRepresentationAttributeHexValue = "Hexadecimal";
        private const string genericArgumentTypeElementName = "GenericType_THIS_IS_A_BUG";
        private const string isGenericAttributeName = "IsGeneric";
        private const string webHostHiddenAttributeName = "WebHostHiddenAttribute";

        private const string eventElementName = "Event";
        private const string interfaceAttributeValue = "Interface";
        private const string interfaceElementName = "Interface";
        private const string classAttributeValue = "Class";
        private const string enumTypeAttributeValue = "enum";
        private const string structTypeAttributeValue = "struct";
        private const string fieldAttributeValue = "Field";
        private const string fieldElementName = "Field";

        // Method-related element and attribute names
        private const string methodElementName = "Method";
        private const string abstractAttributeName = "IsAbstract";
        private const string staticAttributeName = "IsStatic";
        private const string virtualAttributeName = "IsVirtual";
        private const string genericMethodAttributeName = "IsGenericMethod";
        private const string callingConventionAttributeName = "CallingConvention";
        private const string returnTypeAttributeName = "ReturnType";

        // Parameter-related element and attribute names
        private const string parameterElementName = "Parameter";
        private const string positionAttributeName = "Position";
        private const string inAttributeName = "IsIn";
        private const string outAttributeName = "IsOut";
        private const string retvalAttributeName = "IsRetval";
        private const string optionalAttributeName = "IsOptional";
        private const string defaultValueAttributeName = "DefaultValue";

        #endregion
    }
}
