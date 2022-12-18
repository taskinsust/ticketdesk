using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TicketDesk.Domain.Model.VM;

namespace TicketDesk.Domain
{
    public class Utility
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter da;
        private DataTable ResultsData;
        private DataTable ResultsDataSecond;
        private int rowsPerSheet = 100;
        public DateTime Date { get; }

        public Utility()
        {
            ResultsData = new DataTable();
            ResultsDataSecond = new DataTable();
        }

        public List<AvgClosingTimeVM> LoadAvgClosingTime()
        {
            string sql = "";

            sql += @" 
                    select top(20) count(t.TicketStatus) TotalClosed ,convert(date,t.LastUpdateDate) ClosingDate, (SUM(DATEDIFF(HOUR, t.CreatedDate, t.LastUpdateDate)) /count(t.TicketStatus)) AVGHour
                    from [dbo].[Tickets] t group by t.TicketStatus, convert(date,t.LastUpdateDate)
                    having
                    t.TicketStatus =3
                    order by convert(date,t.LastUpdateDate) desc";

            using (con = new SqlConnection(ConfigurationManager.ConnectionStrings["TicketDesk"].ToString()))
            {
                con.Open();
                da = new SqlDataAdapter(sql, con);
                da.SelectCommand.CommandTimeout = 250; //seconds
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<AvgClosingTimeVM> result = ConvertToList<AvgClosingTimeVM>(dt);
                con.Close();
                return result;
            }
        }

        public List<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>()
                    .Select(c => c.ColumnName)
                    .ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var pro in properties)
                {
                    try
                    {
                        if (columnNames.Contains(pro.Name))
                        {
                            if (pro.Name == "SiteInclusionDate" || pro.Name == "ProcessCompletionDate" || pro.Name == "CreationDate" || pro.Name == "ModificationDate" || pro.Name == "CompletionDate" || pro.Name == "ResponseDatetime" || pro.Name == "SignDate")
                            {
                                PropertyInfo pI = objT.GetType().GetProperty(pro.Name);
                                if (row[pro.Name] == DBNull.Value)
                                {
                                    pro.SetValue(objT, null);
                                }
                                else
                                {
                                    pro.SetValue(objT, Convert.ChangeType(row[pro.Name], Date.GetTypeCode()));
                                }
                            }
                            else
                            {
                                PropertyInfo pI = objT.GetType().GetProperty(pro.Name);
                                pro.SetValue(objT, row[pro.Name] == DBNull.Value ? null : Convert.ChangeType(row[pro.Name], pI.PropertyType));
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
                return objT;
            }).ToList();
        }
    }
}
