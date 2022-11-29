using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Xml;

namespace RNPO_WEBSERVICE
{

    /// <summary>
    /// Summary description for SiteSurvey
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class SiteSurvey : System.Web.Services.WebService
    {
        public SqlConnection connection;
        public SqlCommand cmd;
        string strconnection = ConfigurationManager.AppSettings["connstring"];//"Data Source=ATBUKIHQPLP006;Initial Catalog=survey;User ID=sa;Password=sa";
        string strPathPicture = ConfigurationManager.AppSettings["destinationPictures"]; //@"C:/wamp64/www/africell/AFRICELL_SURVEY/Pictures/";
                                                                                         //string strPersonalPhotoName = "";

        [System.Runtime.InteropServices.DllImport("advapi32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
        [WebMethod]
        public void LogonWin()
        {
            try
            {
                IntPtr tokenHandle = new IntPtr(0);
                bool returnValue = LogonUser("administrator", "AFRICELLRDC", "p@ssw0rd", 2, 0, ref tokenHandle);
                if (!returnValue)
                    throw new Exception("Logon failed.");

                System.Security.Principal.WindowsImpersonationContext impersonatedUser = null;
                System.Security.Principal.WindowsIdentity wid = new System.Security.Principal.WindowsIdentity(tokenHandle);
                impersonatedUser = wid.Impersonate();

                System.IO.File.Copy(@"C:\wamp64\www\africell\AFRICELL_SURVEY\Pictures\20200303\KIN001_100_03032020150224.JPEG", @"\\10.100.5.8\Users\Administrator\Documents\", true);

            }
            catch (Exception e)
            {
                String Respo = e.ToString();
            }

        }

        [WebMethod]
        public void mysqlconnec(string surl, string uid, string pwd, string data)
        {

            string server = surl;
            string database = data;
            //string  uid = uid;
            string password = pwd;
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();

        }

        [WebMethod]
        public void login(String SV_userName, String SV_passWord, String SV_Device)
        {
            string SV_IsWeb = "0";
            //String SV_userId, SV_password, SV_deviceid;
            try
            {
                // SV_userName = (Context.Request["SV_userName"] != null) ? Context.Request["SV_userName"] : "";
                // SV_password = (Context.Request["SV_passWord"] != null) ? Context.Request["SV_passWord"] : "";
                // SV_deviceid = (Context.Request["SV_Device"] != null) ? Context.Request["SV_Device"] : "";

                if (SV_userName == null)
                {
                    Context.Response.Write("userName or password invalide.");
                    return;
                }

                if (SV_passWord == null)
                {
                    Context.Response.Write("userName or password invalide.");
                    return;
                }


            }

            catch (Exception e)
            {
                Context.Response.Write("Error:" + e.Message);
                return;
            }


            try
            {
                connection = new SqlConnection(strconnection);
                connection.Open();
                string Sqlcmd = "EXEC [survey_db].[dbo].[sp_login] " +
                  "'" + SV_userName + "'," +
                  "'" + SV_passWord + "'," +
                  "'" + SV_Device + "'" +
                  "'" + SV_IsWeb + "'";
                cmd = new SqlCommand(Sqlcmd, connection);
                int res;
                // res = cmd.ExecuteNonQuery();
                SqlDataReader re = cmd.ExecuteReader();
                string SV_role = "2";
                if (re.HasRows == true)
                {
                    re.Read();
                    SV_userName = re["SV_userName"].ToString();
                    SV_role = re["SV_role"].ToString();
                    // userId = SV_userName;
                    Context.Response.Write("login:" + SV_userName + ":" + SV_role);

                }
                else
                {
                    Context.Response.Write("userName or password invalide.");
                }



            }
            catch (Exception e)
            {
                Context.Response.Write("Error:" + e.Message);
            }




        }

        [WebMethod]
        public void RFdata()
        {

            try
            {

                DateTime dte;
                dte = DateTime.Now.AddDays(0);
                //.ToString("dd-MM-yyyy HH:mm:ss");
                string IndxDate = dte.ToString("ddMMyyyyHHmmss");

                string resImg = "";
                string resInsert = "";

                //Context.Response.Clear();
                Context.Response.ContentType = "text/xml";
                // string strXmlData = Context.Request["strXmlData"];
                //StreamReader xmlStream = new StreamReader(strXmlData);
                StreamReader xmlStream = new StreamReader(Context.Request.InputStream);


                string xmlData = xmlStream.ReadToEnd();

                StringReader xml = new StringReader(xmlData);
                XmlReader reader;

                reader = XmlReader.Create(xml);
                string _FirstName = "";
                string _strPersonalPhoto = "";

                while (reader.Read())
                {

                    if (reader.Name.Equals("sector"))
                    {
                        reader.Read();
                        _FirstName = reader.Value;
                    }

                    if (reader.Name.Equals("pictures"))
                    {
                        reader.Read();
                        _strPersonalPhoto = reader.Value;
                    }

                }

                string strPersonalPhotoName = strPathPicture + _FirstName + "_" + IndxDate + ".JPG";
                string res = "";
                if (!_strPersonalPhoto.Equals(""))
                {

                    resImg = strStringToImage(strPersonalPhotoName, _strPersonalPhoto);
                    //insert call
                    //   res = InsertSite("1",_FirstName, _strPersonalPhoto,strPersonalPhotoName);
                    Context.Response.Write("ok");
                }

                Context.Response.Clear();
                Context.Response.ContentType = "text/xml";
                Context.Response.ContentEncoding = Encoding.UTF8;
                Context.Response.Write("<Reply>");
                Context.Response.Write("<MESSAGE>Successful</MESSAGE>");
                Context.Response.Write("</Reply>");

            }
            catch (Exception ex)
            {
                //sWriteErrorLog1(ex.ToString)
                Context.Response.Clear();
                Context.Response.ContentType = "text/xml";
                Context.Response.ContentEncoding = Encoding.UTF8;
                Context.Response.Write("<Reply>");
                Context.Response.Write("<MESSAGE>" + ex.Message + "</MESSAGE>");
                Context.Response.Write("</Reply>");
            }



        }

        [WebMethod]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Xml)]
        public void Insertrfdata()
        {
            try
            {

                string SV_IdOperation = (Context.Request["SV_IdOperation"] != null) ? Context.Request["SV_IdOperation"] : "";
                string SV_SiteName = (Context.Request["SV_SiteName"] != null) ? Context.Request["SV_SiteName"] : "";
                string SV_SiteConfig = (Context.Request["SV_SiteConfig"] != null) ? Context.Request["SV_SiteConfig"] : "";
                string SV_DateTime = (Context.Request["SV_DateTime"] != null) ? Context.Request["SV_DateTime"] : "";
                string SV_Coordinates = (Context.Request["SV_Coordinates"] != null) ? Context.Request["SV_Coordinates"] : "";
                string SV_AuditedBy = (Context.Request["SV_AuditedBy"] != null) ? Context.Request["SV_AuditedBy"] : "";
                string SV_Rigger = (Context.Request["SV_Rigger"] != null) ? Context.Request["SV_Rigger"] : "";

                string SV_Owner = (Context.Request["SV_Owner"] != null) ? Context.Request["SV_Owner"] : "";
                string SV_Vendor = (Context.Request["SV_Vendor"] != null) ? Context.Request["SV_Vendor"] : "";
                string SV_RooftopTower = (Context.Request["SV_RooftopTower"] != null) ? Context.Request["SV_RooftopTower"] : "";
                string SV_BuildingHweight = (Context.Request["SV_BuildingHweight"] != null) ? Context.Request["SV_BuildingHweight"] : "";
                string SV_TotNumAntennas = (Context.Request["SV_TotNumAntennas"] != null) ? Context.Request["SV_TotNumAntennas"] : "";
                string SV_Tech = (Context.Request["SV_Tech"] != null) ? Context.Request["SV_Tech"] : "";
                string SV_Sector = (Context.Request["SV_Sector"] != null) ? Context.Request["SV_Sector"] : "";
                string SV_Description = (Context.Request["SV_Description"] != null) ? Context.Request["SV_Description"] : "";
                string SV_Pictures = (Context.Request["SV_Pictures"] != null) ? Context.Request["SV_Pictures"] : "";
                string SV_Plan = (Context.Request["SV_Plan"] != null) ? Context.Request["SV_Plan"] : "";
                string SV_Actual = (Context.Request["SV_Actual"] != null) ? Context.Request["SV_Actual"] : "";
                string SV_AntennaModel = (Context.Request["SV_AntennaModel"] != null) ? Context.Request["SV_AntennaModel"] : "";
                string SV_Remarks = (Context.Request["SV_Remarks"] != null) ? Context.Request["SV_Remarks"] : "";
                string SV_Device = (Context.Request["SV_Device"] != null) ? Context.Request["SV_Device"] : "";
                string SV_Status = (Context.Request["SV_Status"] != null) ? Context.Request["SV_Status"] : "";

                string SV_Antenne = (Context.Request["SV_Antenne"] != null) ? Context.Request["SV_Antenne"] : "";
                string SV_Ports = (Context.Request["SV_Ports"] != null) ? Context.Request["SV_Ports"] : "";

                if (SV_IdOperation.Equals(""))
                {
                    Context.Response.Write("Error occur: failed!");
                    return;
                }
                else
                {
                    InsertSite(
                        SV_IdOperation
                       , SV_SiteName
                       , SV_SiteConfig
                       , SV_DateTime
                       , SV_Coordinates
                       , SV_AuditedBy
                       , SV_Rigger
                       , SV_Owner
                       , SV_Vendor
                       , SV_RooftopTower
                       , SV_BuildingHweight
                       , SV_TotNumAntennas
                       , SV_Tech
                       , SV_Sector
                       , SV_Description
                       , SV_Pictures
                       , ""
                       , SV_Plan
                       , SV_Actual
                       , SV_AntennaModel
                       , SV_Remarks
                       , SV_Device
                       , SV_Status
                        , SV_Antenne
                       , SV_Ports
                       );

                    Context.Response.Write("rdfdata : uploaded");
                }

            }
            catch (Exception e)
            {
                //ecrire dans log
                Context.Response.Write(e.Message);

            }




        }

        [WebMethod]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Xml)]
        public void InsertRegistration()
        {

            try
            {

                string SV_IdOperation = (Context.Request["SV_IdOperation"] != null) ? Context.Request["SV_IdOperation"] : "";
                string SV_SiteName = (Context.Request["SV_SiteName"] != null) ? Context.Request["SV_SiteName"] : "";
                string SV_SiteConfig = (Context.Request["SV_SiteConfig"] != null) ? Context.Request["SV_SiteConfig"] : "";
                string SV_DateTime = (Context.Request["SV_DateTime"] != null) ? Context.Request["SV_DateTime"] : "";
                string SV_Coordinates = (Context.Request["SV_Coordinates"] != null) ? Context.Request["SV_Coordinates"] : "";
                string SV_AuditedBy = (Context.Request["SV_AuditedBy"] != null) ? Context.Request["SV_AuditedBy"] : "";
                string SV_Rigger = (Context.Request["SV_Rigger"] != null) ? Context.Request["SV_Rigger"] : "";

                string SV_Owner = (Context.Request["SV_Owner"] != null) ? Context.Request["SV_Owner"] : "";
                string SV_Vendor = (Context.Request["SV_Vendor"] != null) ? Context.Request["SV_Vendor"] : "";
                string SV_RooftopTower = (Context.Request["SV_RooftopTower"] != null) ? Context.Request["SV_RooftopTower"] : "";
                string SV_BuildingHweight = (Context.Request["SV_BuildingHweight"] != null) ? Context.Request["SV_BuildingHweight"] : "";
                string SV_TotNumAntennas = (Context.Request["SV_TotNumAntennas"] != null) ? Context.Request["SV_TotNumAntennas"] : "";

                string SV_Tech = (Context.Request["SV_Tech"] != null) ? Context.Request["SV_Tech"] : "";
                string SV_Sector = (Context.Request["SV_Sector"] != null) ? Context.Request["SV_Sector"] : "";
                string SV_Description = (Context.Request["SV_Description"] != null) ? Context.Request["SV_Description"] : "";
                string SV_Pictures = (Context.Request["SV_Pictures"] != null) ? Context.Request["SV_Pictures"] : "";
                string SV_Plan = (Context.Request["SV_Plan"] != null) ? Context.Request["SV_Plan"] : "";
                string SV_Actual = (Context.Request["SV_Actual"] != null) ? Context.Request["SV_Actual"] : "";
                string SV_AntennaModel = (Context.Request["SV_AntennaModel"] != null) ? Context.Request["SV_AntennaModel"] : "";
                string SV_Remarks = (Context.Request["SV_Remarks"] != null) ? Context.Request["SV_Remarks"] : "";
                string SV_Device = (Context.Request["SV_Device"] != null) ? Context.Request["SV_Device"] : "";
                string SV_Status = (Context.Request["SV_Status"] != null) ? Context.Request["SV_Status"] : "";

                string SV_Antenne = (Context.Request["SV_Antenne"] != null) ? Context.Request["SV_Antenne"] : "";
                string SV_Ports = (Context.Request["SV_Ports"] != null) ? Context.Request["SV_Ports"] : "";

                DateTime dte;
                string strPersonalPhotoName;
                string resInsert = "";
                string strPathFolder;

                dte = DateTime.Now.AddDays(0);
                //.ToString("dd-MM-yyyy HH:mm:ss");
                string IndxDate = dte.ToString("ddMMyyyyHHmmss");
                strPathFolder = strPathPicture + dte.ToString("yyyyMMdd") + "/";

                System.IO.DirectoryInfo diDestination = new System.IO.DirectoryInfo(strPathFolder);

                if (!diDestination.Exists) diDestination.Create();

                //check if champs ok
                if (SV_IdOperation.Equals(""))
                {
                    Context.Response.Write("Error occur: failed!");
                    return;
                }
                else
                {
                    strPersonalPhotoName = strPathFolder + SV_SiteName + SV_Tech + SV_Sector + SV_Description + "_" + IndxDate + ".JPEG";

                    if (!SV_Pictures.Equals(""))
                    {

                        if ((strStringToImage(strPersonalPhotoName, SV_Pictures)).Equals("created"))
                        {
                            //insert call
                            resInsert = InsertSite(
                             SV_IdOperation
                            , SV_SiteName
                            , SV_SiteConfig
                            , SV_DateTime
                            , SV_Coordinates
                            , SV_AuditedBy
                            , SV_Rigger
                            , SV_Owner
                            , SV_Vendor
                            , SV_RooftopTower
                            , SV_BuildingHweight
                            , SV_TotNumAntennas
                            , SV_Tech
                            , SV_Sector
                            , SV_Description
                            , SV_Pictures
                            , strPersonalPhotoName
                            , SV_Plan
                            , SV_Actual
                            , SV_AntennaModel
                            , SV_Remarks
                            , SV_Device
                            , SV_Status
                             , SV_Antenne
                            , SV_Ports
                            );

                        }

                        if (!resInsert.Equals(""))
                        {
                            Context.Response.Write("uploaded :" + resInsert.ToString());
                        }

                    }

                }

                //Context.Response.ContentType = "text/xml";

            }
            catch (Exception e)
            {
                //ecrire dans log
                Context.Response.Write(e.Message);

            }

            //JavaScriptSerializer js = new JavaScriptSerializer();
            //Context.Response.Write(js.Serialize( ));
        }
        [WebMethod]
        public string strStringToImage(string _strFileName, string _PersonalPhotoBytes)
        {
            string ImgSaved = "";
            try
            {
                // convert String to Image and save
                Image image, image1;

                byte[] byteArray;
                byteArray = System.Convert.FromBase64String(_PersonalPhotoBytes);
                MemoryStream imgStream = new MemoryStream(byteArray);
                image = Image.FromStream(imgStream, false, false);
                image1 = image;
                image1.Save(_strFileName);
                ImgSaved = "created";
                return ImgSaved;
            }
            catch (Exception e)
            {
                //ecrire dans log
                //Context.Response.Write(e.Message);
                return e.Message;

            }
        }
        public string GeneratePasswordResetToken()
        {
            try
            {

                string token = Guid.NewGuid().ToString();
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(token);
                return Convert.ToBase64String(plainTextBytes);

                //Random rand = new Random();
                //int number = rand.Next(1, 100);
                //return number.ToString();
            }
            catch
            {
                return "";
            }
        }
        public string GetUniqueKey(int length)
        {
            try
            {
                string guidResult = string.Empty;
                while (guidResult.Length < length)
                {
                    // Get the GUID.
                    guidResult += Guid.NewGuid().ToString().GetHashCode().ToString("x");
                }

                // Make sure length is valid.
                if (length <= 0 || length > guidResult.Length)
                    throw new ArgumentException("Length must be between 1 and " + guidResult.Length);

                // Return the first length bytes.
                return guidResult.Substring(0, length);
            }
            catch
            {
                return "";
            }
        }



