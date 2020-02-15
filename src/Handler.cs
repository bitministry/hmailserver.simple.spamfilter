using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using BitMinistry.Common;
using hMailServer;

namespace BitMinistry.hMailServer.SpamHandler
{
    public class Handler
    {

        public void SetConfigFile(string configFile)
        {
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFile);
            LogDebug("SetConfigFile");
        }

        private void LogDebug(string xs)
        {
            if (ConfigurationManager.AppSettings["debug"] != "true") return;

            SimpleLogger.WriteWinLogEntry( xs , EventLogEntryType.Information, "BitMinistry.hMailServer.SpamHandler");
        }

        public void Check(object msg)
        {
            LogDebug("Check");

            var oMessage = msg as Message;
            if (oMessage == null) return;

            LogDebug($"To [{oMessage.To}]");

            string newSpamFilter = "", newSpamFilterValue = "";

            var toAddr = oMessage.To ?? "";

            var emailsInHeaderFrom = MailSender.ExtractEmails(Cnv.CStr(oMessage.HeaderValue["From"]) );
            string headerFrom = null;
            if (emailsInHeaderFrom.Length > 0)
                headerFrom = emailsInHeaderFrom[0];

            var emailsInBody = MailSender.ExtractEmails(oMessage.Body);
            string previousSender = null;
            if (emailsInBody.Length > 0)
                previousSender = emailsInBody[0];

            if (toAddr.Contains(ConfigurationManager.AppSettings["senderToSpam"]) && previousSender != null )
            {
                newSpamFilter = "senderToSpam";
                newSpamFilterValue = previousSender;
            }
            if (toAddr.Contains(ConfigurationManager.AppSettings["domainToSpam"]) && previousSender !=null )
            {
                newSpamFilter = "domainToSpam";
                newSpamFilterValue = previousSender.Split('@')[1];
            }
            if (toAddr.Contains(ConfigurationManager.AppSettings["subjectToSpam"]))
            {
                newSpamFilter = "subjectToSpam";
                newSpamFilterValue = oMessage.Subject;
            }

            if (toAddr.Contains(ConfigurationManager.AppSettings["bodyToSpam"]))
            {
                newSpamFilter = "bodyToSpam";
                newSpamFilterValue = oMessage.Body;
            }

            LogDebug("previousSender:"+ previousSender);

            if (ConfigurationManager.AppSettings["addReplyPathToBottom"] == "true")
            {
                if (oMessage.HTMLBody.StrLength() > oMessage.Body.StrLength() )
                    oMessage.HTMLBody = oMessage.HTMLBody + "<hr>From:" + oMessage.FromAddress;
                else 
                    oMessage.Body = oMessage.Body + "From:" + Environment.NewLine + oMessage.FromAddress;
            }

            using (var sql = new BSqlCommander(comType: CommandType.StoredProcedure))
            {
                LogDebug("SqlConnection:" + sql.SqlConnectionString);

                sql.AddWithValue("@newSpamFilter", newSpamFilter);
                sql.AddWithValue("@newSpamFilterValue", newSpamFilterValue?.ToLower());
                sql.AddWithValue("@fromEmail", headerFrom?.ToLower() );
                sql.AddWithValue("@replyTo", oMessage.FromAddress?.ToLower());
                sql.AddWithValue("@subject", oMessage.Subject?.ToLower());
                sql.AddWithValue("@body", oMessage.Body?.ToLower());
                oMessage.Subject = sql.ExecuteScalar("sp_check_spam") + oMessage.Subject;
                oMessage.Save();

                LogDebug( sql.Com.CommandAsSql() );
            }
        }
        



    }
}
