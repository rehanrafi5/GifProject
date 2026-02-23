mergeInto(LibraryManager.library, {

    JS_InitGIF: function (w, h, fps) {

        window._gif = new GIF({
            workers: 2,
            quality: 10,
            width: w,
            height: h,
            workerScript: "StreamingAssets/WebGL/gif.worker.js"
        });

        window._gifFPS = fps;
        console.log("GIF INIT", w, h, fps);
    },

    JS_AddFrame: function (base64Ptr) {

        var base64 = UTF8ToString(base64Ptr);

        var img = new Image();
        img.onload = function () {
            window._gif.addFrame(img, { delay: 1000 / window._gifFPS });
        };

        img.src = "data:image/png;base64," + base64;
    },

    JS_FinishGIF: function () {

        window._gif.on('finished', function (blob) {

            var a = document.createElement('a');
            a.href = URL.createObjectURL(blob);
            a.download = "animation.gif";
            a.click();
        });

        window._gif.render();

        console.log("GIF RENDER START");
    }
});
