using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using System.Reflection;
using LightRiver.Net;

namespace LightRiver.ServiceModel
{
    public class BaseHttpService<TResult, TParameter, TParser>
        where TParameter : HttpServiceParameter
        where TParser : BaseParser<TResult, string>, new()
    {
        private string _apiUrl = null;

        public BaseHttpService(string apiUrl)
        {
            _apiUrl = apiUrl;
        }

        public virtual async Task<ParseResult<TResult>> InvokeAsync(TParameter parameter, int timeout = 60000)
        {
            // 1. 取得request字串
            var requestParameterString = PackParameterToSend(parameter);

            // 2. 發送request
            HttpClient httpClient = new HttpClient();
            string responseString = null;
            var method = (parameter == null ? HttpMethod.Get : parameter.Method);
            ParseError error = null;

            try {
                if (method == HttpMethod.Get) {
                    responseString = await GetAsync(httpClient, requestParameterString);
                }
                else if (method == HttpMethod.Post) {
                    responseString = await PostAsync(httpClient, requestParameterString);
                }
            }
            catch (HttpRequestException ex) {
                error = new ParseError(ex.GetHashCode(), ex.Message);
                return new ParseResult<TResult>(error);
            }
            catch (TaskCanceledException ex) {
                error = new ParseError(ex.GetHashCode(), ex.Message);
                return new ParseResult<TResult>(error);
            }
            catch (WebException ex) {
                error = new ParseError(ex.GetHashCode(), ex.Message);
                return new ParseResult<TResult>(error);
            }
            finally {
                httpClient.Dispose();
            }

            // 3. 解析結果
            var response = Parse(responseString);
            return response;
        }

        protected virtual string PackParameterToSend(TParameter parameter)
        {
            if (parameter == null)
                return null;
#if !NET45
            var propertyArray = parameter.GetType().GetProperties();
#else
            var propertyArray = parameter.GetType().GetRuntimeProperties();
#endif
            string parameterString = string.Empty;

            foreach (var property in propertyArray) {
                var ignoreAttribute = property.GetCustomAttribute<ServiceParameterIgnoreAttribute>();
                if (ignoreAttribute != null)
                    continue;

                var attribute = property.GetCustomAttribute<ServicePrameterPropertyAttribute>();
                string propertyName;
                string propertyValue;
                if (attribute == null) {
                    propertyName = property.Name;
                    propertyValue = string.Format("{0}", property.GetValue(parameter, null));
                }
                else {
                    propertyName = attribute.Name;

                    if (attribute.ConveterType == null) {
                        propertyValue = string.Format("{0}", property.GetValue(parameter, null));
                    }
                    else {
                        var converter = Activator.CreateInstance(attribute.ConveterType) as IServiceParameterConveter;
                        Debug.Assert(converter != null);
                        propertyValue = converter.Convert(property.GetValue(parameter, null));
                    }
                }

                parameterString += string.Format("{0}={1}&", propertyName, Uri.EscapeDataString(propertyValue));
            }

            // remove last & char
            parameterString = parameterString.Remove(parameterString.Length - 1);

            return parameterString;
        }

        protected virtual async Task<string> GetAsync(HttpClient httpClient, string requestParameterString)
        {
            string requestUrl = (string.IsNullOrEmpty(requestParameterString) ? _apiUrl : string.Format("{0}?{1}", _apiUrl, requestParameterString));
            var responseString = await httpClient.GetStringAsync(requestUrl);
            return responseString;
        }

        protected virtual async Task<string> PostAsync(HttpClient httpClient, string requestParameterString)
        {
            var requestParameterBytes = Encoding.UTF8.GetBytes(requestParameterString);
            var streamContent = new StreamContent(new MemoryStream(requestParameterBytes));
            var responseMessage = await httpClient.PostAsync(_apiUrl, streamContent);
            var responseString = await responseMessage.Content.ReadAsStringAsync();
            streamContent.Dispose();
            responseMessage.Dispose();
            return responseString;
        }

        protected virtual ParseResult<TResult> Parse(string source)
        {
            var parser = new TParser();
            return parser.Parse(source);
        }
    }
}
