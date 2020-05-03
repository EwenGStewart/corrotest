using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Formatters.Json.Internal;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
 




namespace corrotest
{


    public static class DemoCorro
    {
        public const string RequestUri = "https://webhook.site/63872f22-4ce6-4ec5-9856-5e355abe3232";

        static HttpClient client = new HttpClient();


        [FunctionName("SendMail")]
        public static async Task<IActionResult> SendMail(

            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Org/{orgname}/Account/{accountnumber}/SendMail")] SendCorroRequest req,
            string orgname,
            string accountnumber,
            ILogger log)
        {
            log.LogInformation($"HTTP trigger function processed a request for {accountnumber}.");


            if (string.IsNullOrWhiteSpace(accountnumber) || accountnumber.Length < 5) throw new ArgumentException("Invalid account number", "accoutnumber");
            if (string.IsNullOrWhiteSpace(orgname) || orgname.Length < 5) throw new ArgumentException("Invalid orgname ", "orgname");

            int dummyAccountSelector = Math.Abs(accountnumber.GetHashCode()) % 3;
            if (accountnumber == "A00001")
            {
                dummyAccountSelector = 0;
            }
            if (accountnumber == "A00002")
            {
                dummyAccountSelector = 1;
            }

            if (accountnumber == "A00003")
            {
                dummyAccountSelector = 2;
            }


            Tally2BackendRequest backendCall = new Tally2BackendRequest() { AccountNumber = accountnumber };
            if (req.RawTemplate)
            {
                backendCall.ActualTemplateName = req.TemplateName;
            }
            else
            {
                // pretend the account system is looking up the actual template to use 
                backendCall.ActualTemplateName = $"Actual_{req.TemplateName}{dummyAccountSelector}";
            }
            backendCall.SubscriptionKey = req?.Test?.SubscriptionKey ?? backendCall.SubscriptionKey;
            backendCall.SubscriptionName = req?.Test?.SubscriptionName ?? backendCall.SubscriptionName;
            backendCall.Data = req.Data;
            backendCall.Email = req.Email ?? "Ewen@tallyIt.com.au";

            backendCall.OrgName = orgname;

            ContactType contactType = ContactType.Default;
            Enum.TryParse<ContactType>(req.ContactType, out contactType);

            backendCall.RequestId = $"{req.RequestId}-{(int)contactType}-{Math.Abs(backendCall.Email.GetHashCode())}";
            switch (contactType)
            {

                case ContactType.Default:
                case ContactType.PrimaryAndSeconmdary:
                case ContactType.Primary:
                case ContactType.All:
                    backendCall.ContactType = "Primary";
                    break;


                case ContactType.LifeSupport:
                case ContactType.Site:
                    backendCall.ContactType = "Site";
                    break;

                default:
                    backendCall.ContactType = req.ContactType.ToString();
                    break;
            }
            backendCall.AddressType = req.AddressType;



            switch (dummyAccountSelector)
            {
                case 0:
                    backendCall.AccountName = "Daniell and Ewen Stewart";
                    backendCall.AddressType = "Billing";
                    backendCall.AddressLocation = "143 new street Brighton VIC 3186 ";
                    backendCall.Addressee = null;
                    backendCall.Address = "143 New Street";
                    backendCall.Address2 = null;
                    backendCall.Address3 = null;
                    backendCall.AddressState = "VIC";
                    backendCall.AddressSuburb = "Brighton";
                    backendCall.AddressPostCode = "3186";
                    backendCall.AddressCountry = "Australia";
                    backendCall.BusinessAccount = false;
                    backendCall.BuisnessName = null;
                    backendCall.BusinessNumber = null;
                    backendCall.ContactType = "Primary account holder";

                    backendCall.ContactTitle = "Mr";
                    backendCall.ContactFirstName = "Ewen";
                    backendCall.ContactFamilyName = "Stewart";
                    backendCall.ContactFullName = "Mr Ewen Stewart";
                    backendCall.SiteLocation = "143 new street Brighton VIC 3106 ";
                    backendCall.SiteRef = "1234567890-1";
                    backendCall.SiteType = "NMI ";
                    backendCall.ServiceRef = null;


                    break;
                case 1:
                    backendCall.AccountName = "Sorted Services";
                    backendCall.AddressType = "Billing";

                    backendCall.AddressLocation = "11-13 Cubitt Street Cremorne VIC 3121";


                    if (backendCall.ContactType.Equals("Billing", StringComparison.OrdinalIgnoreCase) || backendCall.ContactType.Equals("Secondary", StringComparison.OrdinalIgnoreCase))
                    {

                        backendCall.Addressee = "Ms Nyomi Watts";
                        backendCall.AddresseeDept = "Account payable";
                        backendCall.AddresseeCompany = "Sorted Services Pty Ltd";
                        backendCall.Address = "11-13 Cubitt Street";
                        backendCall.Address2 = null;
                        backendCall.Address3 = null;
                        backendCall.AddressState = "VIC";
                        backendCall.AddressSuburb = "Cremorne";
                        backendCall.AddressPostCode = "3121";
                        backendCall.AddressCountry = "Australia";

                        backendCall.ContactTitle = "Ms";
                        backendCall.ContactFirstName = "Nyomi";
                        backendCall.ContactFamilyName = "Watts";
                        backendCall.ContactJobTitle = "Administration Manager";
                        backendCall.ContactJobDepartment = "Account Payable";
                        backendCall.ContactFullName = "Ms Nyomi Watts";
                    }
                    else if (backendCall.ContactType.Equals("Site", StringComparison.OrdinalIgnoreCase))
                    {
                        backendCall.Addressee = "Site manager";
                        backendCall.AddresseeCompany = "Sorted Services Pty Ltd";
                        backendCall.Address = "9 Cubitt Street";
                        backendCall.Address2 = null;
                        backendCall.Address3 = null;
                        backendCall.AddressState = "VIC";
                        backendCall.AddressSuburb = "Cremorne";
                        backendCall.AddressPostCode = "3121";
                        backendCall.AddressCountry = "Australia";

                        backendCall.ContactTitle = "Mr";
                        backendCall.ContactFirstName = "Fred";
                        backendCall.ContactFamilyName = "Qwerty";
                        backendCall.ContactJobTitle = "Building Manager";
                        backendCall.ContactFullName = "Mr Fred Qwerty";



                    }
                    else
                    {
                        backendCall.Addressee = "Mr Andrew Duncan";
                        backendCall.AddresseeDept = "Director";
                        backendCall.AddresseeCompany = "Sorted Services Pty Ltd";

                        backendCall.Address = "Sleeves Up";
                        backendCall.Address2 = "11-13 Cubitt street";
                        backendCall.Address3 = null;
                        backendCall.AddressState = "VIC";
                        backendCall.AddressSuburb = "Cremorne";
                        backendCall.AddressPostCode = "3121";
                        backendCall.AddressCountry = "Australia";

                        backendCall.ContactTitle = "Mr";
                        backendCall.ContactFirstName = "Andrew";
                        backendCall.ContactFamilyName = "Duncan";
                        backendCall.ContactJobTitle = "Director";
                        backendCall.ContactFullName = "Mr Andrew Duncan";


                    }


                    backendCall.BusinessAccount = true;
                    backendCall.BuisnessName = "Sorted Services Pty Ltd";
                    backendCall.BusinessNumber = "123456787980";



                    backendCall.SiteLocation = "9 Cubitt Street Cremoren VIC 3121  ";
                    backendCall.SiteRef = "Sads5XS890-1";
                    backendCall.SiteType = "MIRN";
                    backendCall.ServiceRef = "1saasd";


                    break;
                default:


                    backendCall.AddressType = "Site";
                    backendCall.AddressLocation = "143 new street Brighton VIC 3186 ";
                    backendCall.Addressee = "Occupier";
                    backendCall.Address = "143 New Street";
                    backendCall.Address2 = null;
                    backendCall.Address3 = null;
                    backendCall.AddressState = "VIC";
                    backendCall.AddressSuburb = "Brighton";
                    backendCall.AddressPostCode = "3186";
                    backendCall.AddressCountry = "Australia";
                    backendCall.BusinessAccount = false;
                    backendCall.BuisnessName = null;
                    backendCall.BusinessNumber = null;


                    backendCall.ContactTitle = "Occupier";
                    backendCall.SiteLocation = "143 new street Brighton VIC 3106 ";
                    backendCall.SiteRef = "1234567890-1";
                    backendCall.SiteType = "NMI ";
                    backendCall.ServiceRef = null;

                    break;
            }





            string target = req.Test.ServiceURI ?? RequestUri;


            await client.PostAsJsonAsync(target, backendCall);

            if (contactType == ContactType.All && dummyAccountSelector == 1)
            {
                req.ContactType = "Secondary"; await SendMail(req, orgname, accountnumber, log);
                req.ContactType = ContactType.Site.ToString(); await SendMail(req, orgname, accountnumber, log);
                req.ContactType = ContactType.Billing.ToString(); await SendMail(req, orgname, accountnumber, log);
            }

            else if (contactType == ContactType.PrimaryAndSeconmdary && dummyAccountSelector == 1)
            {
                req.ContactType = ContactType.Secondary.ToString(); await SendMail(req, orgname, accountnumber, log);
            }


            return new OkObjectResult(backendCall.RequestId);
        }


