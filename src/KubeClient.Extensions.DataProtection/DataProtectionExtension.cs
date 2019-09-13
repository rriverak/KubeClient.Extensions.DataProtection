using KubeClient.Extensions.Configuration;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace KubeClient.Extensions.DataProtection
{
    /// <summary>
    ///     <see cref="IDataProtectionBuilder"/> extension methods to persist Keys in a Kubernetes Secret.
    /// </summary>
    public static class DataProtectionExtension
    {
        /// <summary>
        /// Add or Create a Kubernetes Secret as a Repository. 
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IDataProtectionBuilder"/> to Configure.
        /// </param>
        /// <param name="clientOptions">
        /// <see cref="KubeClientOptions"/> for the <see cref="KubeApiClient"/> used to communicate with the Kubernetes API.
        /// </param>
        /// <param name="secretName">
        ///  The name of the target Secret.
        /// </param>
        /// <param name="kubeNamespace">
        ///  The namespace of the target Secret.
        /// </param>
        /// <returns>
        /// The configured <see cref="IDataProtectionBuilder"/>.
        /// </returns>
        public static IDataProtectionBuilder PersistKeysToKubeSecret(this IDataProtectionBuilder builder, KubeClientOptions clientOptions, string secretName, string kubeNamespace = null)
        {
            KubeApiClient client = KubeApiClient.Create(clientOptions);
            return PersistKeysToKubeSecretInternal(builder, client, secretName, kubeNamespace);
        }

        /// <summary>
        /// Internal Implementation 
        /// </summary>
        /// <param name="builder">
        /// The <see cref="IDataProtectionBuilder"/> to Configure.
        /// </param>
        /// <param name="client">
        /// <see cref="KubeApiClient"/> used to communicate with the Kubernetes API.
        /// </param>
        /// <param name="secretName">
        ///  The name of the target Secret.
        /// </param>
        /// <param name="kubeNamespace">
        ///  The namespace of the target Secret.
        /// </param>
        /// <returns></returns>
        private static IDataProtectionBuilder PersistKeysToKubeSecretInternal(IDataProtectionBuilder builder, KubeApiClient client, string secretName, string kubeNamespace = null)
        {
            builder.Services.Configure<KeyManagementOptions>(options =>
            {
                // Add KubeClientXmlRepository as KeyStore
                options.XmlRepository = new KubeClientXmlRepository(client, secretName, kubeNamespace);
            });
            return builder;
        }
    }
}
