using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFIntegration.JRSandbox;
using SFIntegration.JRSandbox.Metadata;

namespace SFIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            LoginResult loginResult = Login();

            CreateMetadataObjects(loginResult.sessionId, loginResult.metadataServerUrl);
        }

        private static LoginResult Login()
        {
            LoginResult loginResult = null;

            using (var client = new SoapClient())
            {
                string username = "jonathan.d.robertson1985@gmail.com";

                // Salesforce uses password + token to authenticate external systems
                // To get a token... https://help.salesforce.com/articleView?id=user_security_token.htm&type=5
                string password = "$uckyPass1";
                string token = "lDGd3rMQtLHJJazKnjRYUHcu";
                string loginPassword = password + token;

                // throws exception if not successful
                loginResult = client.login(null, null, username, loginPassword);
            }

            return loginResult;
        }

        //private static void Logout(string sessionId, string url)
        //{
        //    var soapClient = new SoapClient("Soap", url);
        //    var result = soapClient.logout(new JRSandbox.SessionHeader null);
        //    Console.WriteLine($"Logged out of session ID {sessionId}");
        //}

        private static void CreateMetadataObjects(string sessionId, string metadataUrl)
        {
            string name = "JRTestName";
            string label = "JRTestLabel";
            string pluralLabel = "JRTestLabels";

            var customObject = new CustomObject
            {
                fullName = name + "__c",
                label = label,
                pluralLabel = pluralLabel,
                deploymentStatus = DeploymentStatus.Deployed,
                deploymentStatusSpecified = true,
                description = "Created by metadata API",
                enableActivities = true,
                sharingModel = SharingModel.ReadWrite,
                sharingModelSpecified = true
            };
            var nameField = new CustomField
            {
                type = FieldType.Text,
                typeSpecified = true,
                label = label + " Name"
            };

            customObject.nameField = nameField;

            var objects = new Metadata[] { customObject };

            // send it
            MetadataPortTypeClient metaClient = new MetadataPortTypeClient("Metadata", metadataUrl);
            var sessionHeader = new JRSandbox.Metadata.SessionHeader { sessionId = sessionId };

            var result = metaClient.createMetadata(sessionHeader, null, null, objects);

        }
    }
}
