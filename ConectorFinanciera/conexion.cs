using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



// CRM
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
//using Microsoft.Crm.Sdk.Messages;

//AppConfig
using System.Configuration;

//
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Reflection;
using System.Collections.Specialized;
using System.Collections;
using System.Xml;


namespace ConectorFinanciera
{
    /*public class conexion
    {
        #region VariablesConexion

        // To get discovery service address and organization unique name, 
        // Sign in to your CRM org and click Settings, Customization, Developer Resources.
        // On Developer Resource page, find the discovery service address under Service Endpoints and organization unique name under Your Organization Information.
        private String _discoveryServiceAddress;
        //private String _organizationAddress = ConfigurationManager.AppSettings["UrlCRM"];
        private String _organizationUniqueName;
        //private String _organizationFriendlyName = ConfigurationManager.AppSettings["OrganizacionCRM"];
        //// Provide your user name and password.
        private String _userName;
        private String _password;
        private String _domain;

        public conexion(string usuario, string pass, string org, string urldisc, string domain)
        {
            _userName = usuario;
            _password = pass;
            _organizationUniqueName = org;
            _discoveryServiceAddress = urldisc;
            _domain = domain;
        }

        #endregion VariablesConexion
        public OrganizationServiceProxy getService()
        {


            IServiceManagement<IDiscoveryService> serviceManagement =
                    ServiceConfigurationFactory.CreateManagement<IDiscoveryService>(
                    new Uri(_discoveryServiceAddress));

            //Set the EndPointType
            AuthenticationProviderType endpointType = serviceManagement.AuthenticationType;

            // Set the credentials.
            AuthenticationCredentials authCredentials = GetCredentials(endpointType);

            String organizationUri = String.Empty;
            // Get the discovery service proxy.
            using (DiscoveryServiceProxy discoveryProxy =
                GetProxy<IDiscoveryService, DiscoveryServiceProxy>(serviceManagement, authCredentials))
            {
                // Obtain organization information from the Discovery service. 
                if (discoveryProxy != null)
                {
                    // Obtain information about the organizations that the system user belongs to.
                    OrganizationDetailCollection orgs = DiscoverOrganizations(discoveryProxy);
                    // Obtains the Web address (Uri) of the target organization.
                    organizationUri = FindOrganization(_organizationUniqueName,
                        orgs.ToArray()).Endpoints[EndpointType.OrganizationService];

                }
            }


            IServiceManagement<IOrganizationService> orgServiceManagement =
            ServiceConfigurationFactory.CreateManagement<IOrganizationService>(
            new Uri(organizationUri));

            // Set the credentials.
            AuthenticationCredentials credentials = GetCredentials(endpointType);

            // Get the organization service proxy.
            OrganizationServiceProxy organizationProxy =
            GetProxy<IOrganizationService, OrganizationServiceProxy>(orgServiceManagement, credentials);
            organizationProxy.EnableProxyTypes();

            return organizationProxy;

        }



        // Obtain the AuthenticationCredentials for AuthenticationProviderType.LiveId
        private AuthenticationCredentials GetCredentials(AuthenticationProviderType endpointType)
        {
            AuthenticationCredentials authCredentials = new AuthenticationCredentials();
            switch (endpointType)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    authCredentials.ClientCredentials.Windows.ClientCredential =
                        new System.Net.NetworkCredential(_userName,
                            _password,
                            _domain);
                    break;
                case AuthenticationProviderType.LiveId:
                    authCredentials.ClientCredentials.UserName.UserName = _userName;
                    authCredentials.ClientCredentials.UserName.Password = _password;
                    authCredentials.SupportingCredentials = new AuthenticationCredentials();
                    authCredentials.SupportingCredentials.ClientCredentials =
                        ConectorFinanciera.DeviceIdManager.LoadOrRegisterDevice();
                    break;
                default: // For Federated and OnlineFederated environments.                    
                    authCredentials.ClientCredentials.UserName.UserName = _userName;
                    authCredentials.ClientCredentials.UserName.Password = _password;
                    // For OnlineFederated single-sign on, you could just use current UserPrincipalName instead of passing user name and password.
                    // authCredentials.UserPrincipalName = UserPrincipal.Current.UserPrincipalName;  //Windows/Kerberos
                    break;
            }

            return authCredentials;

        }


        /// <summary>
        /// Discovers the organizations that the calling user belongs to.
        /// </summary>
        /// <param name="service">A Discovery service proxy instance.</param>
        /// <returns>Array containing detailed information on each organization that 
        /// the user belongs to.</returns>
        public OrganizationDetailCollection DiscoverOrganizations(
            IDiscoveryService service)
        {
            if (service == null) throw new ArgumentNullException("service");
            RetrieveOrganizationsRequest orgRequest = new RetrieveOrganizationsRequest();
            RetrieveOrganizationsResponse orgResponse =
                (RetrieveOrganizationsResponse)service.Execute(orgRequest);

            return orgResponse.Details;
        }


        // Finds a specific organization detail in the array of organization details
        // returned from the Discovery service.
        public OrganizationDetail FindOrganization(string orgUniqueName,
            OrganizationDetail[] orgDetails)
        {
            if (String.IsNullOrWhiteSpace(orgUniqueName))
                throw new ArgumentNullException("orgUniqueName");
            if (orgDetails == null)
                throw new ArgumentNullException("orgDetails");
            OrganizationDetail orgDetail = null;

            foreach (OrganizationDetail detail in orgDetails)
            {
                if (String.Compare(detail.UniqueName, orgUniqueName,
                    StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    orgDetail = detail;
                    break;
                }
            }
            return orgDetail;
        }


        // Generic method to obtain discovery/organization service proxy instance.
        private TProxy GetProxy<TService, TProxy>(
           IServiceManagement<TService> serviceManagement,
           AuthenticationCredentials authCredentials)
            where TService : class
            where TProxy : ServiceProxy<TService>
        {
            Type classType = typeof(TProxy);

            if (serviceManagement.AuthenticationType !=
                AuthenticationProviderType.ActiveDirectory)
            {
                AuthenticationCredentials tokenCredentials =
                    serviceManagement.Authenticate(authCredentials);
                // Obtain discovery/organization service proxy for Federated, LiveId and OnlineFederated environments. 
                // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and SecurityTokenResponse.
                return (TProxy)classType
                    .GetConstructor(new Type[] { typeof(IServiceManagement<TService>), typeof(SecurityTokenResponse) })
                    .Invoke(new object[] { serviceManagement, tokenCredentials.SecurityTokenResponse });
            }

            // Obtain discovery/organization service proxy for ActiveDirectory environment.
            // Instantiate a new class of type using the 2 parameter constructor of type IServiceManagement and ClientCredentials.
            return (TProxy)classType
                .GetConstructor(new Type[] { typeof(IServiceManagement<TService>), typeof(ClientCredentials) })
                .Invoke(new object[] { serviceManagement, authCredentials.ClientCredentials });
        }
    }*/

