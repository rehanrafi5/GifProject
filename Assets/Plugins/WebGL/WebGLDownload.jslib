mergeInto(LibraryManager.library, {
    DownloadFileFromUnity: function(pathPtr, filenamePtr) {
        var path = UTF8ToString(pathPtr);
        var filename = UTF8ToString(filenamePtr);

        var link = document.createElement('a');
        link.href = path;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
});
