using B2BPartnerTravelothon.Repository.Email;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace B2BPartnerTravelothon.Procedures
{
    public class TravelothonFlightProc
    {
        public int FlightProc(int FlightId, decimal @Commission)
        {
            string constring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            try
            {
                using (SqlConnection con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_b2c_flight", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FlightId", FlightId);
                        cmd.Parameters.AddWithValue("@Commission", @Commission);

                        SqlParameter param = new SqlParameter("@PointsEarned", SqlDbType.Int);
                        param.SourceColumn = "PointsEarned";
                        cmd.Parameters.Add(param);
                        cmd.Parameters["@PointsEarned"].Direction = ParameterDirection.Output;

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();

                        return Convert.ToInt32(cmd.Parameters["@PointsEarned"].Value.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                var emailService = new EMail();
                var Subject = "Flight Proc Inputs";
                var Destination = System.Configuration.ConfigurationManager.AppSettings.Get("AdminEmail");

                emailService.SendAsync(new IdentityMessage()
                {
                    Body = e.Message + " " + e.GetBaseException(),
                    Subject = Subject,
                    Destination = Destination
                });
            }
            return 0;
        }
    }
}