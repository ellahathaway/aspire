// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Sockets;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;

namespace Aspire.Hosting;

public static class ResourceBuilderExtensions
{
    private const string ConnectionStringEnvironmentName = "ConnectionStrings__";

    public static IDistributedApplicationResourceBuilder<T> WithEnvironment<T>(this IDistributedApplicationResourceBuilder<T> builder, string name, string? value) where T : IDistributedApplicationResource
    {
        return builder.WithAnnotation(new EnvironmentCallbackAnnotation(name, () => value ?? string.Empty));
    }

    public static IDistributedApplicationResourceBuilder<T> WithEnvironment<T>(this IDistributedApplicationResourceBuilder<T> builder, string name, Func<string> callback) where T : IDistributedApplicationResourceWithEnvironment
    {
        return builder.WithAnnotation(new EnvironmentCallbackAnnotation(name, callback));
    }

    public static IDistributedApplicationResourceBuilder<T> WithEnvironment<T>(this IDistributedApplicationResourceBuilder<T> builder, Action<EnvironmentCallbackContext> callback) where T : IDistributedApplicationResourceWithEnvironment
    {
        return builder.WithAnnotation(new EnvironmentCallbackAnnotation(callback));
    }

    private static bool ContainsAmbiguousEndpoints(IEnumerable<AllocatedEndpointAnnotation> endpoints)
    {
        // An ambiguous endpoint is where any scheme (
        return endpoints.GroupBy(e => e.UriScheme).Any(g => g.Count() > 1);
    }

    private static Action<EnvironmentCallbackContext> CreateServiceReferenceEnvironmentPopulationCallback(ServiceReferenceAnnotation serviceReferencesAnnotation)
    {
        return (context) =>
        {
            var name = serviceReferencesAnnotation.Resource.Name;

            var allocatedEndPoints = serviceReferencesAnnotation.Resource.Annotations
                .OfType<AllocatedEndpointAnnotation>()
                .Where(a => serviceReferencesAnnotation.UseAllBindings || serviceReferencesAnnotation.BindingNames.Contains(a.Name));

            var containsAmiguousEndpoints = ContainsAmbiguousEndpoints(allocatedEndPoints);

            var i = 0;
            foreach (var allocatedEndPoint in allocatedEndPoints)
            {
                var bindingNameQualifiedUriStringKey = $"services__{name}__{i++}";
                context.EnvironmentVariables[bindingNameQualifiedUriStringKey] = allocatedEndPoint.BindingNameQualifiedUriString;

                if (!containsAmiguousEndpoints)
                {
                    var uriStringKey = $"services__{name}__{i++}";
                    context.EnvironmentVariables[uriStringKey] = allocatedEndPoint.UriString;
                }
            }
        };
    }

    public static IDistributedApplicationResourceBuilder<TDestination> WithReference<TDestination>(this IDistributedApplicationResourceBuilder<TDestination> builder, IDistributedApplicationResourceBuilder<IDistributedApplicationResourceWithConnectionString> source, string? connectionName = null, bool optional = false)
        where TDestination : IDistributedApplicationResourceWithEnvironment
    {
        var resource = source.Resource;
        connectionName ??= resource.Name;

        return builder.WithEnvironment(context =>
        {
            var connectionStringName = $"{ConnectionStringEnvironmentName}{connectionName}";

            if (context.PublisherName == "manifest")
            {
                context.EnvironmentVariables[connectionStringName] = $"{{{resource.Name}.connectionString}}";
                return;
            }

            var connectionString = resource.GetConnectionString() ??
                builder.ApplicationBuilder.Configuration.GetConnectionString(resource.Name);

            if (string.IsNullOrEmpty(connectionString))
            {
                if (optional)
                {
                    // This is an optional connection string, so we can just return.
                    return;
                }

                throw new DistributedApplicationException($"A connection string for '{resource.Name}' could not be retrieved.");
            }

            context.EnvironmentVariables[connectionStringName] = connectionString;
        });
    }