    public class conexion
    {
        string _usuario, _pass, _org, _urldisc, _domain;
        public conexion(string usuario, string pass, string org, string urldisc, string domain)
        {
            _usuario = usuario;
            _pass = pass;
            _org = org;
            _urldisc = urldisc;
            _domain = domain;
        }

        public IOrganizationService getService()
        {
            OrganizationServiceProxy serviceProxy = null;
            IServiceConfiguration<IOrganizationService> config = ServiceConfigurationFactory.CreateConfiguration<IOrganizationService>(new Uri(_urldisc));

            switch (config.AuthenticationType)
            {
                case AuthenticationProviderType.Federation:
                    ClientCredentials clientCredentials = new ClientCredentials();
                    clientCredentials.UserName.UserName = _domain + "\\" + _usuario;
                    clientCredentials.UserName.Password = _pass;
                    serviceProxy = new OrganizationServiceProxy(config, clientCredentials);
                    break;
                case AuthenticationProviderType.ActiveDirectory:
                    ClientCredentials credentials = new ClientCredentials();
                    credentials.Windows.ClientCredential = new NetworkCredential(_usuario, _pass, _domain);
                    serviceProxy = new OrganizationServiceProxy(new Uri(_urldisc), null, credentials, null);
                    break;
            }

            if (serviceProxy.IsAuthenticated)
                return serviceProxy;

            return null;
        }
    }
}
