mergeInto(LibraryManager.library, {
    DownloadGif: function (fileNamePtr, byteArrayPtr, byteArrayLength) {
        try {
            var fileName = UTF8ToString(fileNamePtr);
            var bytes = HEAPU8.slice(byteArrayPtr, byteArrayPtr + byteArrayLength);

            var blob = new Blob([bytes], { type: "image/gif" });
            var url = URL.createObjectURL(blob);

            var link = document.createElement("a");
            link.href = url;
            link.download = fileName;

            document.body.appendChild(link);
            link.click();

            document.body.removeChild(link);
            URL.revokeObjectURL(url);
        }
        catch (e) {
            console.error("DownloadGif failed:", e);
        }
    }
});
