﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BitAuto.CarDataUpdate.HtmlBuilder.DataCenter.WCF {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="DataCenter.WCF.IDataProvide")]
    public interface IDataProvide {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataProvide/SetUserData", ReplyAction="http://tempuri.org/IDataProvide/SetUserDataResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(System.Collections.Generic.Dictionary<string, object>))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(int[]))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(string[]))]
        void SetUserData(int userId, string key, object value);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataProvide/SetUserDataBatch", ReplyAction="http://tempuri.org/IDataProvide/SetUserDataBatchResponse")]
        void SetUserDataBatch(System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> outDic);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataProvide/GetUserData", ReplyAction="http://tempuri.org/IDataProvide/GetUserDataResponse")]
        System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> GetUserData(int[] userIds, string[] keys);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataProvide/RemoveUserData", ReplyAction="http://tempuri.org/IDataProvide/RemoveUserDataResponse")]
        void RemoveUserData(int[] userIds, string[] keys);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataProvide/GetForumDataByCity", ReplyAction="http://tempuri.org/IDataProvide/GetForumDataByCityResponse")]
        System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> GetForumDataByCity(int[] cityIds, string[] keys);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataProvide/GetForumDataByProvince", ReplyAction="http://tempuri.org/IDataProvide/GetForumDataByProvinceResponse")]
        System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> GetForumDataByProvince(int[] provinceIds, string[] keys);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDataProvide/GetForumDataBySerial", ReplyAction="http://tempuri.org/IDataProvide/GetForumDataBySerialResponse")]
        System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> GetForumDataBySerial(int[] serialIds, string[] keys);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IDataProvideChannel : BitAuto.CarDataUpdate.HtmlBuilder.DataCenter.WCF.IDataProvide, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DataProvideClient : System.ServiceModel.ClientBase<BitAuto.CarDataUpdate.HtmlBuilder.DataCenter.WCF.IDataProvide>, BitAuto.CarDataUpdate.HtmlBuilder.DataCenter.WCF.IDataProvide {
        
        public DataProvideClient() {
        }
        
        public DataProvideClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DataProvideClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DataProvideClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DataProvideClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public void SetUserData(int userId, string key, object value) {
            base.Channel.SetUserData(userId, key, value);
        }
        
        public void SetUserDataBatch(System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> outDic) {
            base.Channel.SetUserDataBatch(outDic);
        }
        
        public System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> GetUserData(int[] userIds, string[] keys) {
            return base.Channel.GetUserData(userIds, keys);
        }
        
        public void RemoveUserData(int[] userIds, string[] keys) {
            base.Channel.RemoveUserData(userIds, keys);
        }
        
        public System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> GetForumDataByCity(int[] cityIds, string[] keys) {
            return base.Channel.GetForumDataByCity(cityIds, keys);
        }
        
        public System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> GetForumDataByProvince(int[] provinceIds, string[] keys) {
            return base.Channel.GetForumDataByProvince(provinceIds, keys);
        }
        
        public System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<string, object>> GetForumDataBySerial(int[] serialIds, string[] keys) {
            return base.Channel.GetForumDataBySerial(serialIds, keys);
        }
    }
}
