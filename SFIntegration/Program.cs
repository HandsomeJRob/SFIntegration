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
        private static string _sessionId;
        private static string _metadataUrl;

        static void Main(string[] args)
        {
            LoginResult loginResult = Login();

            _sessionId = loginResult.sessionId;
            _metadataUrl = loginResult.metadataServerUrl;

            // WORKS...
            //CreateNewObject("MetadataObject1");

            CreateWorkflow();
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
        //    var result = soapClient.logout(new JRSandbox.SessionHeader {  sessionId = _sessionId })

        //    //var result = soapClient.logout();
        //    Console.WriteLine($"Logged out of session ID {sessionId}");
        //}

        private static void CreateWorkflow()
        {
            // Notes:
            // Fullname - must match ObjectName.RuleName
            // Formula - FieldName <operator> value

            var wfRule = new WorkflowRule
            {
                fullName = "Opportunity.MetaDataRule",
                description = "Created from metadata API",
                triggerType = WorkflowTriggerTypes.onAllChanges,
                formula = "Amount > -1",
                active = false,
            };

            SendMetadata(new Metadata[] { wfRule });
        }



        private static void CreateNewObject(string name)
        {
            string label = name;
            string pluralLabel = name + "s";

            // objects are similar to tables in relational databases...
            // a place to hold data, and ui and crud almost comes for free
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

            SendMetadata(objects);
        }

        private static void SendMetadata(Metadata[] metadata)
        {
            MetadataPortTypeClient metaClient = new MetadataPortTypeClient("Metadata", _metadataUrl);
            var sessionHeader = new JRSandbox.Metadata.SessionHeader { sessionId = _sessionId };

            var result = metaClient.createMetadata(sessionHeader, null, null, metadata);
        }
    }
}
