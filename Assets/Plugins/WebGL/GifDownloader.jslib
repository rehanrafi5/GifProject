mergeInto(LibraryManager.library, {
    DownloadGifFile: function (byteArrayPtr, byteArrayLength, fileNamePtr) 
    {
        var fileName = UTF8ToString(fileNamePtr);

        var bytes = new Uint8Array(byteArrayLength);
        for (var i = 0; i < byteArrayLength; i++) {
            bytes[i] = HEAPU8[byteArrayPtr + i];
        }

        var blob = new Blob([bytes], { type: "image/gif" });

        var link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = fileName;

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
});