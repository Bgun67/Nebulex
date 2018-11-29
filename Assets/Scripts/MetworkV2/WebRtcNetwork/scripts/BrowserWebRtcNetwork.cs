﻿/* 
 * Copyright (C) 2015 Christoph Kutza
 * 
 * Please refer to the LICENSE file for license information
 */
using Byn.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Byn.Net
{
    /// <summary>
    /// Uses an underlaying java script library to give network access in browsers.
    /// 
    /// Use WebRtcNetwork.IsAvailable() first to check if it can run. If the java script part of the library
    /// is included + the browser supports WebRtc it should return true. If the java script part of the
    /// library is not included you can inject it at runtime by using 
    /// WebRtcNetwork.InjectJsCode(). It is recommended to include the js files though.
    /// 
    /// To allow incoming connections use StartServer() or StartServer("my room name")
    /// To connect others use Connect("room name");
    /// To send messages use SendData.
    /// You will need to handle incoming events by polling the Dequeue method.
    /// </summary>
    public class BrowserWebRtcNetwork : IWebRtcNetwork
    {

        //these are functions implemented in the java script plugin file WebRtcNetwork.jslib
        #region CAPI imports
        [DllImport("__Internal")]
        private static extern bool UnityWebRtcNetworkIsAvailable();

        [DllImport("__Internal")]
        private static extern int UnityWebRtcNetworkCreate(string lConfiguration);

        [DllImport("__Internal")]
        private static extern void UnityWebRtcNetworkRelease(int lIndex);

        [DllImport("__Internal")]
        private static extern int UnityWebRtcNetworkConnect(int lIndex, string lRoom);

        [DllImport("__Internal")]
        private static extern void UnityWebRtcNetworkStartServer(int lIndex, string lRoom);
        [DllImport("__Internal")]
        private static extern void UnityWebRtcNetworkStopServer(int lIndex);
        
        [DllImport("__Internal")]
        private static extern void UnityWebRtcNetworkDisconnect(int lIndex, int lConnectionId);

        [DllImport("__Internal")]
        private static extern void UnityWebRtcNetworkShutdown(int lIndex);
        [DllImport("__Internal")]
        private static extern void UnityWebRtcNetworkUpdate(int lIndex);
        [DllImport("__Internal")]
        private static extern void UnityWebRtcNetworkFlush(int lIndex);

        [DllImport("__Internal")]
        private static extern void UnityWebRtcNetworkSendData(int lIndex, int lConnectionId, byte[] lUint8ArrayDataPtr, int lUint8ArrayDataOffset, int lUint8ArrayDataLength, bool lReliable);

        [DllImport("__Internal")]
        private static extern int UnityWebRtcNetworkPeekEventDataLength(int lIndex);

        [DllImport("__Internal")]
        private static extern bool UnityWebRtcNetworkDequeue(int lIndex,
            int[] lTypeIntArrayPtr,
            int[] lConidIntArrayPtr,
            byte[] lUint8ArrayDataPtr, int lUint8ArrayDataOffset, int lUint8ArrayDataLength,
            int[] lDataLenIntArray);
        [DllImport("__Internal")]
        private static extern bool UnityWebRtcNetworkPeek(int lIndex,
            int[] lTypeIntArrayPtr,
            int[] lConidIntArrayPtr,
            byte[] lUint8ArrayDataPtr, int lUint8ArrayDataOffset, int lUint8ArrayDataLength,
            int[] lDataLenIntArray);
        #endregion

        private static bool sInjectionTried = false;

        /// <summary>
        /// This injects the library using ExternalEval. Browsers seem to load some libraries asynchronously though.
        /// This means that directly after InjectJsCode some things aren't available yet. 
        /// So starting a server/connecting won't work directly after this call yet. Add at least 1-2 seconds waiting time
        /// for the browser to download the libraries or better -> include everything needed into the websites header
        /// so this call isn't needed at all!
        /// </summary>
        static public void InjectJsCode()
        {

            //use sInjectionTried to block multiple calls.
			if ((Application.platform == RuntimePlatform.WebGLPlayer) && sInjectionTried == false)
            {
                sInjectionTried = true;
                Debug.Log("injecting webrtcnetworkplugin");

                TextAsset txt = Resources.Load<TextAsset>("webrtcnetworkplugin");

                if(txt == null)
                {
                    //Debug.LogError("Failed to find webrtcnetworkplugin.txt in Resource folder. Can't inject the JS plugin!");
                    //return;
                }

                StringBuilder jsCode = new StringBuilder();
                jsCode.Append("console.log('Start eval webrtcnetworkplugin!');");
				jsCode.Append ("var gCAPIWebRtcNetworkInstances={};var gCAPIWebRtcNetworkInstancesNextIndex=1;function CAPIWebRtcNetworkIsAvailable(){if(window.RTCPeerConnection||window.webkitRTCPeerConnection||window.mozRTCPeerConnection)return true;return false}function CAPIWebRtcNetworkCreate(e){console.debug(\"CAPIWebRtcNetworkCreate called\");var t=gCAPIWebRtcNetworkInstancesNextIndex;gCAPIWebRtcNetworkInstancesNextIndex++;var n=\"LocalNetwork\";var i=null;var o;if(e==null||typeof e!==\"string\"||e.length===0){console.error(\"invalid configuration. Returning -1! Config: \"+e);return-1}else{console.debug(\"parsing configuration\");var r=JSON.parse(e);if(r){if(r.signaling){n=r.signaling.class;i=r.signaling.param}if(r.iceServers){o=r.iceServers}var a=window[n];var s=new SignalingConfig(new a(i));console.debug(\"setup webrtc network\");var c={iceServers:o};gCAPIWebRtcNetworkInstances[t]=new WebRtcNetwork(s,c)}else{console.error(\"Parsing configuration failed. Configuration: \"+e);return-1}}return t}function CAPIWebRtcNetworkRelease(e){if(e in gCAPIWebRtcNetworkInstances){gCAPIWebRtcNetworkInstances[e].Dispose();delete gCAPIWebRtcNetworkInstances[e]}}function CAPIWebRtcNetworkConnect(e,t){return gCAPIWebRtcNetworkInstances[e].Connect(t)}function CAPIWebRtcNetworkStartServer(e,t){gCAPIWebRtcNetworkInstances[e].StartServer(t)}function CAPIWebRtcNetworkStopServer(e){gCAPIWebRtcNetworkInstances[e].StopServer()}function CAPIWebRtcNetworkDisconnect(e,t){gCAPIWebRtcNetworkInstances[e].Disconnect(new ConnectionId(t))}function CAPIWebRtcNetworkShutdown(e){gCAPIWebRtcNetworkInstances[e].Shutdown()}function CAPIWebRtcNetworkUpdate(e){gCAPIWebRtcNetworkInstances[e].Update()}function CAPIWebRtcNetworkFlush(e){gCAPIWebRtcNetworkInstances[e].Flush()}function CAPIWebRtcNetworkSendData(e,t,n,i){gCAPIWebRtcNetworkInstances[e].SendData(new ConnectionId(t),n,i)}function CAPIWebRtcNetworkSendDataEm(e,t,n,i,o,r){console.debug(\"SendDataEm: \"+r+\" length \"+o+\" to \"+t);var a=new Uint8Array(n.buffer,i,o);gCAPIWebRtcNetworkInstances[e].SendData(new ConnectionId(t),a,r)}function CAPIWebRtcNetworkDequeue(e){return gCAPIWebRtcNetworkInstances[e].Dequeue()}function CAPIWebRtcNetworkPeek(e){return gCAPIWebRtcNetworkInstances[e].Peek()}function CAPIWebRtcNetworkPeekEventDataLength(e){var t=gCAPIWebRtcNetworkInstances[e].Peek();return CAPIWebRtcNetworkCheckEventLength(t)}function CAPIWebRtcNetworkCheckEventLength(e){if(e==null){return-1}else if(e.RawData==null){return 0}else if(typeof e.RawData===\"string\"){return e.RawData.length}else{return e.RawData.length}}function CAPIWebRtcNetworkEventDataToUint8Array(e,t,n,i){if(e==null){return 0}else if(typeof e===\"string\"){var o=0;for(o=0;o<e.length&&o<i;o++){t[n+o]=e.charCodeAt(o)}return o}else{var o=0;for(o=0;o<e.length&&o<i;o++){t[n+o]=e[o]}return o}}function CAPIWebRtcNetworkDequeueEm(e,t,n,i,o,r,a,s,c,l){var u=CAPIWebRtcNetworkDequeue(e);if(u==null)return false;t[n]=u.Type;i[o]=u.ConnectionId.id;var g=CAPIWebRtcNetworkEventDataToUint8Array(u.RawData,r,a,s);c[l]=g;return true}function CAPIWebRtcNetworkPeekEm(e,t,n,i,o,r,a,s,c,l){var u=CAPIWebRtcNetworkPeek(e);if(u==null)return false;t[n]=u.Type;i[o]=u.ConnectionId.id;var g=CAPIWebRtcNetworkEventDataToUint8Array(u.RawData,r,a,s);c[l]=g;return true}var DefaultValues=function(){function e(){}Object.defineProperty(e,\"DefaultIceServers\",{get:function(){return e.mDefaultIceServer},enumerable:true,configurable:true});Object.defineProperty(e,\"DefaultSignalingServer\",{get:function(){return e.mDefaultSignalingServer},enumerable:true,configurable:true});e.mDefaultIceServer=[\"stun:stun.l.google.com:19302\"];e.mDefaultSignalingServer=\"wss://because-why-not.com:12777\";return e}();var __extends=this&&this.__extends||function(e,t){for(var n in t)if(t.hasOwnProperty(n))e[n]=t[n];function i(){this.constructor=e}e.prototype=t===null?Object.create(t):(i.prototype=t.prototype,new i)};var Queue=function(){function e(){this.mArr=new Array}e.prototype.Enqueue=function(e){this.mArr.push(e)};e.prototype.TryDequeue=function(e){var t=false;if(this.mArr.length>0){e.val=this.mArr.shift();t=true}return t};e.prototype.Dequeue=function(){if(this.mArr.length>0){return this.mArr.shift()}else{return null}};e.prototype.Peek=function(){if(this.mArr.length>0){return this.mArr[0]}else{return null}};e.prototype.Count=function(){return this.mArr.length};return e}();var List=function(){function e(){this.mArr=new Array}Object.defineProperty(e.prototype,\"Internal\",{get:function(){return this.mArr},enumerable:true,configurable:true});e.prototype.Add=function(e){this.mArr.push(e)};Object.defineProperty(e.prototype,\"Count\",{get:function(){return this.mArr.length},enumerable:true,configurable:true});return e}();var Output=function(){function e(){}return e}();var Debug=function(){function e(){}e.Log=function(e){if(e==null){console.debug(e)}console.debug(e)};e.LogError=function(e){console.debug(e)};e.LogWarning=function(e){console.debug(e)};return e}();var Encoder=function(){function e(){}return e}();var UTF16Encoding=function(e){__extends(t,e);function t(){e.call(this)}t.prototype.GetBytes=function(e){return this.stringToBuffer(e)};t.prototype.GetString=function(e){return this.bufferToString(e)};t.prototype.bufferToString=function(e){var t=new Uint16Array(e.buffer,e.byteOffset,e.byteLength/2);return String.fromCharCode.apply(null,t)};t.prototype.stringToBuffer=function(e){var t=new ArrayBuffer(e.length*2);var n=new Uint16Array(t);for(var i=0,o=e.length;i<o;i++){n[i]=e.charCodeAt(i)}var r=new Uint8Array(t);return r};return t}(Encoder);var Encoding=function(){function e(){}Object.defineProperty(e,\"UTF16\",{get:function(){return new UTF16Encoding},enumerable:true,configurable:true});return e}();var Random=function(){function e(){}e.getRandomInt=function(e,t){e=Math.ceil(e);t=Math.floor(t);return Math.floor(Math.random()*(t-e))+e};return e}();var Helper=function(){function e(){}e.tryParseInt=function(e){try{if(/^(\\-|\\+)?([0-9]+)$/.test(e)){var t=Number(e);if(isNaN(t)==false)return t}}catch(e){}return null};return e}();var SLog=function(){function e(){}e.L=function(e){console.log(e)};e.Log=function(e){console.log(e)};e.LogWarning=function(e){console.debug(e)};e.LogError=function(e){console.error(e)};return e}();var NetEventType;(function(e){e[e[\"Invalid\"]=0]=\"Invalid\";e[e[\"UnreliableMessageReceived\"]=1]=\"UnreliableMessageReceived\";e[e[\"ReliableMessageReceived\"]=2]=\"ReliableMessageReceived\";e[e[\"ServerInitialized\"]=3]=\"ServerInitialized\";e[e[\"ServerInitFailed\"]=4]=\"ServerInitFailed\";e[e[\"ServerClosed\"]=5]=\"ServerClosed\";e[e[\"NewConnection\"]=6]=\"NewConnection\";e[e[\"ConnectionFailed\"]=7]=\"ConnectionFailed\";e[e[\"Disconnected\"]=8]=\"Disconnected\";e[e[\"FatalError\"]=100]=\"FatalError\";e[e[\"Warning\"]=101]=\"Warning\";e[e[\"Log\"]=102]=\"Log\"})(NetEventType||(NetEventType={}));var NetEventDataType;(function(e){e[e[\"Null\"]=0]=\"Null\";e[e[\"ByteArray\"]=1]=\"ByteArray\";e[e[\"UTF16String\"]=2]=\"UTF16String\"})(NetEventDataType||(NetEventDataType={}));var NetworkEvent=function(){function e(e,t,n){this.type=e;this.connectionId=t;this.data=n}Object.defineProperty(e.prototype,\"RawData\",{get:function(){return this.data},enumerable:true,configurable:true});Object.defineProperty(e.prototype,\"MessageData\",{get:function(){if(typeof this.data!=\"string\")return this.data;return null},enumerable:true,configurable:true});Object.defineProperty(e.prototype,\"Info\",{get:function(){if(typeof this.data==\"string\")return this.data;return null},enumerable:true,configurable:true});Object.defineProperty(e.prototype,\"Type\",{get:function(){return this.type},enumerable:true,configurable:true});Object.defineProperty(e.prototype,\"ConnectionId\",{get:function(){return this.connectionId},enumerable:true,configurable:true});e.prototype.toString=function(){var e=\"NetworkEvent[\";e+=\"NetEventType: (\";e+=NetEventType[this.type];e+=\"), id: (\";e+=this.connectionId.id;e+=\"), Data: (\";e+=this.data;e+=\")]\";return e};e.parseFromString=function(t){var n=JSON.parse(t);var i;if(n.data==null){i=null}else if(typeof n.data==\"string\"){i=n.data}else if(typeof n.data==\"object\"){var o=n.data;var r=0;for(var a in o){r++}var s=new Uint8Array(Object.keys(o).length);for(var c=0;c<s.length;c++)s[c]=o[c];i=s}else{console.error(\"data can't be parsed\")}var l=new e(n.type,n.connectionId,i);return l};e.toString=function(e){return JSON.stringify(e)};e.fromByteArray=function(t){var n=t[0];var i=t[1];var o=new Int16Array(t.buffer,t.byteOffset+2,1)[0];var r=null;if(i==NetEventDataType.ByteArray){var a=new Uint32Array(t.buffer,t.byteOffset+4,1)[0];var s=new Uint8Array(t.buffer,t.byteOffset+8,a);r=s}else if(i==NetEventDataType.UTF16String){var c=new Uint32Array(t.buffer,t.byteOffset+4,1)[0];var l=new Uint16Array(t.buffer,t.byteOffset+8,c);var u=\"\";for(var g=0;g<l.length;g++){u+=String.fromCharCode(l[g])}r=u}var f=new ConnectionId(o);var h=new e(n,f,r);return h};e.toByteArray=function(e){var t;var n=4;if(e.data==null){t=NetEventDataType.Null}else if(typeof e.data==\"string\"){t=NetEventDataType.UTF16String;var i=e.data;n+=i.length*2+4}else{t=NetEventDataType.ByteArray;var o=e.data;n+=4+o.length}var r=new Uint8Array(n);r[0]=e.type;r[1]=t;var a=new Int16Array(r.buffer,r.byteOffset+2,1);a[0]=e.connectionId.id;if(t==NetEventDataType.ByteArray){var o=e.data;var s=new Uint32Array(r.buffer,r.byteOffset+4,1);s[0]=o.length;for(var c=0;c<o.length;c++){r[8+c]=o[c]}}else if(t==NetEventDataType.UTF16String){var i=e.data;var s=new Uint32Array(r.buffer,r.byteOffset+4,1);s[0]=i.length;var l=new Uint16Array(r.buffer,r.byteOffset+8,i.length);for(var c=0;c<l.length;c++){l[c]=i.charCodeAt(c)}}return r};return e}();var ConnectionId=function(){function e(e){this.id=e}e.INVALID=new e(-1);return e}();var LocalNetwork=function(){function e(){this.mNextNetworkId=new ConnectionId(1);this.mServerAddress=null;this.mEvents=new Queue;this.mConnectionNetwork={};this.mIsDisposed=false;this.mId=e.sNextId;e.sNextId++}Object.defineProperty(e.prototype,\"IsServer\",{get:function(){return this.mServerAddress!=null},enumerable:true,configurable:true});e.prototype.StartServer=function(t){if(t===void 0){t=null}if(t==null)t=\"\"+this.mId;if(t in e.mServers){this.Enqueue(NetEventType.ServerInitFailed,ConnectionId.INVALID,t);return}e.mServers[t]=this;this.mServerAddress=t;this.Enqueue(NetEventType.ServerInitialized,ConnectionId.INVALID,t)};e.prototype.StopServer=function(){if(this.IsServer){this.Enqueue(NetEventType.ServerClosed,ConnectionId.INVALID,this.mServerAddress);delete e.mServers[this.mServerAddress];this.mServerAddress=null}};e.prototype.Connect=function(t){var n=this.NextConnectionId();var i=false;if(t in e.mServers){var o=e.mServers[t];if(o!=null){o.ConnectClient(this);this.mConnectionNetwork[n.id]=e.mServers[t];this.Enqueue(NetEventType.NewConnection,n,null);i=true}}if(i==false){this.Enqueue(NetEventType.ConnectionFailed,n,\"Couldn't connect to the given server with id \"+t)}return n};e.prototype.Shutdown=function(){for(var e in this.mConnectionNetwork){this.Disconnect(new ConnectionId(+e))}this.StopServer()};e.prototype.Dispose=function(){if(this.mIsDisposed==false){this.Shutdown()}};e.prototype.SendData=function(e,t,n){if(e.id in this.mConnectionNetwork){var i=this.mConnectionNetwork[e.id];i.ReceiveData(this,t,n)}};e.prototype.Update=function(){this.CleanupWreakReferences()};e.prototype.Dequeue=function(){return this.mEvents.Dequeue()};e.prototype.Peek=function(){return this.mEvents.Peek()};e.prototype.Flush=function(){};e.prototype.Disconnect=function(e){if(e.id in this.mConnectionNetwork){var t=this.mConnectionNetwork[e.id];if(t!=null){t.InternalDisconnectNetwork(this);this.InternalDisconnect(e)}else{this.CleanupWreakReferences()}}};e.prototype.FindConnectionId=function(e){for(var t in this.mConnectionNetwork){var n=this.mConnectionNetwork[t];if(n!=null){return new ConnectionId(+t)}}return ConnectionId.INVALID};e.prototype.NextConnectionId=function(){var e=this.mNextNetworkId;this.mNextNetworkId=new ConnectionId(e.id+1);return e};e.prototype.ConnectClient=function(e){var t=this.NextConnectionId();this.mConnectionNetwork[t.id]=e;this.Enqueue(NetEventType.NewConnection,t,null)};e.prototype.Enqueue=function(e,t,n){var i=new NetworkEvent(e,t,n);this.mEvents.Enqueue(i)};e.prototype.ReceiveData=function(e,t,n){var i=this.FindConnectionId(e);var o=new Uint8Array(t.length);for(var r=0;r<o.length;r++){o[r]=t[r]}var a=NetEventType.UnreliableMessageReceived;if(n)a=NetEventType.ReliableMessageReceived;this.Enqueue(a,i,o)};e.prototype.InternalDisconnect=function(e){if(e.id in this.mConnectionNetwork){this.Enqueue(NetEventType.Disconnected,e,null);delete this.mConnectionNetwork[e.id]}};e.prototype.InternalDisconnectNetwork=function(e){this.InternalDisconnect(this.FindConnectionId(e))};e.prototype.CleanupWreakReferences=function(){};e.sNextId=1;e.mServers={};return e}();function WebRtcNetwork_test1(){console.log(\"test1\");var e=\"test1234\";var t;if(window.location.protocol!=\"https:\"){t=\"ws://localhost:12776\"}else{t=\"wss://localhost:12777\"}var n={iceServers:[{urls:[\"stun:stun.l.google.com:19302\"]}]};var i=new WebRtcNetwork(new SignalingConfig(new LocalNetwork),n);i.StartServer();var o=new WebRtcNetwork(new SignalingConfig(new LocalNetwork),n);setInterval(function(){i.Update();var t=null;while(t=i.Dequeue()){console.log(\"server inc: \"+t.toString());if(t.Type==NetEventType.ServerInitialized){console.log(\"server started. Address \"+t.Info);o.Connect(t.Info)}else if(t.Type==NetEventType.ServerInitFailed){console.error(\"server start failed\")}else if(t.Type==NetEventType.NewConnection){console.log(\"server new incoming connection\")}else if(t.Type==NetEventType.Disconnected){console.log(\"server peer disconnected\");console.log(\"server shutdown\");i.Shutdown()}else if(t.Type==NetEventType.ReliableMessageReceived){i.SendData(t.ConnectionId,t.MessageData,true)}else if(t.Type==NetEventType.UnreliableMessageReceived){i.SendData(t.ConnectionId,t.MessageData,false)}}i.Flush();o.Update();while(t=o.Dequeue()){console.log(\"client inc: \"+t.toString());if(t.Type==NetEventType.NewConnection){console.log(\"client connection established\");var n=stringToBuffer(e);o.SendData(t.ConnectionId,n,true)}else if(t.Type==NetEventType.ReliableMessageReceived){var r=bufferToString(t.MessageData);if(r!=e){console.error(\"Test failed sent string %s but received string %s\",e,r)}var n=stringToBuffer(e);o.SendData(t.ConnectionId,n,false)}else if(t.Type==NetEventType.UnreliableMessageReceived){var r=bufferToString(t.MessageData);if(r!=e){console.error(\"Test failed sent string %s but received string %s\",e,r)}console.log(\"client disconnecting\");o.Disconnect(t.ConnectionId);console.log(\"client shutting down\");o.Shutdown()}}o.Flush()},100)}function WebsocketNetwork_sharedaddress(){console.log(\"WebsocketNetwork shared address test\");var e=\"test1234\";var t=true;var n=true;var i;var o;if(window.location.protocol!=\"https:\"&&n){i=\"wss://because-why-not.com:12776/testshare\";if(t)i=\"ws://localhost:12776/testshare\"}else{i=\"wss://because-why-not.com:12777/testshare\";if(t)i=\"wss://localhost:12777/testshare\"}var r=\"sharedaddresstest\";var a=new WebsocketNetwork(i);var s=new WebsocketNetwork(i);var c=new WebsocketNetwork(i);var l=stringToBuffer(\"network1 says hi\");var u=stringToBuffer(\"network2 says hi\");var g=stringToBuffer(\"network3 says hi\");a.StartServer(r);s.StartServer(r);c.StartServer(r);function f(e,t){e.Update();var n=null;while(n=e.Dequeue()){if(n.Type==NetEventType.ServerInitFailed||n.Type==NetEventType.ConnectionFailed||n.Type==NetEventType.ServerClosed){console.error(t+\"inc: \"+n.toString())}else{console.log(t+\"inc: \"+n.toString())}if(n.Type==NetEventType.ServerInitialized){}else if(n.Type==NetEventType.ServerInitFailed){}else if(n.Type==NetEventType.NewConnection){var i=stringToBuffer(t+\"says hi!\");e.SendData(n.ConnectionId,i,true)}else if(n.Type==NetEventType.Disconnected){}else if(n.Type==NetEventType.ReliableMessageReceived){var o=bufferToString(n.MessageData);console.log(t+\" received: \"+o)}else if(n.Type==NetEventType.UnreliableMessageReceived){}}e.Flush()}var h=0;setInterval(function(){f(a,\"network1 \");f(s,\"network2 \");f(c,\"network3 \");h+=100;if(h==1e4){console.log(\"network1 shutdown\");a.Shutdown()}if(h==15e3){console.log(\"network2 shutdown\");s.Shutdown()}if(h==2e4){console.log(\"network3 shutdown\");c.Shutdown()}},100)}function CAPIWebRtcNetwork_test1(){console.log(\"test1\");var e=\"test1234\";var t='{ \"signaling\" :  { \"class\": \"LocalNetwork\", \"param\" : null}, \"iceServers\":[\"stun:stun.l.google.com:19302\"]}';var n=CAPIWebRtcNetworkCreate(t);CAPIWebRtcNetworkStartServer(n,\"Room1\");var i=CAPIWebRtcNetworkCreate(t);setInterval(function(){CAPIWebRtcNetworkUpdate(n);var t=null;while(t=CAPIWebRtcNetworkDequeue(n)){console.log(\"server inc: \"+t.toString());if(t.Type==NetEventType.ServerInitialized){console.log(\"server started. Address \"+t.Info);CAPIWebRtcNetworkConnect(i,t.Info)}else if(t.Type==NetEventType.ServerInitFailed){console.error(\"server start failed\")}else if(t.Type==NetEventType.NewConnection){console.log(\"server new incoming connection\")}else if(t.Type==NetEventType.Disconnected){console.log(\"server peer disconnected\");console.log(\"server shutdown\");CAPIWebRtcNetworkShutdown(n)}else if(t.Type==NetEventType.ReliableMessageReceived){CAPIWebRtcNetworkSendData(n,t.ConnectionId.id,t.MessageData,true)}else if(t.Type==NetEventType.UnreliableMessageReceived){CAPIWebRtcNetworkSendData(n,t.ConnectionId.id,t.MessageData,false)}}CAPIWebRtcNetworkFlush(n);CAPIWebRtcNetworkUpdate(i);while(t=CAPIWebRtcNetworkDequeue(i)){console.log(\"client inc: \"+t.toString());if(t.Type==NetEventType.NewConnection){console.log(\"client connection established\");var o=stringToBuffer(e);CAPIWebRtcNetworkSendData(i,t.ConnectionId.id,o,true)}else if(t.Type==NetEventType.ReliableMessageReceived){var r=bufferToString(t.MessageData);if(r!=e){console.error(\"Test failed sent string %s but received string %s\",e,r)}var o=stringToBuffer(e);CAPIWebRtcNetworkSendData(i,t.ConnectionId.id,o,false)}else if(t.Type==NetEventType.UnreliableMessageReceived){var r=bufferToString(t.MessageData);if(r!=e){console.error(\"Test failed sent string %s but received string %s\",e,r)}console.log(\"client disconnecting\");CAPIWebRtcNetworkDisconnect(i,t.ConnectionId.id);console.log(\"client shutting down\");CAPIWebRtcNetworkShutdown(i)}}CAPIWebRtcNetworkFlush(i)},100)}var WebRtcNetworkServerState;(function(e){e[e[\"Invalid\"]=0]=\"Invalid\";e[e[\"Offline\"]=1]=\"Offline\";e[e[\"Starting\"]=2]=\"Starting\";e[e[\"Online\"]=3]=\"Online\"})(WebRtcNetworkServerState||(WebRtcNetworkServerState={}));var SignalingConfig=function(){function e(e){this.mNetwork=e}e.prototype.GetNetwork=function(){return this.mNetwork};return e}();var SignalingInfo=function(){function e(e,t,n){this.mConnectionId=e;this.mIsIncoming=t;this.mCreationTime=n;this.mSignalingConnected=true}e.prototype.IsSignalingConnected=function(){return this.mSignalingConnected};Object.defineProperty(e.prototype,\"ConnectionId\",{get:function(){return this.mConnectionId},enumerable:true,configurable:true});e.prototype.IsIncoming=function(){return this.mIsIncoming};e.prototype.GetCreationTimeMs=function(){return Date.now()-this.mCreationTime};e.prototype.SignalingDisconnected=function(){this.mSignalingConnected=false};return e}();var WebRtcNetwork=function(){function e(e,t){this.mTimeout=6e4;this.mInSignaling={};this.mNextId=new ConnectionId(1);this.mSignaling=null;this.mEvents=new Queue;this.mIdToConnection={};this.mConnectionIds=new Array;this.mServerState=WebRtcNetworkServerState.Offline;this.mIsDisposed=false;this.mSignaling=e;this.mSignalingNetwork=this.mSignaling.GetNetwork();this.mRtcConfig=t}Object.defineProperty(e.prototype,\"IdToConnection\",{get:function(){return this.mIdToConnection},enumerable:true,configurable:true});e.prototype.GetConnections=function(){return this.mConnectionIds};e.prototype.SetLog=function(e){this.mLogDelegate=e};e.prototype.StartServer=function(e){this.mServerState=WebRtcNetworkServerState.Starting;this.mSignalingNetwork.StartServer(e)};e.prototype.StopServer=function(){if(this.mServerState==WebRtcNetworkServerState.Starting){this.mSignalingNetwork.StopServer()}else if(this.mServerState==WebRtcNetworkServerState.Online){this.mSignalingNetwork.StopServer()}};e.prototype.Connect=function(e){console.log(\"Connecting ...\");return this.AddOutgoingConnection(e)};e.prototype.Update=function(){this.CheckSignalingState();this.UpdateSignalingNetwork();this.UpdatePeers()};e.prototype.Dequeue=function(){if(this.mEvents.Count()>0)return this.mEvents.Dequeue();return null};e.prototype.Peek=function(){if(this.mEvents.Count()>0)return this.mEvents.Peek();return null};e.prototype.Flush=function(){this.mSignalingNetwork.Flush()};e.prototype.SendData=function(e,t,n){if(e==null||t==null||t.length==0)return;var i=this.mIdToConnection[e.id];if(i){i.SendData(t,n)}else{Debug.LogWarning(\"unknown connection id\")}};e.prototype.Disconnect=function(e){var t=this.mIdToConnection[e.id];if(t){this.HandleDisconnect(e)}};e.prototype.Shutdown=function(){for(var e=0,t=this.mConnectionIds;e<t.length;e++){var n=t[e];this.Disconnect(n)}this.StopServer();this.mSignalingNetwork.Shutdown()};e.prototype.DisposeInternal=function(){if(this.mIsDisposed==false){this.Shutdown();this.mIsDisposed=true}};e.prototype.Dispose=function(){this.DisposeInternal()};e.prototype.CreatePeer=function(e,t){var n=new WebRtcDataPeer(e,t);return n};e.prototype.CheckSignalingState=function(){var e=new Array;var t=new Array;for(var n in this.mInSignaling){var i=this.mInSignaling[n];i.Update();var o=i.SignalingInfo.GetCreationTimeMs();var r=new Output;while(i.DequeueSignalingMessage(r)){var a=this.StringToBuffer(r.val);this.mSignalingNetwork.SendData(new ConnectionId(+n),a,true)}if(i.GetState()==WebRtcPeerState.Connected){e.push(i.SignalingInfo.ConnectionId)}else if(i.GetState()==WebRtcPeerState.SignalingFailed||o>this.mTimeout){t.push(i.SignalingInfo.ConnectionId)}}for(var s=0,c=e;s<c.length;s++){var l=c[s];this.ConnectionEstablished(l)}for(var u=0,g=t;u<g.length;u++){var l=g[u];this.SignalingFailed(l)}};e.prototype.UpdateSignalingNetwork=function(){this.mSignalingNetwork.Update();var e;while((e=this.mSignalingNetwork.Dequeue())!=null){if(e.Type==NetEventType.ServerInitialized){this.mServerState=WebRtcNetworkServerState.Online;this.mEvents.Enqueue(new NetworkEvent(NetEventType.ServerInitialized,ConnectionId.INVALID,e.RawData))}else if(e.Type==NetEventType.ServerInitFailed){this.mServerState=WebRtcNetworkServerState.Offline;this.mEvents.Enqueue(new NetworkEvent(NetEventType.ServerInitFailed,ConnectionId.INVALID,e.RawData))}else if(e.Type==NetEventType.ServerClosed){this.mServerState=WebRtcNetworkServerState.Offline;this.mEvents.Enqueue(new NetworkEvent(NetEventType.ServerClosed,ConnectionId.INVALID,e.RawData))}else if(e.Type==NetEventType.NewConnection){var t=this.mInSignaling[e.ConnectionId.id];if(t){t.StartSignaling()}else{this.AddIncomingConnection(e.ConnectionId)}}else if(e.Type==NetEventType.ConnectionFailed){this.SignalingFailed(e.ConnectionId)}else if(e.Type==NetEventType.Disconnected){var t=this.mInSignaling[e.ConnectionId.id];if(t){t.SignalingInfo.SignalingDisconnected()}}else if(e.Type==NetEventType.ReliableMessageReceived){var t=this.mInSignaling[e.ConnectionId.id];if(t){var n=this.BufferToString(e.MessageData);t.AddSignalingMessage(n)}else{Debug.LogWarning(\"Signaling message from unknown connection received\")}}}};e.prototype.UpdatePeers=function(){var e=new Array;for(var t in this.mIdToConnection){var n=this.mIdToConnection[t];n.Update();var i=new Output;while(n.DequeueEvent(i)){this.mEvents.Enqueue(i.val)}if(n.GetState()==WebRtcPeerState.Closed){e.push(n.ConnectionId)}}for(var o=0,r=e;o<r.length;o++){var a=r[o];this.HandleDisconnect(a)}};e.prototype.AddOutgoingConnection=function(e){Debug.Log(\"new outgoing connection\");var t=this.mSignalingNetwork.Connect(e);var n=new SignalingInfo(t,false,Date.now());var i=this.CreatePeer(this.NextConnectionId(),this.mRtcConfig);i.SetSignalingInfo(n);this.mInSignaling[t.id]=i;return i.ConnectionId};e.prototype.AddIncomingConnection=function(e){Debug.Log(\"new incoming connection\");var t=new SignalingInfo(e,true,Date.now());var n=this.CreatePeer(this.NextConnectionId(),this.mRtcConfig);n.SetSignalingInfo(t);this.mInSignaling[e.id]=n;n.NegotiateSignaling();return n.ConnectionId};e.prototype.ConnectionEstablished=function(e){var t=this.mInSignaling[e.id];delete this.mInSignaling[e.id];this.mSignalingNetwork.Disconnect(e);this.mConnectionIds.push(t.ConnectionId);this.mIdToConnection[t.ConnectionId.id]=t;this.mEvents.Enqueue(new NetworkEvent(NetEventType.NewConnection,t.ConnectionId,null))};e.prototype.SignalingFailed=function(e){var t=this.mInSignaling[e.id];if(t){delete this.mInSignaling[e.id];this.mEvents.Enqueue(new NetworkEvent(NetEventType.ConnectionFailed,t.ConnectionId,null));if(t.SignalingInfo.IsSignalingConnected()){this.mSignalingNetwork.Disconnect(e)}t.Dispose()}};e.prototype.HandleDisconnect=function(e){var t=this.mIdToConnection[e.id];if(t){t.Dispose()}var n=this.mConnectionIds.indexOf(e);if(n!=-1){this.mConnectionIds.splice(n,1)}delete this.mIdToConnection[e.id];var i=new NetworkEvent(NetEventType.Disconnected,e,null);this.mEvents.Enqueue(i)};e.prototype.NextConnectionId=function(){var e=new ConnectionId(this.mNextId.id);this.mNextId.id++;return e};e.prototype.StringToBuffer=function(e){var t=new ArrayBuffer(e.length*2);var n=new Uint16Array(t);for(var i=0,o=e.length;i<o;i++){n[i]=e.charCodeAt(i)}var r=new Uint8Array(t);return r};e.prototype.BufferToString=function(e){var t=new Uint16Array(e.buffer,e.byteOffset,e.byteLength/2);return String.fromCharCode.apply(null,t)};return e}();var WebRtcPeerState;(function(e){e[e[\"Invalid\"]=0]=\"Invalid\";e[e[\"Created\"]=1]=\"Created\";e[e[\"Signaling\"]=2]=\"Signaling\";e[e[\"SignalingFailed\"]=3]=\"SignalingFailed\";e[e[\"Connected\"]=4]=\"Connected\";e[e[\"Closing\"]=5]=\"Closing\";e[e[\"Closed\"]=6]=\"Closed\"})(WebRtcPeerState||(WebRtcPeerState={}));var WebRtcInternalState;(function(e){e[e[\"None\"]=0]=\"None\";e[e[\"Signaling\"]=1]=\"Signaling\";e[e[\"SignalingFailed\"]=2]=\"SignalingFailed\";e[e[\"Connected\"]=3]=\"Connected\";e[e[\"Closed\"]=4]=\"Closed\"})(WebRtcInternalState||(WebRtcInternalState={}));var AWebRtcPeer=function(){function e(e){this.mState=WebRtcPeerState.Invalid;this.mRtcInternalState=WebRtcInternalState.None;this.mIncomingSignalingQueue=new Queue;this.mOutgoingSignalingQueue=new Queue;this.mDidSendRandomNumber=false;this.mRandomNumerSent=0;this.mOfferOptions={offerToReceiveAudio:0,offerToReceiveVideo:0};this.gConnectionConfig={optional:[{DtlsSrtpKeyAgreement:true}]};this.SetupPeer(e);this.OnSetup();this.mState=WebRtcPeerState.Created}e.prototype.GetState=function(){return this.mState};e.prototype.SetupPeer=function(e){var t=this;var n=window.RTCPeerConnection||window.mozRTCPeerConnection||window.webkitRTCPeerConnection;this.mPeer=new n(e,this.gConnectionConfig);this.mPeer.onicecandidate=function(e){t.OnIceCandidate(e)};this.mPeer.oniceconnectionstatechange=function(e){t.OnIceConnectionChange()};this.mPeer.onnegotiationneeded=function(e){t.OnRenegotiationNeeded()};this.mPeer.onsignalingstatechange=function(e){t.OnSignalingChange()}};e.prototype.DisposeInternal=function(){this.Cleanup()};e.prototype.Dispose=function(){if(this.mPeer!=null){this.DisposeInternal()}};e.prototype.Cleanup=function(){if(this.mState==WebRtcPeerState.Closed||this.mState==WebRtcPeerState.Closing){return}this.mState=WebRtcPeerState.Closing;this.OnCleanup();if(this.mPeer!=null)this.mPeer.close();this.mState=WebRtcPeerState.Closed};e.prototype.Update=function(){if(this.mState!=WebRtcPeerState.Closed&&this.mState!=WebRtcPeerState.Closing&&this.mState!=WebRtcPeerState.SignalingFailed)this.UpdateState();if(this.mState==WebRtcPeerState.Signaling||this.mState==WebRtcPeerState.Created)this.HandleIncomingSignaling()};e.prototype.UpdateState=function(){if(this.mRtcInternalState==WebRtcInternalState.Closed){this.Cleanup()}else if(this.mRtcInternalState==WebRtcInternalState.SignalingFailed){this.mState=WebRtcPeerState.SignalingFailed}else if(this.mRtcInternalState==WebRtcInternalState.Connected){this.mState=WebRtcPeerState.Connected}};e.prototype.HandleIncomingSignaling=function(){while(this.mIncomingSignalingQueue.Count()>0){var e=this.mIncomingSignalingQueue.Dequeue();var t=Helper.tryParseInt(e);if(t!=null){if(this.mDidSendRandomNumber){if(t<this.mRandomNumerSent){SLog.L(\"Signaling negotiation complete. Starting signaling.\");this.StartSignaling()}else if(t==this.mRandomNumerSent){this.NegotiateSignaling()}else{SLog.L(\"Signaling negotiation complete. Waiting for signaling.\")}}else{}}else{var n=JSON.parse(e);if(n.sdp){var i=new RTCSessionDescription(n);if(i.type==\"offer\"){this.CreateAnswer(i)}else{this.RecAnswer(i)}}else{var o=new RTCIceCandidate(n);if(o!=null){this.mPeer.addIceCandidate(o,function(){},function(e){Debug.LogError(e)})}}}}};e.prototype.AddSignalingMessage=function(e){Debug.Log(\"incoming Signaling message \"+e);this.mIncomingSignalingQueue.Enqueue(e)};e.prototype.DequeueSignalingMessage=function(e){{if(this.mOutgoingSignalingQueue.Count()>0){e.val=this.mOutgoingSignalingQueue.Dequeue();return true}else{e.val=null;return false}}};e.prototype.EnqueueOutgoing=function(e){{Debug.Log(\"Outgoing Signaling message \"+e);this.mOutgoingSignalingQueue.Enqueue(e)}};e.prototype.StartSignaling=function(){this.OnStartSignaling();this.CreateOffer()};e.prototype.NegotiateSignaling=function(){var e=Random.getRandomInt(0,2147483647);this.mRandomNumerSent=e;this.mDidSendRandomNumber=true;this.EnqueueOutgoing(\"\"+e)};e.prototype.CreateOffer=function(){var e=this;Debug.Log(\"CreateOffer\");this.mPeer.createOffer(function(t){var n=JSON.stringify(t);e.mPeer.setLocalDescription(t,function(){e.RtcSetSignalingStarted();e.EnqueueOutgoing(n)},function(t){Debug.LogError(t);e.RtcSetSignalingFailed()})},function(t){Debug.LogError(t);e.RtcSetSignalingFailed()},this.mOfferOptions)};e.prototype.CreateAnswer=function(e){var t=this;Debug.Log(\"CreateAnswer\");this.mPeer.setRemoteDescription(e,function(){t.mPeer.createAnswer(function(e){var n=JSON.stringify(e);t.mPeer.setLocalDescription(e,function(){t.RtcSetSignalingStarted();t.EnqueueOutgoing(n)},function(e){Debug.LogError(e);t.RtcSetSignalingFailed()})},function(e){Debug.LogError(e);t.RtcSetSignalingFailed()})},function(e){Debug.LogError(e);t.RtcSetSignalingFailed()})};e.prototype.RecAnswer=function(e){var t=this;Debug.Log(\"RecAnswer\");this.mPeer.setRemoteDescription(e,function(){},function(e){Debug.LogError(e);t.RtcSetSignalingFailed()})};e.prototype.RtcSetSignalingStarted=function(){if(this.mRtcInternalState==WebRtcInternalState.None){this.mRtcInternalState=WebRtcInternalState.Signaling}};e.prototype.RtcSetSignalingFailed=function(){this.mRtcInternalState=WebRtcInternalState.SignalingFailed};e.prototype.RtcSetConnected=function(){if(this.mRtcInternalState==WebRtcInternalState.Signaling)this.mRtcInternalState=WebRtcInternalState.Connected};e.prototype.RtcSetClosed=function(){if(this.mRtcInternalState==WebRtcInternalState.Connected)this.mRtcInternalState=WebRtcInternalState.Closed};e.prototype.OnIceCandidate=function(e){if(e&&e.candidate){var t=e.candidate;var n=JSON.stringify(t);this.EnqueueOutgoing(n)}};e.prototype.OnIceConnectionChange=function(){Debug.Log(this.mPeer.iceConnectionState);if(this.mPeer.iceConnectionState==\"failed\"){this.mState=WebRtcPeerState.SignalingFailed}};e.prototype.OnIceGatheringChange=function(){Debug.Log(this.mPeer.iceGatheringState)};e.prototype.OnRenegotiationNeeded=function(){};e.prototype.OnSignalingChange=function(){Debug.Log(this.mPeer.signalingState);if(this.mPeer.signalingState==\"closed\"){this.RtcSetClosed()}};return e}();var WebRtcDataPeer=function(e){__extends(t,e);function t(t,n){e.call(this,n);this.mInfo=null;this.mEvents=new Queue;this.mReliableDataChannelReady=false;this.mUnreliableDataChannelReady=false;this.mConnectionId=t}Object.defineProperty(t.prototype,\"ConnectionId\",{get:function(){return this.mConnectionId},enumerable:true,configurable:true});Object.defineProperty(t.prototype,\"SignalingInfo\",{get:function(){return this.mInfo},enumerable:true,configurable:true})\n;t.prototype.SetSignalingInfo=function(e){this.mInfo=e};t.prototype.OnSetup=function(){var e=this;this.mPeer.ondatachannel=function(t){e.OnDataChannel(t.channel)}};t.prototype.OnStartSignaling=function(){var e={};this.mReliableDataChannel=this.mPeer.createDataChannel(t.sLabelReliable,e);this.RegisterObserverReliable();var n={};n.maxRetransmits=0;n.ordered=false;this.mUnreliableDataChannel=this.mPeer.createDataChannel(t.sLabelUnreliable,n);this.RegisterObserverUnreliable()};t.prototype.OnCleanup=function(){if(this.mReliableDataChannel!=null)this.mReliableDataChannel.close();if(this.mUnreliableDataChannel!=null)this.mUnreliableDataChannel.close()};t.prototype.RegisterObserverReliable=function(){var e=this;this.mReliableDataChannel.onmessage=function(t){e.ReliableDataChannel_OnMessage(t)};this.mReliableDataChannel.onopen=function(t){e.ReliableDataChannel_OnOpen()};this.mReliableDataChannel.onclose=function(t){e.ReliableDataChannel_OnClose()};this.mReliableDataChannel.onerror=function(t){e.ReliableDataChannel_OnError(\"\")}};t.prototype.RegisterObserverUnreliable=function(){var e=this;this.mUnreliableDataChannel.onmessage=function(t){e.UnreliableDataChannel_OnMessage(t)};this.mUnreliableDataChannel.onopen=function(t){e.UnreliableDataChannel_OnOpen()};this.mUnreliableDataChannel.onclose=function(t){e.UnreliableDataChannel_OnClose()};this.mUnreliableDataChannel.onerror=function(t){e.UnreliableDataChannel_OnError(\"\")}};t.prototype.SendData=function(e,t){var n=e;if(t){this.mReliableDataChannel.send(n)}else{this.mUnreliableDataChannel.send(n)}};t.prototype.DequeueEvent=function(e){{if(this.mEvents.Count()>0){e.val=this.mEvents.Dequeue();return true}}return false};t.prototype.Enqueue=function(e){{this.mEvents.Enqueue(e)}};t.prototype.OnDataChannel=function(e){var n=e;if(n.label==t.sLabelReliable){this.mReliableDataChannel=n;this.RegisterObserverReliable()}else if(n.label==t.sLabelUnreliable){this.mUnreliableDataChannel=n;this.RegisterObserverUnreliable()}else{Debug.LogError(\"Datachannel with unexpected label \"+n.label)}};t.prototype.RtcOnMessageReceived=function(e,t){var n=NetEventType.UnreliableMessageReceived;if(t){n=NetEventType.ReliableMessageReceived}if(e.data instanceof ArrayBuffer){var i=new Uint8Array(e.data);this.Enqueue(new NetworkEvent(n,this.mConnectionId,i))}else if(e.data instanceof Blob){var o=this.mConnectionId;var r=new FileReader;var a=this;r.onload=function(){var e=new Uint8Array(this.result);a.Enqueue(new NetworkEvent(n,a.mConnectionId,e))};r.readAsArrayBuffer(e.data)}else{Debug.LogError(\"Invalid message type. Only blob and arraybuffer supported: \"+e.data)}};t.prototype.ReliableDataChannel_OnMessage=function(e){Debug.Log(\"ReliableDataChannel_OnMessage \");this.RtcOnMessageReceived(e,true)};t.prototype.ReliableDataChannel_OnOpen=function(){Debug.Log(\"mReliableDataChannelReady\");this.mReliableDataChannelReady=true;if(this.IsRtcConnected()){this.RtcSetConnected();Debug.Log(\"Fully connected\")}};t.prototype.ReliableDataChannel_OnClose=function(){this.RtcSetClosed()};t.prototype.ReliableDataChannel_OnError=function(e){Debug.LogError(e);this.RtcSetClosed()};t.prototype.UnreliableDataChannel_OnMessage=function(e){Debug.Log(\"UnreliableDataChannel_OnMessage \");this.RtcOnMessageReceived(e,false)};t.prototype.UnreliableDataChannel_OnOpen=function(){Debug.Log(\"mUnreliableDataChannelReady\");this.mUnreliableDataChannelReady=true;if(this.IsRtcConnected()){this.RtcSetConnected();Debug.Log(\"Fully connected\")}};t.prototype.UnreliableDataChannel_OnClose=function(){this.RtcSetClosed()};t.prototype.UnreliableDataChannel_OnError=function(e){Debug.LogError(e);this.RtcSetClosed()};t.prototype.IsRtcConnected=function(){return this.mReliableDataChannelReady&&this.mUnreliableDataChannelReady};t.sLabelReliable=\"reliable\";t.sLabelUnreliable=\"unreliable\";return t}(AWebRtcPeer);var WebsocketConnectionStatus;(function(e){e[e[\"Uninitialized\"]=0]=\"Uninitialized\";e[e[\"NotConnected\"]=1]=\"NotConnected\";e[e[\"Connecting\"]=2]=\"Connecting\";e[e[\"Connected\"]=3]=\"Connected\";e[e[\"Disconnecting\"]=4]=\"Disconnecting\"})(WebsocketConnectionStatus||(WebsocketConnectionStatus={}));var WebsocketServerStatus;(function(e){e[e[\"Offline\"]=0]=\"Offline\";e[e[\"Starting\"]=1]=\"Starting\";e[e[\"Online\"]=2]=\"Online\";e[e[\"ShuttingDown\"]=3]=\"ShuttingDown\"})(WebsocketServerStatus||(WebsocketServerStatus={}));var WebsocketNetwork=function(){function e(e){this.mStatus=WebsocketConnectionStatus.Uninitialized;this.mOutgoingQueue=new Array;this.mIncomingQueue=new Array;this.mServerStatus=WebsocketServerStatus.Offline;this.mConnecting=new Array;this.mConnections=new Array;this.mNextOutgoingConnectionId=new ConnectionId(1);this.mUrl=null;this.mIsDisposed=false;this.mUrl=e;this.mStatus=WebsocketConnectionStatus.NotConnected}e.prototype.getStatus=function(){return this.mStatus};e.prototype.WebsocketConnect=function(){var e=this;this.mStatus=WebsocketConnectionStatus.Connecting;this.mSocket=new WebSocket(this.mUrl);this.mSocket.binaryType=\"arraybuffer\";this.mSocket.onopen=function(){e.OnWebsocketOnOpen()};this.mSocket.onerror=function(t){e.OnWebsocketOnError(t)};this.mSocket.onmessage=function(t){e.OnWebsocketOnMessage(t)};this.mSocket.onclose=function(t){e.OnWebsocketOnClose(t)}};e.prototype.WebsocketCleanup=function(){this.mSocket.onopen=null;this.mSocket.onerror=null;this.mSocket.onmessage=null;this.mSocket.onclose=null;if(this.mSocket.readyState==this.mSocket.OPEN||this.mSocket.readyState==this.mSocket.CONNECTING){this.mSocket.close()}this.mSocket=null};e.prototype.EnsureServerConnection=function(){if(this.mStatus==WebsocketConnectionStatus.NotConnected){this.WebsocketConnect()}};e.prototype.CheckSleep=function(){if(this.mStatus==WebsocketConnectionStatus.Connected&&this.mServerStatus==WebsocketServerStatus.Offline&&this.mConnecting.length==0&&this.mConnections.length==0){this.Cleanup()}};e.prototype.OnWebsocketOnOpen=function(){console.log(\"onWebsocketOnOpen\");this.mStatus=WebsocketConnectionStatus.Connected};e.prototype.OnWebsocketOnClose=function(e){console.log(\"Closed: \"+JSON.stringify(e));if(this.mStatus==WebsocketConnectionStatus.Disconnecting||this.mStatus==WebsocketConnectionStatus.NotConnected)return;this.Cleanup();this.mStatus=WebsocketConnectionStatus.NotConnected};e.prototype.OnWebsocketOnMessage=function(e){if(this.mStatus==WebsocketConnectionStatus.Disconnecting||this.mStatus==WebsocketConnectionStatus.NotConnected)return;var t=NetworkEvent.fromByteArray(new Uint8Array(e.data));this.HandleIncomingEvent(t)};e.prototype.OnWebsocketOnError=function(e){if(this.mStatus==WebsocketConnectionStatus.Disconnecting||this.mStatus==WebsocketConnectionStatus.NotConnected)return;console.log(\"WebSocket Error \"+e)};e.prototype.Cleanup=function(){if(this.mStatus==WebsocketConnectionStatus.Disconnecting||this.mStatus==WebsocketConnectionStatus.NotConnected)return;this.mStatus=WebsocketConnectionStatus.Disconnecting;for(var e=0,t=this.mConnecting;e<t.length;e++){var n=t[e];this.EnqueueIncoming(new NetworkEvent(NetEventType.ConnectionFailed,new ConnectionId(n),null))}this.mConnecting=new Array;for(var i=0,o=this.mConnections;i<o.length;i++){var n=o[i];this.EnqueueIncoming(new NetworkEvent(NetEventType.Disconnected,new ConnectionId(n),null))}this.mConnections=new Array;if(this.mServerStatus==WebsocketServerStatus.Starting){this.EnqueueIncoming(new NetworkEvent(NetEventType.ServerInitFailed,ConnectionId.INVALID,null))}else if(this.mServerStatus==WebsocketServerStatus.Online){this.EnqueueIncoming(new NetworkEvent(NetEventType.ServerClosed,ConnectionId.INVALID,null))}else if(this.mServerStatus==WebsocketServerStatus.ShuttingDown){this.EnqueueIncoming(new NetworkEvent(NetEventType.ServerClosed,ConnectionId.INVALID,null))}this.mServerStatus=WebsocketServerStatus.Offline;this.mOutgoingQueue=new Array;this.WebsocketCleanup();this.mStatus=WebsocketConnectionStatus.NotConnected};e.prototype.EnqueueOutgoing=function(e){this.mOutgoingQueue.push(e)};e.prototype.EnqueueIncoming=function(e){this.mIncomingQueue.push(e)};e.prototype.TryRemoveConnecting=function(e){var t=this.mConnecting.indexOf(e.id);if(t!=-1){this.mConnecting.splice(t,1)}};e.prototype.TryRemoveConnection=function(e){var t=this.mConnections.indexOf(e.id);if(t!=-1){this.mConnections.splice(t,1)}};e.prototype.HandleIncomingEvent=function(e){if(e.Type==NetEventType.NewConnection){this.TryRemoveConnecting(e.ConnectionId);this.mConnections.push(e.ConnectionId.id)}else if(e.Type==NetEventType.ConnectionFailed){this.TryRemoveConnecting(e.ConnectionId)}else if(e.Type==NetEventType.Disconnected){this.TryRemoveConnection(e.ConnectionId)}else if(e.Type==NetEventType.ServerInitialized){this.mServerStatus=WebsocketServerStatus.Online}else if(e.Type==NetEventType.ServerInitFailed){this.mServerStatus=WebsocketServerStatus.Offline}else if(e.Type==NetEventType.ServerClosed){this.mServerStatus=WebsocketServerStatus.ShuttingDown;this.mServerStatus=WebsocketServerStatus.Offline}this.EnqueueIncoming(e)};e.prototype.HandleOutgoingEvents=function(){while(this.mOutgoingQueue.length>0){var e=this.mOutgoingQueue.shift();var t=NetworkEvent.toByteArray(e);this.mSocket.send(t)}};e.prototype.NextConnectionId=function(){var e=this.mNextOutgoingConnectionId;this.mNextOutgoingConnectionId=new ConnectionId(this.mNextOutgoingConnectionId.id+1);return e};e.prototype.GetRandomKey=function(){var e=\"\";for(var t=0;t<7;t++){e+=String.fromCharCode(65+Math.round(Math.random()*25))}return e};e.prototype.Dequeue=function(){if(this.mIncomingQueue.length>0)return this.mIncomingQueue.shift();return null};e.prototype.Peek=function(){if(this.mIncomingQueue.length>0)return this.mIncomingQueue[0];return null};e.prototype.Update=function(){this.CheckSleep()};e.prototype.Flush=function(){if(this.mStatus==WebsocketConnectionStatus.Connected)this.HandleOutgoingEvents()};e.prototype.SendData=function(e,t,n){if(e==null||t==null||t.length==0)return;var i;if(n){i=new NetworkEvent(NetEventType.ReliableMessageReceived,e,t)}else{i=new NetworkEvent(NetEventType.UnreliableMessageReceived,e,t)}this.EnqueueOutgoing(i)};e.prototype.Disconnect=function(e){var t=new NetworkEvent(NetEventType.Disconnected,e,null);this.EnqueueOutgoing(t)};e.prototype.Shutdown=function(){this.Cleanup();this.mStatus=WebsocketConnectionStatus.NotConnected};e.prototype.Dispose=function(){if(this.mIsDisposed==false){this.Shutdown();this.mIsDisposed=true}};e.prototype.StartServer=function(e){if(e==null){e=\"\"+this.GetRandomKey()}if(this.mServerStatus==WebsocketServerStatus.Offline){this.EnsureServerConnection();this.mServerStatus=WebsocketServerStatus.Starting;this.EnqueueOutgoing(new NetworkEvent(NetEventType.ServerInitialized,ConnectionId.INVALID,e))}else{this.EnqueueIncoming(new NetworkEvent(NetEventType.ServerInitFailed,ConnectionId.INVALID,e))}};e.prototype.StopServer=function(){this.EnqueueOutgoing(new NetworkEvent(NetEventType.ServerClosed,ConnectionId.INVALID,null))};e.prototype.Connect=function(e){this.EnsureServerConnection();var t=this.NextConnectionId();this.mConnecting.push(t.id);var n=new NetworkEvent(NetEventType.NewConnection,t,e);this.EnqueueOutgoing(n);return t};return e}();function bufferToString(e){var t=new Uint16Array(e.buffer,e.byteOffset,e.byteLength/2);return String.fromCharCode.apply(null,t)}function stringToBuffer(e){var t=new ArrayBuffer(e.length*2);var n=new Uint16Array(t);for(var i=0,o=e.length;i<o;i++){n[i]=e.charCodeAt(i)}var r=new Uint8Array(t);return r}function WebsocketNetwork_test1(){console.log(\"test1\");var e=\"test1234\";var t=true;var n=false;var i;var o;if(window.location.protocol!=\"https:\"&&n){i=\"wss://because-why-not.com:12776\";if(t)i=\"ws://localhost:12776\"}else{i=\"wss://because-why-not.com:12777\";if(t)i=\"wss://localhost:12777\"}var r=new WebsocketNetwork(i);r.StartServer();var a=new WebsocketNetwork(i);setInterval(function(){r.Update();var t=null;while(t=r.Dequeue()){console.log(\"server inc: \"+t.toString());if(t.Type==NetEventType.ServerInitialized){console.log(\"server started. Address \"+t.Info);a.Connect(t.Info)}else if(t.Type==NetEventType.ServerInitFailed){console.error(\"server start failed\")}else if(t.Type==NetEventType.NewConnection){console.log(\"server new incoming connection\")}else if(t.Type==NetEventType.Disconnected){console.log(\"server peer disconnected\");console.log(\"server shutdown\");r.Shutdown()}else if(t.Type==NetEventType.ReliableMessageReceived){r.SendData(t.ConnectionId,t.MessageData,true)}else if(t.Type==NetEventType.UnreliableMessageReceived){r.SendData(t.ConnectionId,t.MessageData,false)}}r.Flush();a.Update();while(t=a.Dequeue()){console.log(\"client inc: \"+t.toString());if(t.Type==NetEventType.NewConnection){console.log(\"client connection established\");var n=stringToBuffer(e);a.SendData(t.ConnectionId,n,true)}else if(t.Type==NetEventType.ReliableMessageReceived){var i=bufferToString(t.MessageData);if(i!=e){console.error(\"Test failed sent string %s but received string %s\",e,i)}var n=stringToBuffer(e);a.SendData(t.ConnectionId,n,false)}else if(t.Type==NetEventType.UnreliableMessageReceived){var i=bufferToString(t.MessageData);if(i!=e){console.error(\"Test failed sent string %s but received string %s\",e,i)}console.log(\"client disconnecting\");a.Disconnect(t.ConnectionId);console.log(\"client shutting down\");a.Shutdown()}}a.Flush()},100)}");
					//txt.text);
                jsCode.Append("console.log('completed eval webrtcnetworkplugin!');");
                ExternalEval(jsCode.ToString());
            }
        }

        protected static void ExternalEval(string jscode)
        {
#if UNITY_5_6_OR_NEWER
            //work around. Starting unity 5.6 the ExternalEval will run the code in a local
            //scope making accessing it later impossible.
            //This abomination will run eval in a global scope
            Application.ExternalCall("(1, eval)", jscode);
#else
            Application.ExternalEval(jscode);
#endif

        }

        /// <summary>
        /// Will return true if the environment supports the WebRTCNetwork plugin
        /// (needs to run in Chrome or Firefox + the javascript file needs to be loaded in the html page!)
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool IsAvailable()
        {
            try
            {
                return UnityWebRtcNetworkIsAvailable();
            }catch(EntryPointNotFoundException)
            {
                //not available at all
                return false;
            }
        }


        protected int mReference = -1;

        /// <summary>
        /// Returns true if the server is running or the client is connected.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                if (mIsServer)
                    return true;

                if (mConnections.Count > 0)
                    return true;
                return false;
            }
        }

        private bool mIsServer = false;
        /// <summary>
        /// True if the server is running allowing incoming connections
        /// </summary>
        public bool IsServer
        {
            get { return mIsServer; }
        }



        

        private List<ConnectionId> mConnections = new List<ConnectionId>();



        private int[] mTypeidBuffer = new int[1];
        private int[] mConidBuffer = new int[1];
        private int[] mDataWrittenLenBuffer = new int[1];

        private Queue<NetworkEvent> mEvents = new Queue<NetworkEvent>();
        

        /// <summary>
        /// Creates a new network by using a JSON configuration string. This is used to configure the server connection for the signaling channel
        /// and to define webrtc specific configurations such as stun server used to connect through firewalls.
        /// 
        /// 
        /// </summary>
        /// <param name="config"></param>
        public BrowserWebRtcNetwork(string websocketUrl, IceServer[] lIceServers)
        {
            string conf = ConstructorParamToJson(websocketUrl, lIceServers);
            SLog.L("Creating BrowserWebRtcNetwork config: " + conf, this.GetType().Name);
            mReference = UnityWebRtcNetworkCreate(conf);
        }


        protected static void IceServersToJson(IceServer[] lIceServers, StringBuilder iceServersJson)
        {
            if (lIceServers == null)
            {
                iceServersJson.Append("null");
            }
            else if (lIceServers.Length == 0)
            {
                iceServersJson.Append("[]");
            }
            else
            {

                iceServersJson.Append("["); //start iceServers array
                for (int i = 0; i < lIceServers.Length; i++)
                {
                    if (i > 0)
                    {
                        iceServersJson.Append(",");
                    }
                    iceServersJson.Append("{"); // start iceServers[i] object


                    //urls field is an array of strings for each url iceServers[i].urls
                    iceServersJson.Append("\"");
                    iceServersJson.Append("urls");
                    iceServersJson.Append("\"");
                    iceServersJson.Append(":");
                    if (lIceServers[i].Urls == null)
                    {
                        iceServersJson.Append("null");
                    }
                    else if (lIceServers[i].Urls.Count == 0)
                    {
                        iceServersJson.Append("[]");
                    }
                    else
                    {
                        iceServersJson.Append("[");
                        for (int k = 0; k < lIceServers[i].Urls.Count; k++)
                        {
                            if (k > 0)
                            {
                                iceServersJson.Append(",");
                            }
                            iceServersJson.Append("\"");
                            iceServersJson.Append(lIceServers[i].Urls[k]);
                            iceServersJson.Append("\"");
                        }
                        iceServersJson.Append("]");//end iceServers[i].urls array
                    }

                    if (lIceServers[i].Username != null)
                    {
                        iceServersJson.Append(",");
                        iceServersJson.Append("\"");
                        iceServersJson.Append("username");
                        iceServersJson.Append("\"");
                        iceServersJson.Append(":");
                        iceServersJson.Append("\"");
                        iceServersJson.Append(lIceServers[i].Username);
                        iceServersJson.Append("\"");
                    }
                    if (lIceServers[i].Credential != null)
                    {
                        iceServersJson.Append(",");
                        iceServersJson.Append("\"");
                        iceServersJson.Append("credential");
                        iceServersJson.Append("\"");
                        iceServersJson.Append(":");
                        iceServersJson.Append("\"");
                        iceServersJson.Append(lIceServers[i].Credential);
                        iceServersJson.Append("\"");
                    }

                    iceServersJson.Append("}");// end iceServers[i] object
                }
                iceServersJson.Append("]"); // end iceServers array
            }
        }

        /// <summary>
        /// Returns a json object used to initialize the js side of this class.
        /// 
        /// Result should look like this:
        /// { "signaling" :  { "class": "WebsocketNetwork", "param" : "ws://because-why-not.com:12776/chatapp"}, "iceServers":[{"urls":["turn:because-why-not.com:12779"],"username":"testuser13","credential":"testpassword"}]}
        /// </summary>
        /// <param name="websocketUrl"></param>
        /// <param name="lIceServers"></param>
        /// <returns></returns>
        private static string ConstructorParamToJson(string websocketUrl, IceServer[] lIceServers)
        {
            StringBuilder iceServersJson = new StringBuilder();
            IceServersToJson(lIceServers, iceServersJson);

            //string websocketUrl = "ws://localhost:12776";
            string conf;
            if (websocketUrl == null)
            {
                //use LocalNetwork to simulate a program wide signaling network (used for unit tests)
                conf = "{ \"signaling\" :  { \"class\": \"LocalNetwork\", \"param\" : null}, \"iceServers\":" + iceServersJson + "}";
            }
            else
            {
                //create the js class WebsocketNetwork and use the url as parameter
                conf = "{ \"signaling\" :  { \"class\": \"WebsocketNetwork\", \"param\" : \"" + websocketUrl + "\"}, \"iceServers\":" + iceServersJson + "}";
            }
            return conf;
        }

        /// <summary>
        /// For subclasses that provide their own init process
        /// </summary>
        protected BrowserWebRtcNetwork()
        {

        }

        /// <summary>
        /// Destructor to make sure everything gets disposed. Sadly, WebGL doesn't seem to call this ever.
        /// </summary>
        ~BrowserWebRtcNetwork()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the underlaying java script library. If you have long running systems that don't reuse instances make sure
        /// you always call dispose as unity doesn't seem to call destructors reliably. You might fill up your java script
        /// memory with lots of unused instances.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            //just to follow the pretty dispose pattern
            if (disposing)
            {
                if (mReference != -1)
                {
                    UnityWebRtcNetworkRelease(mReference);
                    mReference = -1;
                }
            }
            else
            {
                if (mReference != -1)
                    UnityWebRtcNetworkRelease(mReference);
            }
        }
        
        /// <summary>
        /// Starts a server using a random number as address/name.
        /// 
        /// Read the ServerInitialized events Info property to get the address name.
        /// </summary>
        public void StartServer()
        {
            StartServer("" + UnityEngine.Random.Range(0, 16777216));
        }

        /// <summary>
        /// Allows to listen to incoming connections using a given name/address.
        /// 
        /// This is in addition to the definition of the IBaseNetwork interface which is
        /// shared with other network systems enforcing the use of ip:port as address, thus
        /// can't allow custom addresses.
        /// </summary>
        /// <param name="name">Name/Address can be any kind of string. There might be restrictions though depending
        /// on the underlaying signaling channel.
        /// An invalid name will result in an InitFailed event being return in Dequeue.</param>
        public void StartServer(string name)
        {
            if (this.mIsServer == true)
            {
                UnityEngine.Debug.LogError("Already in server mode.");
                return;
            }
            UnityWebRtcNetworkStartServer(mReference, name);
        }

        public void StopServer()
        {
            UnityWebRtcNetworkStopServer(mReference);
        }


        /// <summary>
        /// Connects to the given name or address.
        /// </summary>
        /// <param name="name"> The address identifying the server  </param>
        /// <returns>
        /// The connection id. (WebRTCNetwork doesn't allow multiple connections yet! So you can ignore this for now)
        /// </returns>
        public ConnectionId Connect(string name)
        {
            //until fully supported -> block connecting to others while running a server
            if (this.mIsServer == true)
            {
                UnityEngine.Debug.LogError("Can't create outgoing connections while in server mode!");
                return ConnectionId.INVALID;
            }

            ConnectionId id = new ConnectionId();
            id.id = (short)UnityWebRtcNetworkConnect(mReference, name);
            return id;
        }


        /// <summary>
        /// Retrieves an event from the js library, handles it internally and then adds it to a queue for delivery to the user.
        /// </summary>
        /// <param name="evt"> The new network event or an empty struct if none is found.</param>
        /// <returns>True if event found, false if no events queued.</returns>
        private bool DequeueInternal(out NetworkEvent evt)
        {
            int length = UnityWebRtcNetworkPeekEventDataLength(mReference);
            if(length == -1) //-1 == no event available
            {
                evt = new NetworkEvent();
                return false;
            }
            else
            {
                ByteArrayBuffer buf = ByteArrayBuffer.Get(length);
                bool eventFound = UnityWebRtcNetworkDequeue(mReference, mTypeidBuffer, mConidBuffer, buf.array, 0, buf.array.Length, mDataWrittenLenBuffer);
                //set the write correctly
                buf.PositionWriteRelative = mDataWrittenLenBuffer[0];

                NetEventType type = (NetEventType)mTypeidBuffer[0];
                ConnectionId id;
                id.id = (short)mConidBuffer[0];
                object data = null;

                if (buf.PositionWriteRelative == 0 || buf.PositionWriteRelative == -1) //no data
                {
                    data = null;
                    //was an empty buffer -> release it and 
                    buf.Dispose();
                }
                else if (type == NetEventType.ReliableMessageReceived || type == NetEventType.UnreliableMessageReceived)
                {
                    //store the data for the user to use
                    data = buf;
                }
                else
                {
                    //non data message with data attached -> can only be a string
                    string stringData = Encoding.ASCII.GetString(buf.array, 0, buf.PositionWriteRelative);
                    data = stringData;
                    buf.Dispose();

                }


                evt = new NetworkEvent(type, id, data);
                UnityEngine.Debug.Log("event" + type + " received");
                HandleEventInternally(ref evt);
                return eventFound;
            }

        }

        /// <summary>
        /// Handles events internally. Needed to change the internal states: Server flag and connection id list.
        /// 
        /// Would be better to remove that in the future from the main library and treat it separately. 
        /// </summary>
        /// <param name="evt"> event to handle </param>
        private void HandleEventInternally(ref NetworkEvent evt)
        {
            if(evt.Type == NetEventType.NewConnection)
            {
                mConnections.Add(evt.ConnectionId);
            }else if(evt.Type == NetEventType.Disconnected)
            {
                mConnections.Remove(evt.ConnectionId);
            }else if(evt.Type == NetEventType.ServerInitialized)
            {
                mIsServer = true;
            }
            else if (evt.Type == NetEventType.ServerClosed || evt.Type == NetEventType.ServerInitFailed)
            {
                mIsServer = false;
            }
        }

        /// <summary>
        /// Sends a byte array
        /// </summary>
        /// <param name="conId">Connection id the message should be delivered to.</param>
        /// <param name="data">Content/Buffer that contains the content</param>
        /// <param name="offset">Start index of the content in data</param>
        /// <param name="length">Length of the content in data</param>
        /// <param name="reliable">True to use the ordered, reliable transfer, false for unordered and unreliable</param>
        public void SendData(ConnectionId conId, byte[] data, int offset, int length, bool reliable)
        {
            UnityWebRtcNetworkSendData(mReference, conId.id, data, offset, length, reliable);
        }

        /// <summary>
        /// Shuts webrtc down. All connection will be disconnected + if the server is started it will be stopped.
        /// 
        /// The instance itself isn't released yet! Use Dispose to destroy the network entirely.
        /// </summary>
        public void Shutdown()
        {
            UnityWebRtcNetworkShutdown(mReference);
        }

        /// <summary>
        /// Dequeues a new event
        /// </summary>
        /// <param name="evt"></param>
        /// <returns></returns>
        public bool Dequeue(out NetworkEvent evt)
        {
            evt = new NetworkEvent();
            if (mEvents.Count == 0)
                return false;

            evt = mEvents.Dequeue();
            return true;
        }

        public bool Peek(out NetworkEvent evt)
        {
            evt = new NetworkEvent();
            if (mEvents.Count == 0)
                return false;

            evt = mEvents.Peek();
            return true;
        }

        /// <summary>
        /// Needs to be called to read data from the underlaying network and update this class.
        /// 
        /// Use Dequeue to get the events it read.
        /// </summary>
        public virtual void Update()
        {
            UnityWebRtcNetworkUpdate(mReference);
            
            NetworkEvent ev = new NetworkEvent();

            //DequeueInternal will read the message from js, change the state of this object
            //e.g. if a server is successfully opened it will set mIsServer to true
            while(DequeueInternal(out ev))
            {
                //add it for delivery to the user
                mEvents.Enqueue(ev);
            }
        }

        /// <summary>
        /// Flushes messages. Not needed in WebRtcNetwork but use it at the end of a frame 
        /// if you want to be able to replace WebRtcNetwork with other implementations
        /// </summary>
        public void Flush()
        {
            UnityWebRtcNetworkFlush(mReference);
        }

        /// <summary>
        /// Disconnects the given connection id.
        /// </summary>
        /// <param name="id">Id to disconnect</param>
        public void Disconnect(ConnectionId id)
        {
            UnityWebRtcNetworkDisconnect(mReference, id.id);
        }

    }
}
