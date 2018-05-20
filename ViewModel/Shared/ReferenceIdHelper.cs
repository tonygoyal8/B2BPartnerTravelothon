using B2BPartnerTravelothon.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.ViewModel.Shared
{
    public class ReferenceIdHelper
    {
     static public string getRefId(ServiceType st)
        {
            switch (st)
            {
                case ServiceType.Balance_Request:
                    return "TRAB";
                case ServiceType.Flight_Domestic:
                    return "TRAF";
                default:
                    return "TRAR";
            }
        }
    }
}