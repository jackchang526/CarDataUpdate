﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

// 
// 此源代码是由 Microsoft.VSDesigner 4.0.30319.42000 版自动生成。
// 
#pragma warning disable 1591

namespace BitAuto.CarDataUpdate.DataProcesser.com.bitauto.baa.api1 {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="topicServiceSoap", Namespace="http://tempuri.org/")]
    public partial class topicService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private ApiSoapHeader apiSoapHeaderValueField;
        
        private System.Threading.SendOrPostCallback GetTopicListByRatingOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetTopicOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetTopicListByGuidsOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetTopicListOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetGoodTopicListOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetTopicListBySortOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetTopicListBySerialIdAndIssueKeyOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public topicService() {
            this.Url = global::BitAuto.CarDataUpdate.DataProcesser.Properties.Settings.Default.DataProcesser_com_bitauto_baa_api1_topicService;
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
        public event GetTopicListByRatingCompletedEventHandler GetTopicListByRatingCompleted;
        
        /// <remarks/>
        public event GetTopicCompletedEventHandler GetTopicCompleted;
        
        /// <remarks/>
        public event GetTopicListByGuidsCompletedEventHandler GetTopicListByGuidsCompleted;
        
        /// <remarks/>
        public event GetTopicListCompletedEventHandler GetTopicListCompleted;
        
        /// <remarks/>
        public event GetGoodTopicListCompletedEventHandler GetGoodTopicListCompleted;
        
        /// <remarks/>
        public event GetTopicListBySortCompletedEventHandler GetTopicListBySortCompleted;
        
        /// <remarks/>
        public event GetTopicListBySerialIdAndIssueKeyCompletedEventHandler GetTopicListBySerialIdAndIssueKeyCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("ApiSoapHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetTopicListByRating", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetTopicListByRating(int serialId, int rating, int pageIndex, int pageSize, out long recordCount) {
            object[] results = this.Invoke("GetTopicListByRating", new object[] {
                        serialId,
                        rating,
                        pageIndex,
                        pageSize});
            recordCount = ((long)(results[1]));
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetTopicListByRatingAsync(int serialId, int rating, int pageIndex, int pageSize) {
            this.GetTopicListByRatingAsync(serialId, rating, pageIndex, pageSize, null);
        }
        
        /// <remarks/>
        public void GetTopicListByRatingAsync(int serialId, int rating, int pageIndex, int pageSize, object userState) {
            if ((this.GetTopicListByRatingOperationCompleted == null)) {
                this.GetTopicListByRatingOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetTopicListByRatingOperationCompleted);
            }
            this.InvokeAsync("GetTopicListByRating", new object[] {
                        serialId,
                        rating,
                        pageIndex,
                        pageSize}, this.GetTopicListByRatingOperationCompleted, userState);
        }
        
        private void OnGetTopicListByRatingOperationCompleted(object arg) {
            if ((this.GetTopicListByRatingCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetTopicListByRatingCompleted(this, new GetTopicListByRatingCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("ApiSoapHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetTopic", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetTopic(string topicGuid) {
            object[] results = this.Invoke("GetTopic", new object[] {
                        topicGuid});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetTopicAsync(string topicGuid) {
            this.GetTopicAsync(topicGuid, null);
        }
        
        /// <remarks/>
        public void GetTopicAsync(string topicGuid, object userState) {
            if ((this.GetTopicOperationCompleted == null)) {
                this.GetTopicOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetTopicOperationCompleted);
            }
            this.InvokeAsync("GetTopic", new object[] {
                        topicGuid}, this.GetTopicOperationCompleted, userState);
        }
        
        private void OnGetTopicOperationCompleted(object arg) {
            if ((this.GetTopicCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetTopicCompleted(this, new GetTopicCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("ApiSoapHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetTopicListByGuids", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetTopicListByGuids(string[] topicGuids) {
            object[] results = this.Invoke("GetTopicListByGuids", new object[] {
                        topicGuids});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetTopicListByGuidsAsync(string[] topicGuids) {
            this.GetTopicListByGuidsAsync(topicGuids, null);
        }
        
        /// <remarks/>
        public void GetTopicListByGuidsAsync(string[] topicGuids, object userState) {
            if ((this.GetTopicListByGuidsOperationCompleted == null)) {
                this.GetTopicListByGuidsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetTopicListByGuidsOperationCompleted);
            }
            this.InvokeAsync("GetTopicListByGuids", new object[] {
                        topicGuids}, this.GetTopicListByGuidsOperationCompleted, userState);
        }
        
        private void OnGetTopicListByGuidsOperationCompleted(object arg) {
            if ((this.GetTopicListByGuidsCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetTopicListByGuidsCompleted(this, new GetTopicListByGuidsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("ApiSoapHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetTopicList", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetTopicList(int serialId, int carId, int pageIndex, int pageSize, out long recordCount) {
            object[] results = this.Invoke("GetTopicList", new object[] {
                        serialId,
                        carId,
                        pageIndex,
                        pageSize});
            recordCount = ((long)(results[1]));
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetTopicListAsync(int serialId, int carId, int pageIndex, int pageSize) {
            this.GetTopicListAsync(serialId, carId, pageIndex, pageSize, null);
        }
        
        /// <remarks/>
        public void GetTopicListAsync(int serialId, int carId, int pageIndex, int pageSize, object userState) {
            if ((this.GetTopicListOperationCompleted == null)) {
                this.GetTopicListOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetTopicListOperationCompleted);
            }
            this.InvokeAsync("GetTopicList", new object[] {
                        serialId,
                        carId,
                        pageIndex,
                        pageSize}, this.GetTopicListOperationCompleted, userState);
        }
        
        private void OnGetTopicListOperationCompleted(object arg) {
            if ((this.GetTopicListCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetTopicListCompleted(this, new GetTopicListCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("ApiSoapHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetGoodTopicList", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetGoodTopicList(int serialId, int carId, int isGood, int pageIndex, int pageSize, out long recordCount) {
            object[] results = this.Invoke("GetGoodTopicList", new object[] {
                        serialId,
                        carId,
                        isGood,
                        pageIndex,
                        pageSize});
            recordCount = ((long)(results[1]));
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetGoodTopicListAsync(int serialId, int carId, int isGood, int pageIndex, int pageSize) {
            this.GetGoodTopicListAsync(serialId, carId, isGood, pageIndex, pageSize, null);
        }
        
        /// <remarks/>
        public void GetGoodTopicListAsync(int serialId, int carId, int isGood, int pageIndex, int pageSize, object userState) {
            if ((this.GetGoodTopicListOperationCompleted == null)) {
                this.GetGoodTopicListOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetGoodTopicListOperationCompleted);
            }
            this.InvokeAsync("GetGoodTopicList", new object[] {
                        serialId,
                        carId,
                        isGood,
                        pageIndex,
                        pageSize}, this.GetGoodTopicListOperationCompleted, userState);
        }
        
        private void OnGetGoodTopicListOperationCompleted(object arg) {
            if ((this.GetGoodTopicListCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetGoodTopicListCompleted(this, new GetGoodTopicListCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("ApiSoapHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetTopicListBySort", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetTopicListBySort(int serialId, int carId, int sortType, int pageIndex, int pageSize, out long recordCount) {
            object[] results = this.Invoke("GetTopicListBySort", new object[] {
                        serialId,
                        carId,
                        sortType,
                        pageIndex,
                        pageSize});
            recordCount = ((long)(results[1]));
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetTopicListBySortAsync(int serialId, int carId, int sortType, int pageIndex, int pageSize) {
            this.GetTopicListBySortAsync(serialId, carId, sortType, pageIndex, pageSize, null);
        }
        
        /// <remarks/>
        public void GetTopicListBySortAsync(int serialId, int carId, int sortType, int pageIndex, int pageSize, object userState) {
            if ((this.GetTopicListBySortOperationCompleted == null)) {
                this.GetTopicListBySortOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetTopicListBySortOperationCompleted);
            }
            this.InvokeAsync("GetTopicListBySort", new object[] {
                        serialId,
                        carId,
                        sortType,
                        pageIndex,
                        pageSize}, this.GetTopicListBySortOperationCompleted, userState);
        }
        
        private void OnGetTopicListBySortOperationCompleted(object arg) {
            if ((this.GetTopicListBySortCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetTopicListBySortCompleted(this, new GetTopicListBySortCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapHeaderAttribute("ApiSoapHeaderValue")]
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetTopicListBySerialIdAndIssueKey", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetTopicListBySerialIdAndIssueKey(int serialId, string issueKey, int pageIndex, int pageSize, out long recordCount) {
            object[] results = this.Invoke("GetTopicListBySerialIdAndIssueKey", new object[] {
                        serialId,
                        issueKey,
                        pageIndex,
                        pageSize});
            recordCount = ((long)(results[1]));
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetTopicListBySerialIdAndIssueKeyAsync(int serialId, string issueKey, int pageIndex, int pageSize) {
            this.GetTopicListBySerialIdAndIssueKeyAsync(serialId, issueKey, pageIndex, pageSize, null);
        }
        
        /// <remarks/>
        public void GetTopicListBySerialIdAndIssueKeyAsync(int serialId, string issueKey, int pageIndex, int pageSize, object userState) {
            if ((this.GetTopicListBySerialIdAndIssueKeyOperationCompleted == null)) {
                this.GetTopicListBySerialIdAndIssueKeyOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetTopicListBySerialIdAndIssueKeyOperationCompleted);
            }
            this.InvokeAsync("GetTopicListBySerialIdAndIssueKey", new object[] {
                        serialId,
                        issueKey,
                        pageIndex,
                        pageSize}, this.GetTopicListBySerialIdAndIssueKeyOperationCompleted, userState);
        }
        
        private void OnGetTopicListBySerialIdAndIssueKeyOperationCompleted(object arg) {
            if ((this.GetTopicListBySerialIdAndIssueKeyCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetTopicListBySerialIdAndIssueKeyCompleted(this, new GetTopicListBySerialIdAndIssueKeyCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1055.0")]
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    public delegate void GetTopicListByRatingCompletedEventHandler(object sender, GetTopicListByRatingCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetTopicListByRatingCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetTopicListByRatingCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
        public long recordCount {
            get {
                this.RaiseExceptionIfNecessary();
                return ((long)(this.results[1]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    public delegate void GetTopicCompletedEventHandler(object sender, GetTopicCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetTopicCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetTopicCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    public delegate void GetTopicListByGuidsCompletedEventHandler(object sender, GetTopicListByGuidsCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetTopicListByGuidsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetTopicListByGuidsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    public delegate void GetTopicListCompletedEventHandler(object sender, GetTopicListCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetTopicListCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetTopicListCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
        public long recordCount {
            get {
                this.RaiseExceptionIfNecessary();
                return ((long)(this.results[1]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    public delegate void GetGoodTopicListCompletedEventHandler(object sender, GetGoodTopicListCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetGoodTopicListCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetGoodTopicListCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
        public long recordCount {
            get {
                this.RaiseExceptionIfNecessary();
                return ((long)(this.results[1]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    public delegate void GetTopicListBySortCompletedEventHandler(object sender, GetTopicListBySortCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetTopicListBySortCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetTopicListBySortCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
        public long recordCount {
            get {
                this.RaiseExceptionIfNecessary();
                return ((long)(this.results[1]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    public delegate void GetTopicListBySerialIdAndIssueKeyCompletedEventHandler(object sender, GetTopicListBySerialIdAndIssueKeyCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetTopicListBySerialIdAndIssueKeyCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetTopicListBySerialIdAndIssueKeyCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
        public long recordCount {
            get {
                this.RaiseExceptionIfNecessary();
                return ((long)(this.results[1]));
            }
        }
    }
}

#pragma warning restore 1591