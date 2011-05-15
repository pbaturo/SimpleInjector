﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SimpleInjector.Extensions
{
    /// <summary>
    /// Extension methods with non-generic method overloads.
    /// </summary>
    public static class NonGenericRegistrationsExtensions
    {
        private static readonly MethodInfo register = 
            Helpers.GetGenericMethod(c => c.Register<object, object>());

        private static readonly MethodInfo registerSingle = 
            Helpers.GetGenericMethod(c => c.RegisterSingle<object, object>());

        private static readonly MethodInfo registerSingleConcrete =
            Helpers.GetGenericMethod(c => c.RegisterSingle<object>());

        private static readonly MethodInfo registerByFunc = 
            Helpers.GetGenericMethod(c => c.Register<object>((Func<object>)null));

        private static readonly MethodInfo registerAll = 
            Helpers.GetGenericMethod(c => c.RegisterAll<object>((IEnumerable<object>)null));

        private static readonly MethodInfo registerSingleByFunc =
            Helpers.GetGenericMethod(c => c.RegisterSingle<object>((Func<object>)null));

        private static readonly MethodInfo registerSingleByT = 
            Helpers.GetGenericMethod(c => c.RegisterSingle<object>((object)null));

        /// <summary>
        /// Registers that the same instance of type <paramref name="concreteType"/> will be returned every 
        /// time it is requested.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="concreteType">The concrete type that will be registered.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/> or 
        /// <paramref name="concreteType"/> are null references (Nothing in VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="concreteType"/> does not represent
        /// a concrete type.</exception>
        public static void RegisterSingle(this Container container, Type concreteType)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(concreteType, "concreteType");
            Requires.IsConcreteType(concreteType, "concreteType");

            registerSingleConcrete.MakeGenericMethod(concreteType).Invoke(container, null);
        }

        /// <summary>
        /// Registers that the same instance of type <paramref name="implementation"/> will be returned every 
        /// time a <paramref name="serviceType"/> type is requested.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="implementation">The actual type that will be returned when requested.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="implementation"/> are null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> and 
        /// <paramref name="implementation"/> represent the same type or <paramref name="implementation"/> is
        /// no sub type from <paramref name="serviceType"/>.</exception>
        public static void RegisterSingle(this Container container, Type serviceType, Type implementation)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(implementation, "implementation");
            Requires.ServiceIsAssignableFromImplementation(serviceType, implementation, "serviceType");
            Requires.ServiceTypeDiffersFromImplementationType(serviceType, implementation, "serviceType",
                "implementation");

            registerSingle.MakeGenericMethod(serviceType, implementation).Invoke(container, null);
        }

        /// <summary>
        /// Registers the specified delegate that allows constructing a single instance of 
        /// <typeparamref name="TService"/>. This delegate will be called at most once during the lifetime of 
        /// the application.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instanceCreator">The delegate that will be used for creating that single instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="instanceCreator"/> or null references (Nothing in
        /// VB).</exception>
        public static void RegisterSingle(this Container container, Type serviceType,
            Func<object> instanceCreator)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(instanceCreator, "instanceCreator");
            Requires.TypeIsNotOpenGeneric(serviceType, "serviceType");

            // Build the following delegate: () => (ServiceType)instanceCreator();
            object creator = Expression.Lambda(
                Expression.Convert(
                    Expression.Invoke(Expression.Constant(instanceCreator), new Expression[0]),
                    serviceType),
                new ParameterExpression[0])
                .Compile();

            registerSingleByFunc.MakeGenericMethod(serviceType).Invoke(container, new[] { creator });
        }

        /// <summary>
        /// Registers a single instance. This <paramref name="instance"/> must be thread-safe.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instance">The instance to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="instance"/> or null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="instance"/> is
        /// no sub type from <paramref name="serviceType"/>.</exception>
        public static void RegisterSingle(this Container container, Type serviceType, object instance)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(instance, "instance");
            Requires.ServiceIsAssignableFromImplementation(serviceType, instance.GetType(), "serviceType");

            registerSingleByT.MakeGenericMethod(serviceType).Invoke(container, new[] { instance });
        }

        /// <summary>
        /// Registers that a new instance of <paramref name="implementation"/> will be returned every time a
        /// <paramref name="serviceType"/> is requested.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="implementation">The actual type that will be returned when requested.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="implementation"/> or null references (Nothing in
        /// VB).</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceType"/> and 
        /// <paramref name="implementation"/> represent the same type or <paramref name="implementation"/> is
        /// no sub type from <paramref name="serviceType"/>.</exception>
        public static void Register(this Container container, Type serviceType, Type implementation)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(implementation, "implementation");
            Requires.ServiceIsAssignableFromImplementation(serviceType, implementation, "serviceType");
            Requires.ServiceTypeDiffersFromImplementationType(serviceType, implementation, "serviceType",
                "implementation");

            register.MakeGenericMethod(serviceType, implementation).Invoke(container, null);
        }

        /// <summary>
        /// Registers the specified delegate that allows returning instances of <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface to register.</param>
        /// <param name="instanceCreator">The delegate that will be used for creating new instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="instanceCreator"/> or null references (Nothing in
        /// VB).</exception>
        public static void Register(this Container container, Type serviceType, Func<object> instanceCreator)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(instanceCreator, "instanceCreator");

            // Build the following delegate: () => (T)instanceCreator();
            object creator = Expression.Lambda(
                Expression.Convert(
                    Expression.Invoke(Expression.Constant(instanceCreator), new Expression[0]),
                    serviceType),
                new ParameterExpression[0])
                .Compile();

            registerByFunc.MakeGenericMethod(serviceType).Invoke(container, new[] { creator });
        }

        /// <summary>
        /// Registers a <paramref name="collection"/> of elements of type <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="collection">The collection of items to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/> or <paramref name="collection"/> are null references (Nothing in
        /// VB).</exception>
        public static void RegisterAll(this Container container, Type serviceType, IEnumerable collection)
        {
            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(collection, "collection");

            object castedCollection;

            if (typeof(IEnumerable<>).MakeGenericType(serviceType).IsAssignableFrom(collection.GetType()))
            {
                // The collection is a IEnumerable<[ServiceType]>. We can simply cast it. 
                // Better for performance
                castedCollection = collection;
            }
            else
            {
                // The collection is not a IEnumerable<[ServiceType]>. We wrap it in a 
                // CastEnumerator<[ServiceType]> to be able to supply it to the RegisterAll<T> method.
                var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(serviceType);

                castedCollection = castMethod.Invoke(null, new[] { collection });
            }

            registerAll.MakeGenericMethod(serviceType).Invoke(container, new[] { castedCollection });
        }

        /// <summary>
        /// Registers an collection of <paramref name="serviceTypes"/>, which instances will be resolved when
        /// enumerating the set returned when a collection of <paramref name="serviceType"/> objects is 
        /// requested. On enumeration the container is called for each type in the list.
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/>, or <paramref name="implementationTypes"/> are null references
        /// (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="typesToRegister"/> contains a null
        /// (Nothing in VB) element, when the <paramref name="openGenericServiceType"/> is not an open generic
        /// type, or one of the types supplied in <paramref name="typesToRegister"/> does not implement a 
        /// closed version of <paramref name="openGenericServiceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A method without the type parameter already exists. This extension method " +
                "is more intuitive to developers.")]
        public static void RegisterAll<TService>(this Container container, params Type[] serviceTypes)
        {
            RegisterAll(container, typeof(TService), serviceTypes);
        }

        /// <summary>
        /// Registers a collection of instances of <paramref name="implementationTypes"/> to be returned when
        /// a collection of <paramref name="serviceType"/> objects is requested.
        /// </summary>
        /// <typeparam name="TService">The base type or interface for elements in the collection.</typeparam>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="implementationTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/>, or <paramref name="implementationTypes"/> are null references
        /// (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, when the <paramref name="openGenericServiceType"/> is not an open generic
        /// type, or one of the types supplied in <paramref name="typesToRegister"/> does not implement a 
        /// closed version of <paramref name="openGenericServiceType"/>.
        /// </exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "A method without the type parameter already exists. This extension method " +
                "is more intuitive to developers.")]
        public static void RegisterAll<TService>(this Container container, 
            IEnumerable<Type> implementationTypes)
        {
            RegisterAll(container, typeof(TService), implementationTypes);
        }

        /// <summary>
        /// Registers an collection of <paramref name="serviceTypes"/>, which instances will be resolved when
        /// enumerating the set returned when a collection of <paramref name="serviceType"/> objects is 
        /// requested. On enumeration the container is called for each type in the list.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/>, or <paramref name="implementationTypes"/> are null references
        /// (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, when the <paramref name="openGenericServiceType"/> is not an open generic
        /// type, or one of the types supplied in <paramref name="typesToRegister"/> does not implement a 
        /// closed version of <paramref name="openGenericServiceType"/>.
        /// </exception>
        public static void RegisterAll(this Container container, Type serviceType,
            params Type[] serviceTypes)
        {
            RegisterAll(container, serviceType, (IEnumerable<Type>)serviceTypes);
        }

        /// <summary>
        /// Registers an collection of <paramref name="serviceTypes"/>, which instances will be resolved when
        /// enumerating the set returned when a collection of <paramref name="serviceType"/> objects is 
        /// requested. On enumeration the container is called for each type in the list.
        /// </summary>
        /// <param name="container">The container to make the registrations in.</param>
        /// <param name="serviceType">The base type or interface for elements in the collection.</param>
        /// <param name="serviceTypes">The collection of <see cref="Type"/> objects whose instances
        /// will be requested from the container.</param>
        /// <exception cref="ArgumentNullException">Thrown when either <paramref name="container"/>,
        /// <paramref name="serviceType"/>, or <paramref name="implementationTypes"/> are null references
        /// (Nothing in VB).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serviceTypes"/> contains a null
        /// (Nothing in VB) element, when the <paramref name="openGenericServiceType"/> is not an open generic
        /// type, or one of the types supplied in <paramref name="typesToRegister"/> does not implement a 
        /// closed version of <paramref name="openGenericServiceType"/>.
        /// </exception>
        public static void RegisterAll(this Container container, Type serviceType,
            IEnumerable<Type> serviceTypes)
        {
            serviceTypes = serviceTypes != null ? serviceTypes.ToArray() : null;

            Requires.IsNotNull(container, "container");
            Requires.IsNotNull(serviceType, "serviceType");
            Requires.IsNotNull(serviceTypes, "implementationTypes");
            Requires.DoesNotContainNullValues(serviceTypes, "implementationTypes");
            Requires.ServiceIsAssignableFromImplementations(serviceType, serviceTypes, "serviceTypes");

            IEnumerable<object> instances = GetInstanceIterator(container, serviceTypes);

            container.RegisterAll(serviceType, instances);
        }

        private static IEnumerable<object> GetInstanceIterator(Container container,
            IEnumerable<Type> implementationTypes)
        {
            foreach (var implementationType in implementationTypes)
            {
                yield return container.GetInstance(implementationType);
            }
        }
    }
}