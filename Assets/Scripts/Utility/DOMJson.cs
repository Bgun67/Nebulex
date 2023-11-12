// Copyright Michael Gunther
// Distributed under the Unity EULA
// Use of this code for commercial and non-commercial uses must be purchased through the unity asset store
//#define DEBUG_CMD

using System;
using System.Collections.Generic;
#if DEBUG_CMD
using UnityEngine;
#endif

namespace DOMJson
{
    public class InvalidJsonKeyException : System.Exception{
        public InvalidJsonKeyException() : base() {}
        public InvalidJsonKeyException(string message) : base(message) {}
        public InvalidJsonKeyException(string message, Exception inner) : base(message, inner) {}
    }
    public class InvalidJsonTypeException : System.Exception{
        public InvalidJsonTypeException() : base() {}
        public InvalidJsonTypeException(string message) : base(message) {}
        public InvalidJsonTypeException(string message, Exception inner) : base(message, inner) {}
    }
    public class JsonSyntaxException : System.Exception{
        public JsonSyntaxException() : base() {}
        public JsonSyntaxException(string message) : base(message) {}
        public JsonSyntaxException(string message, Exception inner) : base(message, inner) {}
    }
    
    public class JsonObject{
        private bool hasObjects = false;
        private Dictionary<string, JsonObject> m_DictObjects = new Dictionary<string, JsonObject>();
        private bool hasListObjects = false;
        private List<JsonObject> m_ListObjects = new List<JsonObject>();

        private bool hasString = false;
        private string m_StringVal;

        private bool hasFloating = false;
        private double m_DoubleVal;

        private bool hasBool = false;
        private bool m_BoolVal;

        public string ToJson(){
            return this.ToString();
        }
        public override string ToString(){
            return ToString(-1);
        }
        public string ToString(int numTabs)
        {
            if (hasString){
                return String.Format("\"{0}\"", m_StringVal);
            }
            else if (hasFloating){
                return m_DoubleVal.ToString();
            }
            else if (hasBool){
                return m_BoolVal ? "true" : "false";
            }
            else if (hasListObjects)
                return ParserUtils.Stringify(m_ListObjects, numTabs + 1);
            else if (hasObjects){
                return ParserUtils.Stringify(m_DictObjects, numTabs + 1);
            }
            else
                return "{}";
        }

        /////////////////////////////////////////////////////////////
        ///Casts from JsonObject
        /////////////////////////////////////////////////////////////
        public static implicit operator string(JsonObject jsonObject){
            if (jsonObject.hasString)
                return jsonObject.m_StringVal;
            else
                throw new InvalidJsonTypeException("Object cannot be converted to type 'string'");
        }
        public static implicit operator int(JsonObject jsonObject){
            if (jsonObject.hasFloating)
                return (int)Math.Round(jsonObject.m_DoubleVal);
            else
                throw new InvalidJsonTypeException("Object cannot be converted to type 'int'");
        }

        public static implicit operator float(JsonObject jsonObject){
            if (jsonObject.hasFloating)
                return (float)jsonObject.m_DoubleVal;
            else
                throw new InvalidJsonTypeException("Object cannot be converted to type 'float'");
        }
        public static implicit operator double(JsonObject jsonObject){
            if (jsonObject.hasFloating)
                return (double)jsonObject.m_DoubleVal;
            else
                throw new InvalidJsonTypeException("Object cannot be converted to type 'double'");
        }

        public static implicit operator bool(JsonObject jsonObject){
            if (jsonObject.hasBool)
                return jsonObject.m_BoolVal;
            else
                throw new InvalidJsonTypeException("Object cannot be converted to type 'bool'");
        }

        public static implicit operator Dictionary<string, JsonObject>(JsonObject jsonObject){
            if (jsonObject.hasObjects)
                return jsonObject.m_DictObjects;
            else
                throw new InvalidJsonTypeException("Object cannot be converted to type 'dictionary'");
        }

        public static implicit operator List<JsonObject>(JsonObject jsonObject){
            if (jsonObject.hasListObjects)
                return jsonObject.m_ListObjects;
            else
                throw new InvalidJsonTypeException("Object cannot be converted to type 'list'");
        }

        public Dictionary<string, JsonObject> AsDict(){
            if (this.hasObjects)
                return this.m_DictObjects;
            else
                throw new InvalidJsonTypeException("Object cannot be converted to type 'dictionary'");
        }

        public List<JsonObject> AsList(){
            if (this.hasListObjects)
                return this.m_ListObjects;
            else
                throw new InvalidJsonTypeException("Object cannot be converted to type 'list'");
        }



