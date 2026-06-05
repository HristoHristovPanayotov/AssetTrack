Front-end libraries (Bootstrap, jQuery, jQuery-Validation) are loaded from a CDN
in _Layout.cshtml and _ValidationScriptsPartial.cshtml so the app runs out of the box.

To serve them locally instead (e.g. for offline grading), restore the included
libman.json from the solution root:  Right-click the project > "Restore Client-Side
Libraries", or run  `libman restore`  from the AssetTrack.Web folder. Then swap the
CDN <link>/<script> tags in the layout for the local ~/lib/... paths.