        [FunctionName("CorroEvent")]
        public static async Task<IActionResult> CorroEvent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Event/{requestid}")] CloudEvent req,
            string requestId,
            ILogger log)
        {


           
            if (string.IsNullOrWhiteSpace(req.EventType)) throw new Exception("Missing event type");
            if (string.IsNullOrWhiteSpace(req.Id)) throw new Exception("Missing event id");
            if (string.IsNullOrWhiteSpace(req.Subject)) throw new Exception("Missing event subject");

            await client.PostAsJsonAsync(DemoCorro.RequestUri, new { RequestId = requestId, EventType=req.EventType , Subject=req.Subject , Id=req.Id , Data = req.Data } );

            return new OkObjectResult(true);


        }


    }



    public class CloudEvent
    {
        public string Topic { get; set; }
        public string Subject { get; set; }
        public string EventType { get; set; }
        public DateTime EventTime { get; set; }
        public string Id { get; set; }
        public Object Data { get; set; }
    }





    public class SendCorroRequest
    {

        /// <summary>
        ///  Name of the template 
        /// </summary>
        public string TemplateName { get; set; } = "template";

        /// <summary>
        ///  If true the template name will not be replaced with the actual template name from the options system 
        /// </summary>
        public bool RawTemplate { get; set; } = false;


        /// <summary>
        ///  Idempotency token 
        /// </summary>
        public Guid RequestId { get; set; }


        /// <summary>
        ///  Override the email address 
        /// </summary>
        public string Email { get; set; }


        /// <summary>
        ///  Optional Payload - passed to the caller 
        /// </summary>
        public object Data { get; set; }



        public string ContactType { get; set; } = null;

        public string AddressType { get; set; } = null;


        /// <summary>
        ///  ONLY  For testing - just use this for passing elements taht would normally be done by the 
        /// </summary>
        public TestInfo Test { get; set; } = new TestInfo();






    }


    public enum ContactType
    {
        Default = 0,   // use default contact for the template 
        Primary,       // send to the primary contact 
        Secondary,      // send to the secondary contact 
        Site,           // send to the site contact 
        LifeSupport,   //send to the life support contatc 
        Broker,        // send to the broker contact 
        Billing,       // send to the billing contact 
        PrimaryAndSeconmdary,   // send to primary and optionally secondary 
        All       // send to ALL contacts 
    }

    public enum AddressType
    {
        Default = 0,   //default address for template type 
        BillingAddress,
        SiteAddress,
        NoticeAddress
    }




    public class Tally2BackendRequest
    {
        public string OrgName { get; set; }
        public string SubscriptionName { get; set; }
        public string SubscriptionKey { get; set; }
        public string RequestId { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string ActualTemplateName { get; set; }
        public string Email { get; set; }



        public bool BusinessAccount { get; set; }

        public string BuisnessName { get; set; }

        public string BusinessNumber { get; set; }


        public string ContactType { get; set; }


        public string ContactJobTitle { get; set; }

        public string ContactJobDepartment { get; set; }
        public string ContactTitle { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactFamilyName { get; set; }

        public string ContactFullName { get; set; }



        public string AddressType { get; set; }

        /// <summary>
        /// Single line address 
        /// </summary>
        public string AddressLocation { get; set; }

        public string Addressee { get; set; }
        public string AddresseeTitle { get; set; }
        public string AddresseeDept { get; set; }
        public string AddresseeCompany { get; set; }
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string AddressSuburb { get; set; }
        public string AddressPostCode { get; set; }
        public string AddressState { get; set; }
        public string AddressCountry { get; set; }


        public string SiteType { get; set; }
        public string SiteRef { get; set; }
        public string ServiceRef { get; set; }

        public string SiteNickName { get; set; }

        /// <summary>
        ///  Single Address Line 
        /// </summary>
        public string SiteLocation { get; set; }


        public object Data { get; set; }


    }





    /// <summary>
    ///  just for testing 
    /// </summary>
    public class TestInfo
    {
        public string ServiceURI { get; set; } = DemoCorro.RequestUri;

        public string SubscriptionName { get; set; } = "subsription name";

        public string SubscriptionType { get; set; } = "SENDGRID";

        public string SubscriptionKey { get; set; } = "subscription Key";


    }

    public class Dummy
    {
        public string x = "A fat dog";
        public int y = 42;
        public DateTime z = DateTime.Now;

    }




}
