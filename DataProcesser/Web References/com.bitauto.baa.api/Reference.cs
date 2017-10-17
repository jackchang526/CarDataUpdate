﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.17929
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

// 
// 此源代码是由 Microsoft.VSDesigner 4.0.30319.17929 版自动生成。
// 
#pragma warning disable 1591

namespace BitAuto.CarDataUpdate.DataProcesser.com.bitauto.baa.api {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="userCarServiceSoap", Namespace="http://tempuri.org/")]
    public partial class userCarService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private ApiSoapHeader apiSoapHeaderValueField;
        
        private System.Threading.SendOrPostCallback GetUserCarTopicListOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public userCarService() {
            this.Url = global::BitAuto.CarDataUpdate.DataProcesser.Properties.Settings.Default.DataProcesser_com_bitauto_baa_api_userCarService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public ApiSoapHeader ApiSoapHeaderValue {
            get {
                return this.apiSoapHeaderValueField;
            }
            set {
                this.apiSoapHeaderValueField = value;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event GetUserCarTopicListCompletedEventHandler GetUserCarTopicListCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("ApiSoapHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetUserCarTopicList", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetUserCarTopicList(int serialId, int carId, string categoryPinYin, string wordName, int pageIndex, int pageSize, out int recordCount) {
            object[] results = this.Invoke("GetUserCarTopicList", new object[] {
                        serialId,
                        carId,
                        categoryPinYin,
                        wordName,
                        pageIndex,
                        pageSize});
            recordCount = ((int)(results[1]));
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetUserCarTopicListAsync(int serialId, int carId, string categoryPinYin, string wordName, int pageIndex, int pageSize) {
            this.GetUserCarTopicListAsync(serialId, carId, categoryPinYin, wordName, pageIndex, pageSize, null);
        }
        
        /// <remarks/>
        public void GetUserCarTopicListAsync(int serialId, int carId, string categoryPinYin, string wordName, int pageIndex, int pageSize, object userState) {
            if ((this.GetUserCarTopicListOperationCompleted == null)) {
                this.GetUserCarTopicListOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetUserCarTopicListOperationCompleted);
            }
            this.InvokeAsync("GetUserCarTopicList", new object[] {
                        serialId,
                        carId,
                        categoryPinYin,
                        wordName,
                        pageIndex,
                        pageSize}, this.GetUserCarTopicListOperationCompleted, userState);
        }
        
        private void OnGetUserCarTopicListOperationCompleted(object arg) {
            if ((this.GetUserCarTopicListCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetUserCarTopicListCompleted(this, new GetUserCarTopicListCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.17929")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/", IsNullable=false)]
    public partial class ApiSoapHeader : System.Web.Services.Protocols.SoapHeader {
        
        private string appKeyField;
        
        private string appPwdField;
        
        private System.Xml.XmlAttribute[] anyAttrField;
        
        /// <remarks/>
        public string AppKey {
            get {
                return this.appKeyField;
            }
            set {
                this.appKeyField = value;
            }
        }
        
        /// <remarks/>
        public string AppPwd {
            get {
                return this.appPwdField;
            }
            set {
                this.appPwdField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAnyAttributeAttribute()]
        public System.Xml.XmlAttribute[] AnyAttr {
            get {
                return this.anyAttrField;
            }
            set {
                this.anyAttrField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    public delegate void GetUserCarTopicListCompletedEventHandler(object sender, GetUserCarTopicListCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.0.30319.17929")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetUserCarTopicListCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetUserCarTopicListCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
        
        /// <remarks/>
        public int recordCount {
            get {
                this.RaiseExceptionIfNecessary();
                return ((int)(this.results[1]));
            }
        }
    }
}

#pragma warning restore 1591