/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace AutomationTool.Web.API
{
    public static class API
    {
        /// <summary>
        /// Gets data using WebAPI URL.
        /// </summary>
        /// <param name="StateAbbr">State Abbreviation</param>
        /// <param name="WebAPIUrl">WebAPI Url</param>
        /// <param name="URI">Web API initial url</param>
        /// <returns>Table of data from database using WebAPI</returns>
        public static Tuple<System.Net.HttpStatusCode, List<T>> GetData<T>(this string APIUrl, string URI)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpClientHandler handler = new HttpClientHandler() { PreAuthenticate = true, UseDefaultCredentials = true };
                using (var client = new HttpClient(handler))
                {
                    // client.Timeout = TimeSpan(300000);
                    client.BaseAddress = new Uri(URI);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.GetAsync(APIUrl).Result;

                    bool isReadyToGetInfo = false;
                    switch (response.StatusCode)
                    {
                        case System.Net.HttpStatusCode.Accepted:
                        case System.Net.HttpStatusCode.OK:
                            isReadyToGetInfo = true;
                            break;
                        case System.Net.HttpStatusCode.Ambiguous:
                        case System.Net.HttpStatusCode.BadGateway:
                        case System.Net.HttpStatusCode.BadRequest:
                        case System.Net.HttpStatusCode.Conflict:
                        case System.Net.HttpStatusCode.Continue:
                        case System.Net.HttpStatusCode.Created:
                        case System.Net.HttpStatusCode.ExpectationFailed:
                        case System.Net.HttpStatusCode.Forbidden:
                        case System.Net.HttpStatusCode.Found:
                        case System.Net.HttpStatusCode.GatewayTimeout:
                        case System.Net.HttpStatusCode.Gone:
                        case System.Net.HttpStatusCode.HttpVersionNotSupported:
                        case System.Net.HttpStatusCode.InternalServerError:
                        case System.Net.HttpStatusCode.LengthRequired:
                        case System.Net.HttpStatusCode.MethodNotAllowed:
                        case System.Net.HttpStatusCode.Moved:
                        case System.Net.HttpStatusCode.NoContent:
                        case System.Net.HttpStatusCode.NonAuthoritativeInformation:
                        case System.Net.HttpStatusCode.NotAcceptable:
                        case System.Net.HttpStatusCode.NotFound:
                        case System.Net.HttpStatusCode.NotImplemented:
                        case System.Net.HttpStatusCode.NotModified:
                        case System.Net.HttpStatusCode.PartialContent:
                        case System.Net.HttpStatusCode.PaymentRequired:
                        case System.Net.HttpStatusCode.PreconditionFailed:
                        case System.Net.HttpStatusCode.ProxyAuthenticationRequired:
                        case System.Net.HttpStatusCode.RedirectKeepVerb:
                        case System.Net.HttpStatusCode.RedirectMethod:
                        case System.Net.HttpStatusCode.RequestEntityTooLarge:
                        case System.Net.HttpStatusCode.RequestTimeout:
                        case System.Net.HttpStatusCode.RequestUriTooLong:
                        case System.Net.HttpStatusCode.RequestedRangeNotSatisfiable:
                        case System.Net.HttpStatusCode.ResetContent:
                        case System.Net.HttpStatusCode.ServiceUnavailable:
                        case System.Net.HttpStatusCode.SwitchingProtocols:
                        case System.Net.HttpStatusCode.Unauthorized:
                        case System.Net.HttpStatusCode.UnsupportedMediaType:
                        case System.Net.HttpStatusCode.Unused:
                        case System.Net.HttpStatusCode.UpgradeRequired:
                        case System.Net.HttpStatusCode.UseProxy:
                            isReadyToGetInfo = false;
                            break;
                        default:
                            isReadyToGetInfo = false;
                            break;
                    }

                    if (isReadyToGetInfo)
                    {
                        string data = response.Content.ReadAsStringAsync().Result;
                        Tuple<JArray, JObject> JSONParsedObject = data.GetJSONArray();
                        List<T> records = new List<T>();
                        if (JSONParsedObject != null)
                        {
                            if (JSONParsedObject.Item1 != null)
                                records = JSONParsedObject.Item1.ToObject<List<T>>();
                            else
                                records.Add(JSONParsedObject.Item2.ToObject<T>());

                            return Tuple.Create<System.Net.HttpStatusCode, List<T>>(response.StatusCode, records);
                        }
                        else
                            return Tuple.Create<System.Net.HttpStatusCode, List<T>>(response.StatusCode, null);
                    }
                    else
                        return Tuple.Create<System.Net.HttpStatusCode, List<T>>(response.StatusCode, null);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Gets the JArray
        /// </summary>
        /// <param name="JSONData">JSON String</param>
        /// <returns>Miscellaneous object</returns>
        private static Tuple<JArray, JObject> GetJSONArray(this string JSONData)
        {
            try
            {
                JArray result = JArray.Parse(JSONData);
                if (result.Any())
                {
                    return Tuple.Create<JArray, JObject>(result, null);
                }
                else
                    return Tuple.Create<JArray, JObject>(null, null);
            }
            catch
            {
                return JSONData.GetJSONObject();
            }
        }

        /// <summary>
        /// Gets the JObject
        /// </summary>
        /// <param name="JSONData">JSON String</param>
        /// <returns>Miscellaneous object</returns>
        private static Tuple<JArray, JObject> GetJSONObject(this string JSONData)
        {
            try
            {
                JObject result = JObject.Parse(JSONData);
                if (result != null)
                    return Tuple.Create<JArray, JObject>(null, result);
                else
                    return Tuple.Create<JArray, JObject>(null, null);
            }
            catch
            {
                return Tuple.Create<JArray, JObject>(null, null);
            }
        }

        /// <summary>
        /// Gets data using WebAPI URL.
        /// </summary>
        /// <param name="StateAbbr">State Abbreviation</param>
        /// <param name="WebAPIUrl">WebAPI Url</param>
        /// <param name="URI">Web API initial url</param>
        /// <returns>Table of data from database using WebAPI</returns>
        public static Tuple<System.Net.HttpStatusCode, JArray, JObject> GetData(this string APIUrl, string URI)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpClientHandler handler = new HttpClientHandler() { PreAuthenticate = true, UseDefaultCredentials = true };
                using (var client = new HttpClient(handler))
                {
                    // client.Timeout = TimeSpan(300000);
                    client.BaseAddress = new Uri(URI);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.GetAsync(APIUrl).Result;

                    bool isReadyToGetInfo = false;
                    switch (response.StatusCode)
                    {
                        case System.Net.HttpStatusCode.Accepted:
                        case System.Net.HttpStatusCode.OK:
                            isReadyToGetInfo = true;
                            break;
                        case System.Net.HttpStatusCode.Ambiguous:
                        case System.Net.HttpStatusCode.BadGateway:
                        case System.Net.HttpStatusCode.BadRequest:
                        case System.Net.HttpStatusCode.Conflict:
                        case System.Net.HttpStatusCode.Continue:
                        case System.Net.HttpStatusCode.Created:
                        case System.Net.HttpStatusCode.ExpectationFailed:
                        case System.Net.HttpStatusCode.Forbidden:
                        case System.Net.HttpStatusCode.Found:
                        case System.Net.HttpStatusCode.GatewayTimeout:
                        case System.Net.HttpStatusCode.Gone:
                        case System.Net.HttpStatusCode.HttpVersionNotSupported:
                        case System.Net.HttpStatusCode.InternalServerError:
                        case System.Net.HttpStatusCode.LengthRequired:
                        case System.Net.HttpStatusCode.MethodNotAllowed:
                        case System.Net.HttpStatusCode.Moved:
                        case System.Net.HttpStatusCode.NoContent:
                        case System.Net.HttpStatusCode.NonAuthoritativeInformation:
                        case System.Net.HttpStatusCode.NotAcceptable:
                        case System.Net.HttpStatusCode.NotFound:
                        case System.Net.HttpStatusCode.NotImplemented:
                        case System.Net.HttpStatusCode.NotModified:
                        case System.Net.HttpStatusCode.PartialContent:
                        case System.Net.HttpStatusCode.PaymentRequired:
                        case System.Net.HttpStatusCode.PreconditionFailed:
                        case System.Net.HttpStatusCode.ProxyAuthenticationRequired:
                        case System.Net.HttpStatusCode.RedirectKeepVerb:
                        case System.Net.HttpStatusCode.RedirectMethod:
                        case System.Net.HttpStatusCode.RequestEntityTooLarge:
                        case System.Net.HttpStatusCode.RequestTimeout:
                        case System.Net.HttpStatusCode.RequestUriTooLong:
                        case System.Net.HttpStatusCode.RequestedRangeNotSatisfiable:
                        case System.Net.HttpStatusCode.ResetContent:
                        case System.Net.HttpStatusCode.ServiceUnavailable:
                        case System.Net.HttpStatusCode.SwitchingProtocols:
                        case System.Net.HttpStatusCode.Unauthorized:
                        case System.Net.HttpStatusCode.UnsupportedMediaType:
                        case System.Net.HttpStatusCode.Unused:
                        case System.Net.HttpStatusCode.UpgradeRequired:
                        case System.Net.HttpStatusCode.UseProxy:
                            isReadyToGetInfo = false;
                            break;
                        default:
                            isReadyToGetInfo = false;
                            break;
                    }

                    if (isReadyToGetInfo)
                    {
                        string data = response.Content.ReadAsStringAsync().Result;
                        Tuple<JArray, JObject> JSONParsedObject = data.GetJSONArray();
                        if (JSONParsedObject != null)
                            return Tuple.Create<System.Net.HttpStatusCode, JArray, JObject>(response.StatusCode, JSONParsedObject.Item1, JSONParsedObject.Item2);
                        else
                            return Tuple.Create<System.Net.HttpStatusCode, JArray, JObject>(response.StatusCode, null, null);
                    }
                    else
                        return Tuple.Create<System.Net.HttpStatusCode, JArray, JObject>(response.StatusCode, null, null);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        /// <summary>
        /// Updates the Record.
        /// </summary>
        /// <typeparam name="T">Type Object</typeparam>
        /// <param name="TObject">Object of Type T</param>
        /// <returns>Status code.</returns>
        public static Tuple<System.Net.HttpStatusCode, string> UpdateRecord<T>(this T TObject, string PostUrl, string URI)
        {
            Tuple<System.Net.HttpStatusCode, string> Status = Tuple.Create<System.Net.HttpStatusCode, string>(default, string.Empty);
            try
            {
                HttpClientHandler handler = new HttpClientHandler() { PreAuthenticate = true, UseDefaultCredentials = true };
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri(URI);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsJsonAsync(PostUrl, TObject).Result;
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        // return URI of the created resource.
                        string update = response.Content.ReadAsStringAsync().Result;
                        Status = Tuple.Create<System.Net.HttpStatusCode, string>(response.StatusCode, update);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return Status;
        }

        /// <summary>
        /// Updates the Record.
        /// </summary>
        /// <typeparam name="T">Type Object</typeparam>
        /// <param name="TObject">Object of Type T</param>
        /// <returns>Status code.</returns>
        public static Tuple<System.Net.HttpStatusCode, string> UpdateRecord(this JObject TObject, string PostUrl, string URI)
        {
            Tuple<System.Net.HttpStatusCode, string> Status = Tuple.Create<System.Net.HttpStatusCode, string>(default, string.Empty);
            try
            {
                HttpClientHandler handler = new HttpClientHandler() { PreAuthenticate = true, UseDefaultCredentials = true };
                using (var client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri(URI);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = client.PostAsJsonAsync(PostUrl, TObject).Result;
                    response.EnsureSuccessStatusCode();
                    if (response.IsSuccessStatusCode)
                    {
                        // return URI of the created resource.
                        string update = response.Content.ReadAsStringAsync().Result;
                        Status = Tuple.Create<System.Net.HttpStatusCode, string>(response.StatusCode, update);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return Status;
        }
    }
}
