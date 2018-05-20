using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Constants
{
    public enum ServiceType
    {
        Balance_Request=0,
        Flight_Domestic = 1,
        Flight_International = 2,
        Mobile = 3,
        Dth = 4,
        DataCard = 5
    }

    public enum FlightTypeFlag
    {
        Domestic = 1,
        FSC = 2,
        LCC = 3
    }
    public enum TripTypeFlag
    {
        O = 1,
        R = 2
    }
    public enum StatusFlag
    {
        Payment_Refund_Initiated=-5,
        Payment_Failed=-4,
        Payment_Initiated=-3,
        Payment_Inprogress=-2,
        Payment_Success=-1,
        Pending=0,
        Success=1,
        Unknown_Status=2,
        Failed=3,
        Purged=4
    }
   public enum RequestType {
        GET=0,
        POST=1,
        PUT=2,
        DELETE=3
    }
    public enum PaymentGatewayStatus {
        Failed=0,
        Credit=1
    }

    public enum APIId
    {
        Hermes=1
    }

    public enum Toaster {
        ERROR=0,
        SUCCESS=1,
        WARNING=2,
        INFO=3
    }

    public enum BalanceRequestFlag
    {
   
        Inprogress = 0,
        Approved = 1,
        Denied = 2
    }

    public static class Roles
    {
        public const string AD = "Administrator";
        public const string MD = "Master Distributor";
        public const string TD = "Distributor";
        public const string TA = "Agent";
    }
    public enum RolesID
    {
        Administrator = 1,
        MasterDistributor = 2,
        Distributor = 3,
        Agent=4
    }


    public static class MemberType
    {
        public const string GOLD = "Gold";
        public const string SILVER = "Silver";
    }

    public enum BalanceRequestPurpose
    {

        For_All_Services = 1,
        From_Rail_To_Agent_Account = 2,
        From_Agent_Account_To_Rail = 3
    }
    public enum UserCasesStatus
    {
        Inprogress=1,
        Closed=2
    }
    public enum UserCaseReason
    {
        Cancellation=1,
        Partial_Cancellation=2,
        Reschedule=3,
        Update_Passenger_Name=4,
        Other=5
    }
    public enum Severity
    {
        Critical=1,
        High=2,
        Medium=3,
        Low=4
    }
}