    public static IDistributedApplicationResourceBuilder<TDestination> WithReference<TDestination, TSource>(this IDistributedApplicationResourceBuilder<TDestination> builder, IDistributedApplicationResourceBuilder<TSource> source)
        where TDestination : IDistributedApplicationResourceWithEnvironment where TSource: ProjectResource
    {
        ApplyBinding(builder, source.Resource);
        return builder;
    }

    public static IDistributedApplicationResourceBuilder<TDestination> WithReference<TDestination>(this IDistributedApplicationResourceBuilder<TDestination> builder, EndpointReference endpointReference)
        where TDestination : IDistributedApplicationResourceWithEnvironment
    {
        ApplyBinding(builder, endpointReference.Owner, endpointReference.BindingName);
        return builder;
    }

    private static void ApplyBinding<T>(IDistributedApplicationResourceBuilder<T> builder, IDistributedApplicationResourceWithBindings resourceWithBindings, string? bindingName = null)
        where T : IDistributedApplicationResourceWithEnvironment
    {
        // When adding a service reference we get to see whether there is a ServiceReferencesAnnotation
        // on the resource, if there is then it means we have already been here before and we can just
        // skip this and note the service binding that we want to apply to the environment in the future
        // in a single pass. There is one ServiceReferenceAnnotation per service binding source.
        var serviceReferenceAnnotation = builder.Resource.Annotations
            .OfType<ServiceReferenceAnnotation>()
            .Where(sra => sra.Resource == (IDistributedApplicationResource)resourceWithBindings)
            .SingleOrDefault();

        if (serviceReferenceAnnotation == null)
        {
            serviceReferenceAnnotation = new ServiceReferenceAnnotation(resourceWithBindings);
            builder.WithAnnotation(serviceReferenceAnnotation);

            var callback = CreateServiceReferenceEnvironmentPopulationCallback(serviceReferenceAnnotation);
            builder.WithEnvironment(callback);
        }

        // If no specific binding name is specified, go and add all the bindings.
        if (bindingName == null)
        {
            serviceReferenceAnnotation.UseAllBindings = true;
        }
        else
        {
            serviceReferenceAnnotation.BindingNames.Add(bindingName);
        }
    }

    public static IDistributedApplicationResourceBuilder<T> WithServiceBinding<T>(this IDistributedApplicationResourceBuilder<T> builder, int? hostPort = null, string? scheme = null, string? name = null) where T : IDistributedApplicationResource
    {
        if (builder.Resource.Annotations.OfType<ServiceBindingAnnotation>().Any(sb => sb.Name == name))
        {
            throw new DistributedApplicationException($"Service binding with name '{name}' already exists");
        }

        var annotation = new ServiceBindingAnnotation(ProtocolType.Tcp, scheme, name, port: hostPort);
        return builder.WithAnnotation(annotation);
    }

    public static EndpointReference GetEndpoint<T>(this IDistributedApplicationResourceBuilder<T> builder, string name) where T: IDistributedApplicationResourceWithBindings
    {
        return builder.Resource.GetEndpoint(name);
    }

    public static IDistributedApplicationResourceBuilder<T> AsHttp2Service<T>(this IDistributedApplicationResourceBuilder<T> builder) where T : IDistributedApplicationResourceWithBindings
    {
        return builder.WithAnnotation(new Http2ServiceAnnotation());
    }

    public static IDistributedApplicationResourceBuilder<T> ExcludeFromManifest<T>(this IDistributedApplicationResourceBuilder<T> builder) where T : IDistributedApplicationResource
    {
        foreach (var annotation in builder.Resource.Annotations.OfType<ManifestPublishingCallbackAnnotation>())
        {
            builder.Resource.Annotations.Remove(annotation);
        }

        return builder.WithAnnotation(ManifestPublishingCallbackAnnotation.Ignore);
    }
}