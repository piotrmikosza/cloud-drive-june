﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace client.ServiceReference1 {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="FileContract", Namespace="http://schemas.datacontract.org/2004/07/server")]
    [System.SerializableAttribute()]
    public partial class FileContract : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private byte[] BytesField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FilePathField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private client.ServiceReference1.Status FileStatusField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.DateTime LastModificationField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NewFilePathField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private client.ServiceReference1.User UserField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public byte[] Bytes {
            get {
                return this.BytesField;
            }
            set {
                if ((object.ReferenceEquals(this.BytesField, value) != true)) {
                    this.BytesField = value;
                    this.RaisePropertyChanged("Bytes");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FilePath {
            get {
                return this.FilePathField;
            }
            set {
                if ((object.ReferenceEquals(this.FilePathField, value) != true)) {
                    this.FilePathField = value;
                    this.RaisePropertyChanged("FilePath");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public client.ServiceReference1.Status FileStatus {
            get {
                return this.FileStatusField;
            }
            set {
                if ((this.FileStatusField.Equals(value) != true)) {
                    this.FileStatusField = value;
                    this.RaisePropertyChanged("FileStatus");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime LastModification {
            get {
                return this.LastModificationField;
            }
            set {
                if ((this.LastModificationField.Equals(value) != true)) {
                    this.LastModificationField = value;
                    this.RaisePropertyChanged("LastModification");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string NewFilePath {
            get {
                return this.NewFilePathField;
            }
            set {
                if ((object.ReferenceEquals(this.NewFilePathField, value) != true)) {
                    this.NewFilePathField = value;
                    this.RaisePropertyChanged("NewFilePath");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public client.ServiceReference1.User User {
            get {
                return this.UserField;
            }
            set {
                if ((object.ReferenceEquals(this.UserField, value) != true)) {
                    this.UserField = value;
                    this.RaisePropertyChanged("User");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="User", Namespace="http://schemas.datacontract.org/2004/07/server")]
    [System.SerializableAttribute()]
    public partial class User : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string IpField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Ip {
            get {
                return this.IpField;
            }
            set {
                if ((object.ReferenceEquals(this.IpField, value) != true)) {
                    this.IpField = value;
                    this.RaisePropertyChanged("Ip");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Status", Namespace="http://schemas.datacontract.org/2004/07/server")]
    public enum Status : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        New = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Deleted = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Renamed = 2,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.IServer")]
    public interface IServer {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServer/GetFiles", ReplyAction="http://tempuri.org/IServer/GetFilesResponse")]
        client.ServiceReference1.FileContract[] GetFiles(System.DateTime lastModification);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServer/GetFiles", ReplyAction="http://tempuri.org/IServer/GetFilesResponse")]
        System.Threading.Tasks.Task<client.ServiceReference1.FileContract[]> GetFilesAsync(System.DateTime lastModification);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServer/SetFile", ReplyAction="http://tempuri.org/IServer/SetFileResponse")]
        void SetFile(client.ServiceReference1.FileContract fileContract);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServer/SetFile", ReplyAction="http://tempuri.org/IServer/SetFileResponse")]
        System.Threading.Tasks.Task SetFileAsync(client.ServiceReference1.FileContract fileContract);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServer/SendMessage", ReplyAction="http://tempuri.org/IServer/SendMessageResponse")]
        string SendMessage(string command, string value);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServer/SendMessage", ReplyAction="http://tempuri.org/IServer/SendMessageResponse")]
        System.Threading.Tasks.Task<string> SendMessageAsync(string command, string value);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IServerChannel : client.ServiceReference1.IServer, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ServerClient : System.ServiceModel.ClientBase<client.ServiceReference1.IServer>, client.ServiceReference1.IServer {
        
        public ServerClient() {
        }
        
        public ServerClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ServerClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServerClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ServerClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public client.ServiceReference1.FileContract[] GetFiles(System.DateTime lastModification) {
            return base.Channel.GetFiles(lastModification);
        }
        
        public System.Threading.Tasks.Task<client.ServiceReference1.FileContract[]> GetFilesAsync(System.DateTime lastModification) {
            return base.Channel.GetFilesAsync(lastModification);
        }
        
        public void SetFile(client.ServiceReference1.FileContract fileContract) {
            base.Channel.SetFile(fileContract);
        }
        
        public System.Threading.Tasks.Task SetFileAsync(client.ServiceReference1.FileContract fileContract) {
            return base.Channel.SetFileAsync(fileContract);
        }
        
        public string SendMessage(string command, string value) {
            return base.Channel.SendMessage(command, value);
        }
        
        public System.Threading.Tasks.Task<string> SendMessageAsync(string command, string value) {
            return base.Channel.SendMessageAsync(command, value);
        }
    }
}
