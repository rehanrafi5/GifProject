mergeInto(LibraryManager.library, {

    CreateGIF: function(namePtr, delay, w, h) {
        var name = UTF8ToString(namePtr);
        window.gifFrames = [];
        window.gifDelay = delay;
        window.gifWidth = w;
        window.gifHeight = h;
        window.gifName = name;
    },

    AddFrame: function(ptr, len) {
        var data = HEAPU8.slice(ptr, ptr + len);
        window.gifFrames.push(data);
    },

    FinishGIF: function() {
        // Use gif.js
        var gif = new GIF({
            workers: 2,
            quality: 10,
            width: window.gifWidth,
            height: window.gifHeight,
            transparent: 0x00FF00 // chroma key for alpha
        });

        for (var i = 0; i < window.gifFrames.length; i++) {
            var src = window.gifFrames[i];
            var rgba = new Uint8ClampedArray(src.length);

            for (var p = 0; p < src.length; p += 4) {
                var r = src[p];
                var g = src[p + 1];
                var b = src[p + 2];
                var a = src[p + 3];

                if (a < 10) {
                    rgba[p] = 0;
                    rgba[p + 1] = 255;
                    rgba[p + 2] = 0;
                    rgba[p + 3] = 255;
                } else {
                    rgba[p] = r;
                    rgba[p + 1] = g;
                    rgba[p + 2] = b;
                    rgba[p + 3] = 255;
                }
            }

            var frame = new ImageData(rgba, window.gifWidth, window.gifHeight);
            gif.addFrame(frame, { delay: window.gifDelay });
        }

        gif.on('finished', function(blob) {
            var url = URL.createObjectURL(blob);
            var a = document.createElement("a");
            a.href = url;
            a.download = window.gifName + ".gif";
            a.click();
        });

        gif.render();
    }
});
