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
        private static string _enterpriseUrl;

        static void Main(string[] args)
        {
            // Note: user used for sync must have api access.  
            // It may be wise to create a new Salesforce user with appropriate permissions, 
            // to be used for sync purposes.

            // inputs needed from user
            // 1. Username
            // 2. Password
            // 3. Token for user (see https://help.salesforce.com/articleView?id=user_security_token.htm&type=5)
            
            // inputs coming from somewhere... config?
            // 1. Workflow rule name
            // 2. Workflow rule description (optional)
            // 3. Workflow rule formula (condition for sync)
            // 4. Workflow action name
            // 5. Workflow action description (optional)
            // 6. Workflow action callout endpoint
            // 7. Names of fields we want to sync on Opportunity
            // 8. API version (currently 45)
            // 9. Activate now?


            LoginResult loginResult = Login();

            _sessionId = loginResult.sessionId;
            _metadataUrl = loginResult.metadataServerUrl;
            _enterpriseUrl = loginResult.serverUrl;

            try
            {
                //CreateNewObject("MetadataObject1");

                CreateWorkflow();
            }
            finally
            {
                Logout();
            }
        }

        private static LoginResult Login()
        {
            LoginResult loginResult = null;

            using (var client = new SoapClient())
            {

                // Salesforce uses password + token to authenticate external systems
                // To get a token... https://help.salesforce.com/articleView?id=user_security_token.htm&type=5
                // https://blog.mkorman.uk/integrating-with-metadata-api/

                //string username = "jonathan.d.robertson1985@gmail.com";
                //string password = "$uckyPass1";
                //string token = "lDGd3rMQtLHJJazKnjRYUHcu";
                //string loginPassword = password + token;

                // testing wsdl with alternate sandbox
                string username = "spambot4cats2u@outlook.com";
                string password = "$uckyPass1";
                string token = "Xri9fsalKQyZM45a7LkaBp1lv";
                string loginPassword = password + token;

                // throws exception if not successful
                loginResult = client.login(null, null, username, loginPassword);
            }

            return loginResult;
        }

        private static void Logout()
        {
            var soapClient = new SoapClient("Soap", _enterpriseUrl);

            var result = soapClient.logout(new JRSandbox.SessionHeader { sessionId = _sessionId }, null);

            Console.WriteLine($"Logged out of session ID {_sessionId}");
        }

        private static void CreateWorkflow()
        {
            var wfAction = new WorkflowOutboundMessage
            {
                name = "JROpportunityAction",
                fullName = "Opportunity.JROpportunityAction",
                endpointUrl = "https://prod-17.centralus.logic.azure.com:443/workflows/8ce4281445fe47e695a4ee65d14913cd/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=7K9iP14qF4cy-dTTfz3PMKQn9h5Ll3qzFQ-t1Pyjw3I",
                //integrationUser = "jonathan.d.robertson1985@gmail.com",
                integrationUser = "spambot4cats2u@outlook.com",
                description = "Check out this action",
                apiVersion = 45,
                fields = new string[]
                {
                    "AccountId",
                    "Amount",
                    "Id",
                    "CloseDate",
                    "IsWon",
                    "Name",
                    "Type",
                    "OrderNumber__c"
                }
            };

            // Notes:
            // Fullname - must match ObjectName.RuleName
            // Formula - FieldName <operator> value

            var wfRule = new WorkflowRule
            {
                fullName = "Opportunity.JRMetaDataRule",
                description = "Created from metadata API",
                triggerType = WorkflowTriggerTypes.onAllChanges,
                formula = "Amount > -1",
                active = false,
                actions = new WorkflowActionReference[]
                {
                    new WorkflowActionReference
                    {
                        name = "JROpportunityAction",
                        type = WorkflowActionType.OutboundMessage
                    }
                }
            };

            SendMetadata(new Metadata[]
            {
                wfAction,
                wfRule
            });
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

            bool success = result.All(r => r.success);
        }
    }
}