        /////////////////////////////////////////////////////////////
        ///Casts to JsonObject
        /////////////////////////////////////////////////////////////
        public static implicit operator JsonObject(float val){
            return new JsonObject(val);
        }
        public static implicit operator JsonObject(double val){
            return new JsonObject(val);
        }
        public static implicit operator JsonObject(int val){
            return new JsonObject(val);
        }
        public static implicit operator JsonObject(bool val){
            return new JsonObject(val);
        }
        public static implicit operator JsonObject(string val){
            return new JsonObject(val);
        }

        public JsonObject Get(string key, JsonObject defaultValue){
            if(m_DictObjects.ContainsKey(key)){
                return this[key];
            }
            else{
                return defaultValue;
            }
        }

        public JsonObject this[string key]
        {
            get{
                if(m_DictObjects.ContainsKey(key)){
                    return m_DictObjects[key];
                }
                else{
                    throw new InvalidJsonKeyException("Key not present: " + key);
                }
            }
            set{
                if(m_DictObjects.ContainsKey(key)){
                    m_DictObjects[key] = value;
                    hasObjects = true;
                }else{
                    m_DictObjects.Add(key, value);
                    hasObjects = true;
                }
            }
        }
        public JsonObject this[int index]
        {
            get{
                if(m_ListObjects.Count < index){
                    return m_ListObjects[index];
                }
                else{
                    throw new InvalidJsonKeyException("Index not present: " + index);
                }
            }
            set{
                if(m_ListObjects.Count < index){
                    m_ListObjects[index] = value;
                    hasListObjects = true;
                }else{
                    for (int i = m_ListObjects.Count; i <= index; i++){
                        m_ListObjects.Add(value);
                        hasListObjects = true;
                    }
                }
            }
        }

        public JsonObject(){
        }
        private JsonObject(int val){
            this.hasFloating = true;
            this.m_DoubleVal = val;
        }
        private JsonObject(float val){
            this.hasFloating = true;
            this.m_DoubleVal = val;
        }
        private JsonObject(double val){
            this.hasFloating = true;
            this.m_DoubleVal = val;
        }
        private JsonObject(string val){
            this.hasString = true;
            this.m_StringVal = val;
        }
        private JsonObject(bool val){
            this.hasBool = true;
            this.m_BoolVal = val;
        }
        public static JsonObject FromDict(Dictionary<string, string> val){
            JsonObject jsonObject = new JsonObject();
            foreach (string key in val.Keys){
                jsonObject.hasObjects = true;
                jsonObject[key] = val[key];
            }
            return jsonObject;
        }
        public static JsonObject FromDict(Dictionary<string, int> val){
            JsonObject jsonObject = new JsonObject();
            foreach (string key in val.Keys){
                jsonObject.hasObjects = true;
                jsonObject[key] = val[key];
            }
            return jsonObject;
        }
        public static JsonObject FromDict(Dictionary<string, float> val){
            JsonObject jsonObject = new JsonObject();
            foreach (string key in val.Keys){
                jsonObject.hasObjects = true;
                jsonObject[key] = val[key];
            }
            return jsonObject;
        }
        public static JsonObject FromDict(Dictionary<string, double> val){
            JsonObject jsonObject = new JsonObject();
            foreach (string key in val.Keys){
                jsonObject.hasObjects = true;
                jsonObject[key] = val[key];
            }
            return jsonObject;
        }
        public static JsonObject FromDict(Dictionary<string, bool> val){
            JsonObject jsonObject = new JsonObject();
            foreach (string key in val.Keys){
                jsonObject.hasObjects = true;
                jsonObject[key] = val[key];
            }
            return jsonObject;
        }

        public static JsonObject FromList(List<string> vals){
            JsonObject jsonObject = new JsonObject();
            for(int i = 0; i< vals.Count; i++){
                jsonObject.hasListObjects = true;
                jsonObject[i] = vals[i];
            }
            return jsonObject;
        }
        public static JsonObject FromList(List<int> vals){
            JsonObject jsonObject = new JsonObject();
            for(int i = 0; i< vals.Count; i++){
                jsonObject.hasListObjects = true;
                jsonObject[i] = vals[i];
            }
            return jsonObject;
        }
        public static JsonObject FromList(List<float> vals){
            JsonObject jsonObject = new JsonObject();
            for(int i = 0; i< vals.Count; i++){
                jsonObject.hasListObjects = true;
                jsonObject[i] = vals[i];
            }
            return jsonObject;
        }
        public static JsonObject FromList(List<double> vals){
            JsonObject jsonObject = new JsonObject();
            for(int i = 0; i< vals.Count; i++){
                jsonObject.hasListObjects = true;
                jsonObject[i] = vals[i];
            }
            return jsonObject;
        }
        public static JsonObject FromList(List<bool> vals){
            JsonObject jsonObject = new JsonObject();
            for(int i = 0; i< vals.Count; i++){
                jsonObject.hasListObjects = true;
                jsonObject[i] = vals[i];
            }
            return jsonObject;
        }

