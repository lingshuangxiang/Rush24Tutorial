using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Web {
    public class Http : MonoBehaviour {
        #region Singleton

        public static bool debug = false;

        static Http instance;

        static Http Instance {
            get {
                if (instance == null) {
                    if(FindObjectOfType<Http>() == null) {
                        instance = new GameObject().AddComponent<Http>();
                        instance.gameObject.name = "Http";
                    } else {
                        instance = FindObjectOfType<Http>();
                    }
                }
                try {
                    DontDestroyOnLoad(instance.gameObject);
                } catch {

                }
                return instance;
            }
        }

        #endregion

        #region Local

        private void Update () {
            foreach (var request in requests) {
                float progress = request.Value.progress;
            }
        }

        Dictionary<string, string> globalHeaders = new Dictionary<string, string>();
        Dictionary<string, HttpRequest> requests = new Dictionary<string, HttpRequest>();

        static void _removeFromDict (string guid) {
            Instance.requests.Remove(guid);
        }

        #endregion

        #region Public methods

        public static void SetGlobalHeader(string name, string value) {
            Instance.globalHeaders.Add(name, value);
        }

        public static bool RemoveGlobalHeader (string name) {
            if (Instance.globalHeaders.ContainsKey(name)) {
                Instance.globalHeaders.Remove(name);
                return true;
            } else {
                return false;
            }
        }

        public static Dictionary<string, string> GetGlobalHeaders () {
            return Instance.globalHeaders;
        }

        #endregion

        #region Get texture

        public static HttpRequest GetTexture (string url, bool nonReadable = false) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Texture", url, Instance.globalHeaders, nonReadable, "", "");
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        public static HttpRequest GetTexture (Uri uri, bool nonReadable = false) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Texture", uri, Instance.globalHeaders, nonReadable, "", "");
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        #endregion

        #region Get asset bundle

        public static HttpRequest GetAssetBundle (string url) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "AssetBundle", url, Instance.globalHeaders, false, "", "");
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        public static HttpRequest GetAssetBundle (Uri uri) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "AssetBundle", uri, Instance.globalHeaders, false, "", "");
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        #endregion

        #region Get

        public static HttpRequest Get(string url) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Get", url, Instance.globalHeaders, false, "", "");
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        public static HttpRequest Get (Uri uri) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Get", uri, Instance.globalHeaders, false, "", "");
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        #endregion

        #region Post

        public static HttpRequest Post (string url, string postData) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Post", url, Instance.globalHeaders, false, postData, "");
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        public static HttpRequest Post (Uri uri, string postData) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Post", uri, Instance.globalHeaders, false, postData, "");
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        public static HttpRequest Post (string url, WWWForm postData) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Post", url, Instance.globalHeaders, false, postData, null);
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        public static HttpRequest Post (Uri uri, WWWForm postData) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Post", uri, Instance.globalHeaders, false, postData, null);
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        public static HttpRequest Post (string url, Dictionary<string, string> postData) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Post", url, Instance.globalHeaders, false, postData);
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        public static HttpRequest Post (Uri uri, Dictionary<string, string> postData) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Post", uri, Instance.globalHeaders, false, postData);
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        #endregion

        #region Post json

        public static HttpRequest PostJson (string url, string json) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "PostJson", url, Instance.globalHeaders, false, json, "");
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        public static HttpRequest PostJson (Uri uri, string json) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "PostJson", uri, Instance.globalHeaders, false, json, "");
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        #endregion

        #region Put

        public static HttpRequest Put (string url, string putData) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Put", url, Instance.globalHeaders, false, "", putData);
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        public static HttpRequest Put (Uri uri, string putData) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Put", uri, Instance.globalHeaders, false, "", putData);
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        public static HttpRequest Put (string url, byte[] putData) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Put", url, Instance.globalHeaders, false, null, putData);
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        public static HttpRequest Put (Uri uri, byte[] putData) {
            string newGuid = Guid.NewGuid().ToString();
            HttpRequest newRequest = new HttpRequest(newGuid, _removeFromDict, "Put", uri, Instance.globalHeaders, false, null, putData);
            Instance.requests.Add(newGuid, newRequest);
            return newRequest;
        }

        #endregion
    }

    public class HttpRequest {
        #region Private fields

        string guid;
        UnityWebRequest request;
        UnityWebRequestAsyncOperation operation;
        Action<string> removeAction;
        bool isStarted = false;
        float _downloadProgress = 0;
        float _uploadProgress = 0;
        float _progress = 0;

        public struct HttpResponce {
            public string text;
            public byte[] bytes;
            public long responceCode;
            public string error;
            public bool isError;
            public Texture texture;
            public AssetBundle assetBundle;
        }

        Action<float> onUploadProgress;
        Action<float> onDownloadProgress;
        Action<float> onProgress;
        Action<HttpResponce> onSuccess;
        Action<object> onSuccessClass;
        Type classType;
        Action<HttpResponce> onError;

        Dictionary<string, string> headers = new Dictionary<string, string>();

        #endregion

        #region Public fields

        public string url { get {
                return request.url;
            }
        }

        public bool isDone {
            get {
                return request.isDone;
            }
        }

        [Obsolete]
        public bool isError {
            get {
                return request.isHttpError;
            }
        }

        [Obsolete]
        public bool isNetworkError {
            get {
                return request.isNetworkError;
            }
        }

        public string error {
            get {
                return request.error;
            }
        }

        public long responseCode {
            get {
                return request.responseCode;
            }
        }

        public string text {
            get {
                return request.downloadHandler.text;
            }
        }

        public byte[] bytes {
            get {
                return request.downloadHandler.data;
            }
        }

        public Texture texture {
            get {
                try {
                    return DownloadHandlerTexture.GetContent(request);
                } catch {
                    return null;
                }
            }
        }

        public AssetBundle assetBundle {
            get {
                try {
                    return DownloadHandlerAssetBundle.GetContent(request);
                } catch {
                    return null;
                }
            }
        }

        public Dictionary<string, string> requestHeaders {
            get {
                return headers;
            }
        }

        public Dictionary<string, string> responseHeaders {
            get {
                return request.GetResponseHeaders();
            }
        }

        public float progress {
            get {
                if (isStarted) {
                    float newProgress = operation.progress;
                    if (newProgress != _progress) {
                        onProgress?.Invoke(newProgress);
                    }
                    _progress = newProgress;

                    newProgress = request.uploadProgress;
                    if (newProgress != _uploadProgress) {
                        onUploadProgress?.Invoke(newProgress);
                    }
                    _uploadProgress = newProgress;

                    newProgress = request.downloadProgress;
                    if (newProgress != _downloadProgress) {
                        onDownloadProgress?.Invoke(newProgress);
                    }
                    _downloadProgress = newProgress;
                }
                return _progress;
            }
        }

        #endregion

        #region Constructors

        void _constructor (string guid, Action<string> removeAction, Dictionary<string, string> globalHeaders) {
            this.guid = guid;
            this.removeAction = removeAction;
            if(globalHeaders != null) {
                foreach(var header in globalHeaders) {
                    headers.Add(header.Key, header.Value);
                }
            }

            if(Http.debug) {
                Debug.Log("New request: " + guid);
            }
        }

        public HttpRequest(string guid, Action<string> removeAction, string type, string url, Dictionary<string, string> globalHeaders = null, bool nonReadable = false, string postData = "", string putData = "") {
            _constructor(guid, removeAction, globalHeaders);

            switch (type) {
                case "Get":
                    request = UnityWebRequest.Get(url);
                    break;
                case "AssetBundle":
                    request = UnityWebRequestAssetBundle.GetAssetBundle(url);
                    break;
                case "Post":
                    request = UnityWebRequest.PostWwwForm(url, postData);
                    break;
                case "PostJson":
                    request = UnityWebRequest.Put(url, postData);
                    request.SetRequestHeader("Content-Type", "application/json");
                    break;
                case "Put":
                    request = UnityWebRequest.Put(url, putData);
                    break;
                case "Texture":
                    request = UnityWebRequestTexture.GetTexture(url, nonReadable);
                    break;
            }
        }

        public HttpRequest (string guid, Action<string> removeAction, string type, Uri uri, Dictionary<string, string> globalHeaders = null, bool nonReadable = false, string postData = "", string putData = "") {
            _constructor(guid, removeAction, globalHeaders);

            switch (type) {
                case "Get":
                    request = UnityWebRequest.Get(uri);
                    break;
                case "AssetBundle":
                    request = UnityWebRequestAssetBundle.GetAssetBundle(uri);
                    break;
                case "Post":
                    request = UnityWebRequest.PostWwwForm(uri, postData);
                    break;
                case "PostJson":
                    request = UnityWebRequest.Put(uri, postData);
                    request.SetRequestHeader("Content-Type", "application/json");
                    break;
                case "Put":
                    request = UnityWebRequest.Put(uri, putData);
                    break;
                case "Texture":
                    request = UnityWebRequestTexture.GetTexture(uri, nonReadable);
                    break;
            }
        }

        public HttpRequest (string guid, Action<string> removeAction, string type, string url, Dictionary<string, string> globalHeaders = null, bool nonReadable = false, WWWForm postData = null, byte[] putData = null) {
            _constructor(guid, removeAction, globalHeaders);

            switch (type) {
                case "Get":
                    request = UnityWebRequest.Get(url);
                    break;
                case "Post":
                    request = UnityWebRequest.Post(url, postData);
                    break;
                case "Put":
                    request = UnityWebRequest.Put(url, putData);
                    break;
                case "Texture":
                    request = UnityWebRequestTexture.GetTexture(url, nonReadable);
                    break;
            }
        }

        public HttpRequest (string guid, Action<string> removeAction, string type, Uri uri, Dictionary<string, string> globalHeaders = null, bool nonReadable = false, WWWForm postData = null, byte[] putData = null) {
            _constructor(guid, removeAction, globalHeaders);

            switch (type) {
                case "Get":
                    request = UnityWebRequest.Get(uri);
                    break;
                case "Post":
                    request = UnityWebRequest.Post(uri, postData);
                    break;
                case "Put":
                    request = UnityWebRequest.Put(uri, putData);
                    break;
                case "Texture":
                    request = UnityWebRequestTexture.GetTexture(uri, nonReadable);
                    break;
            }
        }

        public HttpRequest (string guid, Action<string> removeAction, string type, string url, Dictionary<string, string> globalHeaders = null, bool nonReadable = false, Dictionary<string, string> postData = null) {
            _constructor(guid, removeAction, globalHeaders);

            switch (type) {
                case "Get":
                    request = UnityWebRequest.Get(url);
                    break;
                case "Post":
                    request = UnityWebRequest.Post(url, postData);
                    break;
                case "Texture":
                    request = UnityWebRequestTexture.GetTexture(url, nonReadable);
                    break;
            }
        }

        public HttpRequest (string guid, Action<string> removeAction, string type, Uri uri, Dictionary<string, string> globalHeaders = null, bool nonReadable = false, Dictionary<string, string> postData = null) {
            _constructor(guid, removeAction, globalHeaders);

            switch (type) {
                case "Get":
                    request = UnityWebRequest.Get(uri);
                    break;
                case "Post":
                    request = UnityWebRequest.Post(uri, postData);
                    break;
                case "Texture":
                    request = UnityWebRequestTexture.GetTexture(uri, nonReadable);
                    break;
            }
        }

        #endregion

        #region Events

        void Operation_completed (AsyncOperation obj) {
            if (request.error == null) {
                if (Http.debug) {
                    Debug.Log("Request " + guid + " has finished successfully");
                }

                onSuccess?.Invoke(new HttpResponce() {
                    text = request.downloadHandler.text,
                    bytes = request.downloadHandler.data,
                    responceCode = request.responseCode,
                    isError = false,
                    texture = texture
                });

                if(onSuccessClass != null && classType != null) {
                    var responce = JsonUtility.FromJson(request.downloadHandler.text, classType);
                    onSuccessClass.Invoke(responce);
                }
            } else {
                if (Http.debug) {
                    Debug.Log("Request " + guid + " has finished with error " + request.error);
                }

                onError?.Invoke(new HttpResponce() {
                    error = request.error,
                    responceCode = request.responseCode,
                    isError = true
                });
            }

            request.Dispose();

            removeAction?.Invoke(guid);
        }

        public HttpRequest OnSuccess (Action<HttpResponce> onSuccess) {
            this.onSuccess = onSuccess;
            return this;
        }

        public HttpRequest OnSuccessClass<T> (Action<T> onSuccessClass) where T : class {
            this.onSuccessClass = (responce) => onSuccessClass((T)responce);
            this.classType = typeof(T);
            return this;
        }

        public HttpRequest OnError (Action<HttpResponce> onError) {
            this.onError = onError;
            return this;
        }

        public HttpRequest OnProgress (Action<float> onProgress) {
            this.onProgress = onProgress;
            return this;
        }

        public HttpRequest OnUploadProgress (Action<float> onUploadProgress) {
            this.onUploadProgress = onUploadProgress;
            return this;
        }

        public HttpRequest OnDownloadProgress (Action<float> onDownloadProgress) {
            this.onDownloadProgress = onDownloadProgress;
            return this;
        }

        #endregion

        #region Public methods

        public HttpRequest SetBearerToken (string value) {
            headers.Add("Authorization", "Bearer " + value);
            return this;
        }

        public HttpRequest SetHeader (string name, string value) {
            headers.Add(name, value);
            return this;
        }

        public HttpRequest SetHeaders (Dictionary<string, string> requestHeaders) {
            foreach(var header in requestHeaders) {
                headers.Add(header.Key, header.Value);
            }
            return this;
        }

        public HttpRequest RemoveHeader (string name) {
            headers.Remove(name);
            return this;
        }

        public HttpRequest SetRedirectLimit (int limit) {
            request.redirectLimit = limit;
            return this;
        }

        public HttpRequest SetTimeout (int timeout) {
            request.timeout = timeout;
            return this;
        }

        public void Abort () {
            request.Abort();

            request.Dispose();

            removeAction?.Invoke(guid);
        }

        public HttpRequest Send () {
            foreach(var header in headers) {
                request.SetRequestHeader(header.Key, header.Value);
            }

            operation = request.SendWebRequest();
            operation.completed += Operation_completed;
            isStarted = true;

            if(Http.debug) {
                Debug.Log("Request " + guid + " has started");
            }

            return this;
        }

        #endregion
    }
}