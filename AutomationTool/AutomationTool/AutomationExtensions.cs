using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Data.SqlClient;
using System.Data;
using System.Net.Mail;
using AutomationTool.Converter;
using AutomationTool.Automator;
using AutomationTool.Constants;

namespace AutomationTool.Extension
{
    /// <summary>
    /// Modified By: Sourav Nanda
    /// Modified Date: 11/25/2016
    /// Description: Added comments to the method
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public static class AutomationExtensions
    {
        public static string SharedFolderPath = "";

        #region Check Connections
        /// <summary>
        /// Generic method to check connection.
        /// </summary>
        /// <typeparam name="T">Type of Generic Object</typeparam>
        /// <param name="ToolObject">Object of the class for which connection to be checked</param>
        /// <param name="Error">Returns back the error if any occurred</param>
        /// <param name="PrerequisitePage">Page Name/Title of the application that the automation tool need to start</param>
        /// <param name="ExitApp">Checks whether app need to exit in case of failure</param>
        /// <param name="SkipApp">Skips the step if 'True' else 'False'</param>
        /// <param name="url">URLs of the application to be found</param>
        /// <returns>'True' for successful connection else 'False'</returns>
        public static bool CheckConnection<T>(this T ToolObject, string PrerequisitePage = "", bool ExitApp = true, bool isMessageNeeded = true, params string[] url) where T : IE
        {
            string Error = string.Empty;
            bool rtnValue = true;
            string ClassName = typeof(T).Name;
            try
            {
                if (!ToolObject.Connect(out Error, url))
                {
                    do
                    {
                        string className = ClassName.ToUpper().Replace("BO", "").Replace("DO", "").Replace("DATA", "");
                        className = className.Replace("_", " ");

                        string message = "";
                        if (!string.IsNullOrEmpty(PrerequisitePage))
                            message = "Macro is unable to proceed due to below reasons:" + Environment.NewLine + "         '" + className + "' is not open."
                                            + Environment.NewLine + "         '" + className + "' is open but '" + PrerequisitePage + "' page is not open.";
                        else
                            message = "Macro is unable to proceed due to below reason:" + Environment.NewLine + "         '" + className + "' is not open.";

                        if (ExitApp)
                            message = message + Environment.NewLine + Environment.NewLine + "Please, verify and" + Environment.NewLine + " Click 'Yes' to continue." + Environment.NewLine + " Click 'No' to exit.";
                        else
                            message = message + Environment.NewLine + Environment.NewLine + "Please, verify and" + Environment.NewLine + " Click 'Yes' to continue " + Environment.NewLine + " Click 'No' to proceed anyway.";

                        if (isMessageNeeded)
                        {
                            DialogResult result = MessageBox.Show(message, "Confirmation", MessageBoxButtons.YesNo);
                            if (result == DialogResult.Yes)
                            {
                                rtnValue = true;
                                continue;
                            }
                            else
                            {
                                if (ExitApp)
                                    Environment.Exit(1);
                                rtnValue = ExitApp;
                                break;
                            }
                        }
                        else
                        {
                            if (ExitApp)
                                Environment.Exit(1);
                            rtnValue = ExitApp;
                            break;
                        }
                    } while (!ToolObject.Connect(out Error, url));
                }
            }
            catch (Exception ex)
            {
                ex.LogError(UniqueId: ClassName, MethodName: MethodBase.GetCurrentMethod().Name, ClassName: MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return rtnValue;
        }

        /// <summary>
        /// Checks Session of the tool.
        /// </summary>
        /// <typeparam name="T">Type of Generic Object</typeparam>
        /// <param name="ToolObject">Object of the class for which connection to be checked</param>
        /// <param name="Error">Returns back the error if any occurred</param>
        /// <returns>Boolean</returns>
        public static bool CheckSession<T>(this T ToolObject, out string Error) where T : IE
        {
            Error = string.Empty;
            bool rtnValue = true;
            string ClassName = typeof(T).Name;
            try
            {
                if (!ToolObject.ToolSession(out Error))
                {
                    do
                    {
                        string className = ClassName.Replace("BO", "");
                        className = className.Replace("_", " ");
                        DialogResult result = MessageBox.Show("Please log on to '" + className + "' and then click yes to continue.", "Confirmation", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            rtnValue = true;
                            continue;
                        }
                        else
                        {
                            Environment.Exit(1);
                            rtnValue = false;
                            break;
                        }
                    } while (!ToolObject.ToolSession(out Error));
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: MethodBase.GetCurrentMethod().Name, ClassName: MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return rtnValue;
        }
        #endregion

        #region Encrypt And Decrypt Method.
        /// <summary>
        /// Method for decrypting password.
        /// </summary>
        /// <param name="EncryptedString">String to be decrypted</param>
        /// <returns>Decrypted 'string'</returns>
        public static string Decrypt(this string EncryptedString)
        {
            string Error = "";
            try
            {
                byte[] keyArray;
                byte[] toDecryptArray = Convert.FromBase64String(EncryptedString);
                string key = "Autom@tion";
                MD5CryptoServiceProvider hashMd5 = new MD5CryptoServiceProvider();
                keyArray = hashMd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashMd5.Clear();
                byte[] byte24BitKey = new byte[24];

                for (int i = 0; i < 16; i++)
                {
                    byte24BitKey[i] = keyArray[i];
                }

                for (int i = 0; i < 7; i++)
                {
                    byte24BitKey[i + 16] = keyArray[i];
                }

                TripleDESCryptoServiceProvider tripleDes = new TripleDESCryptoServiceProvider();
                tripleDes.Key = byte24BitKey;
                tripleDes.Mode = CipherMode.ECB;
                tripleDes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cryptoTransform = tripleDes.CreateDecryptor();
                byte[] resultArray = cryptoTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
                tripleDes.Clear();

                UTF8Encoding encoder = new UTF8Encoding();
                return encoder.GetString(resultArray);
            }
            catch (Exception ex)
            {
                MethodBase method = MethodBase.GetCurrentMethod();
                StackTrace trace = new StackTrace(ex, true);
                Error = method.DeclaringType.Name + "." + method.Name + " : " + ex.Message + " in line " + trace.GetFrame(0).GetFileLineNumber();
                return "";
            }
        }

        /// <summary>
        /// Method for encrypting password.
        /// </summary>
        /// <param name="DecryptedString">String to be Encrypt</param>
        /// <returns>Encrypted string</returns>
        public static string Encrypt(this string DecryptedString)
        {
            string Error = "";
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(DecryptedString);

                string key = "Autom@tion";

                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                hashmd5.Clear();

                byte[] key24Array = new byte[24];

                for (int i = 0; i < 16; i++)
                {
                    key24Array[i] = keyArray[i];
                }

                for (int i = 0; i < 7; i++)
                {
                    key24Array[i + 16] = keyArray[i];
                }

                TripleDESCryptoServiceProvider tripledes = new TripleDESCryptoServiceProvider();
                tripledes.Key = key24Array;
                tripledes.Mode = CipherMode.ECB;
                tripledes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cryptoTransform = tripledes.CreateEncryptor();
                byte[] resultArray = cryptoTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                tripledes.Clear();

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception ex)
            {
                MethodBase method = MethodBase.GetCurrentMethod();
                StackTrace trace = new StackTrace(ex, true);
                Error = method.DeclaringType.Name + "." + method.Name + " : " + ex.Message + " in line " + trace.GetFrame(0).GetFileLineNumber();
                return "";
            }
        }
        #endregion

        #region Error Logging
        /// <summary>
        /// Error logging method.
        /// </summary>
        /// <param name="ErrorLog">Exception object</param>
        /// <param name="MethodName">Current Method Name</param>
        /// <param name="ClassName">Current Class Name</param>
        /// <param name="UniqueId">Case id during which error occurred.</param>
        public static void LogError<T>(this T ErrorLog, string MethodName, string ClassName, string UniqueId = "") where T : _Exception
        {
            var path = "";
            string date = DateTime.Now.ToString("MMddyyyy");
            try
            {
                List<string> MSID = GetMSID();
                if (MSID.Any(lst => lst.ToUpper() == Environment.UserName.ToUpper()))
                {
                    string ErrorFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ERROR LOGS";
                    string ApplicationFolder = ErrorFolder + "\\" + Application.ProductName;

                    if (!Directory.Exists(ApplicationFolder))
                        Directory.CreateDirectory(ApplicationFolder);

                    path = Path.Combine(ApplicationFolder + @"\Error_Log" + date + ".txt");
                }
                else
                {
                    if (!string.IsNullOrEmpty(SharedFolderPath) && SharedFolderPath.CheckAccessOnFolder())
                    {
                        string ErrorFolder = SharedFolderPath + @"ERROR LOGS\";
                        string MSIDFolder = ErrorFolder + Environment.UserName.ToUpper();
                        string ApplicationFolder = MSIDFolder + "\\" + Application.ProductName;

                        if (!Directory.Exists(ApplicationFolder))
                            Directory.CreateDirectory(ApplicationFolder);

                        path = Path.Combine(ApplicationFolder + @"\Error_Log" + date + ".txt");
                    }
                    else
                    {
                        string ErrorFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ERROR LOGS\";
                        string ApplicationFolder = ErrorFolder + "\\" + Application.ProductName;

                        if (!Directory.Exists(ApplicationFolder))
                            Directory.CreateDirectory(ApplicationFolder);

                        path = Path.Combine(ApplicationFolder + @"\Error_Log" + date + ".txt");
                    }
                }

                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }
                using (TextWriter writer = new StreamWriter(path, true))
                {
                    string LineNumber = "";
                    if (System.Text.RegularExpressions.Regex.IsMatch(input: ErrorLog.StackTrace, pattern: @"line (\d+)+"))
                        LineNumber = ErrorLog.StackTrace.Substring(ErrorLog.StackTrace.IndexOf("line")).Replace("line", "").Trim();
                    string LogError = "";
                    if (ErrorLog.TargetSite.DeclaringType != null)
                        LogError = string.Format($"Error Log Date : {DateTime.Now.ToString("MM/dd/yyyy")} \r\nCase ID : {UniqueId} \r\nError: {ErrorLog.Message} \r\nClass Name: {ErrorLog.TargetSite.DeclaringType.Name} \r\nMethod Name: {ErrorLog.TargetSite.Name} \r\nRoot Class Name: {ClassName} \r\nRoot Method Name: {MethodName} \r\nError occurred in Line: {LineNumber} \r\nLocation Details: {ErrorLog.StackTrace.Trim()}") + Environment.NewLine;
                    else
                        LogError = string.Format($"Error Log Date : {DateTime.Now.ToString("MM/dd/yyyy")} \r\nCase ID : {UniqueId} \r\nError: {ErrorLog.Message} \r\nClass Name: {""} \r\nMethod Name: {ErrorLog.TargetSite.Name} \r\nRoot Class Name: {ClassName} \r\nRoot Method Name: {MethodName} \r\nError occurred in Line: {LineNumber} \r\nLocation Details: {ErrorLog.StackTrace.Trim()}") + Environment.NewLine;

                    if (string.IsNullOrEmpty(SharedFolderPath))
                    {
                        string EmailMessage = "<html><head><style>div{background-color: white;text-align: left;color: balck;box-shadow: 10px 20px 30px grey;margin: 10% 10% 10% 10%;padding: 0.5% 0% 0.5% 1%;}</style></head><body><div><p>";
                        if (ErrorLog.TargetSite.DeclaringType != null)
                            EmailMessage = EmailMessage + string.Format($"Hello Team, <br /> <br />Error Log Date : {DateTime.Now.ToString("MM/dd/yyyy")} <br />Case ID : {UniqueId} <br />Error: {ErrorLog.Message} <br />Class Name: {ErrorLog.TargetSite.DeclaringType.Name} <br />Method Name: {ErrorLog.TargetSite.Name} <br />Root Class Name: {ClassName} <br />Root Method Name: {MethodName} <br />Error occurred in Line: {LineNumber} <br />Location Details: {ErrorLog.StackTrace.Trim()} <br />");
                        else
                            EmailMessage = EmailMessage + string.Format($"Hello Team, <br /> <br />Error Log Date : {DateTime.Now.ToString("MM/dd/yyyy")} <br />Case ID : {UniqueId} <br />Error: {ErrorLog.Message} <br />Class Name: {""} <br />Method Name: {ErrorLog.TargetSite.Name} <br />Root Class Name: {ClassName} <br />Root Method Name: {MethodName} <br />Error occurred in Line: {LineNumber} <br />Location Details: {ErrorLog.StackTrace.Trim()} <br />");

                        EmailMessage = EmailMessage + string.Format($"<br />Thanks,<br />Rapid Response Team,<br />M & R Operations & Technology</p></div></body></html>");

                        SendMail(FromAddress: Application.ProductName + "Macro@uhc.com",
                             Host: "mailo2.uhc.com",
                             ToAddress: "bhumika_prabhu@uhc.com;swarupa.oruganti@uhc.com;achumol_abraham@uhc.com;nehataj_p@uhc.com;prakash.madeshwaran@uhc.com;",//"XLHMacroSupport_DL@ds.uhc.com;",
                             CcAddress: "sourav.nanda@uhc.com;krithika.shetty@uhc.com;",
                             SubjectLine: Application.ProductName + " Macro Error",
                             BodyText: EmailMessage);
                    }

                    writer.WriteLine(LogError);
                    writer.Flush();
                }
                MessageBox.Show("Error Occurred." + Environment.NewLine + "Please contact the macro support team (XLHMacroSupport_DL@ds.uhc.com).", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName);
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Logs error in a database for provided error object.
        /// </summary>
        /// <param name="ErrorLog">Exception object</param>
        /// <param name="CaseID">Case id during which error occurred</param>
        /// <param name="TrackingNumber">Case number during which error occurred</param>
        /// <param name="myConnectionString">Connection string for connecting to database</param>
        /// <param name="myCommandType">Command Type of the command provided</param>
        /// <param name="MethodName">Current Method Name</param>
        /// <param name="ClassName">Current Class Name</param>
        /// <param name="TableName">Table name into which error to be logged</param>
        /// <param name="StoredProcedureName">Stored procedure name using which error will be logged</param>
        public static void LogError<T>(this T ErrorLog, string CaseID, string TrackingNumber, string myConnectionString, QueryType myCommandType, string MethodName, string ClassName, string TableName = "", string StoredProcedureName = "") where T: _Exception
        {
            SqlConnection Conn = null;
            try
            {
                Conn = new SqlConnection(myConnectionString);
                if (Conn.CheckDatabaseAccess())
                {
                    switch (myCommandType)
                    {
                        case QueryType.ftInline:
                            DataTable TableColumns = new DataTable();
                            string query = "select column_name from INFORMATION_SCHEMA.COLUMNS where Table_Name = '@TableName'";
                            using (SqlCommand cmd = new SqlCommand(query, Conn))
                            {
                                cmd.Parameters.AddWithValue("@TableName", TableName);
                                SqlDataAdapter daCommand = new SqlDataAdapter(cmd);
                                daCommand.Fill(TableColumns);
                            }

                            if (TableColumns != null && TableColumns.Rows.Count > 0)
                            {
                                string Query = "insert into [dbo].[" + TableName + "](";
                                for (int i = 1; i < TableColumns.Rows.Count; i++)
                                {
                                    if (TableColumns.Rows.Count - 1 != i)
                                        Query += TableColumns.Rows[i]["column_name"].ToString() + ",";
                                    else
                                        Query += TableColumns.Rows[i]["column_name"].ToString();
                                }

                                Query = Query + ")" + "values(@CaseID, @TrackingNumber, '@ClassName', '@MethodName', '@ErrMessage', '@ErrStacktrace', '@UserName', '@ProductName', getdate(), 1";

                                using (SqlCommand cmd = new SqlCommand(Query, Conn))
                                {
                                    cmd.Parameters.AddWithValue("@CaseID", CaseID);
                                    cmd.Parameters.AddWithValue("@TrackingNumber", TrackingNumber);
                                    cmd.Parameters.AddWithValue("@ClassName", ClassName);
                                    cmd.Parameters.AddWithValue("@MethodName", MethodName);
                                    cmd.Parameters.AddWithValue("@ErrMessage", ErrorLog.Message);
                                    cmd.Parameters.AddWithValue("@ErrStacktrace", ErrorLog.StackTrace);
                                    cmd.Parameters.AddWithValue("@UserName", Environment.UserName.ToUpper());
                                    cmd.Parameters.AddWithValue("@ProductName", Application.ProductName.ToUpper());
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandTimeout = 160000;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            break;
                        case QueryType.ftStoredProcedure:
                            DataTable SPParameters = new DataTable();
                            query = "select parameter_name from information_schema.parameters where specific_name='@StoredProcedureName'";
                            using (SqlCommand cmd = new SqlCommand(query, Conn))
                            {
                                cmd.Parameters.AddWithValue("@StoredProcedureName", StoredProcedureName);
                                SqlDataAdapter daCommand = new SqlDataAdapter(cmd);
                                daCommand.Fill(SPParameters);
                            }
                            if (SPParameters != null && SPParameters.Rows.Count > 0)
                            {
                                using (SqlCommand cmd = new SqlCommand(StoredProcedureName, Conn))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;

                                    if (!string.IsNullOrEmpty(CaseID))
                                        cmd.Parameters.AddWithValue(SPParameters.Rows[0]["parameter_name"].ToString(), CaseID);
                                    cmd.Parameters.AddWithValue(SPParameters.Rows[1]["parameter_name"].ToString(), TrackingNumber);
                                    cmd.Parameters.AddWithValue(SPParameters.Rows[2]["parameter_name"].ToString(), ClassName);
                                    cmd.Parameters.AddWithValue(SPParameters.Rows[3]["parameter_name"].ToString(), MethodName);
                                    cmd.Parameters.AddWithValue(SPParameters.Rows[4]["parameter_name"].ToString(), ErrorLog.Message);
                                    cmd.Parameters.AddWithValue(SPParameters.Rows[5]["parameter_name"].ToString(), ErrorLog.StackTrace);
                                    cmd.Parameters.AddWithValue(SPParameters.Rows[6]["parameter_name"].ToString(), Environment.UserName.ToUpper());
                                    cmd.Parameters.AddWithValue(SPParameters.Rows[7]["parameter_name"].ToString(), Application.ProductName);
                                    cmd.Parameters.AddWithValue(SPParameters.Rows[8]["parameter_name"].ToString(), 1);

                                    cmd.CommandTimeout = 160000;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                    MessageBox.Show("You donot have access to the database that you provided." + Environment.NewLine + Environment.NewLine + "Please contact your supervisor for getting the access.", Application.ProductName);
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                    Conn.Close();
            }
        }

        /// <summary>
        /// Checks database access for the user.
        /// </summary>
        /// <param name="connection">SQL Connection Object</param>
        /// <returns>boolean</returns>
        private static bool CheckDatabaseAccess(this SqlConnection connection)
        {
            bool rtnValue = false;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                    rtnValue = true;
                }
            }
            catch (Exception)
            {
                rtnValue = false;
            }
            return rtnValue;
        }

        /// <summary>
        /// Gets List of MSID to log error in local path.
        /// </summary>
        /// <returns>List of MSID</returns>
        private static List<string> GetMSID()
        {
            List<string> MSID = new List<string>();
            try
            {
                MSID.Add("kmeehari");
                MSID.Add("snanda12");
                MSID.Add("pmanikan");
                MSID.Add("evelayut");
                MSID.Add("rkptha");
                MSID.Add("sb1014");
                MSID.Add("mnaushee");
                MSID.Add("akuma198");
                MSID.Add("rdandu1");
                MSID.Add("aananth2");
            }
            catch (Exception ex)
            {
                ex.LogError(UniqueId: MSID.Count.ToString(), MethodName: MethodBase.GetCurrentMethod().Name, ClassName: MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return MSID;
        }

        /// <summary>
        /// Checks access to the shared folder.
        /// </summary>
        /// <param name="FolderPath">Gets the folder path to check for the access</param>
        /// <returns>boolean</returns>
        private static bool CheckAccessOnFolder(this string FolderPath)
        {
            bool IsHavingAccess = true;
            try
            {
                DirectoryInfo remoteDirectory = new DirectoryInfo(FolderPath);
                var AccessControl = remoteDirectory.GetAccessControl();
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("No Access to the Folder: " + FolderPath);
                IsHavingAccess = false;
            }
            return IsHavingAccess;
        }
        #endregion

        #region Mailling
        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="FromAddress">From Address</param>
        /// <param name="Host">Host</param>
        /// <param name="ToAddress">To Address</param>
        /// <param name="SubjectLine">Subject Line</param>
        /// <param name="BodyText">Body Text</param>
        /// <param name="IsHTML">'True' if body text is HTML else 'False'</param>
        /// <param name="IsImportant">'True' if mail is important else 'False'</param>
        /// <param name="CcAddress">CC Mail IDs</param>
        /// <param name="BccAddress">BCC Mail IDs</param>
        /// <returns>'True' if successfully sends email else 'False'</returns>
        public static bool SendMail(this string FromAddress, string Host, string ToAddress, string SubjectLine, string BodyText, bool IsHTML = true, bool IsImportant = true, string CcAddress = "", string BccAddress = "")
        {
            SmtpClient smtp = new SmtpClient();
            bool rtnValue = false;
            try
            {
                MailMessage mail = new MailMessage();
                if (!string.IsNullOrEmpty(ToAddress))
                {
                    string[] AllToAddress = ToAddress.Split(';');
                    //Split and added To
                    foreach (string ToAdd in AllToAddress)
                    {
                        if (!string.IsNullOrEmpty(ToAdd))
                        {
                            if (ToAdd.ValidateEmailId())
                                mail.To.Add(ToAdd);
                        }
                    }
                    //Split and added CC
                    if (!string.IsNullOrEmpty(CcAddress))
                    {
                        string[] AllCcAddress = CcAddress.Split(';');
                        foreach (string CcAdd in AllCcAddress)
                        {
                            if (!string.IsNullOrEmpty(CcAdd))
                            {
                                if (CcAdd.ValidateEmailId())
                                    mail.CC.Add(CcAdd);
                            }
                        }
                    }

                    //Split and added BCC
                    if (!string.IsNullOrEmpty(BccAddress))
                    {
                        string[] AllBccAddress = BccAddress.Split(';');
                        foreach (string BccAdd in AllBccAddress)
                        {
                            if (!string.IsNullOrEmpty(BccAdd))
                            {
                                if (BccAdd.ValidateEmailId())
                                    mail.Bcc.Add(BccAdd);
                            }
                        }
                    }

                    //No Valid Email Ids found
                    if (mail.To.Count == 0)
                        return rtnValue;

                    if (IsImportant)
                        mail.Priority = MailPriority.High;

                    mail.From = new MailAddress(FromAddress);
                    mail.Subject = SubjectLine;

                    string Body = BodyText;
                    mail.Body = Body;
                    mail.IsBodyHtml = IsHTML;
                    smtp.Host = Host;
                    //Send Mail
                    smtp.Send(mail);
                    rtnValue = true;
                }
                else
                {
                    MessageBox.Show("To address Email Id is missing", Application.ProductName);
                    rtnValue = false;
                }
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: MethodBase.GetCurrentMethod().Name, ClassName: MethodBase.GetCurrentMethod().DeclaringType.Name);
                rtnValue = false;
            }
            return rtnValue;
        }

        /// <summary>
        /// Validate the Email id
        /// </summary>
        /// <param name="EmailId">Email ID</param>
        /// <returns>'True' if email is valid else 'False'</returns>
        private static bool ValidateEmailId(this string EmailId)
        {
            bool rtnValue = true;
            try
            {
                string MatchEmailPattern =
                           @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
                         + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
                                [0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
                         + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
                                [0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
                         + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

                if (!System.Text.RegularExpressions.Regex.IsMatch(EmailId, MatchEmailPattern))
                    rtnValue = false;
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: MethodBase.GetCurrentMethod().Name, ClassName: MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return rtnValue;
        }
        #endregion

        #region Config Details
        /// <summary>
        /// Gets the URLs from the config file provided
        /// </summary>
        /// <param name="ConfigPath">Local Path to configuration file</param>
        /// <returns>dictionary of the URL and their names in the config file provided</returns>
        public static Dictionary<string, string> GetURLFromConfig(this string ConfigPath)
        {
            Dictionary<string, string> URLs = new Dictionary<string, string>();
            try
            {
                System.Configuration.ExeConfigurationFileMap fileMap = new System.Configuration.ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = ConfigPath;
                System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, System.Configuration.ConfigurationUserLevel.None);
                URLs = config.AppSettings.Settings.AllKeys.ToDictionary(key => key, value => config.AppSettings.Settings[value].Value);
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return URLs;
        }
        #endregion

        #region JSON Convert
        /// <summary>
        /// Loads the Quetion and answers
        /// </summary>
        /// <param name="LocalPath">Path of the INI file</param>
        /// <param name="FileName">Name of file</param>
        /// <param name="FilterName">Filter name</param>
        /// <returns>List of question and answer</returns>
        public static bool SaveObject<T>(this T ClassObject, string LocalPath, string FileName) where T : class
        {
            bool rtnValue = false;
            string JSONString = string.Empty;
            string IniPath = "";
            try
            {
                if (System.Windows.Forms.Application.StartupPath.StartsWith("\\\\"))
                    IniPath = LocalPath + System.Configuration.ConfigurationManager.AppSettings["FolderPath_Citrix"].ToString();
                else
                    IniPath = LocalPath + System.Configuration.ConfigurationManager.AppSettings["FolderPath_NonCitrix"].ToString();

                if (ClassObject != null)
                {
                    JSONString = ClassObject.ToBeaitifiedJSON();
                }

                string path = LocalPath + FileName + ".json";
                if (!string.IsNullOrEmpty(JSONString))
                {
                    if (!System.IO.File.Exists(path: path))
                        System.IO.File.Create(path: path).Close();
                }

                using (System.IO.TextWriter writer = new System.IO.StreamWriter(path, false))
                {
                    writer.WriteLine(JSONString);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return rtnValue;
        }
        #endregion

        #region Search
        /// <summary>
        /// Searches the list using binary search technique
        /// </summary>
        /// <typeparam name="T">Type Parameter</typeparam>
        /// <param name="SearchList">List to be searched</param>
        /// <param name="Key">Search Key</param>
        /// <param name="Comparer"></param>
        /// <returns></returns>
        public static int Search<T>(this IEnumerable<T> SearchList, T Key, Comparer<T> Comparer)
        {
            try
            {
                int high, low, mid;
                high = SearchList.Count() - 1;
                low = 0;
                if (SearchList.ToList()[0].Equals(Key))
                    return 0;
                else if (SearchList.ToList()[high].Equals(Key))
                    return high;
                else
                {
                    while (low <= high)
                    {
                        mid = (high + low) / 2;
                        if (Comparer.Compare(SearchList.ToList()[mid], Key) == 0)
                            return mid;
                        else if (Comparer.Compare(SearchList.ToList()[mid], Key) > 0)
                            high = mid - 1;
                        else
                            low = mid + 1;
                    }
                    return -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
    }
}