        public static JsonObject FromJson(string contents){
            JsonObject self = new JsonObject();
            contents = contents.Trim(new char[]{' ', '\t', '\n', '\r'});

            int stringStart = -1;
            bool isQuoteOpen = false;
            string currentKey = "";

            bool isValQuoteOpen = false;

            bool isJsonOpen = false;
            int jsonStart = -1;

            int openBrackets = 0;
            int openSquareBrackets = 0;

            if (contents.StartsWith("{")){
                self.hasObjects = true;
                #if DEBUG_CMD
                string cmd = "";
                #endif
                for (int i = 1; i < contents.Length - 1; i++){
                    #if DEBUG_CMD
                    //Debug.Log(cmd);
                    cmd += contents[i].ToString();
                    #endif
                    if (contents[i] == '"' && !(i > 0 && contents[i-1] == '\\') && openBrackets <= 0 && !isJsonOpen){
                        if (!isQuoteOpen){
                            isQuoteOpen = true;
                            stringStart = i + 1;
                            #if DEBUG_CMD
                            Debug.LogWarning("Quote Open");
                            #endif
                        }
                        else{
                            isQuoteOpen = false;
                            currentKey = contents.Substring(stringStart, i-stringStart);
                            #if DEBUG_CMD
                            Debug.LogWarning("Quote Close");
                            #endif
                        }
                        continue;
                    }
                    if (contents[i] == '"' && !(i > 0 && contents[i-1] == '\\'))
                        isValQuoteOpen = !isValQuoteOpen;

                    if (isQuoteOpen || isValQuoteOpen)
                        continue;
                    
                    switch (contents[i]){
                        case '{':
                            openBrackets += 1;
                            #if DEBUG_CMD
                            Debug.LogWarning("Open Bracket");
                            #endif
                            break;
                        case '}':
                            openBrackets -= 1;
                            #if DEBUG_CMD
                            Debug.LogWarning("Close Bracket");
                            #endif
                            if (openBrackets < 0){
                                throw new JsonSyntaxException("Syntax error,  extra '}'");
                            }
                            break;
                        case '[':
                            openSquareBrackets += 1;
                            #if DEBUG_CMD
                            Debug.LogWarning("Open Square");
                            #endif
                            break;
                        case ']':
                            openSquareBrackets -= 1;
                            #if DEBUG_CMD
                            Debug.LogWarning("Close Square");
                            #endif
                            if (openSquareBrackets < 0){
                                throw new JsonSyntaxException("Syntax error,  extra ']'");
                            }
                            break;
                        case ':':
                            if (openBrackets > 0 || openSquareBrackets > 0)
                                continue;
                            jsonStart = i + 1;
                            isJsonOpen = true;
                            #if DEBUG_CMD
                            Debug.LogWarning("Open Value");
                            #endif
                            break;
                        case ',':
                            if (openBrackets > 0|| openSquareBrackets > 0)
                                continue;
                            string subContents = contents.Substring(jsonStart, i-jsonStart);
                            isJsonOpen = false;
                            #if DEBUG_CMD
                            Debug.LogWarning("Close Value");
                            #endif
                            self.m_DictObjects.Add(currentKey, JsonObject.FromJson(subContents));
                            break;
                        default:
                            continue;
                    }
                }
                if(isJsonOpen){
                    string subContents = contents.Substring(jsonStart, contents.Length-1-jsonStart);
                    isJsonOpen = false;
                    #if DEBUG_CMD
                    Debug.Log("Forming Sub contents: " + subContents);
                    #endif
                    self.m_DictObjects.Add(currentKey, JsonObject.FromJson(subContents));
                    #if DEBUG_CMD
                    Debug.LogWarning("Close Value");
                    #endif
                }
                #if DEBUG_CMD
                Debug.Log(cmd);
                #endif
            }
            else if(contents.StartsWith('"')){
                self.hasString = true;
                //Remove quote marks
                self.m_StringVal = contents.Substring(1, contents.Length - 2);
                #if DEBUG_CMD
                Debug.Log("Forming String: " + contents.Trim());
                #endif
            }
            else if (contents.StartsWith("[")){
                #if DEBUG_CMD
                Debug.Log("List: " + contents);
                string cmd = "";
                #endif
                self.hasListObjects = true;
                isJsonOpen = true;
                jsonStart = 1;
                for (int i = 1; i < contents.Length - 1; i++){
                    #if DEBUG_CMD
                    Debug.Log(cmd);
                    cmd = contents[i].ToString();
                    #endif
                    if (contents[i] == '"' && !(i > 0 && contents[i-1] == '\\') && openBrackets <= 0 && !isJsonOpen){
                        if (!isQuoteOpen){
                            isQuoteOpen = true;
                            stringStart = i + 1;
                            #if DEBUG_CMD
                            Debug.LogWarning("Quote Open");
                            #endif
                        }
                        else{
                            isQuoteOpen = false;
                            #if DEBUG_CMD
                            Debug.LogWarning("Quote Close");
                            #endif
                        }
                        continue;
                    }

                    if (contents[i] == '"' && !(i > 0 && contents[i-1] == '\\'))
                        isValQuoteOpen = !isValQuoteOpen;

                    if (isQuoteOpen || isValQuoteOpen)
                        continue;
                    
                    switch (contents[i]){
                        case '{':
                            openBrackets += 1;
                            #if DEBUG_CMD
                            Debug.LogWarning("Open Bracket");
                            #endif
                            break;
                        case '}':
                            openBrackets -= 1;
                            #if DEBUG_CMD
                            Debug.LogWarning("Close Bracket");
                            #endif
                            if (openBrackets < 0){
                                throw new JsonSyntaxException("Syntax error,  extra '}'");
                            }
                            break;
                        case '[':
                            openSquareBrackets += 1;
                            #if DEBUG_CMD
                            Debug.LogWarning("Open Square");
                            #endif
                            break;
                        case ']':
                            openSquareBrackets -= 1;
                            #if DEBUG_CMD
                            Debug.LogWarning("Close Square");
                            #endif
                            if (openSquareBrackets < 0){
                                throw new JsonSyntaxException("Syntax error, extra ']'");
                            }
                            break;
                        case ',':
                            if (openBrackets > 0 || openSquareBrackets > 0)
                                continue;
                            string subContents = contents.Substring(jsonStart, i-jsonStart);
                            jsonStart = i + 1;
                            #if DEBUG_CMD
                            Debug.LogWarning("Close Value");
                            Debug.Log("Forming Array Sub contents: " + subContents);
                            #endif
                            self.m_ListObjects.Add(JsonObject.FromJson(subContents));
                            break;
                        default:
                            continue;
                    }
                }
                if(isJsonOpen){
                    string subContents = contents.Substring(jsonStart, contents.Length-1-jsonStart);
                    isJsonOpen = false;
                    #if DEBUG_CMD
                    Debug.Log("Forming Sub contents: " + subContents);
                    #endif
                    self.m_ListObjects.Add(JsonObject.FromJson(subContents));
                    #if DEBUG_CMD
                    Debug.LogWarning("Close Value");
                    #endif
                }
                #if DEBUG_CMD
                Debug.Log(cmd);
                #endif
            }
            else{
                if (double.TryParse(contents, out self.m_DoubleVal)){
                    self.hasFloating = true;
                    #if DEBUG_CMD
                    Debug.Log("Forming Double: " + contents);
                    #endif
                }
                else if (bool.TryParse(contents, out self.m_BoolVal)){
                    self.hasBool = true;
                    #if DEBUG_CMD
                    Debug.Log("Forming Bool: " + contents);
                    #endif
                }
                else{
                    throw new JsonSyntaxException("Syntax error, unknown");
                    #if DEBUG_CMD
                    Debug.Log("Invalid item: " + contents);
                    #endif
                }
            }
            return self;
        }
    }



