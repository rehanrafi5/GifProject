using UnityEngine;
using System.Runtime.InteropServices;

public class GifSender : MonoBehaviour
{
//     // Calls JS to add frame
//     [DllImport("__Internal")]
//     private static extern void AddFrameToGif(string base64PNG);
//
//     // Calls JS to save GIF
//     [DllImport("__Internal")]
//     private static extern void SaveGif();
//
//     public Texture2D frame; // frame to add
//
//     // Add a frame to the GIF
//     public void AddFrame()
//     {
// #if UNITY_WEBGL && !UNITY_EDITOR
//         byte[] bytes = frame.EncodeToPNG();
//         string base64 = System.Convert.ToBase64String(bytes);
//         AddFrameToGif(base64);
// #endif
//     }
//
//     // Finish and save GIF
//     public void FinishGif()
//     {
// #if UNITY_WEBGL && !UNITY_EDITOR
//         SaveGif();
// #endif
//     }
//
//     // Inject gif.js dynamically after page loads
//     void Start()
//     {
// #if UNITY_WEBGL && !UNITY_EDITOR
//         Application.ExternalEval(@"
//             var script = document.createElement('script');
//             script.src = 'StreamingAssets/WebGL/gif.js';
//             document.body.appendChild(script);
//
//             window.gif = new GIF({workers:2, workerScript:'StreamingAssets/WebGL/gif.worker.js', quality:10});
//
//             window.AddFrameToGif = function(base64PNG){
//                 var img = new Image();
//                 img.src = 'data:image/png;base64,'+base64PNG;
//                 img.onload = function(){ window.gif.addFrame(img,{delay:100}); };
//             };
//
//             window.SaveGif = function(){
//                 window.gif.on('finished', function(blob){
//                     var url = URL.createObjectURL(blob);
//                     var a = document.createElement('a');
//                     a.href = url;
//                     a.download = 'unity_gif.gif';
//                     document.body.appendChild(a);
//                     a.click();
//                     document.body.removeChild(a);
//                     URL.revokeObjectURL(url);
//                 });
//                 window.gif.render();
//             };
//         ");
// #endif
//     }
}