        [WebMethod]
        public void showImageFromStringBase64(string sStatus, string sIdoperation,
            string sDescription)
        {

            //string DefaultImagePath = HttpContext.Current.Server.MapPath("~/NoImage.jpg");

            //byte[] imageArray = System.IO.File.ReadAllBytes(DefaultImagePath);
            //string base64ImageRepresentation = Convert.ToBase64String(imageArray);
            //byte[] bytes = Convert.FromBase64String(base64ImageRepresentation);

            //using (MemoryStream ms = new MemoryStream(bytes))
            //{
            //    pic.Image = Image.FromStream(ms);
            //}
            try
            {

                byte[] imageBytes;

                // Pass in the image id we got from the querystring
                //SV_IdOperation, SV_SiteName, SV_Pictures
                string strCmd = " SELECT SV_Pictures FROM[Survey].[dbo].[tbl_SiteVisit] " +
                   "  where SV_Status = '" + sStatus + "' and SV_IdOperation = '" + sIdoperation + "' and SV_Description like '%" + sDescription + "%' ";

                connection = new SqlConnection(strconnection);

                SqlCommand cmd = new SqlCommand(strCmd, connection);
                connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())

                    {
                        imageBytes = System.Convert.FromBase64String(reader["SV_Pictures"].ToString());
                        // reader.GetBytes(0, 0, imageBytes, 0, int.MaxValue);

                        Context.Response.Buffer = true;
                        Context.Response.Charset = "";
                        Context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        Context.Response.ContentType = "image/jpg";
                        Context.Response.BinaryWrite(imageBytes);
                        Context.Response.End();
                    }

                }

            }
            catch (Exception e)
            {
                //Context.Response.Write("Error:" + e.Message);
                String xmlFile = "Error:" + e.Message;
                //Context.Response.Clear();
                //Context.Response.ContentType = "text/xml";
                //Context.Response.ContentEncoding = Encoding.UTF8;
                Context.Response.Write(xmlFile);
            }





        }

        public MemoryStream strBase64TomemoryStream(string _PersonalPhotoBytes)
        {
            MemoryStream imgStream = null;
            try
            {
                // convert String to Image and retourn memorystream
                Image image, image1;

                byte[] byteArray;
                byteArray = System.Convert.FromBase64String(_PersonalPhotoBytes);

                imgStream = new MemoryStream(byteArray);
                image = Image.FromStream(imgStream, false, false);
                image1 = image;
                //image1.Save(_strFileName);
                //ImgSaved = "created";
                return imgStream;
            }
            catch (Exception e)
            {
                //ecrire dans log
                //Context.Response.Write(e.Message);
                return imgStream;

            }
        }



        [WebMethod]
        public void showSite(string siteId)
        {
            string SentResult = String.Empty;
            string StatusCode = String.Empty;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(siteId);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader responseReader = new StreamReader(response.GetResponseStream());

            String resultmsg = responseReader.ReadToEnd();
            responseReader.Close();

            int StartIndex = 0;
            int LastIndex = resultmsg.Length;

            if (LastIndex > 0)
                SentResult = resultmsg.Substring(StartIndex, LastIndex);

            HttpStatusCode objHSC = response.StatusCode;
            StatusCode = objHSC.GetHashCode().ToString();

            responseReader.Dispose();
            Context.Response.Write(SentResult);

            //  return SentResult;

        }
        public string InsertSiteDetails(
                        string SV_IdOperation
                      , string SV_SiteName
                       , string SV_SiteConfig
                      , string SV_DateTime
                      , string SV_Coordinates
                      , string SV_AuditedBy
                      , string SV_Rigger
                      , string SV_Owner
                      , string SV_Vendor
                      , string SV_Vendor_2G
                      , string SV_Vendor_3G
                      , string SV_Vendor_4G
                      , string SV_RooftopTower
                      , string SV_BuildingHweight
                      , string SV_TotNumAntennas
                      , string SV_Tech
                      , string SV_Sector
                      , string SV_Description
                      , string SV_Pictures
                      , string strPersonalPhotoName
                      , string SV_Plan
                      , string SV_Actual
                      , string SV_AntennaModel
                      , string SV_Remarks
                      , string SV_Device
                      , string SV_Status
                      , string SV_Latitude_GPS
                     , string SV_Longitude_GPS
                     , string SV_userid
                     , string SV_Antenne
                     , string SV_Ports
                     , string SV_Flag)

        {
            string result = "";

            connection = new SqlConnection(strconnection);

            try
            {
                connection.Open();
                string Sqlcmd = "EXEC [survey].[dbo].[sp_insert_visit] " +
                  "'" + SV_IdOperation + "'," +
                  "'" + SV_SiteName + "'," +
                  "'" + SV_SiteConfig + "'," +
                  "'" + SV_DateTime + "'," +
                  "'" + SV_Coordinates + "'," +
                  "'" + SV_AuditedBy + "'," +
                  "'" + SV_Rigger + "'," +
                  "'" + SV_Owner + "'," +
                  "'" + SV_Vendor + "'," +
                  "'" + SV_Vendor_2G + "'," +
                  "'" + SV_Vendor_3G + "'," +
                  "'" + SV_Vendor_4G + "'," +
                  "'" + SV_RooftopTower + "'," +
                  "'" + SV_BuildingHweight + "'," +
                  "'" + SV_TotNumAntennas + "'," +
                  "'" + SV_Tech + "', " +
                  "'" + SV_Sector + "'," +
                  "'" + SV_Description + "'," +
                  "'" + SV_Pictures + "'," +
                  "'" + strPersonalPhotoName + "', " +
                  "'" + SV_Plan + "'," +
                  "'" + SV_Actual + "'," +
                  "'" + SV_AntennaModel + "'," +
                  "'" + SV_Remarks + "'," +
                  "'" + SV_Device + "'," +
                  "'" + SV_Status + "'," +
                  "'" + SV_Latitude_GPS + "'," +
                  "'" + SV_Longitude_GPS + "'," +
                  "'" + SV_userid + "'," +
                  "'" + SV_Antenne + "'," +
                  "'" + SV_Ports + "'," +
                   "'" + SV_Flag + "'";
                cmd = new SqlCommand(Sqlcmd, connection);
                int res;
                res = cmd.ExecuteNonQuery();

                /* insert binary pcture
                 * if (!SV_Pictures.Equals(""))
                 {


                     byte[] imageByte;// = File.ReadAllBytes("D:\\11.jpg");
                     imageByte = Convert.FromBase64String(SV_Pictures);
                     byte[] images = null;
                     MemoryStream Stream = new MemoryStream(imageByte);
                     BinaryReader brs = new BinaryReader(Stream);
                     images = brs.ReadBytes((int)Stream.Length);

                     SqlCommand sqlCommand = new SqlCommand("INSERT INTO  [survey].[dbo].pictures (siteName, site_picture) VALUES ('" + SV_SiteName + "', @site_picture)", connection);
                     // sqlCommand.Parameters.AddWithValue("@siteName", siteName);
                     sqlCommand.Parameters.AddWithValue("@site_picture", images);
                     res = sqlCommand.ExecuteNonQuery();
                 }
                 */

                return res.ToString();
            }
            catch (Exception e)
            {
                return "Error:" + e.Message;
            }

        }



        public string InsertSiteDetails_comment(
                       string SV_IdOperation
                     , string SV_SiteName
                      , string SV_Description
                      , string SV_Device
                     , string SV_Status
                     , string SV_userid
                    )

        {
            string result = "";

            connection = new SqlConnection(strconnection);

            try
            {
                connection.Open();
                string Sqlcmd = "EXEC [survey].[dbo].[sp_insert_visit_comment] " +
                  "'" + SV_IdOperation + "'," +
                  "'" + SV_SiteName + "'," +
                  "'" + SV_Description + "'," +
                  "'" + SV_Device + "'," +
                  "'" + SV_Status + "'," +
                  "'" + SV_userid + "'";

                cmd = new SqlCommand(Sqlcmd, connection);
                int res;
                res = cmd.ExecuteNonQuery();
                return res.ToString();
            }
            catch (Exception e)
            {
                return "Error:" + e.Message;
            }

        }


        public int MaxJsonLength { get; set; }
        JavaScriptSerializer jss = new JavaScriptSerializer();
        [WebMethod]
        public void getSiteDetailsById(string sitename, string idoperation)
        {


            connection = new SqlConnection(strconnection);
            List<siteVisit> listSitevisit = new List<siteVisit>();

            try
            {
                connection.Open();
                string Sqlcmd = " SELECT * from  [dbo].[tbl_SiteVisit] WHERE SV_SiteName = '" + sitename + "' and SV_IdOperation='" + idoperation + "' ORDER BY SV_SiteName";

                cmd = new SqlCommand(Sqlcmd, connection);
                SqlDataReader sqlReader = cmd.ExecuteReader();

                if (sqlReader.HasRows)
                {
                    while (sqlReader.Read())
                    {
                        siteVisit details = new siteVisit();
                        details.SV_IdOperation = sqlReader["SV_IdOperation"].ToString();
                        details.SV_SiteName = sqlReader["SV_SiteName"].ToString();
                        details.SV_SiteConfig = sqlReader["SV_SiteConfig"].ToString();
                        details.SV_DateTime = sqlReader["SV_DateTime"].ToString();
                        //details.SV_Coordinates = sqlReader["SV_Coordinates"].ToString();
                        // details.SV_AuditedBy = sqlReader["SV_AuditedBy"].ToString();
                        //details.SV_Rigger = sqlReader["SV_Rigger"].ToString();
                        details.SV_Owner = sqlReader["SV_Owner"].ToString();
                        // details.SV_Vendor = sqlReader["SV_Vendor"].ToString();
                        details.SV_Vendor_2G = sqlReader["SV_Vendor_2G"].ToString();
                        details.SV_Vendor_3G = sqlReader["SV_Vendor_3G"].ToString();
                        details.SV_Vendor_4G = sqlReader["SV_Vendor_4G"].ToString();
                        // details.SV_RooftopTower = sqlReader["SV_RooftopTower"].ToString();
                        // details.SV_BuildingHweight = sqlReader["SV_BuildingHweight"].ToString();
                        // details.SV_TotNumAntennas = sqlReader["SV_TotNumAntennas"].ToString();
                        details.SV_Tech = sqlReader["SV_Tech"].ToString();
                        //details.SV_Sector = sqlReader["SV_Sector"].ToString();
                        details.SV_Description = sqlReader["SV_Description"].ToString();
                        details.SV_Pictures = sqlReader["SV_Pictures"].ToString();
                        //details.SV_Plan = sqlReader["SV_Plan"].ToString();
                        details.SV_Actual = sqlReader["SV_Actual"].ToString();
                        //details.SV_AntennaModel = sqlReader["SV_AntennaModel"].ToString();
                        details.SV_Remarks = sqlReader["SV_Remarks"].ToString();
                        details.SV_Device = sqlReader["SV_Device"].ToString();
                        details.SV_Status = sqlReader["SV_Status"].ToString();
                        details.SV_Latitude_GPS = sqlReader["SV_Latitude"].ToString();
                        details.SV_Longitude_GPS = sqlReader["SV_Longitude"].ToString();
                        details.SV_Antenne = sqlReader["SV_Antenne"].ToString();
                        details.SV_Ports = sqlReader["SV_Ports"].ToString();

                        listSitevisit.Add(details);
                    }


                }


                JavaScriptSerializer js = new JavaScriptSerializer();
                js.MaxJsonLength = Int32.MaxValue;
                int JsonLength = js.Serialize(listSitevisit).Length;

                Context.Response.Write(js.Serialize(listSitevisit));

                connection.Close();
                cmd.Dispose();



            }
            catch (Exception e)
            {
                if (connection.State.Equals("Open"))
                {
                    connection.Close();
                }

                //Context.Response.Write("Error:" + e.Message);
                String xmlFile = "Error:" + e.Message;
                Context.Response.Clear();
                Context.Response.ContentType = "text/xml";
                Context.Response.ContentEncoding = Encoding.UTF8;
                Context.Response.Write(xmlFile);

            }


        }



        public string InsertSite(
                        string SV_IdOperation
                      , string SV_SiteName
                       , string SV_SiteConfig
                      , string SV_DateTime
                      , string SV_Coordinates
                      , string SV_AuditedBy
                      , string SV_Rigger
                      , string SV_Owner
                      , string SV_Vendor
                      , string SV_RooftopTower
                      , string SV_BuildingHweight
                      , string SV_TotNumAntennas
                      , string SV_Tech
                      , string SV_Sector
                      , string SV_Description
                      , string SV_Pictures
                      , string strPersonalPhotoName
                      , string SV_Plan
                      , string SV_Actual
                      , string SV_AntennaModel
                      , string SV_Remarks
                      , string SV_Device
                      , string SV_Status
                      , string SV_Antenne
                      , string SV_Ports)
        {
            string result = "";

            connection = new SqlConnection(strconnection);

            try
            {
                connection.Open();
                string Sqlcmd = "EXEC [survey].[dbo].[sp_insert_registration_site] " +
                  "'" + SV_IdOperation + "'," +
                  "'" + SV_SiteName + "'," +
                  "'" + SV_SiteConfig + "'," +
                  "'" + SV_DateTime + "'," +
                  "'" + SV_Coordinates + "'," +
                  "'" + SV_AuditedBy + "'," +
                  "'" + SV_Rigger + "'," +
                  "'" + SV_Owner + "'," +
                  "'" + SV_Vendor + "'," +
                  "'" + SV_RooftopTower + "'," +
                  "'" + SV_BuildingHweight + "'," +
                  "'" + SV_TotNumAntennas + "'," +
                  "'" + SV_Tech + "', " +
                  "'" + SV_Sector + "'," +
                  "'" + SV_Description + "'," +
                  "'" + SV_Pictures + "'," +
                  "'" + strPersonalPhotoName + "', " +
                  "'" + SV_Plan + "'," +
                  "'" + SV_Actual + "'," +
                  "'" + SV_AntennaModel + "'," +
                  "'" + SV_Remarks + "'," +
                  "'" + SV_Device + "'," +
                  "'" + SV_Status + "'," +
                  "'" + SV_Antenne + "'," +
                  "'" + SV_Ports + "'";

                cmd = new SqlCommand(Sqlcmd, connection);
                int res;
                res = cmd.ExecuteNonQuery();

                if (!SV_Pictures.Equals(""))
                {


                    byte[] imageByte;// = File.ReadAllBytes("D:\\11.jpg");
                    imageByte = Convert.FromBase64String(SV_Pictures);
                    byte[] images = null;
                    MemoryStream Stream = new MemoryStream(imageByte);
                    BinaryReader brs = new BinaryReader(Stream);
                    images = brs.ReadBytes((int)Stream.Length);


                    SqlCommand sqlCommand = new SqlCommand("INSERT INTO  [survey].[dbo].pictures (siteName, site_picture) VALUES ('" + SV_SiteName + "', @site_picture)", connection);
                    // sqlCommand.Parameters.AddWithValue("@siteName", siteName);
                    sqlCommand.Parameters.AddWithValue("@site_picture", images);
                    res = sqlCommand.ExecuteNonQuery();
                }




                return res.ToString();
            }
            catch (Exception e)
            {
                return "Error:" + e.Message;
            }

        }


        [WebMethod]
        public void getAllsiteName(string sitename)
        {
            string xmlFile = "";
            connection = new SqlConnection(strconnection);
            try
            {
                connection.Open();
                string Sqlcmd = "SELECT distinct site_name FROM survey.dbo.tbl_site WHERE  site_name like '%" + sitename + "%' ORDER BY site_name";
                cmd = new SqlCommand(Sqlcmd, connection);
                SqlDataReader sqlReader = cmd.ExecuteReader();


                if (sqlReader.HasRows)
                {
                    while (sqlReader.Read())
                    {
                        //Context.Response.Write(sqlReader["site_name"]);
                        xmlFile = xmlFile + "<siteRecord><SITENAME>" + sqlReader["site_name"] + "</SITENAME></siteRecord>";

                    }


                }

                connection.Close();
                cmd.Dispose();


            }
            catch (Exception e)
            {
                //Context.Response.Write("Error:" + e.Message);
                xmlFile = "<siteRecord><MESSAGE>" + "Error:" + e.Message + "</MESSAGE></siteRecord>";

            }

            Context.Response.Clear();
            Context.Response.ContentType = "text/xml";
            Context.Response.ContentEncoding = Encoding.UTF8;
            Context.Response.Write("<tbl_SiteName>");
            Context.Response.Write(xmlFile);
            Context.Response.Write("</tbl_SiteName>");
        }

        [WebMethod]
        public void getSiteVisited(string siteName)
        {
            connection = new SqlConnection(strconnection);
            try
            {

                System.Drawing.Image image, image1;
                byte[] byteArray;

                connection.Open();
                string Sqlcmd = "SELECT * FROM [survey].[dbo].[registration_site] s where siteName = '" + siteName + "'";
                cmd = new SqlCommand(Sqlcmd, connection);
                SqlDataReader sqlReader = cmd.ExecuteReader();
                if (sqlReader.HasRows)
                {
                    while (sqlReader.Read())
                    {
                        Context.Response.Write(sqlReader["siteName"]);

                        string pp = sqlReader["ImageString"].ToString();

                        // byteArray = Convert.FromBase64String(sqlReader["ImageString"].ToString());
                        byteArray = Encoding.Unicode.GetBytes(sqlReader["ImageString"].ToString());

                        MemoryStream imgStream = new MemoryStream(byteArray);
                        image = Image.FromStream(imgStream, false, false);

                        HttpContext contx;
                        Context.Response.ContentType = "image/png";
                        Context.Response.Write(image);

                    }


                }




                //image1 = image;
                //image1.Save(_strFileName);
                //ImgSaved = "";
                //return ImgSaved;

            }
            catch (Exception e)
            {
                Context.Response.Write("Error:" + e.Message);
            }
        }

        [WebMethod]
        public void setSynchronize()
        {
            try
            {

                String SV_IdOperation = (Context.Request["SV_IdOperation"] != null) ? Context.Request["SV_IdOperation"] : "";
                String SV_DateTime = (Context.Request["SV_DateTime"] != null) ? Context.Request["SV_DateTime"] : "";
                String SV_userid = (Context.Request["SV_userid"] != null) ? Context.Request["SV_userid"] : "";
                String SV_Audited = (Context.Request["SV_Audited"] != null) ? Context.Request["SV_Audited"] : "";
                String SV_Rugger = (Context.Request["SV_Rugger"] != null) ? Context.Request["SV_Rugger"] : "";
                String SV_Device = (Context.Request["SV_Device"] != null) ? Context.Request["SV_Device"] : "";

                Boolean bln = true;
                if (SV_IdOperation.Equals("") || SV_userid.Equals("") || SV_Audited.Equals("") || SV_Rugger.Equals(""))
                {
                    Context.Response.Write("Operation Failed");
                    return;

                }
                else
                {
                    if (sSynchronized(SV_IdOperation, SV_DateTime, SV_userid, SV_Audited, SV_Rugger, SV_Device))
                    {
                        //call  synchronized
                        Context.Response.Write("synchronized");
                    }
                    else
                    {
                        Context.Response.Write("Operation Failed");
                    }
                }

            }
            catch (Exception e)
            {
                Context.Response.Write(e.Message);
            }

        }

        private Boolean sSynchronized(String SV_IdOperation, String SV_DateTime, String SV_userid, String SV_Audited, String SV_Rugger, String SV_Device)
        {

            try
            {
                connection = new SqlConnection(strconnection);
                connection.Open();
                string Sqlcmd = "EXEC [survey].[dbo].[sp_sSynchronized] " +
                    "'" + SV_IdOperation + "'," +
                    "'" + SV_DateTime + "'," +
                    "'" + SV_userid + "'," +
                    "'" + SV_Audited + "'," +
                    "'" + SV_Rugger + "'," +
                    "'" + SV_Device + "'";

                cmd = new SqlCommand(Sqlcmd, connection);
                int res;
                res = cmd.ExecuteNonQuery();
                cmd.Dispose();
                connection.Close();
                return true;

            }
            catch (Exception e)
            {
                Context.Response.Write("Error:" + e.Message);
                return false;
            }



        }

        [WebMethod]
        public void getSiteconfiguration(string sitename)
        {
            getSitenameconfiguration(sitename);
        }
        public void getSitenameconfiguration(string sitename)
        {
            string xmlFile = "";
            connection = new SqlConnection(strconnection);
            List<SiteConfigutation> listsiteonfigutation = new List<SiteConfigutation>();
            String flage = "";

            try
            {
                string Sqlcmd = "";
                if (!sitename.Equals(""))
                {
                    if (sitename.Equals("all"))
                    {
                        flage = "1";
                        /* Sqlcmd = "SELECT a.id, a.Site_Name, s.latitude,s.longitude , a.Ant_model,a.Ant_port,a.full_index as Ant_fullname ,d.SV_datetime,d.SV_IdOperation,d.SV_userid " +
                               " FROM(select SV_IdOperation, SV_SiteName, SV_userid, min(cast(SV_datetime as date)) SV_datetime from tbl_SiteVisit " +
                               " GROUP BY  SV_IdOperation, SV_SiteName, SV_userid) d " +
                               "   inner join(select distinct Site_Name, latitude, longitude from tbl_site) s on d.SV_SiteName = s.Site_Name " +
                               "   inner join tbl_Site_Antenne a on a.Site_Name = d.SV_SiteName " +
                               "      ORDER BY SV_IdOperation,SV_SiteName,full_index,id ";

                     */

                    }
                    else
                    {
                        //Sqlcmd = " select a.id, a.Site_Name, c.latitude,c.longitude , a.Ant_model,a.Ant_port,a.full_index as Ant_fullname,SV_datetime=cast(getdate() as date) , SV_IdOperation=null,SV_userid=null" +
                        //  " from[dbo].[tbl_Site_Antenne] a " +
                        //   " inner join(select distinct Site_Name, latitude, longitude from tbl_site ) c on a.Site_Name=c.Site_Name " +
                        //    " WHERE  a.site_name = '" + sitename + "' ORDER BY a.full_index";

                    }
                }

                Sqlcmd = "Survey.[dbo].[sp_getsiteconfiguration] '','" + sitename + "','" + flage + "'";

                connection.Open();

                cmd = new SqlCommand(Sqlcmd, connection);
                SqlDataReader sqlReader = cmd.ExecuteReader();


                if (sqlReader.HasRows)
                {
                    /* XML FORMAT
                     * while (sqlReader.Read())
                     {
                         //Context.Response.Write(sqlReader["site_name"]);
                         xmlFile = xmlFile + "<siteRecord>" +
                             "<sitename>" + sqlReader["site_name"] + "</sitename>" +
                              "<latitude>" + sqlReader["latitude"] + "</latitude>" +
                              "<longitude>" + sqlReader["longitude"] + "</longitude>" +
                              "<configuration>" + sqlReader["configuration"] + "</configuration>" +
                              "<Ant_technology>" + sqlReader["Ant_technology"] + "</Ant_technology>" +
                              "<Ant_model>" + sqlReader["Ant_model"] + "</Ant_model>" +
                              "<Ant_marketing_name>" + sqlReader["Ant_marketing_name"] + "</Ant_marketing_name>" +
                             "</siteRecord>";


                     }*/
                    //json format

                    //  SiteConfigutation configuration


                    while (sqlReader.Read())
                    {
                        SiteConfigutation configuration = new SiteConfigutation();
                        configuration.Site_id = sqlReader["id"].ToString();
                        configuration.Site_Name = sqlReader["Site_Name"].ToString();
                        configuration.latitude = sqlReader["latitude"].ToString();
                        configuration.longitude = sqlReader["longitude"].ToString();
                        // configuration.configuration = sqlReader["configuration"].ToString();
                        // configuration.Ant_technology = sqlReader["Ant_technology"].ToString();
                        configuration.Ant_model = sqlReader["Ant_model"].ToString();
                        configuration.Ant_port = sqlReader["Ant_port"].ToString();
                        configuration.Ant_fullname = sqlReader["Ant_fullname"].ToString();

                        DateTime oDate = DateTime.Parse(sqlReader["SV_datetime"].ToString());

                        configuration.Datetime = oDate.ToString("yyyy-MM-dd");
                        configuration.SV_IdOperation = sqlReader["SV_IdOperation"].ToString();
                        configuration.SV_userid = sqlReader["SV_userid"].ToString();

                        listsiteonfigutation.Add(configuration);
                    }

                }


                JavaScriptSerializer js = new JavaScriptSerializer();
                Context.Response.Write(js.Serialize(listsiteonfigutation));



                connection.Close();
                cmd.Dispose();


            }
            catch (Exception e)
            {
                //Context.Response.Write("Error:" + e.Message);
                xmlFile = "Error:" + e.Message;
                Context.Response.Clear();
                // Context.Response.ContentType = "text/xml";
                Context.Response.ContentEncoding = Encoding.UTF8;
                Context.Response.Write(xmlFile);

            }


            // Context.Response.Clear();
            //Context.Response.ContentType = "text/xml";
            //Context.Response.ContentEncoding = Encoding.UTF8;
            //Context.Response.Write("<tbl_SiteName>");
            //Context.Response.Write(xmlFile);
            //Context.Response.Write("</tbl_SiteName>"); 
        }

        [WebMethod]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Xml)]
        public void InsertSitevisit()
        {

            try
            {

                string SV_IdOperation = (Context.Request["SV_IdOperation"] != null) ? Context.Request["SV_IdOperation"] : "";
                string SV_SiteName = (Context.Request["SV_SiteName"] != null) ? Context.Request["SV_SiteName"] : "";
                string SV_SiteConfig = (Context.Request["SV_SiteConfig"] != null) ? Context.Request["SV_SiteConfig"] : "";
                string SV_DateTime = (Context.Request["SV_DateTime"] != null) ? Context.Request["SV_DateTime"] : "";
                string SV_Coordinates = (Context.Request["SV_Coordinates"] != null) ? Context.Request["SV_Coordinates"] : "";
                string SV_AuditedBy = (Context.Request["SV_AuditedBy"] != null) ? Context.Request["SV_AuditedBy"] : "";
                string SV_Rigger = (Context.Request["SV_Rigger"] != null) ? Context.Request["SV_Rigger"] : "";

                string SV_Owner = (Context.Request["SV_Owner"] != null) ? Context.Request["SV_Owner"] : "";
                string SV_Vendor = (Context.Request["SV_Vendor"] != null) ? Context.Request["SV_Vendor"] : "";
                string SV_Vendor_2G = (Context.Request["SV_Vendor_2G"] != null) ? Context.Request["SV_Vendor_2G"] : "";
                string SV_Vendor_3G = (Context.Request["SV_Vendor_3G"] != null) ? Context.Request["SV_Vendor_3G"] : "";
                string SV_Vendor_4G = (Context.Request["SV_Vendor_4G"] != null) ? Context.Request["SV_Vendor_4G"] : "";

                string SV_RooftopTower = (Context.Request["SV_RooftopTower"] != null) ? Context.Request["SV_RooftopTower"] : "";
                string SV_BuildingHweight = (Context.Request["SV_BuildingHweight"] != null) ? Context.Request["SV_BuildingHweight"] : "";
                string SV_TotNumAntennas = (Context.Request["SV_TotNumAntennas"] != null) ? Context.Request["SV_TotNumAntennas"] : "";

                string SV_Tech = (Context.Request["SV_Tech"] != null) ? Context.Request["SV_Tech"] : "";
                string SV_Sector = (Context.Request["SV_Sector"] != null) ? Context.Request["SV_Sector"] : "";
                string SV_Description = (Context.Request["SV_Description"] != null) ? Context.Request["SV_Description"] : "";
                string SV_Pictures = (Context.Request["SV_Pictures"] != null) ? Context.Request["SV_Pictures"] : "";
                string SV_Plan = (Context.Request["SV_Plan"] != null) ? Context.Request["SV_Plan"] : "";
                string SV_Actual = (Context.Request["SV_Actual"] != null) ? Context.Request["SV_Actual"] : "";
                string SV_AntennaModel = (Context.Request["SV_AntennaModel"] != null) ? Context.Request["SV_AntennaModel"] : "";
                string SV_Remarks = (Context.Request["SV_Remarks"] != null) ? Context.Request["SV_Remarks"] : "";
                string SV_Device = (Context.Request["SV_Device"] != null) ? Context.Request["SV_Device"] : "";
                string SV_Status = (Context.Request["SV_Status"] != null) ? Context.Request["SV_Status"] : "";

                string SV_userid = (Context.Request["SV_userid"] != null) ? Context.Request["SV_userid"] : "";


                string SV_Latitude_GPS = (Context.Request["SV_Latitude_GPS"] != null) ? Context.Request["SV_Latitude_GPS"] : "";
                string SV_Longitude_GPS = (Context.Request["SV_Longitude_GPS"] != null) ? Context.Request["SV_Longitude_GPS"] : "";

                string SV_Antenne = (Context.Request["SV_Antenne"] != null) ? Context.Request["SV_Antenne"] : "";
                string SV_Ports = (Context.Request["SV_Ports"] != null) ? Context.Request["SV_Ports"] : "";

                string SV_Flag = (Context.Request["SV_Flag"] != null) ? Context.Request["SV_Flag"] : "0";
                string SV_Comment = "";

                switch (SV_Status)
                {
                    case "100":
                        SV_Description = "Site details";
                        SV_Actual = SV_BuildingHweight;
                        break;
                    case "300":
                        SV_Actual = SV_Description;
                        break;
                    case "500":
                        //finish
                        SV_Comment = SV_Description;
                        break;
                }


                DateTime dte;
                string strPersonalPhotoName;
                string resInsert = "";
                string strPathFolder;

                dte = DateTime.Now.AddDays(0);
                //.ToString("dd-MM-yyyy HH:mm:ss");
                string IndxDate = dte.ToString("ddMMyyyyHHmmss");
                strPathFolder = strPathPicture + dte.ToString("yyyyMMdd") + "/";

                System.IO.DirectoryInfo diDestination = new System.IO.DirectoryInfo(strPathFolder);

                if (!diDestination.Exists) diDestination.Create();

                //check if champs ok
                if (SV_IdOperation.Equals("") || SV_SiteName.Equals(""))
                {
                    Context.Response.Write("Error occur: failed!");
                    return;
                }
                else if (!SV_Comment.Equals("") && SV_Status.Equals("500"))
                {
                    resInsert = InsertSiteDetails_comment(
                          SV_IdOperation
                          , SV_SiteName
                          , SV_Description
                          , SV_Device
                          , SV_Status
                          , SV_userid);


                    if (!resInsert.Equals(""))
                    {
                        if (resInsert.IndexOf("Error") >= 0)
                        {
                            Context.Response.Write(resInsert.ToString());
                        }
                        else
                        {
                            Context.Response.Write("uploaded :" + resInsert.ToString());
                        }

                    }

                    return;

                }
                else
                {
                    strPersonalPhotoName = strPathFolder + SV_SiteName + "_" + SV_Status + "_" + IndxDate + ".JPEG";

                    if (!SV_Pictures.Equals(""))
                    {

                        if ((strStringToImage(strPersonalPhotoName, SV_Pictures)).Equals("created"))
                        {
                            //insert call
                            resInsert = InsertSiteDetails(
                            SV_IdOperation
                           , SV_SiteName
                           , SV_SiteConfig
                           , SV_DateTime
                           , SV_Coordinates
                           , SV_AuditedBy
                           , SV_Rigger
                           , SV_Owner
                           , SV_Vendor
                           , SV_Vendor_2G
                           , SV_Vendor_3G
                           , SV_Vendor_4G
                           , SV_RooftopTower
                           , SV_BuildingHweight
                           , SV_TotNumAntennas
                           , SV_Tech
                           , SV_Sector
                           , SV_Description
                           , SV_Pictures
                           , strPersonalPhotoName
                           , SV_Plan
                           , SV_Actual
                           , SV_AntennaModel
                           , SV_Remarks
                           , SV_Device
                           , SV_Status
                           , SV_Latitude_GPS
                           , SV_Longitude_GPS
                           , SV_userid
                           , SV_Antenne
                           , SV_Ports
                           , SV_Flag
                           );

                        }

                        if (!resInsert.Equals(""))
                        {

                            if (resInsert.IndexOf("Error") >= 0)
                            {
                                Context.Response.Write(resInsert.ToString());
                            }
                            else
                            {
                                Context.Response.Write("uploaded :" + resInsert.ToString());
                            }

                        }

                    }

                }

                //Context.Response.ContentType = "text/xml";



            }
            catch (Exception e)
            {
                //ecrire dans log
                Context.Response.Write(e.Message);

            }

            //JavaScriptSerializer js = new JavaScriptSerializer();
            //Context.Response.Write(js.Serialize( ));
        }

        [WebMethod]
        public void getDetails(string sitename, string idoperation)
        {
            string xmlFile = "";
            connection = new SqlConnection(strconnection);
            List<siteDetails> listdetail = new List<siteDetails>();

            try
            {
                connection.Open();
                string Sqlcmd = " select  * from  [dbo].[tbl_SiteDetails] WHERE SV_SiteName = '" + sitename + "' ";//and SV_IdOperation='"+ idoperation + "' ORDER BY SV_SiteName";

                // string Sqlcmd = "SELECT distinct site_name FROM survey.dbo.tbl_site WHERE  site_name like '%" + sitename + "%' ORDER BY site_name";
                cmd = new SqlCommand(Sqlcmd, connection);
                SqlDataReader sqlReader = cmd.ExecuteReader();


                if (sqlReader.HasRows)
                {
                    while (sqlReader.Read())
                    {
                        siteDetails details = new siteDetails();
                        details.sitename = sqlReader["SV_SiteName"].ToString();
                        details.owner = sqlReader["SV_Owner"].ToString();
                        details.vendor_2g = sqlReader["SV_Vendor_2G"].ToString();
                        details.vendor_3g = sqlReader["SV_Vendor_3G"].ToString();
                        details.vendor_4g = sqlReader["SV_Vendor_4G"].ToString();
                        details.latitude_gps = sqlReader["SV_Coordinates"].ToString();
                        details.longitude_gps = sqlReader["SV_Coordinates"].ToString();
                        details.towerbuildHeight = sqlReader["SV_BuildingHweight"].ToString();
                        details.tower_pictures = sqlReader["SV_Pictures"].ToString();

                        listdetail.Add(details);
                    }


                }


                JavaScriptSerializer js = new JavaScriptSerializer();
                Context.Response.Write(js.Serialize(listdetail));

                connection.Close();
                cmd.Dispose();


            }
            catch (Exception e)
            {
                //Context.Response.Write("Error:" + e.Message);
                xmlFile = "Error:" + e.Message;
                Context.Response.Clear();
                // Context.Response.ContentType = "text/xml";
                Context.Response.ContentEncoding = Encoding.UTF8;
                Context.Response.Write(xmlFile);

            }


        }

        [WebMethod]
        public void getMenuArray(string sitename)
        {
            string xmlFile = "";
            connection = new SqlConnection(strconnection);
            List<SiteMenu> listmenu = new List<SiteMenu>();


            try
            {
                connection.Open();
                string Sqlcmd = "SELECT * FROM survey.dbo.tbl_menu";
                cmd = new SqlCommand(Sqlcmd, connection);
                SqlDataReader sqlReader = cmd.ExecuteReader();

                if (sqlReader.HasRows)
                {

                    while (sqlReader.Read())
                    {
                        SiteMenu fmnu = new SiteMenu();

                        fmnu.idmenu = sqlReader["idmenu"].ToString();
                        fmnu.fmnu = sqlReader["fmnu"].ToString();
                        fmnu.label = sqlReader["label"].ToString();
                        fmnu.objects = sqlReader["objects"].ToString();
                        fmnu.parent = sqlReader["parent"].ToString();

                        listmenu.Add(fmnu);
                    }

                }


                JavaScriptSerializer js = new JavaScriptSerializer();
                Context.Response.Write(js.Serialize(listmenu));

                connection.Close();
                cmd.Dispose();


            }
            catch (Exception e)
            {
                //Context.Response.Write("Error:" + e.Message);
                xmlFile = "error:" + e.Message;
                Context.Response.Clear();
                // Context.Response.ContentType = "text/xml";
                Context.Response.ContentEncoding = Encoding.UTF8;
                Context.Response.Write(xmlFile);

            }

        }

        [WebMethod]
        public void setSiteVisit()
        {
            string st = "kin001:20313122832;kin185:20313123221";
            getSiteVisit(st);


        }


        [WebMethod]
        public void getSiteVisit(string strlistofSite)
        {
            try
            {

                string[] listofsite = strlistofSite.Split(';');
                string Sqlcmd = "";


                int numberOfStrings = listofsite.Length;

                for (int i = 0; i < numberOfStrings; i++)
                {
                    char[] delimiterChars = { ':', '\t' };
                    string vlst = listofsite[i].ToString();
                    string[] tab = vlst.Split(delimiterChars);

                    string sitename = "";
                    string idoperation = "";


                    if (tab.Length > 0)
                    {
                        sitename = tab[0];
                        idoperation = tab[1];

                    }

                    if (i == 0)
                    {
                        Sqlcmd += " SELECT * from [dbo].[tbl_SiteVisit] WHERE SV_SiteName = '" + sitename + "' and SV_IdOperation = '" + idoperation + "'";

                    }
                    else
                    {
                        Sqlcmd += " UNION SELECT * from [dbo].[tbl_SiteVisit] WHERE SV_SiteName = '" + sitename + "' and SV_IdOperation = '" + idoperation + "'";

                    }
                    // Context.Response.Write(listSite[i]);

                }
                Sqlcmd = " select * from ( " + Sqlcmd + " ) visit order by SV_DateTime asc ";

                connection = new SqlConnection(strconnection);
                List<siteVisit> listSitevisit = new List<siteVisit>();


                connection.Open();

                cmd = new SqlCommand(Sqlcmd, connection);
                SqlDataReader sqlReader = cmd.ExecuteReader();

                if (sqlReader.HasRows)
                {
                    while (sqlReader.Read())
                    {
                        siteVisit details = new siteVisit();
                        details.SV_IdOperation = sqlReader["SV_IdOperation"].ToString();
                        details.SV_SiteName = sqlReader["SV_SiteName"].ToString();
                        details.SV_SiteConfig = sqlReader["SV_SiteConfig"].ToString();
                        details.SV_DateTime = sqlReader["SV_DateTime"].ToString();
                        //details.SV_Coordinates = sqlReader["SV_Coordinates"].ToString();
                        // details.SV_AuditedBy = sqlReader["SV_AuditedBy"].ToString();
                        //details.SV_Rigger = sqlReader["SV_Rigger"].ToString();
                        details.SV_Owner = sqlReader["SV_Owner"].ToString();
                        // details.SV_Vendor = sqlReader["SV_Vendor"].ToString();
                        details.SV_Vendor_2G = sqlReader["SV_Vendor_2G"].ToString();
                        details.SV_Vendor_3G = sqlReader["SV_Vendor_3G"].ToString();
                        details.SV_Vendor_4G = sqlReader["SV_Vendor_4G"].ToString();
                        // details.SV_RooftopTower = sqlReader["SV_RooftopTower"].ToString();
                        // details.SV_BuildingHweight = sqlReader["SV_BuildingHweight"].ToString();
                        // details.SV_TotNumAntennas = sqlReader["SV_TotNumAntennas"].ToString();
                        details.SV_Tech = sqlReader["SV_Tech"].ToString();
                        //details.SV_Sector = sqlReader["SV_Sector"].ToString();
                        details.SV_Description = sqlReader["SV_Description"].ToString();
                        details.SV_Pictures = sqlReader["SV_Pictures"].ToString();
                        //details.SV_Plan = sqlReader["SV_Plan"].ToString();
                        details.SV_Actual = sqlReader["SV_Actual"].ToString();
                        //details.SV_AntennaModel = sqlReader["SV_AntennaModel"].ToString();
                        details.SV_Remarks = sqlReader["SV_Remarks"].ToString();
                        details.SV_Device = sqlReader["SV_Device"].ToString();
                        details.SV_Status = sqlReader["SV_Status"].ToString();
                        details.SV_Latitude_GPS = sqlReader["SV_Latitude"].ToString();
                        details.SV_Longitude_GPS = sqlReader["SV_Longitude"].ToString();
                        details.SV_Antenne = sqlReader["SV_Antenne"].ToString();
                        details.SV_Ports = sqlReader["SV_Ports"].ToString();

                        listSitevisit.Add(details);
                    }


                }


                JavaScriptSerializer js = new JavaScriptSerializer();
                js.MaxJsonLength = Int32.MaxValue;
                int JsonLength = js.Serialize(listSitevisit).Length;

                Context.Response.Write(js.Serialize(listSitevisit));

                connection.Close();
                cmd.Dispose();



            }
            catch (Exception e)
            {
                if (connection.State.Equals("Open"))
                {
                    connection.Close();
                }

                //Context.Response.Write("Error:" + e.Message);
                String xmlFile = "Error:" + e.Message;
                Context.Response.Clear();
                Context.Response.ContentType = "text/xml";
                Context.Response.ContentEncoding = Encoding.UTF8;
                Context.Response.Write(xmlFile);

            }


        }
        [WebMethod]
        public void getAntenne()
        {
            try
            {

                string Sqlcmd = "";
                Sqlcmd = " select distinct Ant_model from tbl_Antenne order by Ant_model asc; ";

                connection = new SqlConnection(strconnection);
                List<setAntenne> listAntenne = new List<setAntenne>();

                connection.Open();

                cmd = new SqlCommand(Sqlcmd, connection);
                SqlDataReader sqlReader = cmd.ExecuteReader();

                if (sqlReader.HasRows)
                {
                    while (sqlReader.Read())
                    {
                        setAntenne details = new setAntenne();
                        details.antenne = sqlReader["Ant_model"].ToString();
                        // details.technology = sqlReader["Ant_technology"].ToString();
                        listAntenne.Add(details);
                    }


                }


                JavaScriptSerializer js = new JavaScriptSerializer();
                js.MaxJsonLength = Int32.MaxValue;
                int JsonLength = js.Serialize(listAntenne).Length;

                Context.Response.Write(js.Serialize(listAntenne));

                connection.Close();
                cmd.Dispose();



            }
            catch (Exception e)
            {
                if (connection.State.Equals("Open"))
                {
                    connection.Close();
                }

                //Context.Response.Write("Error:" + e.Message);
                String xmlFile = "Error:" + e.Message;
                Context.Response.Clear();
                Context.Response.ContentType = "text/xml";
                Context.Response.ContentEncoding = Encoding.UTF8;
                Context.Response.Write(xmlFile);

            }


        }
        [WebMethod]
        public void addNewsiteId()
        {

            string id = (Context.Request["SV_Id"] != null) ? Context.Request["SV_Id"] : "0";
            string Ant_model = (Context.Request["SV_AntennaModel"] != null) ? Context.Request["SV_AntennaModel"] : "";
            string Site_Name = (Context.Request["SV_Sitename"] != null) ? Context.Request["SV_Sitename"] : "";
            string id_ant = (Context.Request["SV_idAnt"] != null) ? Context.Request["SV_idAnt"] : "0";
            string Ant_port = (Context.Request["SV_Port"] != null) ? Context.Request["SV_Port"] : "";
            string imei = (Context.Request["SV_Device"] != null) ? Context.Request["SV_Device"] : "";
            string users = (Context.Request["SV_userid"] != null) ? Context.Request["SV_userid"] : "";
            string AntfullName = (Context.Request["SV_AntfullName"] != null) ? Context.Request["SV_AntfullName"] : "";
            string nodedesc = (Context.Request["SV_Nodedesc"] != null) ? Context.Request["SV_Nodedesc"] : "";
            string Return_Value = "";

            String strFonction = null;
            try
            {
                //nodedesc="new or add site"
                string Sqlcmd = "";

                if (nodedesc.Trim().Equals(""))
                {
                    //new site
                    Sqlcmd = " DECLARE @return_value int  " +
                              " EXEC  @return_value = [dbo].[sp_Insert_SiteAntenne] " +
                              " @Site_Name = N'" + Site_Name + "'" +
                              ",@Ant_model = N'" + Ant_model + "'" +
                              ",@Ant_port  = N'" + Ant_port + "'" +

                              " SELECT 'Return_Value' = @return_value";
                }
                else
                {

                    if (nodedesc.Trim().ToLower().Equals("add"))
                    {
                        strFonction = "sp_add_newSite";
                    }
                    else if (nodedesc.Trim().ToLower().Equals("edit"))
                    {
                        strFonction = "sp_editAntenna";
                    }


                    if (id.Equals("0") || strFonction.Equals(null))
                    {
                        //Context.Response.Write("Error:" + e.Message);
                        String xmlFile = "error:id not found!!!";
                        Context.Response.Clear();

                        Context.Response.ContentEncoding = Encoding.UTF8;
                        Context.Response.Write(xmlFile);

                        return;
                    }

                    Sqlcmd = " DECLARE @return_value int  " +
                         " EXEC  @return_value = [dbo].[" + strFonction + "] " +
                         " @id =" + id + "" +
                         ",@Ant_model = N'" + Ant_model + "'" +
                         ",@Site_Name = N'" + Site_Name + "'" +
                         ",@id_ant = " + id_ant + "" +
                         ",@Ant_port = " + Ant_port + " " +
                         ",@imei = N'" + imei + "'" +
                         ",@users = N'" + users + "'" +
                         ",@fullName = N'" + AntfullName + "'" +
                         " SELECT 'Return_Value' = @return_value";

                }



                connection = new SqlConnection(strconnection);
                connection.Open();

                cmd = new SqlCommand(Sqlcmd, connection);
                SqlDataReader sqlReader = cmd.ExecuteReader();
                if (sqlReader.HasRows)
                {
                    while (sqlReader.Read())
                    {
                        Return_Value = sqlReader["Return_Value"].ToString();
                    }
                }


                if (Return_Value.Equals("0"))
                {
                    String xmlFile = Return_Value;
                    Context.Response.Clear();

                    //Context.Response.ContentEncoding = Encoding.UTF8;
                    //Context.Response.Write(xmlFile);

                    getSitenameconfiguration(Site_Name);
                }



            }
            catch (Exception e)
            {
                if (connection.State.Equals("Open"))
                {
                    connection.Close();
                }

                //Context.Response.Write("Error:" + e.Message);
                String xmlFile = "error:" + e.Message;
                Context.Response.Clear();

                Context.Response.ContentEncoding = Encoding.UTF8;
                Context.Response.Write(xmlFile);
            }
        }



        public string userId;


    }





    public class SiteMenu
    {
        public String idmenu { get; set; }
        public String fmnu { get; set; }
        public String label { get; set; }
        public String objects { get; set; }
        public String parent { get; set; }



    }
    public class SiteConfigutation
    {
        public String Site_id { get; set; }
        public String Site_Name { get; set; }
        public String latitude { get; set; }
        public String longitude { get; set; }
        public String configuration { get; set; }
        public String Ant_technology { get; set; }
        public String Ant_model { get; set; }
        public String Ant_port { get; set; }
        public String Ant_fullname { get; set; }
        public String Datetime { get; set; }
        public String SV_IdOperation { get; set; }
        public String SV_userid { get; set; }

    }
    public class siteDetails
    {
        public String sitename { get; set; }
        public String owner { get; set; }
        public String vendor_2g { get; set; }
        public String vendor_3g { get; set; }
        public String vendor_4g { get; set; }
        public String latitude_gps { get; set; }
        public String longitude_gps { get; set; }
        public String towerbuildHeight { get; set; }
        public String tower_pictures { get; set; }


    }
    public class siteVisit
    {
        public String SV_IdOperation { get; set; }
        public String SV_SiteName { get; set; }
        public String SV_SiteConfig { get; set; }
        public String SV_DateTime { get; set; }
        // public String SV_Coordinates { get; set; }
        //public String SV_AuditedBy { get; set; }
        //public String SV_Rigger { get; set; }
        public String SV_Owner { get; set; }
        //public String SV_Vendor { get; set; }
        public String SV_Vendor_2G { get; set; }
        public String SV_Vendor_3G { get; set; }
        public String SV_Vendor_4G { get; set; }
        //public String SV_RooftopTower { get; set; }
        //public String SV_BuildingHweight { get; set; }
        //public String SV_TotNumAntennas { get; set; }
        public String SV_Tech { get; set; }
        //public String SV_Sector { get; set; }
        public String SV_Description { get; set; }
        public String SV_Pictures { get; set; }
        //public String strPersonalPhotoName { get; set; }
        //public String SV_Plan { get; set; }
        public String SV_Actual { get; set; }
        //public String SV_AntennaModel { get; set; }
        public String SV_Remarks { get; set; }
        public String SV_Device { get; set; }
        public String SV_Status { get; set; }
        public String SV_Latitude_GPS { get; set; }
        public String SV_Longitude_GPS { get; set; }
        public String SV_Antenne { get; set; }
        public String SV_Ports { get; set; }


    }
    public class setAntenne
    {
        public String antenne { get; set; }
        public String technology { get; set; }

    }

}

