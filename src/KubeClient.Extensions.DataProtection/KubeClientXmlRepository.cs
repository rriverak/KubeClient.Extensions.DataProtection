using KubeClient.Models;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace KubeClient.Extensions.DataProtection
{
    sealed class KubeClientXmlRepository : IXmlRepository, IDisposable  
    {
        /// <summary>
        /// Watcher Subscribtion
        /// </summary>
        private IDisposable _watchSubscription;

        /// <summary>
        /// <see cref="KubeApiClient"/> used to communicate with the Kubernetes API.
        /// </summary>
        private readonly KubeApiClient _client;

        /// <summary>
        ///  The name of the target Secret.
        /// </summary>
        private readonly string _secretName;

        /// <summary>
        ///  The namespace of the target Secret.
        /// </summary>
        private readonly string _kubeNamespace;

        /// <summary>
        /// <see cref="SecretV1"/> used to manage the XML Keyfiles.
        /// </summary>
        private SecretV1 _keyManagementSecret { get; set; }
        /// <summary>
        /// An XML repository backed by KubeClient.
        /// </summary>
        /// <param name="client">
        /// <see cref="KubeApiClient"/> used to communicate with the Kubernetes API.
        /// </param>
        /// <param name="secretName">
        ///  The name of the target Secret.
        /// </param>
        /// <param name="kubeNamespace">
        ///  The namespace of the target Secret.
        /// </param>
        public KubeClientXmlRepository(KubeApiClient client, string secretName, string kubeNamespace = null)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            if (String.IsNullOrEmpty(secretName))
            {
                throw new ArgumentNullException(nameof(secretName));
            }

            this._client = client;
            this._secretName = secretName;
            this._kubeNamespace = kubeNamespace;

            // Init
            this.LoadOrCreateSecret();
            this.AttachWatcher();
        }


        /// <summary>
        /// Implement <see cref="IXmlRepository"/>
        /// </summary>
        public IReadOnlyCollection<XElement> GetAllElements()
        {
            return this.GetAllElementsCore().ToList().AsReadOnly();
        }

        /// <summary>
        /// Implement <see cref="IXmlRepository"/>
        /// </summary>
        public void StoreElement(XElement element, string friendlyName)
        {
            // Convert to XML String
            var xmlString = element.ToString(SaveOptions.DisableFormatting);

            // Convert to Base64 String
            var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlString));

            // Add XML File Extension to allow others File-Mapping
            if (string.IsNullOrEmpty(Path.GetExtension(friendlyName))){
                friendlyName += ".xml";
            }
            
            // Add to Data
            this._keyManagementSecret.Data.Add(friendlyName, base64String);

            // Patch the Secret
            this._client.SecretsV1().Update(_secretName, (patch) =>
            {
                patch.Replace(e => e.Data, this._keyManagementSecret.Data);
            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Load or Create the Kubernetes Secret
        /// </summary>
        private void LoadOrCreateSecret()
        {
            // Try to get the Secret
            SecretV1 secret = this._client.SecretsV1().Get(_secretName, _kubeNamespace).GetAwaiter().GetResult();
            if (secret == null)
            {
                // Create a new Secret
                secret = this._client.SecretsV1().Create(new SecretV1() { Metadata = new ObjectMetaV1() { Name = _secretName, Namespace = _kubeNamespace } }).GetAwaiter().GetResult();
            }
            // Use the Secret
            this._keyManagementSecret = secret;
        }

        /// <summary>
        /// Get all Elements from Repository 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<XElement> GetAllElementsCore()
        {
            foreach (var key in this._keyManagementSecret.Data)
            {
                // Convert from Base64 to XMLString
                var xmlString = Encoding.UTF8.GetString(Convert.FromBase64String(key.Value));

                // XMLString to XElement
                yield return XElement.Parse(xmlString);
            }
        }

        /// <summary>
        /// Attach the Watcher to the Secret for Changes
        /// </summary>
        private void AttachWatcher()
        {
            // Attach the Watcher
            this._client.SecretsV1().Watch(_secretName, _kubeNamespace).Subscribe(OnKeyManagementSecretChanged);
        }

        /// <summary>
        /// If the Secret <see cref="ResourceEventType"/> is Modified the internal Propertie will be reset in this Event.
        /// </summary>
        /// <param name="secretEvent">
        /// Event Argument <see cref="IResourceEventV1"/>
        /// </param>
        private void OnKeyManagementSecretChanged(IResourceEventV1<SecretV1> secretEvent)
        {
            if (secretEvent == null)
            {
                throw new ArgumentNullException(nameof(secretEvent));
            }
            if (secretEvent.Resource == null)
            {
                throw new ArgumentNullException(nameof(secretEvent.Resource));
            }

            if(secretEvent.EventType == ResourceEventType.Modified)
            {
                // Attach the changed Secret
                this._keyManagementSecret = secretEvent.Resource;
            }
        }

        /// <summary>
        /// Implement <see cref="IDisposable"/>
        /// </summary>
        public void Dispose()
        {
            if (_watchSubscription != null)
            {
                _watchSubscription.Dispose();
                _watchSubscription = null;
            }
            _client.Dispose();
        }
    }
}