    public class ParserUtils{


        public static bool __test__ (){
            string jsonString = 
            @"
            {
                ""hello"": ""okay"",
                ""goodbye"": [10]
            }
            ";

            //jsonString = @"{""hello"":""okay"",""goodbye"":""see ya""}";
            var obj = JsonObject.FromJson(jsonString);
            //Debug.Log(Stringify<List<JsonObject>>(obj["goodbye"])));

            string stringObj = obj.ToJson();
            #if DEBUG_CMD
            Debug.Log(stringObj);
            #endif
            
            return true;
        }


        public static string Stringify(Dictionary<string, JsonObject> dict, int numTabs = 0){
            string tabs = "";
            for (int i =0; i<numTabs; i++){
                tabs += "\t";
            }

            string str = "{\r\n";

            string joinStr = "";

            foreach(string key in dict.Keys){
                str += joinStr;
                str += tabs + "\t" + "\"" + key + "\": " + dict[key].ToString(numTabs);
                joinStr = ",\r\n";
            }
            str += "\r\n" + tabs + "}";
            return str;
        }
        public static string Stringify(List<JsonObject> list, int numTabs = 0){
            string tabs = "";
            for (int i =0; i<numTabs; i++){
                tabs += "\t";
            }
            string str = "[\r\n";

            string joinStr = "";
            foreach(JsonObject item in list){
                str += joinStr;
                str += tabs + "\t" + item.ToString(numTabs);
                joinStr = ",\r\n";
            }
            str += "\r\n" + tabs + "]";
            return str;
        }

    }
}