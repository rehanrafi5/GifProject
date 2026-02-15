mergeInto(LibraryManager.library, {

    InitGifJS: function () {
        if (window.gifJSLoaded) {
            console.log("GIF already loaded");
            return;
        }

        var script = document.createElement("script");
        script.src = "StreamingAssets/WebGL/gif.js";
        script.onload = function () {
            window.gifJSLoaded = true;
            console.log("GIF library loaded");
        };

        document.head.appendChild(script);
    },

    StartGifRecordingJS: function (width, height, fps, duration, fileNamePtr) {
        if (!window.gifJSLoaded || typeof GIF === "undefined") {
            console.error("GIF library not loaded yet");
            return;
        }

        var fileName = UTF8ToString(fileNamePtr);

        window.gifRecorder = new GIF({
            workers: 2,
            quality: 10,
            width: width,
            height: height,
            workerScript: "StreamingAssets/WebGL/gif.worker.js"
        });

        window.gifFileName = fileName || "recorded.gif";

        console.log("GIF recording started: " + window.gifFileName);
    },

    AddFrameToGifJS: function (pngPtr, length) {
        if (!window.gifRecorder) {
            console.error("GIF recorder not initialized");
            return;
        }

        var bytes = new Uint8Array(Module.HEAPU8.buffer, pngPtr, length);
        var blob = new Blob([bytes], { type: "image/png" });
        var url = URL.createObjectURL(blob);

        var img = new Image();
        img.onload = function () {
            window.gifRecorder.addFrame(img, { delay: 100 });
            URL.revokeObjectURL(url);
        };

        img.src = url;
    },

    FinishGifRecordingJS: function () {
        if (!window.gifRecorder) {
            console.error("GIF recorder not initialized");
            return;
        }

        console.log("Rendering GIF...");

        window.gifRecorder.on("finished", function (blob) {
            var a = document.createElement("a");
            a.href = URL.createObjectURL(blob);
            a.download = window.gifFileName || "recorded.gif";
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);

            console.log("GIF DOWNLOAD TRIGGERED");
        });

        window.gifRecorder.render();
    }

});
